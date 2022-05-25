using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Networking.v2.Layer3;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Represents a collection of router resources of the <see cref="NetworkingService"/>.
    /// </summary>
    /// <exclude />
    public class RouterCollection<T> : ResourceCollection<T>
        where T : IServiceResource
    {
        /// <summary/>
        [JsonProperty("routers")]
        protected IList<T> Routers => Items;
    }

    /// <summary>
    /// Represents a collection of router resources of the <see cref="NetworkingService"/>.
    /// </summary>
    /// <exclude />
    public class RouterCollection : RouterCollection<Router>
    { }
}
