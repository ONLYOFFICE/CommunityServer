using System;
using Newtonsoft.Json;

namespace Rackspace.RackConnect.v3
{
    /// <summary>
    /// Represents a reference to a RackConnect Cloud Network.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class NetworkReference
    {
        /// <summary>
        /// The network identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The network name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The CIDR.
        /// </summary>
        [JsonProperty("cidr")]
        public string CIDR { get; set; }

        /// <summary>
        /// Timestamp when the network was created.
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Timestamp when the network was last updated.
        /// </summary>
        [JsonProperty("updated")]
        public DateTime? Updated { get; set; }
    }
}