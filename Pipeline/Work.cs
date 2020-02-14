using System;
using System.Threading;

namespace Pipelines
{
    public class Work
    {
        private int _workersCount;
        private readonly Thread[] _workers;

        public Work(Action work, int workersCount, Action onComplete = default, Action<Exception> onError = default)
        {
            _workers = new Thread[workersCount];
            _workersCount = workersCount;

            for (int i = 0; i < workersCount; i++)
            {
                _workers[i] = new Thread(() =>
                {
                    try
                    {
                        work();
                    }
                    catch (Exception exception)
                    {
                        onError?.Invoke(exception);
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref _workersCount) == 0)
                            onComplete?.Invoke();
                    }
                });
            }
        }

        public void Start()
        {
            foreach (var worker in _workers)
                worker.Start();
        }

        public void Join()
        {
            foreach (var worker in _workers)
                worker.Join();
        }
    }
}