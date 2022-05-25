using Newtonsoft.Json;

namespace Rackspace.RackConnect.v3
{
    /// <summary>
    /// Represents a request to the <see cref="RackConnectService"/> to provision a public IP address.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class PublicIPCreateDefinition
    {
        [JsonProperty("cloud_server")]
        private dynamic CloudServer
        {
            get
            {
                return ServerId != null ? new {id = ServerId} : null;
            }
        }

        /// <summary>
        /// If specified, requests that the IP address be associated to the specified server.
        /// </summary>
        [JsonIgnore]
        public string ServerId { get; set; }

        /// <summary>
        /// Determines whether a Public IP is removed from your environment once the server to which it is attached is deleted.
        /// </summary>
        [JsonProperty("retain")]
        public bool? ShouldRetain { get; set; }
    }
}