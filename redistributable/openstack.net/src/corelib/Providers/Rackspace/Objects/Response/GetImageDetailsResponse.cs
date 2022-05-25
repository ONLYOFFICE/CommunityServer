namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Get Image Details request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Image_Details-d1e4848.html">Get Image Details (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GetImageDetailsResponse
    {
        /// <summary>
        /// Gets detailed information about the image.
        /// </summary>
        [JsonProperty("image")]
        public ServerImage Image { get; private set; }
    }
}
