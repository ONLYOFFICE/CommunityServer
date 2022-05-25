using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a cache resource of the <see cref="IContentDeliveryNetworkService"/>.
    /// <para>
    /// TTL caching rules for the assets under this service. Supports wildcard for fine grained control.
    /// </para>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceCache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCache"/> class.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        /// <param name="timeToLive">The time to live (in seconds).</param>
        public ServiceCache(string name, TimeSpan timeToLive)
        {
            Name = name;
            TimeToLive = timeToLive;
            Rules = new List<ServiceCacheRule>();
        }

        /// <summary>
        /// The cache name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The time to live.
        /// </summary>
        [JsonProperty("ttl")]
        [JsonConverter(typeof(TimeSpanInSecondsConverter))]
        public TimeSpan TimeToLive { get; set; }

        /// <summary>
        /// Rules defining if this ttl should be applied to an asset.
        /// </summary>
        [JsonProperty("rules")]
        public IList<ServiceCacheRule> Rules { get; set; }
    }
}