using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of image resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ImageCollection<TPage, TItem> : Page<TPage, TItem, PageLink>
        where TPage : ImageCollection<TPage, TItem>
        where TItem : IServiceResource
    {
        /// <summary/>
        [JsonProperty("images")]
        protected IList<TItem> Flavors => Items;

        /// <summary/>
        [JsonProperty("images_links")]
        protected IList<PageLink> ServerLinks => Links;
    }

    /// <summary>
    /// Represents a collection of image summary resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ImageSummaryCollection : ImageCollection<ImageSummaryCollection, ImageSummary>
    { }

    /// <inheritdoc cref="ImageCollection{TPage, TItem}" />
    /// <exclude />
    public class ImageCollection : ImageCollection<ImageCollection, Image>
    { }
}
