using System.Collections.Generic;

using Newtonsoft.Json;

using OpenStack.Serialization;

using Rackspace.Serialization;

namespace Rackspace.CloudNetworks.v2.Serialization
{
    /// <inheritdoc cref="OpenStack.Networking.v2.Serialization.NetworkCollection"/>
    public class NetworkCollection : Page<Network>
    {
        /// <summary>
        /// The requested networks.
        /// </summary>
        [JsonProperty("networks")]
        public IList<Network> Networks
        {
            get { return Items; }
            set { Items = value; }
        }

        /// <summary>
        /// The paging navigation links.
        /// </summary>
        [JsonProperty("networks_links")]
        public IList<IPageLink> NetworksLinks
        {
            get { return Links; }
            set { Links = value; }
        }
    }
}
