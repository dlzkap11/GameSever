using System;
using System.ComponentModel.Design.Serialization;
using System.Threading;

namespace SeverCore
{
    //메모리 베리어
    // A) 코드 재배치 억제
    // B) 가시성

    // 1) Full Memory Barrier (ASM MFENCE, C# Thread.MemoryBarrier) : Store/Load 둘 다 막는다
    // 2) Store Memory Barrier (ASM SFENCE) : Store만 막는다
    // 3) Load Memory Barrier (ASM LFENCE) : Load만 막는다

    // atomic = 원자성
    // 


    class SessionManger
    {
        static object _lock = new object();

        public static void TestSession()
        {
            lock (_lock)
            {
               
            }
        }
        public static void Test()
        {
            lock (_lock)
            {
                UserManger.TestUser();
            }
        }

    }
    class UserManger
    {
        static object _lock = new object();

        public static void Test()
        {
            //Monitor.TryEnter(); //실패했을시 일정시간 이후 나가기
            lock (_lock)
            {
                SessionManger.TestSession();
            }
        }

        public static void TestUser()
        {
            lock(_lock)
            {

            }
        }
    }

    class Progaram
    {
        static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {

            for (int i = 0; i < 100; i++)
            {
                //상호배제 Mutual Exclusive
                //CriticalSection(window), std:mutex(C++)
                //데드락 DeadLock 

                //lock (_obj) //실제구조는 Monitor와 같음
                //{
                //    number++;
                //}
                SessionManger.Test();
                /*
                try
                {
                    Monitor.Enter(_obj); //문을 잠그는 행위
                    number++;
                    return;
                }
                finally
                {
                    Monitor.Exit(_obj); // 잠금 해제
                }
                */
            }
                           
        }
        

        static void Thread_2()
        {
            for (int i = 0; i < 100; i++)
            {
                //lock (_obj) //실제구조는 Monitor와 같음
                //{
                //    number--;
                //}
                UserManger.Test();
            }
                
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();

            Thread.Sleep(100);

            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}