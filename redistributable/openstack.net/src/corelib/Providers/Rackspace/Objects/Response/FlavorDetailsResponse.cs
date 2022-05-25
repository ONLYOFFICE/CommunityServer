namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Get Flavor Details request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Flavor_Details-d1e4317.html">Get Flavor Details (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class FlavorDetailsResponse
    {
        /// <summary>
        /// Gets detailed information about the flavor.
        /// </summary>
        [JsonProperty("flavor")]
        public FlavorDetails Flavor { get; private set; }
    }
}
