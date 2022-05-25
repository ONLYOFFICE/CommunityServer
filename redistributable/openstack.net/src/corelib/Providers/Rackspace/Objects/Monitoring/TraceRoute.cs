namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON result of a traceroute operation from
    /// a monitoring zone in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.PerformTraceRouteFromMonitoringZoneAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class TraceRoute : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Hops"/> property.
        /// </summary>
        [JsonProperty("result")]
        private TraceRouteHop[] _hops;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceRoute"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected TraceRoute()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="TraceRouteHop"/> objects describing the
        /// results of each hop in the traceroute.
        /// </summary>
        public ReadOnlyCollection<TraceRouteHop> Hops
        {
            get
            {
                if (_hops == null)
                    return null;

                return new ReadOnlyCollection<TraceRouteHop>(_hops);
            }
        }
    }
}
