namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains detailed information about a flavor.
    /// </summary>
    /// <seealso cref="IComputeProvider.ListFlavorsWithDetails"/>
    /// <seealso cref="IComputeProvider.GetFlavor"/>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class FlavorDetails : Flavor
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="RxtxFactor"/> property.
        /// </summary>
        [JsonProperty("rxtx_factor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private double? _rxtxFactor;

        /// <summary>
        /// This is the backing field for the <see cref="DataDiskSize"/> property.
        /// </summary>
        [JsonProperty("OS-FLV-EXT-DATA:ephemeral", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _dataDiskSize;
#pragma warning restore 649

        /// <summary>
        /// Gets the "OS-FLV-DISABLED:disabled" property associated with the flavor.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("OS-FLV-DISABLED:disabled")]
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets the size of the system disk for the flavor, in GB.
        /// </summary>
        [JsonProperty("disk")] 
        public int DiskSizeInGB { get; private set; }

        /// <summary>
        /// Gets the amount of memory associated with the flavor, in MB.
        /// </summary>
        [JsonProperty("ram")]
        public int RAMInMB { get; private set; }

        /// <summary>
        /// Gets the number of virtual CPUs for the flavor.
        /// </summary>
        [JsonProperty("vcpus")]
        public int VirtualCPUCount { get; private set; }

        /// <summary>
        /// Gets the aggregate outbound bandwidth, in megabits per second, across all attached
        /// network interfaces (PublicNet, ServiceNet, and Cloud Networks).
        /// </summary>
        /// <remarks>
        /// Outbound public Internet bandwidth can be up to 40% of the aggregate limit. Host
        /// networking is redundant, and bandwidth is delivered over two separate bonded
        /// interfaces, each able to carry 50% of the aggregate limit. We recommend using
        /// multiple Layer 4 connections to maximize throughput. Inbound traffic is not limited.
        ///
        /// <note>
        /// This property is a Rackspace-specific extension to the OpenStack Compute Service.
        /// </note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-gettingstarted/content/nova_list_flavors.html">List Flavors with the nova Client (Rackspace Next Generation Cloud Servers Getting Started - API v2)</seealso>
        /// <preliminary/>
        public double? RxtxFactor
        {
            get
            {
                return _rxtxFactor;
            }
        }

        /// <summary>
        /// Gets the size of the data disks for the flavor, in GB.
        /// </summary>
        /// <remarks>
        /// <note>
        /// This property is a Rackspace-specific extension to the OpenStack Compute Service.
        /// </note>
        /// </remarks>
        /// <value>
        /// The size of the data disks for the flavor, in GB. This property returns <see langword="null"/>
        /// if the JSON response from the server does not include the underlying property.
        /// </value>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-gettingstarted/content/nova_list_flavors.html">List Flavors with the nova Client (Rackspace Next Generation Cloud Servers Getting Started - API v2)</seealso>
        /// <preliminary/>
        public int? DataDiskSize
        {
            get
            {
                return _dataDiskSize;
            }
        }
    }
}
