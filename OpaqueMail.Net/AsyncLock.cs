/// AsyncSemaphore and AsyncLock code by Stephen Toub.
/// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx
/// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Thread-safe asynchronous lock.
    /// </summary>
    public class AsyncLock
    {
        private readonly AsyncSemaphore semaphore;
        private readonly Task<Releaser> releaser;
        
        public AsyncLock()
        {
            semaphore = new AsyncSemaphore(1);
            releaser = Task.FromResult(new Releaser(this));
        }

        public Task<Releaser> LockAsync()
        {
            Task wait = semaphore.WaitAsync();
            return wait.IsCompleted ? releaser : wait.ContinueWith((_, state) => new Releaser((AsyncLock)state), this, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock lockToRelease;

            internal Releaser(AsyncLock toRelease)
            {
                lockToRelease = toRelease;
            }

            public void Dispose()
            {
                if (lockToRelease != null)
                    lockToRelease.semaphore.Release();
            }
        }
    }

    /// <summary>
    /// Thread-safe asynchronous semaphore.
    /// </summary>
    public class AsyncSemaphore
    {
        private readonly static Task completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();
        private int semaphoreCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
                throw new ArgumentOutOfRangeException("Initial semaphore count out of range.");

            semaphoreCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (waiters)
            {
                if (semaphoreCount > 0)
                {
                    --semaphoreCount;
                    return completed;
                }
                else
                {
                    TaskCompletionSource<bool> waiter = new TaskCompletionSource<bool>();
                    waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            
            lock (waiters)
            {
                if (waiters.Count > 0)
                    toRelease = waiters.Dequeue();
                else
                    ++semaphoreCount;
            }

            if (toRelease != null)
                toRelease.SetResult(true);
        }
    }
}
