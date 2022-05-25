namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class AddLoadBalancerMetadataRequest
    {
        [JsonProperty("metadata")]
        private LoadBalancerMetadataItem[] _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddLoadBalancerMetadataRequest"/> class
        /// with the specified metadata.
        /// </summary>
        /// <param name="metadata">The metadata to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains a pair whose key is <see langword="null"/> or empty, or whose value is <see langword="null"/>.</exception>
        public AddLoadBalancerMetadataRequest(IEnumerable<KeyValuePair<string, string>> metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            _metadata = metadata.Select(i => new LoadBalancerMetadataItem(i.Key, i.Value)).ToArray();
        }
    }
}
