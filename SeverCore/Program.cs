using System;
using System.Threading;

namespace SeverCore
{
    class Progaram
    {
        static void MainThread(object state)
        {
            for(int i = 0; i < 5; i++)
                Console.WriteLine("Hello Thread!");
        }


        static void Main(string[] args)
        {          
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);

            for (int i = 0; i < 5; i++)
            {
                Task t = new Task(() => { while (true) { }; }, TaskCreationOptions.LongRunning);
                t.Start();
            }
                

            //for (int i = 0; i < 5; i++)
            //    ThreadPool.QueueUserWorkItem((obj) => { while (true) { }; });

            ThreadPool.QueueUserWorkItem(MainThread);

            //Thread t = new Thread(MainThread);
            //t.Name = "Test Thread";
            //t.IsBackground = true; //백그라운드실행(main이 종료되면 같이 종료)
            //t.Start();
            //Console.WriteLine("Waiting for Thread!");

            //t.Join(); //작업이 끝나는 것을 기다림
            //Console.WriteLine("Hello World!");
            while (true)
            {

            }
        }
    }
}