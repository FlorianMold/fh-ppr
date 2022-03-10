using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HandsOnSharp
{
    public class Introduction
    {
        private static readonly int n = 500000000;
        public static volatile int one = 1;

        static void Count(int c, ref int result)
        {
            var sum = 0;
            for (int i = 0; i < c; i++)
            {
                sum += one;
            }
            Interlocked.Add(ref result, sum);
        }

        static void TestCountSerial()
        {
            var result = 0;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Count(n, ref result);
            Count(n, ref result);
            sw.Stop();
            Console.WriteLine("result: {0} in {1}ms", result, sw.Elapsed.TotalMilliseconds);
        }

        static void TestCountLocal()
        {
            var result = 0;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var t1 = new Thread(() => Count(n, ref result));
            t1.Start();
            var t2 = new Thread(() => Count(n, ref result));
            t2.Start();
            t1.Join();
            t2.Join();
            sw.Stop();
            Console.WriteLine("result: {0} in {1}ms", result, sw.Elapsed.TotalMilliseconds);
        }

        static R MyLock1<R>(object lockObj, Func<R> f)
        {
            Monitor.Enter(lockObj);
            // what if asynchronous exception happens here?
            try
            {
                var r = f();
                return r;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        static R MyLock2<R>(object lockObj, Func<R> f)
        {
            var lockTaken = false;
            try
            {
                //...
                Monitor.Enter(lockObj, ref lockTaken);
                return f();
            }
            finally
            {
                if(lockTaken)
                    Monitor.Exit(lockObj);
            }
        }

        private static SpinLock spinLock = new SpinLock(enableThreadOwnerTracking: false);
        public static void SpinLockTest()
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);
                // critical section
            }
            finally
            {
                if (lockTaken) spinLock.Exit();
            }
        }

        public static void Deadlock()
        {
            var r1 = new object();
            var r2 = new object();

            var t1 = new Thread(() =>
            {
                // circular wait
                // hold and wait
                // no premption
                // mutual exclusion
                Monitor.Enter(r1);
                Thread.Sleep(100);
                Monitor.Enter(r2);
                Thread.Sleep(100);
                Monitor.Exit(r2);
                Monitor.Exit(r1);
            });


            var t2 = new Thread(() =>
            {
                Monitor.Enter(r1);
                Thread.Sleep(100);
                Monitor.Enter(r2);
                Monitor.Exit(r2);
                Monitor.Exit(r1);
            });


            t1.Start(); t2.Start();
            t1.Join(); t2.Join();
        }

        public static void OwnLock1()
        {
            var lockObj = new object();
            var value = 0;
            var t1 = new Thread(() =>
            {
                Monitor.Enter(lockObj);
                value++;
                Monitor.Exit(lockObj);
            });

            var t2 = new Thread(() =>
            {
                Monitor.Enter(lockObj);
                value--;
                Monitor.Exit(lockObj);
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Console.WriteLine("result is: {0}", value);
        }

        public static void OwnLock2()
        {
            var lockObj = new object();
            var value = 0;
            var t1 = new Thread(() =>
            {
                lock(lockObj)
                {

                }

                //MyLock2(lockObj, () =>
                //{
                //    return value++;
                //});
            });

            var t2 = new Thread(() =>
            {
                MyLock2(lockObj, () =>
                {
                    return value--;
                });
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Console.WriteLine("result is: {0}", value);
        }

        public static void OwnLock3()
        {
            var lockObj = new object();
            var value = 0;
            var t1 = new Thread(() =>
            {
                lock (lockObj)
                {
                    value++;
                }
               
            });

            var t2 = new Thread(() =>
            {
                lock (lockObj)
                {
                    value--;
                }
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Console.WriteLine("result is: {0}", value);
        }

        static void CreateManyUselessThreads()
        {
            var n = 20000;
            var s = new CountdownEvent(n);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < n; i++)
            {
                var t = new Thread(() => { s.Signal(); });
                t.Start();
            }

            s.Wait();
            sw.Stop();
            var perThread = (float)sw.Elapsed.TotalMilliseconds / (float)n;
            Console.WriteLine("took: {0}ms ({1}ms/thread is {2} threads per second)", sw.Elapsed.TotalMilliseconds, perThread, 1000.0 / perThread);
        }
    }
}