using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of server group resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ServerGroupCollection<T> : ResourceCollection<T>
        where T : IServiceResource
    {
        /// <summary />
        [JsonProperty("server_groups")]
        protected IList<T> ServerGroups => Items;
    }

    /// <inheritdoc cref="ServerGroupCollection{T}" />
    /// <exclude />
    public class ServerGroupCollection : ServerGroupCollection<ServerGroup>
    { }
}
