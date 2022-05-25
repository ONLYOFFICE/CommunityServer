using Newtonsoft.Json;

namespace Rackspace.CloudNetworks.v2
{
    /// <summary>
    /// Represents a subnet host route.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class HostRoute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostRoute" /> class.
        /// </summary>
        /// <param name="destinationCIDR">The destination CIDR.</param>
        /// <param name="nextHop">The next hop IP address.</param>
        public HostRoute(string destinationCIDR, string nextHop)
        {
            DestinationCIDR = destinationCIDR;
            NextHop = nextHop;
        }

        /// <summary>
        /// The destination CIDR.
        /// </summary>
        [JsonProperty("destination")]
        public string DestinationCIDR { get; set; }

        /// <summary>
        /// The IP address of the next hop.
        /// </summary>
        [JsonProperty("nexthop")]
        public string NextHop { get; set; }

        #region Equality
        private bool Equals(HostRoute other)
        {
            return string.Equals(DestinationCIDR, other.DestinationCIDR) && string.Equals(NextHop, other.NextHop);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as HostRoute;
            return other != null && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (DestinationCIDR.GetHashCode() * 397) ^ NextHop.GetHashCode();
            }
        }
        #endregion
    }
}