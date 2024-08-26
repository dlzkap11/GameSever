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
    // SpinLock -> 대기상태에서 무한으로 뺑뺑이 도는 락(무작정 기다리기때문에 CPU점유율이 확 튀는 현상이 발생 할 수 있음...)
    class Lock
    {
        //bool <- 커널
        AutoResetEvent _available = new AutoResetEvent(true);

        //ManualResetEvent _available_2 = new ManualResetEvent(true); //lock구현시에는 잘 안씀..

        public void Acquire()
        {
            _available.WaitOne(); // 입장 시도
            //_available.Reset(); //bool = false(WaitOne에서 알아서 해줌 Auto)

            //_available_2.WaitOne();
            //_available_2.Reset(); //문을 닫는다.

        }

        public void Release()
        {
            _available.Set();  //flag = true

            //_available_2.Set();
        }
    }

    class Program
    {
        static int _num = 0;
        static Lock _lock = new Lock();
        static Mutex _lock_2 = new Mutex(); //커널동기화객체..

        // bool AutoResetEvent 


        // int ThreadId  Mutex -> 좀 더 비용이 많이 듬


        static void Thread_1()
        {
            for(int i = 0; i < 10000; i++)
            {
                //_lock.Acquire(); //들어가기전 확인 및 들어간 후 락을 잠금
                //_num++;
                //_lock.Release(); //끝나고 락을 풀어줌

                _lock_2.WaitOne();
                _num++;
                _lock_2.ReleaseMutex();
            }
            
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                //_lock.Acquire(); //들어가기전 확인 및 들어간 후 락을 잠금
                //_num--;
                //_lock.Release(); //끝나고 락을 풀어줌

                _lock_2.WaitOne();
                _num--;
                _lock_2.ReleaseMutex();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);

        }
    }
   
    
}