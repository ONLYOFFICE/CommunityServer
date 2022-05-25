using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of server resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ServerCollection<TPage, TItem> : Page<TPage, TItem, PageLink>
        where TPage : ServerCollection<TPage, TItem>
        where TItem : IServiceResource
    {
        /// <summary/>
        [JsonProperty("servers")]
        protected IList<TItem> Servers => Items;

        /// <summary/>
        [JsonProperty("servers_links")]
        protected IList<PageLink> ServerLinks => Links;
    }

    /// <summary>
    /// Represents a collection of server summary resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ServerSummaryCollection : ServerCollection<ServerSummaryCollection, ServerSummary>
    { }

    /// <inheritdoc cref="ServerCollection{TPage, TItem}" />
    /// <exclude />
    public class ServerCollection : ServerCollection<ServerCollection, Server>
    { }
}
