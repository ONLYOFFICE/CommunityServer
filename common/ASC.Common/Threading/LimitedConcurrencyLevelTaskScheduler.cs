/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
