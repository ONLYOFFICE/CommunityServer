using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of flavor resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class FlavorCollection<TPage, TItem> : Page<TPage, TItem, PageLink>
        where TPage : FlavorCollection<TPage, TItem>
        where TItem : IServiceResource
    {
        /// <summary/>
        [JsonProperty("flavors")]
        protected IList<TItem> Flavors => Items;

        /// <summary/>
        [JsonProperty("flavors_links")]
        protected IList<PageLink> FlavorLinks => Links;
    }

    /// <summary>
    /// Represents a collection of flavor summary resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class FlavorSummaryCollection : FlavorCollection<FlavorSummaryCollection, FlavorSummary>
    { }

    /// <inheritdoc cref="FlavorCollection{TPage, TItem}" />
    /// <exclude />
    public class FlavorCollection : FlavorCollection<FlavorCollection, Flavor>
    { }
}
