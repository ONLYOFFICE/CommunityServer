using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Networking;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// An IP address associated with a server.
    /// </summary>
    public class ServerAddress : IHaveExtraData
    {
        /// <summary>
        /// The IP address.
        /// </summary>
        [JsonProperty("addr")]
        public string IP { get; set; }

        /// <summary>
        /// The IP version, e.g. v4 or v6.
        /// </summary>
        [JsonProperty("version")]
        public IPVersion Version { get; set; }

        /// <summary>
        /// The MAC address.
        /// </summary>
        [JsonProperty("OS-EXT-IPS-MAC:mac_addr")]
        public string MAC { get; set; }

        /// <summary>
        /// The IP address type, e.g. fixed or floating.
        /// </summary>
        [JsonProperty("OS-EXT-IPS:type")]
        public AddressType Type { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();
    }
}