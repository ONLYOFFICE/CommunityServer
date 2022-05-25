//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: TaskFactoryExtensions_Delayed.cs
//
//--------------------------------------------------------------------------

namespace System.Threading.Tasks
{
    partial class TaskFactoryExtensions
    {
        #region TaskFactory No Action
        /// <summary>Creates a Task that will complete after the specified delay.</summary>
        /// <param name="factory">The TaskFactory.</param>
        /// <param name="millisecondsDelay">The delay after which the Task should transition to RanToCompletion.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the timed task.</param>
        /// <returns>A Task that will be completed after the specified duration and that's cancelable with the specified token.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, CancellationToken cancellationToken)
        {
            // Validate arguments
            if (factory == null) throw new ArgumentNullException("factory");
            if (millisecondsDelay < 0) throw new ArgumentOutOfRangeException("millisecondsDelay");

            // Check for a pre-canceled token
            if (cancellationToken.IsCancellationRequested)
                return factory.FromCancellation(cancellationToken);

            // Create the timed task
            var tcs = new TaskCompletionSource<object>(factory.CreationOptions);
            var ctr = default(CancellationTokenRegistration);

            // Create the timer but don't start it yet.  If we start it now,
            // it might fire before ctr has been set to the right registration.
            var timer = new Timer(self =>
            {
                // Clean up both the cancellation token and the timer, and try to transition to completed
                ctr.Dispose();
                ((Timer)self).Dispose();
                tcs.TrySetResult(null);
            });

            // Register with the cancellation token.
            if (cancellationToken.CanBeCanceled)
            {
                // When cancellation occurs, cancel the timer and try to transition to canceled.
                // There could be a race, but it's benign.
                ctr = cancellationToken.Register(() =>
                {
                    timer.Dispose();
                    tcs.TrySetCanceled();
                });
            }

            // Start the timer and hand back the task...
            try { timer.Change(millisecondsDelay, Timeout.Infinite); }
            catch(ObjectDisposedException) {} // in case there's a race with cancellation; this is benign

            return tcs.Task;
        }
        #endregion
    }
}
