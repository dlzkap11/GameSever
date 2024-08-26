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
    class SpinLock
    {
        volatile int _locked = 0; 

        public void Acquire()
        {
            while (true)
            {
                //int original = Interlocked.Exchange(ref _locked, 1);
                //if (original == 0)
                //    break;
                //{
                //    int original = _locked;
                //    _locked = 1;
                //    if (original == 0)
                //        break;
                //}

                // CAS Compare-And-Swap
                int expected = 0; //예상하는 값
                int desired = 1;  //원하는 값
                if(Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                    break;
                //{
                //    if(_locked == 0)
                //        _locked = 1;
                //}
            }
            
        }

        public void Release()
        {
            _locked = 0; 
        }
    }

    class Program
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            _lock.Acquire(); //들어가기전 확인 및 들어간 후 락을 잠금
            _num++;
            _lock.Release(); //끝나고 락을 풀어줌
        }

        static void Thread_2()
        {
            _lock.Acquire();
            _num--;
            _lock.Release();
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