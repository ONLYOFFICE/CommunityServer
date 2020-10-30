/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASC.Common.Threading
{
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool busy;

        private readonly LinkedList<Task> tasks = new LinkedList<Task>();

        private readonly int maxDegreeOfParallelism;

        private int delegatesQueuedOrRunning = 0;


        public override int MaximumConcurrencyLevel
        {
            get { return maxDegreeOfParallelism; }
        }


        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            }
            this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        }


        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.
            // If there aren't enough delegates currently queued or running to process tasks, schedule another. 
            lock (tasks)
            {
                tasks.AddLast(task);
                if (delegatesQueuedOrRunning < maxDegreeOfParallelism)
                {
                    ++delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                busy = true;
                try
                {
                    while (true)
                    {
                        Task item;
                        lock (tasks)
                        {
                            // When there are no more items to be processed, note that we're done processing, and get out.
                            if (tasks.Count == 0)
                            {
                                --delegatesQueuedOrRunning;
                                break;
                            }

                            item = tasks.First.Value;
                            tasks.RemoveFirst();
                        }

                        TryExecuteTask(item);
                    }
                }
                finally
                {
                    busy = false;
                }
            }, null);
        }

        // Attempts to execute the specified task on the current thread. 
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (busy)
            {
                // If the task was previously queued, remove it from the queue
                if (taskWasPreviouslyQueued)
                {
                    // Try to run the task. 
                    if (TryDequeue(task))
                    {
                        return TryExecuteTask(task);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return TryExecuteTask(task);
                }
            }

            return false; // If this thread isn't already processing a task, we don't support inlining
        }

        // Attempt to remove a previously scheduled task from the scheduler. 
        protected override bool TryDequeue(Task task)
        {
            lock (tasks)
            {
                return tasks.Remove(task);
            }
        }

        // Gets an enumerable of the tasks currently scheduled on this scheduler. 
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var taken = false;
            try
            {
                Monitor.TryEnter(tasks, ref taken);
                if (taken)
                {
                    return tasks;
                }
                throw new NotSupportedException();
            }
            finally
            {
                if (taken)
                {
                    Monitor.Exit(tasks);
                }
            }
        }
    }
}
