namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Addresses request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses-d1e3014.html">List Addresses (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListAddressesResponse
    {
        /// <summary>
        /// Gets the IP address details.
        /// </summary>
        [JsonProperty("addresses")]
        public ServerAddresses Addresses { get; private set; }
    }
}
