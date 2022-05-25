namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Flavors with Details request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Flavors-d1e4188.html">List Flavors (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListFlavorDetailsResponse
    {
        /// <summary>
        /// Gets a collection of detailed information about the flavors.
        /// </summary>
        [JsonProperty("flavors")]
        public FlavorDetails[] Flavors { get; private set; }
    }
}
