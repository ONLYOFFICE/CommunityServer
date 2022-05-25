namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an Alarm resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Alarm.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AlarmId.Converter))]
    public sealed class AlarmId : ResourceIdentifier<AlarmId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The alarm identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public AlarmId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AlarmId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AlarmId FromValue(string id)
            {
                return new AlarmId(id);
            }
        }
    }
}
