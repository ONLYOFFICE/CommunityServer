namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a metric in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Metric.Name"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(MetricName.Converter))]
    public sealed class MetricName : ResourceIdentifier<MetricName>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricName"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The metric identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public MetricName(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="MetricName"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override MetricName FromValue(string id)
            {
                return new MetricName(id);
            }
        }
    }
}
