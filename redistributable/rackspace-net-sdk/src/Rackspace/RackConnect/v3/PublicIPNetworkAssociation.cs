using Newtonsoft.Json;

namespace Rackspace.RackConnect.v3
{
    /// <summary>
    /// Represents an association between a Public IP and a RackConnect Cloud Network.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class PublicIPNetworkAssociation : NetworkReference
    {
        /// <summary>
        /// The private IP (v4) address on the network.
        /// </summary>
        [JsonProperty("private_ip_v4")]
        public string PrivateIPv4Address { get; set; }
    }
}