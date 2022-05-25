namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an alarm changelog in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="AlarmChangelog.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AlarmChangelogId.Converter))]
    public sealed class AlarmChangelogId : ResourceIdentifier<AlarmChangelogId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmChangelogId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The changelog identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public AlarmChangelogId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AlarmChangelogId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AlarmChangelogId FromValue(string id)
            {
                return new AlarmChangelogId(id);
            }
        }
    }
}
