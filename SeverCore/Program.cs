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

                //쉬는 시간
                //Thread.Sleep(1); // 무조건 휴식 -> 1ms 쉬겠다 희망
                //Thread.Sleep(0); // 조건부 양보 -> 나보다 우선순위가 낮은 애들한테는 양보 불가 -> 우선순위가 나보다 같거나 높은 쓰레드가 없으면 다시 본인한테 강약약강
                Thread.Yield();  // 관대한 양보 -> 관대하게 양보, 지금 실행이 가능한 쓰레드가 있으면 실행해라 -> 실행 가능 쓰레드가 없으면 본인이 시간 소진


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
        static Lock _lock = new Lock();

        static void Thread_1()
        {
            for(int i = 0; i < 1000000; i++)
            {
                _lock.Acquire(); //들어가기전 확인 및 들어간 후 락을 잠금
                _num++;
                _lock.Release(); //끝나고 락을 풀어줌
            }
            
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000; i++)
            {
                _lock.Acquire(); //들어가기전 확인 및 들어간 후 락을 잠금
                _num--;
                _lock.Release(); //끝나고 락을 풀어줌
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