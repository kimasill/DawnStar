using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ServerCore;

namespace Server.Game.Job
{
    struct TaskTimerElem : IComparable<TaskTimerElem>
    {
        public int execTick; // ?ㅽ뻾 ?쒓컙
        public IJob job;

        public int CompareTo(TaskTimerElem other)
        {
            return other.execTick - execTick;
        }
    }

    public class TaskTimer
    {
        PriorityQueue<TaskTimerElem> _pq = new PriorityQueue<TaskTimerElem>();
        object _lock = new object();      

        public void Enqueue(IJob job, int tickAfter = 0)
        {
            TaskTimerElem jobElement;
            jobElement.execTick = Environment.TickCount + tickAfter;
            jobElement.job = job;

            lock (_lock)
            {
                _pq.Enqueue(jobElement);
            }
        }

        public void ExecuteAll()
        {
            while (true)
            {
                int now = Environment.TickCount;

                TaskTimerElem jobElement;

                lock (_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    jobElement = _pq.Peek();
                    if (jobElement.execTick > now)
                        break;

                    _pq.Dequeue();
                }

                jobElement.job.Execute();
            }
        }
    }
}
