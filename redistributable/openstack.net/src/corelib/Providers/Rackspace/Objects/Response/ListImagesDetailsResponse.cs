namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Images with Details request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Images-d1e4435.html">List Images (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListImagesDetailsResponse
    {
        /// <summary>
        /// Gets a collection of detailed information about the images.
        /// </summary>
        [JsonProperty("images")]
        public ServerImage[] Images { get; private set; }
    }
}
