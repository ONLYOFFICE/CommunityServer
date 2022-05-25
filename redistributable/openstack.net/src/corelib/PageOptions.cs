using System.Collections.Generic;
using OpenStack.Serialization;

namespace OpenStack
{
    /// <summary>
    /// Paging options when listing a resource that supports paging.
    /// </summary>
    public class PageOptions : FilterOptions
    {
        /// <summary>
        /// The number of resources to return per page.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// The identifier of the first resource to return on the page.
        /// </summary>
        public Identifier StartingAt { get; set; }

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            return new Dictionary<string, object>
            {
                {"marker", StartingAt},
                {"limit", PageSize}
            };
        }
    }

    /// <summary>
    /// Options when list a resource that supports filtering.
    /// </summary>
    public abstract class FilterOptions : IQueryStringBuilder
    {
        /// <summary />
        protected abstract IDictionary<string, object> BuildQueryString();

        IDictionary<string, object> IQueryStringBuilder.Build()
        {
            return BuildQueryString();
        }
    }
}
