using ServerCore;
using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    class Program
    {
        class GameSession : Session
        {
            public override void OnConnected(EndPoint endPoint)
            {
                Console.WriteLine($"Onconnected : {endPoint}");

                // 보낸다
                for (int i = 0; i < 5; i++)
                {
                    byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World! {i}");
                    Send(sendBuff);
                }
            }

            public override void OnDisconnected(EndPoint endPoint)
            {
                Console.WriteLine($"OnDisconnected : {endPoint}");
            }

            public override void OnRecv(ArraySegment<byte> buffer)
            {
                string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
                Console.WriteLine($"[From Server] {recvData}");
            }

            public override void OnSend(int numOfBytes)
            {
                Console.WriteLine($"Transferred bytes: {numOfBytes}");
            }
        }

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                /*
                // 휴대폰 설정
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //문지기한테 입장 문의
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");
                                  
                    // 받는다
                    
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server]{recvData}");
                    
                    // 나간다
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }


                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                */
            }
            
            Thread.Sleep(1000);

        }
    }
}
