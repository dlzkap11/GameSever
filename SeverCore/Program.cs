using System;
using System.Threading;

namespace SeverCore
{
    //메모리 베리어
    // A) 코드 재배치 억제
    // B) 가시성

    // 1) Full Memory Barrier (ASM MFENCE, C# Thread.MemoryBarrier) : Store/Load 둘 다 막는다
    // 2) Store Memory Barrier (ASM SFENCE) : Store만 막는다
    // 3) Load Memory Barrier (ASM LFENCE) : Load만 막는다

    class Progaram
    {
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;
        static int x1 = 0;
        static int x2 = 0;
        static int r3 = 0;
        static int r4 = 0;

        static void Thread_1()
        {
            y = 1; //Store y


            Thread.MemoryBarrier();
            x1 = 1;

            //----------------------
            //Thread.MemoryBarrier();

            r1 = x; //Load x
            r3 = x2;
            
        }

        static void Thread_2() 
        {
            x = 1; //Store x
            Thread.MemoryBarrier();
            x2 = 1;

            //Thread.MemoryBarrier();

            r2 = y; //Load y
            r4 = x1;
            
        }

        static void Main(string[] args)
        {
            int count = 0;
            while (true)
            {
                count++;
                x = y = r1 = r2 = x1 = x2 = r3 = r4 = 0;
                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);
                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);

                if (r1 == 0 && r2 == 0 && r3 == 0 && r4 == 0)
                    break;
                
            }

            Console.WriteLine($"{count}번만에 빠져나옴");
        }
    }
}