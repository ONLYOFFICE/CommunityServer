namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the View Node Service Events request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Node-Events-d1e264.html">View Node Service Events (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListNodeServiceEventsResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="NodeServiceEvents"/> property.
        /// </summary>
        [JsonProperty("nodeServiceEvents")]
        private NodeServiceEvent[] _nodeServiceEvents;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListNodeServiceEventsResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListNodeServiceEventsResponse()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="NodeServiceEvent"/> objects describing the node service
        /// events for a load balancer.
        /// </summary>
        public ReadOnlyCollection<NodeServiceEvent> NodeServiceEvents
        {
            get
            {
                if (_nodeServiceEvents == null)
                    return null;

                return new ReadOnlyCollection<NodeServiceEvent>(_nodeServiceEvents);
            }
        }
    }
}
