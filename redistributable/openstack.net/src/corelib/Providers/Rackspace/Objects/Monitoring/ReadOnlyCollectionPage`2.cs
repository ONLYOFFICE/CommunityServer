using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using net.openstack.Core;
    using net.openstack.Core.Collections;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class provides read-only access to a single page of results returned
    /// by a paginated API call in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the list.</typeparam>
    /// <typeparam name="TMarker">The type of marker used to identify the pages in this collection.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class ReadOnlyCollectionPage<T, TMarker> : ReadOnlyCollectionPage<T>
        where TMarker : ResourceIdentifier<TMarker>
    {
        /// <summary>
        /// This is the backing field for both <see cref="CanHaveNextPage"/> and <see cref="GetNextPageAsync"/>.
        /// </summary>
        private readonly Func<TMarker, CancellationToken, Task<ReadOnlyCollectionPage<T, TMarker>>> _getNextPageAsync;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        private IDictionary<string, object> _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollectionPage{T, TMarker}"/> class
        /// that is a read-only wrapper around the specified list and metadata.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <param name="getNextPageAsync">A function that returns a <see cref="Task{TResult}"/> representing the asynchronous operation to get the next page of items in the collection. If specified, this function implements <see cref="GetNextPageAsync"/>. If the value is <see langword="null"/>, then <see cref="CanHaveNextPage"/> will return <see langword="false"/>.</param>
        /// <param name="metadata">The metadata associated with the list.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="list"/> is <see langword="null"/>.</exception>
        public ReadOnlyCollectionPage(IList<T> list, Func<TMarker, CancellationToken, Task<ReadOnlyCollectionPage<T, TMarker>>> getNextPageAsync, IDictionary<string, object> metadata)
            : base(list)
        {
            _getNextPageAsync = getNextPageAsync;
            _metadata = metadata ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets a collection of metadata associated with the page of results.
        /// </summary>
        public ReadOnlyDictionary<string, object> Metadata
        {
            get
            {
                return new ReadOnlyDictionary<string, object>(_metadata);
            }
        }

        /// <summary>
        /// Gets the marker for the current page.
        /// </summary>
        public TMarker Marker
        {
            get
            {
                object marker;
                if (!_metadata.TryGetValue("marker", out marker) || marker == null)
                    return null;

                JToken token = JToken.FromObject(marker);
                return token.ToObject<TMarker>();
            }
        }

        /// <summary>
        /// Gets the marker for the next page.
        /// </summary>
        public TMarker NextMarker
        {
            get
            {
                object marker;
                if (!_metadata.TryGetValue("next_marker", out marker) || marker == null)
                    return null;

                JToken token = JToken.FromObject(marker);
                return token.ToObject<TMarker>();
            }
        }

        /// <inheritdoc/>
        public override bool CanHaveNextPage
        {
            get
            {
                return _getNextPageAsync != null && NextMarker != null;
            }
        }

        /// <inheritdoc/>
        public override Task<ReadOnlyCollectionPage<T>> GetNextPageAsync(CancellationToken cancellationToken)
        {
            if (!CanHaveNextPage)
                throw new InvalidOperationException("Cannot obtain the next page when CanHaveNextPage is false.");

            return _getNextPageAsync(NextMarker, cancellationToken)
                .Select(task => (ReadOnlyCollectionPage<T>)task.Result);
        }
    }
}
