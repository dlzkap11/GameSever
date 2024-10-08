﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    //재귀적 락을 허용할지 (Yes) WriteLock -> WriteLock OK,  WriteLock -> ReadLock OK, ReadLock -> WriteLock NO
    //스핀란 정책 (5000번 -> Yield)
    public class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        //[Unused(1)]  [WriteThreadId(15비트)] [ReadCount(16비트)]
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {

            //동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인있으면 return   
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if(Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                _writeCount++;
                return;
            }
            

            //아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for(int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    //시도를 해서 성공하면 return
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                    
                    //if (_flag == EMPTY_FLAG)
                    //    _flag = desired;
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if (lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);    
        }

        public void ReadLock()
        {
            //동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }


            //아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++) 
                {
                    int expected = (_flag & READ_MASK); // A(0) B(0)
                    if(Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected) // A(0->1) B(0->1) , A(1) B(0->1)=실패 후 다시 시도
                        return;

                    //if ((_flag & WRITE_MASK) == 0)
                    //{
                    //    _flag = _flag + 1;
                    //    return;
                    //}
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }

    }
}
