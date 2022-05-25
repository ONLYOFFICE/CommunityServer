//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: TaskFactoryExtensions_ContinueWhenAllAny.cs
//
//--------------------------------------------------------------------------

namespace System.Threading.Tasks
{
    partial class TaskFactoryExtensions
    {
        /// <summary>
        /// Creates a continuation Task that will complete upon
        /// the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the array of completed tasks.</returns>
        public static Task<Task<TAntecedentResult>[]> WhenAll<TAntecedentResult>(
            this TaskFactory factory, params Task<TAntecedentResult>[] tasks)
        {
            return factory.ContinueWhenAll(tasks, completedTasks => completedTasks);
        }
    }
}
