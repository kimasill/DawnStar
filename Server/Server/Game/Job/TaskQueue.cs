using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game.Job
{
    public class TaskQueue
    {
        TaskTimer _timer = new TaskTimer();
        Queue<IJob> _jobQueue = new Queue<IJob>();
        object _lock = new object();
        bool _flush = false;

        public IJob EnqueueAfter(int tickAfter, Action action) { return EnqueueAfter(tickAfter, new Job(action)); }
        public IJob EnqueueAfter<T1>(int tickAfter, Action<T1> action, T1 t1) {return EnqueueAfter(tickAfter, new Job<T1>(action, t1)); }
        public IJob EnqueueAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { return EnqueueAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
        public IJob EnqueueAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { return EnqueueAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }
        public IJob EnqueueAfter<T1, T2, T3, T4>(int tickAfter, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { return EnqueueAfter(tickAfter, new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }

        public IJob EnqueueAfter(int tickAfter, IJob job)
        {
            _timer.Enqueue(job, tickAfter);
            return job;
        }
        public void Enqueue(Action action){Enqueue(new Job(action));}
        public void Enqueue<T1>(Action<T1> action, T1 t1){Enqueue(new Job<T1>(action, t1));}
        public void Enqueue<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2){Enqueue(new Job<T1, T2>(action, t1, t2));}
        public void Enqueue<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3){Enqueue(new Job<T1, T2, T3>(action, t1, t2, t3));}
        public void Enqueue<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { Enqueue(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }

        public void Enqueue(IJob job)
        {
            lock (_lock)
            {
                _jobQueue.Enqueue(job);               
            }
        }
        public void Enqueue(Func<Task> asyncAction)
        {
            Enqueue(new AsyncJob(asyncAction));
        }

        public void ExecuteAll()
        {
            _timer.ExecuteAll();
            while (true)
            {
                IJob job = Pop();
                if (job == null)
                    return;

                job.Execute();
            }
        }

        private IJob Pop()
        {
            lock (_lock)
            {
                if (_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
    public class AsyncJob : IJob
    {
        private readonly Func<Task> _asyncAction;

        public AsyncJob(Func<Task> asyncAction)
        {
            _asyncAction = asyncAction;
        }

        public override void Execute()
        {
            _asyncAction().GetAwaiter().GetResult();
        }
    }
}
