namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Net;
    using System.Net.NetworkInformation;
    using net.openstack.Core.Domain.Converters;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the network interface data reported agents for
    /// the <see cref="HostInformationType.NetworkInterfaces"/> information type.
    /// </summary>
    /// <see cref="IMonitoringService.GetNetworkInterfaceInformationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NetworkInterfaceInformation : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type")]
        private string _type;

        /// <summary>
        /// This is the backing field for the <see cref="IPAddress"/> property.
        /// </summary>
        [JsonProperty("address")]
        [JsonConverter(typeof(IPAddressSimpleConverter))]
        private IPAddress _address;

        /// <summary>
        /// This is the backing field for the <see cref="IPAddressV6"/> property.
        /// </summary>
        [JsonProperty("address6")]
        [JsonConverter(typeof(IPAddressSimpleConverter))]
        private IPAddress _address6;

        /// <summary>
        /// This is the backing field for the <see cref="Broadcast"/> property.
        /// </summary>
        [JsonProperty("broadcast")]
        [JsonConverter(typeof(IPAddressSimpleConverter))]
        private IPAddress _broadcast;

        /// <summary>
        /// This is the backing field for the <see cref="PhysicalAddress"/> property.
        /// </summary>
        [JsonProperty("hwaddr")]
        [JsonConverter(typeof(PhysicalAddressSimpleConverter))]
        private PhysicalAddress _physicalAddress;

        /// <summary>
        /// This is the backing field for the <see cref="NetworkMask"/> property.
        /// </summary>
        [JsonProperty("netmask")]
        private string _netmask;

        /// <summary>
        /// This is the backing field for the <see cref="MaximumTransmissionUnit"/> property.
        /// </summary>
        [JsonProperty("mtu")]
        private int? _mtu;

        /// <summary>
        /// This is the backing field for the <see cref="TransmitBytes"/> property.
        /// </summary>
        [JsonProperty("tx_bytes")]
        private long? _transmitBytes;

        /// <summary>
        /// This is the backing field for the <see cref="TransmitPackets"/> property.
        /// </summary>
        [JsonProperty("tx_packets")]
        private long? _transmitPackets;

        /// <summary>
        /// This is the backing field for the <see cref="TransmitErrors"/> property.
        /// </summary>
        [JsonProperty("tx_errors")]
        private long? _transmitErrors;

        /// <summary>
        /// This is the backing field for the <see cref="TransmitOverruns"/> property.
        /// </summary>
        [JsonProperty("tx_overruns")]
        private long? _transmitOverruns;

        /// <summary>
        /// This is the backing field for the <see cref="TransmitCarrierErrors"/> property.
        /// </summary>
        [JsonProperty("tx_carrier")]
        private long? _transmitCarrierErrors;

        /// <summary>
        /// This is the backing field for the <see cref="TransmitCollisions"/> property.
        /// </summary>
        [JsonProperty("tx_collisions")]
        private long? _transmitCollisions;

        /// <summary>
        /// This is the backing field for the <see cref="TransmitDropped"/> property.
        /// </summary>
        [JsonProperty("tx_dropped")]
        private long? _transmitDropped;

        /// <summary>
        /// This is the backing field for the <see cref="ReceiveBytes"/> property.
        /// </summary>
        [JsonProperty("rx_bytes")]
        private long? _receiveBytes;

        /// <summary>
        /// This is the backing field for the <see cref="ReceivePackets"/> property.
        /// </summary>
        [JsonProperty("rx_packets")]
        private long? _receivePackets;

        /// <summary>
        /// This is the backing field for the <see cref="ReceiveErrors"/> property.
        /// </summary>
        [JsonProperty("rx_errors")]
        private long? _receiveErrors;

        /// <summary>
        /// This is the backing field for the <see cref="ReceiveOverruns"/> property.
        /// </summary>
        [JsonProperty("rx_overruns")]
        private long? _receiveOverruns;

        /// <summary>
        /// This is the backing field for the <see cref="ReceiveInvalidFrames"/> property.
        /// </summary>
        [JsonProperty("rx_frame")]
        private long? _receiveInvalidFrames;

        /// <summary>
        /// This is the backing field for the <see cref="ReceiveDropped"/> property.
        /// </summary>
        [JsonProperty("rx_dropped")]
        private long? _receiveDropped;

        /// <summary>
        /// This is the backing field for the <see cref="Flags"/> property.
        /// </summary>
        [JsonProperty("flags")]
        private long? _flags;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkInterfaceInformation"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NetworkInterfaceInformation()
        {
        }

        /// <summary>
        /// Gets the name of the network interface.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the interface type (e.g. Ethernet, Local Loopback, etc.).
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the IP V4 address of the network interface.
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// Gets the IP V6 address of the network interface.
        /// </summary>
        public IPAddress IPAddressV6
        {
            get
            {
                return _address6;
            }
        }

        /// <summary>
        /// Gets the broadcast address of the network.
        /// </summary>
        public IPAddress Broadcast
        {
            get
            {
                return _broadcast;
            }
        }

        /// <summary>
        /// Gets the physical address of the network interface.
        /// </summary>
        public PhysicalAddress PhysicalAddress
        {
            get
            {
                return _physicalAddress;
            }
        }

        /// <summary>
        /// Gets the network mask as a string.
        /// </summary>
        public string NetworkMask
        {
            get
            {
                return _netmask;
            }
        }

        /// <summary>
        /// Gets the Ethernet maximum transmission unit (MTU).
        /// </summary>
        public int? MaximumTransmissionUnit
        {
            get
            {
                return _mtu;
            }
        }

        /// <summary>
        /// Gets the total number of bytes sent.
        /// </summary>
        public long? TransmitBytes
        {
            get
            {
                return _transmitBytes;
            }
        }

        /// <summary>
        /// Gets the total number of packets sent.
        /// </summary>
        public long? TransmitPackets
        {
            get
            {
                return _transmitPackets;
            }
        }

        /// <summary>
        /// Gets the total number of transmit errors.
        /// </summary>
        public long? TransmitErrors
        {
            get
            {
                return _transmitErrors;
            }
        }

        /// <summary>
        /// Gets the total number of transmit buffer overruns.
        /// </summary>
        public long? TransmitOverruns
        {
            get
            {
                return _transmitOverruns;
            }
        }

        /// <summary>
        /// Gets the total number of carrier errors (usually cable disconnects).
        /// </summary>
        public long? TransmitCarrierErrors
        {
            get
            {
                return _transmitCarrierErrors;
            }
        }

        /// <summary>
        /// Gets the total number of packet collisions on transmit.
        /// </summary>
        public long? TransmitCollisions
        {
            get
            {
                return _transmitCollisions;
            }
        }

        /// <summary>
        /// Gets the total number of dropped transmit packets.
        /// </summary>
        public long? TransmitDropped
        {
            get
            {
                return _transmitDropped;
            }
        }

        /// <summary>
        /// Gets the total number of bytes received.
        /// </summary>
        public long? ReceiveBytes
        {
            get
            {
                return _receiveBytes;
            }
        }

        /// <summary>
        /// Gets the total number of packets received.
        /// </summary>
        public long? ReceivePackets
        {
            get
            {
                return _receivePackets;
            }
        }

        /// <summary>
        /// Gets the total number of receive errors.
        /// </summary>
        public long? ReceiveErrors
        {
            get
            {
                return _receiveErrors;
            }
        }

        /// <summary>
        /// Gets the total number of receive buffer overruns.
        /// </summary>
        public long? ReceiveOverruns
        {
            get
            {
                return _receiveOverruns;
            }
        }

        /// <summary>
        /// Gets the total number of errors caused by malformed frames.
        /// </summary>
        public long? ReceiveInvalidFrames
        {
            get
            {
                return _receiveInvalidFrames;
            }
        }

        /// <summary>
        /// Gets the total number of dropped packets received.
        /// </summary>
        public long? ReceiveDropped
        {
            get
            {
                return _receiveDropped;
            }
        }

        /// <summary>
        /// Gets the operating system-specific interface flags.
        /// </summary>
        public long? Flags
        {
            get
            {
                return _flags;
            }
        }
    }
}
