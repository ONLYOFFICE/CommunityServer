namespace net.openstack.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class provides a default implementation of <see cref="IBackoffPolicy"/>.
    /// The default implementation uses a progressive back-off policy with no expiration.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class BackoffPolicy : IBackoffPolicy
    {
        /// <summary>
        /// This is the backing field for the <see cref="Default"/> property.
        /// </summary>
        private static readonly BackoffPolicy _default = new BackoffPolicy();

        /// <summary>
        /// Gets a default instance of <see cref="BackoffPolicy"/>.
        /// </summary>
        public static BackoffPolicy Default
        {
            get
            {
                return _default;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The default implementation uses the following progressive back-off strategy.
        /// <list type="bullet">
        /// <item>No delay before polling the first time.</item>
        /// <item>Poll once per second, up to 10 times.</item>
        /// <item>Poll once per 5 seconds, up to 10 times.</item>
        /// <item>Poll once per 15 seconds, up to 10 times.</item>
        /// <item>Poll once per 30 seconds indefinitely.</item>
        /// </list>
        /// </remarks>
        public virtual IEnumerable<TimeSpan> GetBackoffIntervals()
        {
            // no delay before the first polling
            yield return TimeSpan.Zero;

            // 10x once a second
            for (int i = 0; i < 10; i++)
                yield return TimeSpan.FromSeconds(1);

            // 10x once every 5 seconds
            for (int i = 0; i < 10; i++)
                yield return TimeSpan.FromSeconds(5);

            // 10x once every 15 seconds
            for (int i = 0; i < 10; i++)
                yield return TimeSpan.FromSeconds(15);

            // once every 30 seconds after
            while (true)
                yield return TimeSpan.FromSeconds(30);
        }
    }
}
