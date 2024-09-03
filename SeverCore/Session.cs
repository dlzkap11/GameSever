using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SeverCore
{
    class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();

        List<ArraySegment<byte>> _pendinglist = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();



        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024); //빈 버퍼

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendinglist.Count == 0)
                    RegisterSend();
            }
            
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            //byte[] buff = _sendQueue.Dequeue();
            //_sendArgs.SetBuffer(buff, 0, buff.Length); // 실제 보낼 데이터가 있는 버퍼

            // a[ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendinglist.Add(new ArraySegment<byte>(buff, 0, buff.Length)); //(배열, 시작인덱스, 크기)
            }

            _sendArgs.BufferList = _pendinglist;


            bool pending = _socket.SendAsync(_sendArgs);
            if(pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendinglist.Clear();

                        Console.WriteLine($"Transferred bytes: {_sendArgs.BytesTransferred}");

                        if (_sendQueue.Count > 0)
                            RegisterSend();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed :{e}");
                    }

                }
                else
                {
                    Disconnect();
                }
            }
            
        }

        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if(pending == false)
                OnRecvCompleted(null, _recvArgs);

        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed :{e}");
                }
               
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
