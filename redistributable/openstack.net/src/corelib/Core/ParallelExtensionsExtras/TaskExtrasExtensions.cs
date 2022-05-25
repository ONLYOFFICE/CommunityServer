//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: TaskExtensions.cs
//
//--------------------------------------------------------------------------

using System.Linq;

namespace System.Threading.Tasks
{
    /// <summary>Extensions methods for Task.</summary>
    /// <preliminary/>
    internal static class TaskExtrasExtensions
    {
        #region Exception Handling
        /// <summary>Propagates any exceptions that occurred on the specified task.</summary>
        /// <param name="task">The Task whose exceptions are to be propagated.</param>
        public static void PropagateExceptions(this Task task)
        {
            if (!task.IsCompleted) throw new InvalidOperationException("The task has not completed.");
            if (task.IsFaulted) task.Wait();
        }

        /// <summary>Propagates any exceptions that occurred on the specified tasks.</summary>
        /// <param name="tasks">The Task instances whose exceptions are to be propagated.</param>
        public static void PropagateExceptions(this Task [] tasks)
        {
            if (tasks == null) throw new ArgumentNullException("tasks");
            if (tasks.Any(t => t == null)) throw new ArgumentException("tasks");
            if (tasks.Any(t => !t.IsCompleted)) throw new InvalidOperationException("A task has not completed.");
            Task.WaitAll(tasks);
        }
        #endregion
    }
}
