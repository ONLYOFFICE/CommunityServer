namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an Alarm Example resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="AlarmExample.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AlarmExampleId.Converter))]
    public sealed class AlarmExampleId : ResourceIdentifier<AlarmExampleId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmExampleId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The alarm example identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public AlarmExampleId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AlarmExampleId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AlarmExampleId FromValue(string id)
            {
                return new AlarmExampleId(id);
            }
        }
    }
}
