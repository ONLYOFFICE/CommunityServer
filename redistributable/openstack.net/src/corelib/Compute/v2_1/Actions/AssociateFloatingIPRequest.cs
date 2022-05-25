using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Associates a floating IP address to a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "addFloatingIp")]
    public class AssociateFloatingIPRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateFloatingIPRequest"/> class.
        /// </summary>
        /// <param name="floatingIPAddress">The floating ip address.</param>
        public AssociateFloatingIPRequest(string floatingIPAddress)
        {
            FloatingIPAddress = floatingIPAddress;
        }

        /// <summary>
        /// The floating IP address.
        /// </summary>
        [JsonProperty("address")]
        public string FloatingIPAddress { get; set; }

        /// <summary>
        /// The fixed IP address with which you want to associate the floating IP address.
        /// </summary>
        [JsonProperty("fixed_address")]
        public string FixedIPAddress { get; set; }
    }
}