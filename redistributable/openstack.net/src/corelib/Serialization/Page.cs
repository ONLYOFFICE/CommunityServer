using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Newtonsoft.Json;

namespace OpenStack.Serialization
{
    /// <inheritdoc cref="IPage{T}" />
    /// <exclude />
    [JsonObject(MemberSerialization.OptIn)]
    public class Page<TPage, TItem, TLink> : ResourceCollection<TItem>, IPage<TItem>, IPageBuilder<TPage>
        where TPage : IPage<TItem>
        where TLink : IPageLink
    {
        private Func<Url, CancellationToken, Task<TPage>> _nextPageHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="Page{TPage, TItem,TLink}"/> class.
        /// </summary>
        public Page()
        {
            Links = new List<TLink>();    
        }

        /// <inheritdoc />
        [JsonIgnore]
        public bool HasNextPage => GetNextLink() != null;
        
        /// <inheritdoc />
        void IPageBuilder<TPage>.SetNextPageHandler(Func<Url, CancellationToken, Task<TPage>> value)
        {
            _nextPageHandler = value;
        }

        /// <inheritdoc />
        public async Task<IPage<TItem>> GetNextPageAsync(CancellationToken cancellationToken)
        {
            var nextPageLink = GetNextLink();
            if (nextPageLink == null)
                return Empty();

            return await _nextPageHandler(new Url(nextPageLink.Url), cancellationToken);
        }

        /// <summary>
        /// Returns an empty page
        /// </summary>
        public static IPage<TItem> Empty()
        {
            return EmptyPage.Instance;
        }

        /// <summary>
        /// The paging navigation links.
        /// </summary>
        public IList<TLink> Links { get; set; }

        /// <summary>
        /// Finds the next link.
        /// </summary>
        protected virtual TLink GetNextLink()
        {
            return Links.FirstOrDefault(x => x.IsNextPage);
        }

        private sealed class EmptyPage : Page<TPage, TItem, TLink>
        {
            public static readonly EmptyPage Instance = new EmptyPage();

            private EmptyPage() { }
        }
    }
}