using System;
using System.Threading;

namespace SeverCore
{
    class Progaram
    {
        volatile static bool _stop = false; //volatile 잘 안씀...=

        static void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작");

            while (_stop == false)
            {
                //누군가 stop신호를 해주기를 기다린다
            }

            Console.WriteLine("쓰레드 종료");
        }

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();

            Thread.Sleep(1000);

            _stop = true;


            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기중");
            t.Wait();//Join
            Console.WriteLine("종료 성공");
        }
    }
}