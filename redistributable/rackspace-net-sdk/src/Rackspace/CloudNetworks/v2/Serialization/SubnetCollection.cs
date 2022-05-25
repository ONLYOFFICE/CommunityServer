using System.Collections.Generic;

using Newtonsoft.Json;

using OpenStack.Serialization;

using Rackspace.Serialization;

namespace Rackspace.CloudNetworks.v2.Serialization
{
    /// <inheritdoc cref="OpenStack.Networking.v2.Serialization.SubnetCollection"/>
    public class SubnetCollection : Page<Subnet>
    {
        /// <summary>
        /// The requested subnets.
        /// </summary>
        [JsonProperty("subnets")]
        public IList<Subnet> Subnets
        {
            get { return Items; }
            set { Items = value; }
        }

        /// <summary>
        /// The paging navigation links.
        /// </summary>
        [JsonProperty("subnets_links")]
        public IList<IPageLink> SubnetsLinks
        {
            get { return Links; }
            set { Links = value; }
        }
    }
}