namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net;
    using net.openstack.Core.Domain.Converters;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a single hop in the
    /// result of a traceroute operation in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="TraceRoute.Hops"/>
    /// <seealso cref="IMonitoringService.PerformTraceRouteFromMonitoringZoneAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class TraceRouteHop : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Number"/> property.
        /// </summary>
        [JsonProperty("number")]
        private int? _number;

        /// <summary>
        /// This is the backing field for the <see cref="IPAddress"/> property.
        /// </summary>
        [JsonProperty("ip")]
        [JsonConverter(typeof(IPAddressConverter))]
        private IPAddress _ip;

        /// <summary>
        /// This is the backing field for the <see cref="Hostname"/> property.
        /// </summary>
        [JsonProperty("hostname")]
        private string _hostname;

        /// <summary>
        /// This is the backing field for the <see cref="ResponseTimes"/> property.
        /// </summary>
        [JsonProperty("rtts")]
        private double[] _responseTimes;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceRouteHop"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected TraceRouteHop()
        {
        }

        /// <summary>
        /// Gets the index of this hop in the trace.
        /// </summary>
        /// <remarks>
        /// The first hop in the trace has <see cref="Number"/>=1.
        /// <note type="note">
        /// Hop numbers may repeat themselves in <see cref="TraceRoute.Hops"/>, which
        /// just indicates a split in the route.
        /// </note>
        /// </remarks>
        public int? Number
        {
            get
            {
                return _number;
            }
        }

        /// <summary>
        /// Gets the IP address for this hop in the trace.
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                return _ip;
            }
        }

        /// <summary>
        /// Gets the hostname associated in reverse DNS of this hop in the trace.
        /// </summary>
        public string Hostname
        {
            get
            {
                return _hostname;
            }
        }

        /// <summary>
        /// Gets the response times for ping operations to this hop in the trace.
        /// </summary>
        public ReadOnlyCollection<TimeSpan> ResponseTimes
        {
            get
            {
                if (_responseTimes == null)
                    return null;

                return new ReadOnlyCollection<TimeSpan>(Array.ConvertAll(_responseTimes, TimeSpan.FromSeconds));
            }
        }

        /// <summary>
        /// This implementation of <see cref="JsonConverter"/> allows for JSON serialization
        /// and deserialization of <see cref="IPAddress"/> objects using a simple string
        /// representation. Serialization is performed using <see cref="System.Net.IPAddress.ToString"/>,
        /// and deserialization is performed using <see cref="System.Net.IPAddress.Parse"/>.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        protected class IPAddressConverter : IPAddressSimpleConverter
        {
            /// <remarks>
            /// If <paramref name="str"/> is an empty string or equal to <c>*</c>, this method returns <see langword="null"/>.
            /// Otherwise, this method uses <see cref="System.Net.IPAddress.Parse"/> for deserialization.
            /// </remarks>
            /// <inheritdoc/>
            protected override IPAddress ConvertToObject(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return null;

                if (str == "*")
                    return null;

                return base.ConvertToObject(str);
            }
        }
    }
}
