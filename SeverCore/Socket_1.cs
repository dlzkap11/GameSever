using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SeverCore
{
    class Socket_1
    {
        public void Listen()
        {
            //DNS (Domain Name System)
            //172.1.2.3 -> IP | www.naver.com -> Domain |

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
           

            //문지기
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //문지기교육
                listenSocket.Bind(endPoint);

                // 영업시작
                // backlog : 최대 대기수
                listenSocket.Listen(1000);

                while (true)
                {
                    Console.WriteLine("Listening...");

                    // 손님을 입장시킨다
                    Socket clientSocket = listenSocket.Accept();


                    // 받는다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Client] {recvData}");

                    // 보낸다
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Sever!");
                    clientSocket.Send(sendBuff);

                    // 닫는다
                    clientSocket.Shutdown(SocketShutdown.Both); //예고
                    clientSocket.Close();


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            


        }
        
    }
}
