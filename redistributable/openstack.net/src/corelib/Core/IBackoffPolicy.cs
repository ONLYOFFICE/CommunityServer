namespace net.openstack.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a back-off policy.
    /// </summary>
    /// <preliminary/>
    public interface IBackoffPolicy
    {
        /// <summary>
        /// Gets an enumeration of <see cref="TimeSpan"/> instances representing the
        /// back-off policy intervals.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This enumeration should always be lazily enumerated since implementations may
        /// not bound the number of elements returned.
        /// </note>
        /// </remarks>
        /// <returns>
        /// An enumeration of <see cref="TimeSpan"/> instances representing the back-off
        /// policy intervals.
        /// </returns>
        IEnumerable<TimeSpan> GetBackoffIntervals();
    }
}
