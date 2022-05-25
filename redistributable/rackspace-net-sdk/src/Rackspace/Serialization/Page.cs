using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OpenStack.Serialization;

namespace Rackspace.Serialization
{
    /// <inheritdoc />
    [JsonObject(MemberSerialization.OptIn)]
    public class Page<T> : IPage<T>
    {
        private readonly OpenStack.Serialization.Page<OpenStack.IPage<T>, T, IPageLink> _page;

        private Page(OpenStack.Serialization.Page<OpenStack.IPage<T>, T, IPageLink> page)
        {
            _page = page;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Page{T}"/> class.
        /// </summary>
        public Page()
        {
            _page = new OpenStack.Serialization.Page<OpenStack.IPage<T>, T, IPageLink>();
        }
        
        /// <summary>
        /// The requested items.
        /// </summary>
        protected IList<T> Items
        {
            get { return _page.Items; }
            set { _page.Items = value; }
        }

        /// <summary>
        /// The paging navigation links.
        /// </summary>
        protected IList<IPageLink> Links
        {
            get { return _page.Links; }
            set { _page.Links = value; }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return _page.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_page).GetEnumerator();
        }

        /// <inheritdoc />
        public bool HasNextPage => _page.HasNextPage;

        /// <inheritdoc />
        public async Task<IPage<T>> GetNextPageAsync(CancellationToken cancellation = new CancellationToken())
        {
            var nextPageAsync = (OpenStack.Serialization.Page<OpenStack.IPage<T>, T, IPageLink>)(await _page.GetNextPageAsync(cancellation));
            return new Page<T>(nextPageAsync);
        }
    }
}