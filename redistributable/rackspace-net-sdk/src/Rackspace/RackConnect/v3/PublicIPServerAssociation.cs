using System;
using Newtonsoft.Json;

namespace Rackspace.RackConnect.v3
{
    /// <summary>
    /// Represents a reference to a cloud server which has been allocated a <see cref="PublicIP"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class PublicIPServerAssociation
    {
        /// <summary>
        /// The cloud server identitifer
        /// </summary>
        [JsonProperty("id")]
        public Identifier ServerId { get; set; }

        /// <summary>
        /// The cloud server name
        /// </summary>
        [JsonProperty("name")]
        public string ServerName { get; set; }

        /// <summary>
        /// The network to which the cloud server is attached. 
        /// </summary>
        [JsonProperty("cloud_network")]
        public PublicIPNetworkAssociation Network { get; set; }

        /// <summary>
        /// Timestamp of when the server was created.
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Timestamp of when the server was last updated.
        /// </summary>
        [JsonProperty("updated")]
        public DateTime? Updated { get; set; }
    }
}