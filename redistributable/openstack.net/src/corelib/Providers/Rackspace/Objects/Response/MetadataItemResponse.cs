namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Get Metadata Item request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Metadata_Item-d1e5507.html">Get Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class MetadataItemResponse
    {
        /// <summary>
        /// Gets information about the metadata item. The returned <see cref="Metadata"/> object
        /// will only have one item, containing the key and value for the metadata item.
        /// </summary>
        [JsonProperty("meta")]
        public Metadata Metadata { get; private set; }
    }
}
