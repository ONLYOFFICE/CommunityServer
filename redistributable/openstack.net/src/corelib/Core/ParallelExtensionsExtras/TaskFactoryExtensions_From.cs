//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: TaskFactoryExtensions_From.cs
//
//--------------------------------------------------------------------------

namespace System.Threading.Tasks
{
    partial class TaskFactoryExtensions
    {
        #region TaskFactory
        /// <summary>Creates a Task that has completed in the Canceled state with the specified CancellationToken.</summary>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="cancellationToken">The CancellationToken with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task FromCancellation(this TaskFactory factory, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested) throw new ArgumentOutOfRangeException("cancellationToken");
            return new Task(() => { }, cancellationToken);
        }
        #endregion
    }
}
