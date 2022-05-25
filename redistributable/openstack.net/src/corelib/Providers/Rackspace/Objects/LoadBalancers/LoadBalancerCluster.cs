namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a load balancer cluster to support the
    /// <see cref="LoadBalancer.Cluster"/> property.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerCluster : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerCluster"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerCluster()
        {
        }

        /// <summary>
        /// Gets the name of the cluster a load balancer is located within.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}
