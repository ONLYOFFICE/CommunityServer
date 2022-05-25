namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a monitoring zone resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// A monitoring zone is a location that Rackspace Cloud Monitoring collects data from.
    /// Examples of monitoring zones are "US West", "DFW1" or "ORD1". It is an abstraction
    /// for a general location from which data is collected.
    ///
    /// <para>An "endpoint," also known as a "collector," collects data from the monitoring
    /// zone. The endpoint is mapped directly to an individual machine or a virtual machine.
    /// A monitoring zone contains many endpoints, all of which will be within the IP address
    /// range listed in the response. The opposite is not true, however, as there may be
    /// unallocated IP addresses or unrelated machines within that IP address range.</para>
    ///
    /// <para>A check references a list of monitoring zones it should be run from.</para>
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-monitoring-zones.html">Monitoring Zones (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class MonitoringZone : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private MonitoringZoneId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label")]
        private string _label;

        /// <summary>
        /// This is the backing field for the <see cref="CountryCode"/> property.
        /// </summary>
        [JsonProperty("country_code")]
        private string _countryCode;

        /// <summary>
        /// This is the backing field for the <see cref="SourceAddresses"/> property.
        /// </summary>
        [JsonProperty("source_ips")]
        private string[] _sourceAddresses;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringZone"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected MonitoringZone()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the monitoring zone.
        /// </summary>
        public MonitoringZoneId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the name of the monitoring zone.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
        }

        /// <summary>
        /// Gets the country code for the monitoring zone.
        /// </summary>
        public string CountryCode
        {
            get
            {
                return _countryCode;
            }
        }

        /// <summary>
        /// Gets a collection of IP addresses and/or address ranges (in CIDR notation) from
        /// which checks from this monitoring zone may be performed.
        /// </summary>
        public ReadOnlyCollection<string> SourceAddresses
        {
            get
            {
                if (_sourceAddresses == null)
                    return null;

                return new ReadOnlyCollection<string>(_sourceAddresses);
            }
        }
    }
}
