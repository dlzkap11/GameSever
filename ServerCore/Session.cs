﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendinglist = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);
 

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
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

            OnDisconnected(_socket.RemoteEndPoint);
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

                        OnSend(_sendArgs.BytesTransferred);
                        

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

            _recvBuffer.Clean();
            ArraySegment<byte> segement =  _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segement.Array, segement.Offset, segement.Count);

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
                    //Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }


                    //컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if(_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    //OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));       
                    
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
