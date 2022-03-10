using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Scratch
{
    class BlockingQueue<T>
    {
        private Queue<T> queue = new Queue<T>();
        private SemaphoreSlim semaphore = new SemaphoreSlim(0, int.MaxValue);

        // Thread Safe.
        public void Enqueue(T data)
        {
            if (data == null) throw new ArgumentNullException("data");
            lock (queue)
            {
                queue.Enqueue(data);
            }
            semaphore.Release();
        }

        /// <summary>
        /// Thread safe. Dequeues the queue, blocks until element available
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            semaphore.Wait();
            lock (queue)
            {
                return queue.Dequeue();
            }
        }
    }

    class BlockingQueue2<T>
    {
        private Queue<T> queue = new Queue<T>();

        public void Enqueue(T data)
        {
            if (data == null) throw new ArgumentNullException("data");
            lock (queue)
            {
                queue.Enqueue(data);
                Monitor.Pulse(queue);
            }
        }


        /// <summary>
        /// Thread safe. Dequeues the queue, blocks until element available
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(queue);
                }
                return queue.Dequeue();
            }
        }
    }
}