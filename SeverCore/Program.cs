using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Collections.Specialized.BitVector32;

namespace SeverCore
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Sever!");
            Send(sendBuff);
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
    class Program
    {
       
    //메모리 베리어
    // A) 코드 재배치 억제
    // B) 가시성

    // 1) Full Memory Barrier (ASM MFENCE, C# Thread.MemoryBarrier) : Store/Load 둘 다 막는다
    // 2) Store Memory Barrier (ASM SFENCE) : Store만 막는다
    // 3) Load Memory Barrier (ASM LFENCE) : Load만 막는다

    // atomic = 원자성
    // SpinLock -> 대기상태에서 무한으로 뺑뺑이 도는 락(무작정 기다리기때문에 CPU점유율이 확 튀는 현상이 발생 할 수 있음...)

        static int count = 0; 
        static Lock _lock = new Lock();
        static Socket_1 _socket = new Socket_1();

        //TLS (Thread Local Storage) 영역전개 굳이 락을 걸지않아도 각자의 영역이 있음 쓰레드 전역변수를 사용할 때 많이 사용가능
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My Name is {Thread.CurrentThread.ManagedThreadId}"; });

        static void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated;
            if(repeat)
                Console.WriteLine(ThreadName.Value + "(repeat)");
            else
                Console.WriteLine(ThreadName.Value);
        }


        static void Main(string[] args)
        {

            //ThreadPool.SetMinThreads(1, 1);
            //ThreadPool.SetMaxThreads(3, 3);
            //Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            _socket.Listen();


            //ThreadName.Dispose();


            /*
            Task t1 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    _lock.WriteLock();
                    count++;
                    _lock.WriteUnlock();
                    _lock.WriteUnlock();
                }
            });

            Task t2 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnlock();
                }
            });

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(count);
            */
        }
    }
   
    
}