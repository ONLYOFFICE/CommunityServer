using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of server actions of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ServerActionCollection<T> : ResourceCollection<T>
        where T : IServiceResource
    {
        /// <summary/>
        [JsonProperty("instanceActions")]
        protected IList<T> Events => Items;
    }

    /// <summary>
    /// Represents a collection of server action summary resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ServerActionSummaryCollection : ServerActionCollection<ServerActionSummary>
    { }
}
