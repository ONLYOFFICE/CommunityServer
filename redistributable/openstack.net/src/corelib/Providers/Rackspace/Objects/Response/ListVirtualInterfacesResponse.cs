namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using System.Collections.Generic;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Create Virtual Interface and List Virtual Interfaces requests.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/api_create_virtual_interface.html">Create Virtual Interface (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
    /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListVirtualInterfacesResponse
    {
        /// <summary>
        /// Gets a collection of information about the virtual interfaces.
        /// </summary>
        [JsonProperty("virtual_interfaces")]
        public IEnumerable<VirtualInterface> VirtualInterfaces { get; private set; }
    }
}
