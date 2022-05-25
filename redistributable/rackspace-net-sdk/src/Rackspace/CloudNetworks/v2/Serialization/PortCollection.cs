using System.Collections.Generic;

using Newtonsoft.Json;

using OpenStack.Serialization;

using Rackspace.Serialization;

namespace Rackspace.CloudNetworks.v2.Serialization
{
    /// <inheritdoc cref="OpenStack.Networking.v2.Serialization.PortCollection"/>
    public class PortCollection : Page<Port>
    {
        /// <summary>
        /// The requested ports.
        /// </summary>
        [JsonProperty("ports")]
        public IList<Port> Ports
        {
            get { return Items; }
            set { Items = value; }
        }

        /// <summary>
        /// The paging navigation links.
        /// </summary>
        [JsonProperty("ports_links")]
        public IList<IPageLink> PortsLinks
        {
            get { return Links; }
            set { Links = value; }
        }
    }
}