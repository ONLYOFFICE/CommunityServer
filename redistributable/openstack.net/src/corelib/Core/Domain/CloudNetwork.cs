namespace net.openstack.Core.Domain
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the detailed information for a labeled network in Rackspace Cloud Networks.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/ch_overview.html">Overview (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class CloudNetwork : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the network ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the CIDR for the network.
        /// </summary>
        [JsonProperty("cidr")]
        public string Cidr { get; private set; }

        /// <summary>
        /// Gets the name of the network.
        /// </summary>
        /// <remarks>
        /// The Rackspace Cloud Networks product predefines two networks:
        /// <list type="bullet">
        /// <item><newTerm>PublicNet</newTerm> provides access to the Internet, Rackspace services such as Cloud Monitoring, Managed Cloud Support, RackConnect, Cloud Backup, and certain operating system updates. When you list networks through Cloud Networks, PublicNet is labeled <c>public</c>.</item>
        /// <item><newTerm>ServiceNet</newTerm> Provides access to Rackspace services such as Cloud Files, Cloud Databases, and Cloud Backup, and to certain packages and patches through an internal only, multi-tenant network connection within each Rackspace data center. When you list networks through Cloud Networks, ServiceNet is labeled as <c>private</c>.</item>
        /// </list>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/ch_overview.html">Overview (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        [JsonProperty("label")]
        public string Label { get; private set; }
    }
}
