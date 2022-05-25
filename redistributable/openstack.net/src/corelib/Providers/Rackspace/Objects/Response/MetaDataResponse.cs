namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Metadata request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Metadata-d1e5089.html">List Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class MetaDataResponse
    {
        /// <summary>
        /// Gets the metadata information.
        /// </summary>
        [JsonProperty("metadata")]
        public Metadata Metadata { get; private set; }
    }
}
