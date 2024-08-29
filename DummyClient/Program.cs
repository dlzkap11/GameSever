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
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            while (true)
            {
                // 휴대폰 설정
                Socket socktet = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //문지기한테 입장 문의
                    socktet.Connect(endPoint);
                    Console.WriteLine($"Connected To {socktet.RemoteEndPoint.ToString()}");

                    // 보낸다
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World!");
                    int sendBytes = socktet.Send(sendBuff);

                    // 받는다

                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socktet.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server]{recvData}");

                    // 나간다
                    socktet.Shutdown(SocketShutdown.Both);
                    socktet.Close();
                }


                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            
            Thread.Sleep(1000);

        }
    }
}
