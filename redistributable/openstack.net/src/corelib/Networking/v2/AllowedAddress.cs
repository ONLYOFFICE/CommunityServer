using Newtonsoft.Json;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Defines an arbitrary MAC address/IP_address(CIDR) pairs that are allowed to pass through a port regardless of the subnet associated with the network.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class AllowedAddress
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedAddress"/> class.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        public AllowedAddress(string ipAddress)
        {
            IPAddress = ipAddress;
        }

        /// <summary>
        /// The IP address or CIDR.
        /// </summary>
        [JsonProperty("ip_address")]
        public string IPAddress { get; set; }

        /// <summary>
        /// The MAC address.
        /// </summary>
        [JsonProperty("mac_address")]
        public string MACAddress { get; set; }

        #region Equality

        private bool Equals(AllowedAddress other)
        {
            return string.Equals(IPAddress, other.IPAddress) && string.Equals(MACAddress, other.MACAddress);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as AllowedAddress;
            return other != null && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((IPAddress != null ? IPAddress.GetHashCode() : 0)*397) ^ (MACAddress != null ? MACAddress.GetHashCode() : 0);
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(AllowedAddress left, AllowedAddress right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(AllowedAddress left, AllowedAddress right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}