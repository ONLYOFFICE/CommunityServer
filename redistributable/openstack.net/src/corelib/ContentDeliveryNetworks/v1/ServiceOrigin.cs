using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a service origin resource of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceOrigin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOrigin"/> class.
        /// </summary>
        /// <param name="origin">The URL or IP address.</param>
        public ServiceOrigin(string origin)
        {
            Origin = origin;
            Rules = new List<ServiceOriginRule>();
        }

        /// <summary>
        /// URL or IP address from which to pull canonical content.
        /// </summary>
        [JsonProperty("origin")]
        public string Origin { get; set; }

        /// <summary>
        /// Port used to access the origin.
        /// </summary>
        [JsonProperty("port")]
        public int? Port { get; set; }

        /// <summary>
        /// Specifies if the origin should be accessed via SSL/HTTPS.
        /// </summary>
        [JsonProperty("ssl")]
        public bool? SSL { get; set; }

        /// <summary>
        /// Rules defining the conditions when this origin should be accessed 
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        [JsonProperty("rules")]
        public IList<ServiceOriginRule> Rules { get; set; }
    }
}