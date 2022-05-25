namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the system data reported agents for
    /// the <see cref="HostInformationType.System"/> information type.
    /// </summary>
    /// <see cref="IMonitoringService.GetSystemInformationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class SystemInformation : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Architecture"/> property.
        /// </summary>
        [JsonProperty("arch")]
        private string _arch;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Vendor"/> property.
        /// </summary>
        [JsonProperty("vendor")]
        private string _vendor;

        /// <summary>
        /// This is the backing field for the <see cref="VendorVersion"/> property.
        /// </summary>
        [JsonProperty("vendor_version")]
        private string _vendorVersion;

        /// <summary>
        /// This is the backing field for the <see cref="Version"/> property.
        /// </summary>
        [JsonProperty("version")]
        private string _version;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemInformation"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected SystemInformation()
        {
        }

        /// <summary>
        /// Gets the CPU architecture.
        /// </summary>
        public string Architecture
        {
            get
            {
                return _arch;
            }
        }

        /// <summary>
        /// Gets the generic name of the operating system.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the name of the vendor.
        /// </summary>
        public string Vendor
        {
            get
            {
                return _vendor;
            }
        }

        /// <summary>
        /// Gets the vendor version name.
        /// </summary>
        public string VendorVersion
        {
            get
            {
                return _vendorVersion;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public string Version
        {
            get
            {
                return _version;
            }
        }
    }
}
