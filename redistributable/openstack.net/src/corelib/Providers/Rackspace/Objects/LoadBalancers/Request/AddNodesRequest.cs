namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class AddNodesRequest
    {
        [JsonProperty("nodes")]
        private NodeConfiguration[] _nodes;

        public AddNodesRequest(IEnumerable<NodeConfiguration> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            _nodes = nodes.ToArray();
        }
    }
}
