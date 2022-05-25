using Newtonsoft.Json;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Represents an IP address associated with a port resource of the <see cref="NetworkingService"/>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class IPAddressAssociation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressAssociation"/> class.
        /// <para>
        /// an available IP from that subnet will be allocated to the port.
        /// </para>
        /// </summary>
        /// <param name="subnetId">The subnet identifier.</param>
        public IPAddressAssociation(Identifier subnetId)
        {
            SubnetId = subnetId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressAssociation"/> class.
        /// </summary>
        /// <param name="subnetId">The subnet identifier.</param>
        /// <param name="ipAddress">The ip address.</param>
        [JsonConstructor]
        public IPAddressAssociation(Identifier subnetId, string ipAddress)
        {
            SubnetId = subnetId;
            IPAddress = ipAddress;
        }

        /// <summary>
        /// The subnet identifier.
        /// </summary>
        [JsonProperty("subnet_id")]
        public Identifier SubnetId { get; set; }

        /// <summary>
        /// The subnet identifier.
        /// </summary>
        [JsonProperty("ip_address")]
        public string IPAddress { get; set; }

        #region Equality

        private bool Equals(IPAddressAssociation other)
        {
            return Equals(SubnetId, other.SubnetId) && string.Equals(IPAddress, other.IPAddress);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as IPAddressAssociation;
            return other != null && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (SubnetId.GetHashCode() * 397) ^ (IPAddress != null ? IPAddress.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(IPAddressAssociation left, IPAddressAssociation right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(IPAddressAssociation left, IPAddressAssociation right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}