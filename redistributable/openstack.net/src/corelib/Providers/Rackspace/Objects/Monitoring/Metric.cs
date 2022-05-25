namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a Metric resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// When Monitoring checks run, they generate metrics. These metrics are stored as
    /// full resolution data points in the Cloud Monitoring system. Full resolution data
    /// points are periodically rolled-up (condensed) into coarser data points.
    ///
    /// <para>Depending on your needs, you can use the metrics API to fetch individual
    /// data points (fine-grained) or rolled-up data points (coarse-grained) over a
    /// period of time.</para>
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/metrics-api.html">Metrics (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Metric : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private MetricName _name;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="Metric"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Metric()
        {
        }

        /// <summary>
        /// Gets the name of the metric.
        /// </summary>
        public MetricName Name
        {
            get
            {
                return _name;
            }
        }
    }
}
