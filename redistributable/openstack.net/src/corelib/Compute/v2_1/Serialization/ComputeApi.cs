using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Extensions;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using OpenStack.Authentication;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Builds requests to the Compute API which can be further customized and then executed.
    /// <para>Intended for custom implementations.</para>
    /// </summary>
    /// <exclude />
    /// <seealso href="http://developer.openstack.org/api-ref-compute-v2.1.html">OpenStack Compute API v2.1 Overview</seealso>
    public class ComputeApi
    {
        /// <summary>
        /// The authentication service.
        /// </summary>
        protected readonly IAuthenticationProvider AuthenticationProvider;

        /// <summary>
        /// The Nova microversion header key
        /// </summary>
        public const string MicroversionHeader = "X-OpenStack-Nova-API-Version";

        /// <summary>
        /// The Compute service endpoint.
        /// </summary>
        protected readonly ServiceEndpoint Endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeApi"/> class.
        /// </summary>
        /// <param name="serviceType">The service type for the desired compute provider.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        /// <param name="useInternalUrl">if set to <c>true</c> uses the internal URLs specified in the ServiceCatalog, otherwise the public URLs are used.</param>
        public ComputeApi(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl)
            : this(serviceType, authenticationProvider, region, useInternalUrl, "2.1")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeApi" /> class.
        /// </summary>
        /// <param name="serviceType">The service type for the desired compute provider.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        /// <param name="useInternalUrl">if set to <c>true</c> uses the internal URLs specified in the ServiceCatalog, otherwise the public URLs are used.</param>
        /// <param name="microversion">The requested API microversion.</param>
        /// <exception cref="ArgumentNullException">
        /// serviceType
        /// or
        /// authenticationProvider
        /// </exception>
        /// <exception cref="ArgumentException">region cannot be null or empty;region</exception>
        /// <exception cref="ArgumentException">region cannot be null or empty</exception>
        protected ComputeApi(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl, string microversion)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            if (authenticationProvider == null)
                throw new ArgumentNullException("authenticationProvider");
            if (string.IsNullOrEmpty(region))
                throw new ArgumentException("region cannot be null or empty", "region");

            AuthenticationProvider = authenticationProvider;
            Endpoint = new ServiceEndpoint(serviceType, authenticationProvider, region, useInternalUrl, microversion, MicroversionHeader);
        }

        #region Servers

        /// <summary>
        /// Shows details for a server. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetServerAsync<T>(string serverId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetServerRequest(serverId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetServerAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serverId"/> is <see langword="null" />.</exception>
        public virtual Task<PreparedRequest> BuildGetServerRequest(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(serverId == null)
                throw new ArgumentNullException("serverId");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}", cancellationToken);
        }

        /// <summary>
        /// Gets all metadata for a server. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetServerMetadataAsync<T>(string serverId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IChildResource
        {
            return await BuildGetServerMetadataRequest(serverId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this)
                .SetParent(serverId).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetServerMetadataAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serverId"/> is <see langword="null" />.</exception>
        public virtual Task<PreparedRequest> BuildGetServerMetadataRequest(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}/metadata", cancellationToken);
        }

        /// <summary>
        /// Shows details for a metadata item, by key, for a server. 
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<string> GetServerMetadataItemAsync(string serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            dynamic result = await BuildGetServerMetadataItemRequest(serverId, key, cancellationToken)
                .SendAsync()
                .ReceiveJson().ConfigureAwait(false);

            var meta = (IDictionary<string, object>)result.meta;
            return meta[key]?.ToString();
        }

        /// <summary>
        /// Builds the <see cref="GetServerMetadataItemAsync"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildGetServerMetadataItemRequest(string serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            if (key == null)
                throw new ArgumentNullException("key");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}/metadata/{key}", cancellationToken);
        }

        /// <summary>
        /// Builds the <see cref="CreateServerAsync{T}"/> request.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildCreateServerRequest(object server, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareCreateResourceRequest("servers", server, cancellationToken);
        }

        /// <summary>
        /// Creates a server. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="server">The server.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> CreateServerAsync<T>(object server, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildCreateServerRequest(server, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates or replaces a metadata item, by key, for a server. 
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task CreateServerMetadataAsync(string serverId, string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildCreateServerMetadataRequest(serverId, key, value, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="CreateServerMetadataAsync"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual async Task<PreparedRequest> BuildCreateServerMetadataRequest(string serverId, string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            if (key == null)
                throw new ArgumentNullException("key");

            var serverMetadata = new
            {
                meta = new Dictionary<string, string>
                {
                    [key] = value
                }
            };

            PreparedRequest request = await Endpoint.PrepareRequest($"servers/{serverId}/metadata/{key}", cancellationToken).ConfigureAwait(false);
            return request.PreparePutJson(serverMetadata, cancellationToken);
        }

        /// <summary>
        /// Waits for the server to reach the specified status.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="status">The status to wait for.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<TServer> WaitForServerStatusAsync<TServer, TStatus>(string serverId, TStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TServer : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<TServer>> getServer = async () => await GetServerAsync<TServer>(serverId, cancellationToken).ConfigureAwait(false);
            return await Endpoint.WaitForStatusAsync(serverId, status, getServer, refreshDelay, timeout, progress, cancellationToken)
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the server to reach the specified status.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="status">The status to wait for.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<TServer> WaitForServerStatusAsync<TServer, TStatus>(string serverId, IEnumerable<TStatus> status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TServer : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<TServer>> getServer = async () => await GetServerAsync<TServer>(serverId, cancellationToken).ConfigureAwait(false);
            return await Endpoint.WaitForStatusAsync(serverId, status, getServer, refreshDelay, timeout, progress, cancellationToken)
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the server to be deleted.
        /// <para>Treats a 404 NotFound exception as confirmation that it is deleted.</para>
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="deletedStatus">The deleted status to wait for.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public Task WaitUntilServerIsDeletedAsync<TServer, TStatus>(string serverId, TStatus deletedStatus = null, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TServer : IServiceResource
            where TStatus : ResourceStatus
        {
            deletedStatus = deletedStatus ?? StringEnumeration.FromDisplayName<TStatus>("DELETED");
            Func<Task<dynamic>> getServer = async () => await GetServerAsync<TServer>(serverId, cancellationToken).ConfigureAwait(false);
            return Endpoint.WaitUntilDeletedAsync(serverId, deletedStatus, getServer, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <summary>
        /// Lists summary information for all servers.
        /// </summary>
        /// <typeparam name="TPage">The return type.</typeparam>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<TPage> ListServerSummariesAsync<TPage>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where TPage : IPageBuilder<TPage>, IEnumerable<IServiceResource>
        {
            Url initialRequestUrl = await BuildListServerSummariesUrl(queryString, cancellationToken).ConfigureAwait(false);
            return await Endpoint.GetResourcePageAsync<TPage>(initialRequestUrl, cancellationToken)
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListServerSummariesAsync{T}"/> URL.
        /// </summary>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<Url> BuildListServerSummariesUrl(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegment("servers")
                .SetQueryParams(queryString?.Build());
        }

        /// <summary>
        /// Lists all servers with details. 
        /// </summary>
        /// <typeparam name="TPage">The return type.</typeparam>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<TPage> ListServersAsync<TPage>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where TPage : IPageBuilder<TPage>, IEnumerable<IServiceResource>
        {
            Url initialRequestUrl = await BuildListServersUrl(queryString, cancellationToken).ConfigureAwait(false);
            return await Endpoint.GetResourcePageAsync<TPage>(initialRequestUrl, cancellationToken)
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the list servers URL.
        /// </summary>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<Url> BuildListServersUrl(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegment("servers/detail")
                .SetQueryParams(queryString?.Build());
        }

        /// <summary>
        /// Builds the <see cref="UpdateServerAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="server">The server.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildUpdateServerRequest(string serverId, object server, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            
            return Endpoint.PrepareUpdateResourceRequest($"servers/{serverId}", server, cancellationToken);
        }

        /// <summary>
        /// Updates the editable attributes of a server.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="server">The server.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> UpdateServerAsync<T>(string serverId, object server, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildUpdateServerRequest(serverId, server, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates or replaces one or more metadata items for a server.
        /// <para>Omitted keys are not removed unless <paramref name="overwrite"/> is <c>true</c>.</para>
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="overwrite">if set to <c>true</c> overwrite all existing metadata keys.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> UpdateServerMetadataAsync<T>(string serverId, object metadata, bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildUpdateServerMetadataRequest(serverId, metadata, overwrite, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="UpdateServerMetadataAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="overwrite">if set to <c>true</c> overwrite all existing metadata keys.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual async Task<PreparedRequest> BuildUpdateServerMetadataRequest(string serverId, object metadata, bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            
            PreparedRequest request = await Endpoint.PrepareRequest($"servers/{serverId}/metadata", cancellationToken).ConfigureAwait(false);

            if (overwrite)
                return request.PreparePutJson(metadata, cancellationToken);

            return request.PreparePostJson(metadata, cancellationToken);
        }

        /// <summary>
        /// Deletes a server.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteServerAsync(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteServerRequest(serverId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteServerAsync"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildDeleteServerRequest(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            
            return Endpoint.PrepareDeleteResourceRequest($"servers/{serverId}", cancellationToken);
        }

        /// <summary>
        /// Deletes a metadata item, by key, from a server.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteServerMetadataAsync(string serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteServerMetadataRequest(serverId, key, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteServerMetadataAsync"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildDeleteServerMetadataRequest(string serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            if (key == null)
                throw new ArgumentNullException("key");

            return Endpoint.PrepareDeleteResourceRequest($"servers/{serverId}/metadata/{key}", cancellationToken);
        }

        /// <summary>
        /// Waits for an image to reach the specified state.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="status">The image status.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<TImage> WaitForImageStatusAsync<TImage, TStatus>(string imageId, TStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TImage : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<TImage>> getImage = async () => await GetImageAsync<TImage>(imageId, cancellationToken).ConfigureAwait(false);
            return await Endpoint.WaitForStatusAsync(imageId, status, getImage, refreshDelay, timeout, progress, cancellationToken)
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the image to be deleted.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="deletedStatus">The deleted status to wait for.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public Task WaitUntilImageIsDeletedAsync<TImage, TStatus>(string imageId, TStatus deletedStatus, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TImage : IServiceResource
            where TStatus : ResourceStatus
        {
            deletedStatus = deletedStatus ?? StringEnumeration.FromDisplayName<TStatus>("DELETED");
            Func<Task<dynamic>> getImage = async () => await GetServerAsync<TImage>(imageId, cancellationToken).ConfigureAwait(false);
            return Endpoint.WaitUntilDeletedAsync(imageId, deletedStatus, getImage, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <summary>
        /// Creates a snapshot image from a server.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> SnapshotServerAsync<T>(string serverId, object request, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            var response = await BuildServerActionRequest(serverId, request, cancellationToken).SendAsync().ConfigureAwait(false);
            response.Headers.TryGetFirst("Location", out var location);
            Identifier imageId = new Uri(location).Segments.Last(); // grab id off the end of the url, e.g. http://172.29.236.100:9292/images/baaab9b9-3635-429e-9969-2899a7cf2d97
            return await GetImageAsync<T>(imageId, cancellationToken)
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Starts a stopped server and changes its status to ACTIVE.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task StartServerAsync(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new Dictionary<string, object> {["os-start"] = "" };
            return BuildServerActionRequest(serverId, request, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Stops a running server and changes its status to SHUTOFF.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task StopServerAsync(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new Dictionary<string, object> {["os-stop"] = "" };
            return BuildServerActionRequest(serverId, request, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Suspends a server and changes its status to SUSPENDED.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task SuspendServerAsync(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new Dictionary<string, object> {["suspend"] = "" };
            return BuildServerActionRequest(serverId, request, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Resumes a suspended server and changes its status to ACTIVE.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task ResumeServerAsync(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new Dictionary<string, object> {["resume"] = "" };
            return BuildServerActionRequest(serverId, request, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Reboots a server.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task RebootServerAsync<TRequest>(string serverId, TRequest request = null, CancellationToken cancellationToken = default(CancellationToken))
            where TRequest : class, new()
        {
            request = request ?? new TRequest();
            return BuildServerActionRequest(serverId, request, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Evacuates a server from a failed host to a new one.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task EvacuateServerAsync(string serverId, object request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildServerActionRequest(serverId, request, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Gets a VNC console for a server.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="type">The remote console type.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<T> GetVncConsoleAsync<T>(string serverId, object type, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var request = JObject.Parse($"{{ 'os-getVNCConsole': {{ 'type': '{type}' }} }}");
            return BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>();
        }

        /// <summary>
        /// Gets a SPICE console for a server. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="type">The remote console type.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<T> GetSpiceConsoleAsync<T>(string serverId, object type, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var request = JObject.Parse($"{{ 'os-getSPICEConsole': {{ 'type': '{type}' }} }}");
            return BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>();
        }

        /// <summary>
        /// Gets a serial console for a server.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="type">The remote console type.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<T> GetSerialConsoleAsync<T>(string serverId, object type, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var request = JObject.Parse($"{{ 'os-getSerialConsole': {{ 'type': '{type}' }} }}");
            return BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>();
        }

        /// <summary>
        /// Gets an RDP console for a server.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="type">The remote console type.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<T> GetRdpConsoleAsync<T>(string serverId, object type, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var request = JObject.Parse($"{{ 'os-getRDPConsole': {{ 'type': '{type}' }} }}");
            return BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>();
        }

        /// <summary>
        /// Shows console output for a server instance.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="length">The number of lines to fetch from the end of console log. -1 indicates unlimited.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<string> GetConsoleOutputAsync(string serverId, int length = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = JObject.Parse($"{{ 'os-getConsoleOutput': {{ 'length': '{length}' }} }}");
            dynamic result = await BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync()
                .ReceiveJson().ConfigureAwait(false);

            return result.output;
        }

        /// <summary>
        /// Puts a server in rescue mode and changes its status to RESCUE. 
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<string> RescueServerAsync(string serverId, object request = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            request = request ?? new Dictionary<string, object> {["rescue"] = null};
            dynamic result = await BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync()
                .ReceiveJson().ConfigureAwait(false);

            return result.adminPass;
        }

        /// <summary>
        /// Unrescues a server. Changes status to ACTIVE.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task UnrescueServerAsync(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            object request = new Dictionary<string, object> {["unrescue"] = null};
            return BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync();
        }

        /// <summary>
        /// Resizes a server.
        /// <para>Depending on the cloud configuration, <see cref="ConfirmResizeServerAsync"/> may need to be called to complete the resize operation.</para>
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="flavorId">The flavor identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task ResizeServerAsync(string serverId, string flavorId, CancellationToken cancellationToken = default(CancellationToken))
        {
            object request = new
            {
                resize = new
                {
                    flavorRef = flavorId
                }
            };
            return BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync();
        }

        /// <summary>
        /// Confirms a pending resize action for a server.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task ConfirmResizeServerAsync(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            object request = new Dictionary<string, object> {["confirmResize"] = null};
            return BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync();
        }

        /// <summary>
        /// Cancels and reverts a pending resize action for a server.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task CancelResizeServerAsync(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            object request = new Dictionary<string, object> {["revertResize"] = null};
            return BuildServerActionRequest(serverId, request, cancellationToken)
                .SendAsync();
        }

        /// <summary>
        /// Associates a floating IP address to the server.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task AssociateFloatingIPAsync(string serverId, object request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildServerActionRequest(serverId, request, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Disassociate a floating IP address from a server.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="floatingIPAddress">The floating IP address to remove.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task DisassociateFloatingIPAsync(string serverId, string floatingIPAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new
            {
                removeFloatingIp = new
                {
                    address = floatingIPAddress
                }
            };
            return BuildServerActionRequest(serverId, request, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds a server action request, where the server operation is specified in the request body.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual async Task<PreparedRequest> BuildServerActionRequest(string serverId, object requestBody, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            var request = await Endpoint.PrepareRequest($"servers/{serverId}/action", cancellationToken).ConfigureAwait(false);
            return request.PreparePostJson(requestBody, cancellationToken);
        }

        /// <summary>
        /// Lists the actions which have been applied to a sever.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListServerActionSummariesAsync<T>(string serverId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListServerActionSummariesRequest(serverId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListServerActionSummariesAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildListServerActionSummariesRequest(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(serverId == null)
                throw new ArgumentNullException("serverId");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}/os-instance-actions", cancellationToken);
        }

        /// <summary>
        /// Shows details for a server action.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetServerActionAsync<T>(string serverId, string actionId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetServerActionRequest(serverId, actionId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetServerActionAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildGetServerActionRequest(string serverId, string actionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            if (serverId == null)
                throw new ArgumentNullException("actionId");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}/os-instance-actions/{actionId}", cancellationToken);
        }

        #endregion

        #region Flavors

        /// <summary>
        /// Shows details for a flavor. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="flavorId">The flavor identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetFlavorAsync<T>(string flavorId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetFlavorRequest(flavorId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetFlavorAsync{T}"/> request.
        /// </summary>
        /// <param name="flavorId">The flavor identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildGetFlavorRequest(string flavorId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(flavorId == null)
                throw new ArgumentNullException("flavorId");

            return Endpoint.PrepareGetResourceRequest($"flavors/{flavorId}", cancellationToken);
        }

        /// <summary>
        /// Lists summary information for available flavors.
        /// </summary>
        /// <typeparam name="TPage">The return type.</typeparam>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<TPage> ListFlavorSummariesAsync<TPage>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where TPage : IPageBuilder<TPage>, IEnumerable<IServiceResource>
        {
            Url initialRequestUrl = await BuildListFlavorSummariesURL(queryString, cancellationToken).ConfigureAwait(false);
            return await Endpoint.GetResourcePageAsync<TPage>(initialRequestUrl, cancellationToken)
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListFlavorSummariesAsync{T}"/> request.
        /// </summary>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<Url> BuildListFlavorSummariesURL(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegment("flavors")
                .SetQueryParams(queryString?.Build());
        }

        /// <summary>
        /// Lists available flavors.
        /// </summary>
        /// <typeparam name="TPage">The return type.</typeparam>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<TPage> ListFlavorsAsync<TPage>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where TPage : IPageBuilder<TPage>, IEnumerable<IServiceResource>
        {
            Url initialRequestUrl = await BuildListFlavorsURL(queryString, cancellationToken).ConfigureAwait(false);
            return await Endpoint.GetResourcePageAsync<TPage>(initialRequestUrl, cancellationToken)
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListFlavorsAsync{T}"/> URL.
        /// </summary>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<Url> BuildListFlavorsURL(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegment("flavors/detail")
                .SetQueryParams(queryString?.Build());
        }

        #endregion

        #region Images

        /// <summary>
        /// Shows details for an image.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetImageAsync<T>(string imageId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetImageRequest(imageId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetImageAsync{T}"/> request.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildGetImageRequest(string imageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(imageId == null)
                throw new ArgumentNullException("imageId");

            return Endpoint.PrepareGetResourceRequest($"images/{imageId}", cancellationToken);
        }

        /// <summary>
        /// Shows metadata for an image.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetImageMetadataAsync<T>(string imageId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IChildResource
        {
            return await BuildGetImageMetadataRequest(imageId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this)
                .SetParent(imageId).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetImageMetadataAsync{T}"/> request.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetImageMetadataRequest(string imageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");

            return Endpoint.PrepareGetResourceRequest($"images/{imageId}/metadata", cancellationToken);
        }

        /// <summary>
        /// Shows details for a metadata item, by key, for an image.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<string> GetImageMetadataItemAsync(string imageId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            dynamic result = await BuildGetImageMetadataItemRequest(imageId, key, cancellationToken)
                .SendAsync()
                .ReceiveJson().ConfigureAwait(false);

            var meta = (IDictionary<string, object>)result.meta;
            return meta[key]?.ToString();
        }

        /// <summary>
        /// Builds the <see cref="GetImageMetadataItemAsync"/> request.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetImageMetadataItemRequest(string imageId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");

            if (key == null)
                throw new ArgumentNullException("key");

            return Endpoint.PrepareGetResourceRequest($"images/{imageId}/metadata/{key}", cancellationToken);
        }

        /// <summary>
        /// Creates or replaces metadata for an image.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task CreateImageMetadataAsync(string imageId, string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildCreateImageMetadataRequest(imageId, key, value, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="CreateImageMetadataAsync"/> request.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<PreparedRequest> BuildCreateImageMetadataRequest(string imageId, string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");

            if (key == null)
                throw new ArgumentNullException("key");

            var imageMetadata = new
            {
                meta = new Dictionary<string, string>
                {
                    [key] = value
                }
            };

            PreparedRequest request = await Endpoint.PrepareRequest($"images/{imageId}/metadata/{key}", cancellationToken).ConfigureAwait(false);
            return request.PreparePutJson(imageMetadata, cancellationToken);
        }

        /// <summary>
        /// Lists summary information for available images.
        /// </summary>
        /// <typeparam name="TPage">The return type.</typeparam>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<TPage> ListImageSummariesAsync<TPage>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where TPage : IPageBuilder<TPage>, IEnumerable<IServiceResource>
        {
            Url initialRequestUrl = await BuildListImageSummariesRequest(queryString, cancellationToken).ConfigureAwait(false);
            return await Endpoint.GetResourcePageAsync<TPage>(initialRequestUrl, cancellationToken)
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListImageSummariesAsync{T}"/> request.
        /// </summary>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<Url> BuildListImageSummariesRequest(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegment("images")
                .SetQueryParams(queryString?.Build());
        }

        /// <summary>
        /// Lists available images.
        /// </summary>
        /// <typeparam name="TPage">The return type.</typeparam>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<TPage> ListImagesAsync<TPage>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where TPage : IPageBuilder<TPage>, IEnumerable<IServiceResource>
        {
            Url initialRequestUrl = await BuildListImagesRequest(queryString, cancellationToken).ConfigureAwait(false);
            return await Endpoint.GetResourcePageAsync<TPage>(initialRequestUrl, cancellationToken)
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListImagesAsync{T}"/> request.
        /// </summary>
        /// <param name="queryString">Options for paging and filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<Url> BuildListImagesRequest(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegment("images/detail")
                .SetQueryParams(queryString?.Build());
        }

        /// <summary>
        /// Creates or replaces one or more metadata items for an image.
        /// <para>Omitted keys are not removed unless <paramref name="overwrite"/> is <c>true</c>.</para>
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="overwrite">if set to <c>true</c> overwrite all existing metadata keys.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> UpdateImageMetadataAsync<T>(string imageId, object metadata, bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildUpdateImageMetadataRequest(imageId, metadata, overwrite, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="UpdateImageMetadataAsync{T}"/> request.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="overwrite">if set to <c>true</c> all existing metadata keys.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<PreparedRequest> BuildUpdateImageMetadataRequest(string imageId, object metadata, bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");

            PreparedRequest request = await Endpoint.PrepareRequest($"images/{imageId}/metadata", cancellationToken).ConfigureAwait(false);

            if (overwrite)
                return request.PreparePutJson(metadata, cancellationToken);

            return request.PreparePostJson(metadata, cancellationToken);
        }

        /// <summary>
        /// Deletes an image.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteImageAsync(string imageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteImageRequest(imageId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteImageAsync"/> request.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildDeleteImageRequest(string imageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");

            return Endpoint.PrepareDeleteResourceRequest($"images/{imageId}", cancellationToken);
        }

        /// <summary>
        /// Deletes a metadata item, by key, for an image.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteImageMetadataAsync(string imageId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteImageMetadataRequest(imageId, key, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteImageMetadataAsync"/> request.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildDeleteImageMetadataRequest(string imageId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");

            if (key == null)
                throw new ArgumentNullException("key");

            return Endpoint.PrepareDeleteResourceRequest($"images/{imageId}/metadata/{key}", cancellationToken);
        }

        #endregion

        #region IP Addresses

        /// <summary>
        /// Shows IP addresses details for a network label of a server instance. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="key">The network key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<IList<T>> GetServerAddressAsync<T>(string serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await BuildGetServerAddressRequest(serverId, key, cancellationToken)
                .SendAsync()
                .ReceiveJson<IDictionary<string, IList<T>>>().ConfigureAwait(false);

            return result[key];
        }

        /// <summary>
        /// Builds the <see cref="GetServerAddressAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetServerAddressRequest(string serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            if (key == null)
                throw new ArgumentNullException("key");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}/ips/{key}", cancellationToken);
        }

        /// <summary>
        /// Lists IP addresses that are assigned to a server. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<T> ListServerAddressesAsync<T>(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildListServerAddressesRequest(serverId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>();
        }

        /// <summary>
        /// Builds the <see cref="ListServerAddressesAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The serverid.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildListServerAddressesRequest(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}/ips", cancellationToken);
        }

        #endregion

        #region Server Volumes

        /// <summary>
        /// Shows details for a volume attachment. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetServerVolumeAsync<T>(string serverId, string volumeId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IChildResource
        {
            return await BuildGetServerVolumeRequest(serverId, volumeId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this)
                .SetParent(serverId).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetServerVolumeAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildGetServerVolumeRequest(string serverId, string volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            if (volumeId == null)
                throw new ArgumentNullException("volumeId");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}/os-volume_attachments/{volumeId}", cancellationToken);
        }

        /// <summary>
        /// Lists the volume attachments for a server.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListServerVolumesAsync<T>(string serverId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IChildResource>
        {
            return await BuildListServerVolumesRequest(serverId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this)
                .SetParentOnChildren(serverId).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListServerVolumesAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildListServerVolumesRequest(string serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            return Endpoint.PrepareGetResourceRequest($"servers/{serverId}/os-volume_attachments", cancellationToken);
        }

        /// <summary>
        /// Attaches a volume to a server.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="serverVolume">The request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> AttachVolumeAsync<T>(string serverId, object serverVolume, CancellationToken cancellationToken = default(CancellationToken))
            where T : IChildResource
        {
            return await BuildAttachVolumeRequest(serverId, serverVolume, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this)
                .SetParent(serverId).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="AttachVolumeAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="serverVolume">The server volume.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildAttachVolumeRequest(string serverId, object serverVolume, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            return Endpoint.PrepareCreateResourceRequest($"servers/{serverId}/os-volume_attachments", serverVolume, cancellationToken);
        }

        /// <summary>
        /// Detaches a volume from a server.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DetachVolumeAsync(string serverId, string volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDetachVolumeRequest(serverId, volumeId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DetachVolumeAsync"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildDetachVolumeRequest(string serverId, string volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");

            if (volumeId == null)
                throw new ArgumentNullException("volumeId");

            return Endpoint.PrepareDeleteResourceRequest($"servers/{serverId}/os-volume_attachments/{volumeId}", cancellationToken);
        }

        #endregion

        #region Keypairs

        /// <summary>
        /// Shows details for a keypair that is associated with the account.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="keypairName">Name of the keypair.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetKeyPairAsync<T>(string keypairName, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            

            return await BuildGetKeyPairRequest(keypairName, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetKeyPairAsync{T}"/> request.
        /// </summary>
        /// <param name="keypairName">Name of the keypair.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetKeyPairRequest(string keypairName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keypairName == null)
                throw new ArgumentNullException("keypairName");

            return Endpoint.PrepareGetResourceRequest($"os-keypairs/{keypairName}", cancellationToken);
        }

        /// <summary>
        /// Generates or imports a keypair.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="keypair">The keypair.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> CreateKeyPairAsync<T>(object keypair, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildCreateKeyPairRequest(keypair, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="CreateKeyPairAsync{T}"/> request.
        /// </summary>
        /// <param name="keypair">The keypair.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">keypair</exception>
        public virtual Task<PreparedRequest> BuildCreateKeyPairRequest(object keypair, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keypair == null)
                throw new ArgumentNullException("keypair");

            return Endpoint.PrepareCreateResourceRequest("os-keypairs", keypair, cancellationToken);
        }

        /// <summary>
        /// Lists keypairs that are associated with the account.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListKeyPairsAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListKeyPairsRequest(cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListKeyPairsAsync{T}"/> request.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildListKeyPairsRequest(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareGetResourceRequest("os-keypairs", cancellationToken);
        }

        /// <summary>
        /// Deletes a keypair.
        /// </summary>
        /// <param name="keypairName">Name of the keypair.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteKeyPairAsync(string keypairName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteKeyPairRequest(keypairName, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteKeyPairAsync"/> request.
        /// </summary>
        /// <param name="keypairName">Name of the keypair.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildDeleteKeyPairRequest(string keypairName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keypairName == null)
                throw new ArgumentNullException("keypairName");

            return Endpoint.PrepareDeleteResourceRequest($"os-keypairs/{keypairName}", cancellationToken);
        }

        #endregion

        #region Security Groups

        /// <summary>
        /// Shows details for a security group.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="securityGroupId">The security group identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetSecurityGroupAsync<T>(string securityGroupId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetSecurityGroupRequest(securityGroupId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetSecurityGroupAsync{T}"/> request.
        /// </summary>
        /// <param name="securityGroupId">The security group identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetSecurityGroupRequest(string securityGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (securityGroupId == null)
                throw new ArgumentNullException("securityGroupId");

            return Endpoint.PrepareGetResourceRequest($"os-security-groups/{securityGroupId}", cancellationToken);
        }

        /// <summary>
        /// Creates a security group.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="securityGroup">The security group.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> CreateSecurityGroupAsync<T>(object securityGroup, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildCreateSecurityGroupRequest(securityGroup, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="CreateSecurityGroupAsync{T}"/> request.
        /// </summary>
        /// <param name="securityGroup">The security group.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildCreateSecurityGroupRequest(object securityGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (securityGroup == null)
                throw new ArgumentNullException("securityGroup");

            return Endpoint.PrepareCreateResourceRequest("os-security-groups", securityGroup, cancellationToken);
        }

        /// <summary>
        /// Creates a rule for a security group.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="rule">The rule.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> CreateSecurityGroupRuleAsync<T>(object rule, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildCreateSecurityGroupRuleRequest(rule, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="CreateSecurityGroupRuleAsync{T}"/> request.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildCreateSecurityGroupRuleRequest(object rule, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (rule == null)
                throw new ArgumentNullException("rule");

            return Endpoint.PrepareCreateResourceRequest("os-security-group-rules", rule, cancellationToken);
        }

        /// <summary>
        /// Lists security groups.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListSecurityGroupsAsync<T>(string serverId = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListSecurityGroupsRequest(serverId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListSecurityGroupsAsync{T}"/> request.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildListSecurityGroupsRequest(string serverId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string path = serverId == null ? "os-security-groups" : $"servers/{serverId}/os-security-groups";
            return Endpoint.PrepareGetResourceRequest(path, cancellationToken);
        }

        /// <summary>
        /// Updates a security group.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="securityGroupId">The security group identifier.</param>
        /// <param name="securityGroup">The security group.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> UpdateSecurityGroupAsync<T>(string securityGroupId, object securityGroup, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildUpdateSecurityGroupRequest(securityGroupId, securityGroup, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="UpdateSecurityGroupAsync{T}"/> request.
        /// </summary>
        /// <param name="securityGroupId">The security group identifier.</param>
        /// <param name="securityGroup">The security group.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException" />
        public virtual Task<PreparedRequest> BuildUpdateSecurityGroupRequest(string securityGroupId, object securityGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (securityGroupId == null)
                throw new ArgumentNullException("securityGroupId");

            return Endpoint.PrepareUpdateResourceRequest($"os-security-groups/{securityGroupId}", securityGroup, cancellationToken);
        }

        /// <summary>
        /// Deletes a security group.
        /// </summary>
        /// <param name="securityGroupId">The security group identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteSecurityGroupAsync(string securityGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteSecurityGroupRequest(securityGroupId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteSecurityGroupAsync"/> request.
        /// </summary>
        /// <param name="securityGroupId">The security group identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildDeleteSecurityGroupRequest(string securityGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (securityGroupId == null)
                throw new ArgumentNullException("securityGroupId");

            return Endpoint.PrepareDeleteResourceRequest($"os-security-groups/{securityGroupId}", cancellationToken);
        }

        /// <summary>
        /// Deletes a security group rule.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteSecurityGroupRuleAsync(string ruleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteSecurityGroupRuleRequest(ruleId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteSecurityGroupRuleAsync"/> request.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildDeleteSecurityGroupRuleRequest(string ruleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ruleId == null)
                throw new ArgumentNullException("ruleId");

            return Endpoint.PrepareDeleteResourceRequest($"os-security-group-rules/{ruleId}", cancellationToken);
        }

        #endregion

        #region Server Groups

        /// <summary>
        /// Shows details for a server group.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverGroupId">The server group identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetServerGroupAsync<T>(string serverGroupId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetServerGroupRequest(serverGroupId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetServerGroupAsync{T}"/> request.
        /// </summary>
        /// <param name="serverGroupId">The server group identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetServerGroupRequest(string serverGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverGroupId == null)
                throw new ArgumentNullException("serverGroupId");

            return Endpoint.PrepareGetResourceRequest($"os-server-groups/{serverGroupId}", cancellationToken);
        }

        /// <summary>
        /// Creates a server group.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="serverGroup">The server group.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> CreateServerGroupAsync<T>(object serverGroup, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildCreateServerGroupRequest(serverGroup, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="CreateServerGroupAsync{T}"/> request.
        /// </summary>
        /// <param name="serverGroup">The server group.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildCreateServerGroupRequest(object serverGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareCreateResourceRequest("os-server-groups", serverGroup, cancellationToken);
        }

        /// <summary>
        /// Lists all server groups for the account. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListServerGroupsAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListServerGroupsRequest(cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListServerGroupsAsync{T}"/> request.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildListServerGroupsRequest(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareGetResourceRequest("os-server-groups", cancellationToken);
        }

        /// <summary>
        /// Deletes a server group.
        /// </summary>
        /// <param name="serverGroupId">The server group identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteServerGroupAsync(string serverGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteServerGroupRequest(serverGroupId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteServerGroupAsync"/> request.
        /// </summary>
        /// <param name="serverGroupId">The server group identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildDeleteServerGroupRequest(string serverGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (serverGroupId == null)
                throw new ArgumentNullException("serverGroupId");

            return Endpoint.PrepareDeleteResourceRequest($"os-server-groups/{serverGroupId}", cancellationToken);
        }

        #endregion

        #region Volumes

        /// <summary>
        /// Shows details for a volume.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetVolumeAsync<T>(string volumeId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetVolumeRequest(volumeId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetVolumeAsync{T}"/> request.
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetVolumeRequest(string volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");

            return Endpoint.PrepareGetResourceRequest($"os-volumes/{volumeId}", cancellationToken);
        }
        
        /// <summary>
        /// Shows details for a volume snapshot.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="snapshotId">The snapshot identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetVolumeSnapshotAsync<T>(string snapshotId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetVolumeSnapshotRequest(snapshotId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetVolumeSnapshotAsync{T}"/> request.
        /// </summary>
        /// <param name="snapshotId">The snapshot identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetVolumeSnapshotRequest(string snapshotId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (snapshotId == null)
                throw new ArgumentNullException("snapshotId");

            return Endpoint.PrepareGetResourceRequest($"os-snapshots/{snapshotId}", cancellationToken);
        }

        /// <summary>
        /// Creates a volume.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="volume">The volume.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> CreateVolumeAsync<T>(object volume, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildCreateVolumeRequest(volume, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="CreateVolumeAsync{T}"/> request.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildCreateVolumeRequest(object volume, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (volume == null)
                throw new ArgumentNullException("volume");

            return Endpoint.PrepareCreateResourceRequest("os-volumes", volume, cancellationToken);
        }

        /// <summary>
        /// Snapshots a volume.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> SnapshotVolumeAsync<T>(object snapshot, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildSnapshotVolumeRequest(snapshot, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="SnapshotVolumeAsync{T}"/> request.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<PreparedRequest> BuildSnapshotVolumeRequest(object snapshot, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");

            return await Endpoint.PrepareCreateResourceRequest("os-snapshots", snapshot, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Lists the volumes associated with the account.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListVolumesAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListVolumesRequest(cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListVolumesAsync{T}"/> request.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildListVolumesRequest(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareGetResourceRequest("os-volumes", cancellationToken);
        }
        
        /// <summary>
        /// Lists volume snapshots.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListVolumeSnapshotsAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListVolumeSnapshotsRequest(cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListVolumeSnapshotsAsync{T}"/> request.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildListVolumeSnapshotsRequest(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareGetResourceRequest("os-snapshots", cancellationToken);
        }

        /// <summary>
        /// Deletes a volume.
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteVolumeAsync(string volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteVolumeRequest(volumeId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteVolumeAsync"/> request.
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildDeleteVolumeRequest(string volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");

            return Endpoint.PrepareDeleteResourceRequest($"os-volumes/{volumeId}", cancellationToken);
        }

        /// <summary>
        /// Deletes a volume snapshot.
        /// </summary>
        /// <param name="snapshotId">The snapshot identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteVolumeSnapshotAsync(string snapshotId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteVolumeSnapshotRequest(snapshotId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteVolumeSnapshotAsync"/> request.
        /// </summary>
        /// <param name="snapshotId">The snapshot identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">snapshotId</exception>
        public virtual Task<PreparedRequest> BuildDeleteVolumeSnapshotRequest(string snapshotId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (snapshotId == null)
                throw new ArgumentNullException("snapshotId");

            return Endpoint.PrepareDeleteResourceRequest($"os-snapshots/{snapshotId}", cancellationToken);
        }

        /// <summary>
        /// Waits for the volume to reach the specified status.
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="status">The status to wait for.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<TVolume> WaitForVolumeStatusAsync<TVolume, TStatus>(string volumeId, TStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TVolume : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<TVolume>> getVolume = async () => await GetVolumeAsync<TVolume>(volumeId, cancellationToken).ConfigureAwait(false);
            return await Endpoint.WaitForStatusAsync(volumeId, status, getVolume, refreshDelay, timeout, progress, cancellationToken)
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the volume snapshot to reach the specified status.
        /// </summary>
        /// <param name="snapshotId">The snapshot identifier.</param>
        /// <param name="status">The status to wait for.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<TSnapshot> WaitForVolumeSnapshotStatusAsync<TSnapshot, TStatus>(string snapshotId, TStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TSnapshot : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<TSnapshot>> getSnapshot = async () => await GetVolumeSnapshotAsync<TSnapshot>(snapshotId, cancellationToken).ConfigureAwait(false);
            return await Endpoint.WaitForStatusAsync(snapshotId, status, getSnapshot, refreshDelay, timeout, progress, cancellationToken)
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the volume to reach the specified status.
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="status">The status to wait for.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<TVolume> WaitForVolumeStatusAsync<TVolume, TStatus>(string volumeId, IEnumerable<TStatus> status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TVolume : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<TVolume>> getVolume = async () => await GetVolumeAsync<TVolume>(volumeId, cancellationToken).ConfigureAwait(false);
            return await Endpoint.WaitForStatusAsync(volumeId, status, getVolume, refreshDelay, timeout, progress, cancellationToken)
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the volume snapshot to reach the specified status.
        /// </summary>
        /// <param name="snapshotId">The snapshot identifier.</param>
        /// <param name="status">The status to wait for.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<TSnapshot> WaitForVolumeSnapshotStatusAsync<TSnapshot, TStatus>(string snapshotId, IEnumerable<TStatus> status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TSnapshot : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<TSnapshot>> getSnapshot = async () => await GetVolumeSnapshotAsync<TSnapshot>(snapshotId, cancellationToken).ConfigureAwait(false);
            return await Endpoint.WaitForStatusAsync(snapshotId, status, getSnapshot, refreshDelay, timeout, progress, cancellationToken)
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the volume to be deleted.
        /// <para>Treats a 404 NotFound exception as confirmation that it is deleted.</para>
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public Task WaitUntilVolumeIsDeletedAsync<TVolume, TStatus>(string volumeId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TVolume : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<dynamic>> getVolume = async () => await GetVolumeAsync<TVolume>(volumeId, cancellationToken).ConfigureAwait(false);
            return Endpoint.WaitUntilDeletedAsync<TStatus>(volumeId, getVolume, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <summary>
        /// Waits for the volume snapshot to be deleted.
        /// <para>Treats a 404 NotFound exception as confirmation that it is deleted.</para>
        /// </summary>
        /// <param name="snapshotId">The snapshot identifier.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public Task WaitUntilVolumeSnapshotIsDeletedAsync<TVolume, TStatus>(string snapshotId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            where TVolume : IServiceResource
            where TStatus : ResourceStatus
        {
            Func<Task<dynamic>> getSnapshot = async () => await GetVolumeSnapshotAsync<TVolume>(snapshotId, cancellationToken).ConfigureAwait(false);
            return Endpoint.WaitUntilDeletedAsync<TStatus>(snapshotId, getSnapshot, refreshDelay, timeout, progress, cancellationToken);
        }

        #endregion

        #region Compute Service

        /// <summary>
        /// Shows rate and absolute limits for the account.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<T> GetLimitsAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildGetLimitsRequest(cancellationToken)
                .SendAsync()
                .ReceiveJson<T>();
        }

        /// <summary>
        /// Builds the <see cref="GetLimitsAsync{T}"/> request.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildGetLimitsRequest(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareGetResourceRequest("limits", cancellationToken);
        }

        /// <summary>
        /// Get current quotas for an account.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<T> GetCurrentQuotasAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildGetCurrentQuotasRequest(cancellationToken)
                .SendAsync()
                .ReceiveJson<T>();
        }

        /// <summary>
        /// Builds the <see cref="GetCurrentQuotasAsync{T}"/> request.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildGetCurrentQuotasRequest(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareGetResourceRequest("os-quota-sets/detail", cancellationToken);
        }

        /// <summary>
        /// Gets the default quotas for an account.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<T> GetDefaultQuotasAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildGetDefaultQuotasRequest(cancellationToken)
                .SendAsync()
                .ReceiveJson<T>();
        }

        /// <summary>
        /// Builds the <see cref="GetDefaultQuotasAsync{T}"/> request.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<PreparedRequest> BuildGetDefaultQuotasRequest(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Endpoint.PrepareGetResourceRequest("os-quota-sets/defaults", cancellationToken);
        }

        #endregion
    }
}
