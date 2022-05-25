namespace net.openstack.Core.Synchronous
{
    using System;
    using System.Collections.ObjectModel;

    using net.openstack.Core.Domain.Queues;

    using CancellationToken = System.Threading.CancellationToken;
    using IQueueingService = net.openstack.Core.Providers.IQueueingService;
    using WebException = System.Net.WebException;

    /// <summary>
    /// Provides extension methods to allow synchronous calls to the methods in <see cref="Claim"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
    public static class ClaimExtensions
    {
        /// <summary>
        /// Refreshes the current claim.
        /// </summary>
        /// <remarks>
        /// This method calls <see cref="IQueueingService.QueryClaimAsync"/> to obtain updated
        /// information about the current claim, and then synchronously invokes <see cref="Claim.RefreshAsync"/>
        /// to update the current instance to match the results.
        /// </remarks>
        /// <param name="claim">The claim.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="claim"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void Refresh(this Claim claim)
        {
            if (claim == null)
                throw new ArgumentNullException("claim");

            try
            {
                claim.RefreshAsync(CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Renews the claim by resetting the age and updating the TTL for the claim.
        /// </summary>
        /// <remarks>
        /// This method calls <see cref="IQueueingService.UpdateClaimAsync"/> to renew the
        /// current claim, and then synchronously updates the current instance to reflect
        /// the new age and time-to-live values.
        /// </remarks>
        /// <param name="claim">The claim.</param>
        /// <param name="timeToLive">
        /// The new Time-To-Live value for the claim. This value may differ from the original TTL of the claim.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="claim"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="timeToLive"/> is negative or <see cref="TimeSpan.Zero"/>.</exception>
        /// <exception cref="InvalidOperationException">If the claim is empty (i.e. <see cref="Claim.Messages"/> is empty).</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void Renew(this Claim claim, TimeSpan timeToLive)
        {
            if (claim == null)
                throw new ArgumentNullException("claim");

            try
            {
                claim.RenewAsync(timeToLive, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }
    }
}
