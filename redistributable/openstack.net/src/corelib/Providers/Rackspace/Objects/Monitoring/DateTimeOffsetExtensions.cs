namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;

    /// <summary>
    /// This class provides utility methods for converting between Unix timestamps and
    /// <see cref="DateTimeOffset"/> objects.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    internal static class DateTimeOffsetExtensions
    {
        /// <summary>
        /// The Epoch used as a reference for Unix timestamps and file times.
        /// </summary>
        internal static readonly DateTimeOffset Epoch = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.Zero);

        /// <summary>
        /// Converts a Unix timestamp (number of milliseconds since the <see cref="Epoch"/>) to a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="timestamp">The Unix timestamp.</param>
        /// <returns>A <see cref="DateTimeOffset"/> representation of the timestamp.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="timestamp"/> is less than 0.</exception>
        public static DateTimeOffset ToDateTimeOffset(long timestamp)
        {
            if (timestamp < 0)
                throw new ArgumentOutOfRangeException("timestamp");

            return Epoch.AddMilliseconds(timestamp);
        }

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> value to a Unix-style timestamp (number of milliseconds since the <see cref="Epoch"/>).
        /// </summary>
        /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
        /// <returns>The number of milliseconds since the <see cref="Epoch"/> until the time indicated by <paramref name="dateTimeOffset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="dateTimeOffset"/> occurs before <see cref="Epoch"/>.</exception>
        public static long ToTimestamp(this DateTimeOffset dateTimeOffset)
        {
            if (dateTimeOffset < Epoch)
                throw new ArgumentOutOfRangeException("Cannot convert a time before the epoch (January 1, 1970, 00:00 UTC) to a timestamp.", "dateTimeOffset");

            return (long)(dateTimeOffset - Epoch).TotalMilliseconds;
        }

        /// <summary>
        /// Converts a Unix timestamp (number of milliseconds since the <see cref="Epoch"/>) to a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="timestamp">The Unix timestamp, or <see langword="null"/>.</param>
        /// <returns>A <see cref="DateTimeOffset"/> representation of the timestamp, or <see langword="null"/> if <paramref name="timestamp"/> is <see langword="null"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="timestamp"/> is less than 0.</exception>
        public static DateTimeOffset? ToDateTimeOffset(long? timestamp)
        {
            if (timestamp == null)
                return null;

            return ToDateTimeOffset(timestamp.Value);
        }

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> value to a Unix-style timestamp (number of milliseconds since the <see cref="Epoch"/>).
        /// </summary>
        /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert, or <see langword="null"/>.</param>
        /// <returns>The number of milliseconds since the <see cref="Epoch"/> until the time indicated by <paramref name="dateTimeOffset"/>, or <see langword="null"/> if <paramref name="dateTimeOffset"/> is <see langword="null"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="dateTimeOffset"/> occurs before <see cref="Epoch"/>.</exception>
        public static long? ToTimestamp(this DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset == null)
                return null;

            return ToTimestamp(dateTimeOffset.Value);
        }
    }
}
