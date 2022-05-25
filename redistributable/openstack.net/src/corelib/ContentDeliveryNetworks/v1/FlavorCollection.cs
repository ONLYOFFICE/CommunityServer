using System.Collections.Generic;
using OpenStack.Serialization;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a collection of flavor resources of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "flavors")]
    public class FlavorCollection : List<Flavor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlavorCollection"/> class.
        /// </summary>
        public FlavorCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlavorCollection"/> class.
        /// </summary>
        /// <param name="flavors">The flavors.</param>
        public FlavorCollection(IEnumerable<Flavor> flavors) : base(flavors)
        {
        }
    }
}