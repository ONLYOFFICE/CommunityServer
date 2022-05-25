using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Extensions;
using Flurl.Http;
using OpenStack.Authentication;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Creates urls
    /// </summary>
    /// <exclude />
    public class ServiceEndpoint
    {
        private readonly IServiceType _serviceType;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly string _microversion;
        private readonly string _microversionHeader;

        /// <summary>
        /// The service region.
        /// </summary>
        public string Region { get; }

        /// <summary>
        /// Specifies if internal URLs will be used.
        /// </summary>
        public bool UseInternalUrl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEndpoint"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        /// <param name="useInternalUrl">Specifies if internal URLs should be used.</param>
        public ServiceEndpoint(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl)
            : this(serviceType, authenticationProvider, region, useInternalUrl, microversion: null, microversionHeader: null)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEndpoint"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        /// <param name="useInternalUrl">Specifies if internal URLs should be used.</param>
        /// <param name="microversion">Specifies the microversion to send with each request.</param>
        /// <param name="microversionHeader">Specifies the header to use when setting the microversion.</param>
        public ServiceEndpoint(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl,
            string microversion, string microversionHeader)
        {
            _serviceType = serviceType;
            _authenticationProvider = authenticationProvider;
            Region = region;
            UseInternalUrl = useInternalUrl;
            _microversion = microversion;
            _microversionHeader = microversionHeader;
        }

        /// <summary>
        /// Gets the service endpoint.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Url"/> representing the service endpoint.</returns>
        public async Task<Url> GetEndpoint(CancellationToken cancellationToken)
        {
            string endpoint = await _authenticationProvider.GetEndpoint(_serviceType, Region, UseInternalUrl, cancellationToken).ConfigureAwait(false);
            return new Url(endpoint);
        }

        /// <summary>
        /// Sets the microversion header, if present
        /// </summary>
        /// <param name="request">The api request.</param>
        public void SetMicroversion(PreparedRequest request)
        {
            if (_microversion == null)
                return;

            request.WithHeader(_microversionHeader, _microversion);
        }
        
        /// <summary>
        /// Builds an authenticated request.
        /// </summary>
        /// <param name="path">The endpoint's path suffix.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<PreparedRequest> PrepareRequest(string path, CancellationToken cancellationToken)
        {
            Url endpoint = await GetEndpoint(cancellationToken).ConfigureAwait(false);
            endpoint.AppendPathSegment(path);

            return PrepareRequest(endpoint, cancellationToken);
        }

        /// <summary>
        /// Builds an authenticated request.
        /// </summary>
        /// <param name="endpoint">The API endpoint.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public PreparedRequest PrepareRequest(Url endpoint, CancellationToken cancellationToken)
        {
            PreparedRequest request = endpoint.Authenticate(_authenticationProvider);

            SetMicroversion(request);

            return request;
        }

        /// <summary>
        /// Builds a request to retrieve a resource.
        /// </summary>
        /// <param name="resourcePath">The resource path, e.g. "servers".</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">resourceId</exception>
        public async Task<PreparedRequest> PrepareGetResourceRequest(string resourcePath, CancellationToken cancellationToken)
        {
            PreparedRequest request = await PrepareRequest(resourcePath, cancellationToken);
            return request.PrepareGet(cancellationToken);
        }

        /// <summary>
        /// Builds a request to create a resource
        /// </summary>
        /// <param name="resourcePath">The resource path, e.g. "servers".</param>
        /// <param name="resource">The resource.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">resource</exception>
        public virtual async Task<PreparedRequest> PrepareCreateResourceRequest(string resourcePath, object resource, CancellationToken cancellationToken)
        {
            if(resource == null)
                throw new ArgumentNullException("resource");

            PreparedRequest request = await PrepareRequest(resourcePath, cancellationToken);
            return request.PreparePostJson(resource, cancellationToken);
        }

        /// <summary>
        /// Builds a request to retrieve a list of resources.
        /// </summary>
        /// <param name="resourcePath">The resource path, e.g. "servers".</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">resourceId</exception>
        public async Task<PreparedRequest> PrepareListResourcesRequest(string resourcePath, CancellationToken cancellationToken)
        {
            PreparedRequest request = await PrepareRequest(resourcePath, cancellationToken);
            return request.PrepareGet(cancellationToken);
        }

        /// <summary>
        /// Builds a request to update a resource.
        /// </summary>
        /// <param name="resourcePath">The resource path, e.g. "servers".</param>
        /// <param name="resource">The resource.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">resourceId or resource</exception>
        public virtual async Task<PreparedRequest> PrepareUpdateResourceRequest(string resourcePath, object resource, CancellationToken cancellationToken)
        {
            PreparedRequest request = await PrepareRequest(resourcePath, cancellationToken);
            return request.PreparePutJson(resource, cancellationToken);
        }

        /// <summary>
        /// Builds a request to delete a resource.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">serverId</exception>
        public virtual async Task<PreparedRequest> PrepareDeleteResourceRequest(string resourcePath, CancellationToken cancellationToken)
        {
            PreparedRequest request = await PrepareRequest(resourcePath, cancellationToken);
            return request
                .PrepareDelete(cancellationToken)
                .AllowHttpStatus(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Gets a page of resources.
        /// </summary>
        /// <typeparam name="TPage">The resource type.</typeparam>
        /// <param name="pageUrl">The page URL.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<TPage> GetResourcePageAsync<TPage>(Url pageUrl, CancellationToken cancellationToken)
            where TPage : IPageBuilder<TPage>
        {
            PreparedRequest request = PrepareRequest(pageUrl, cancellationToken);

            var results = await request
                .PrepareGet(cancellationToken)
                .SendAsync()
                .ReceiveJson<TPage>();

            results.SetNextPageHandler(GetResourcePageAsync<TPage>);

            return results;
        }

        /// <summary>
        /// Waits for the server to reach the specified status.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="status">The status to wait for.</param>
        /// <param name="getResource">Function which retrieves the resource.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public Task<TResource> WaitForStatusAsync<TResource, TStatus>(string resourceId, TStatus status, Func<Task<TResource>> getResource, TimeSpan? refreshDelay, TimeSpan? timeout, IProgress<bool> progress, CancellationToken cancellationToken)
            where TStatus : ResourceStatus
        {
            return WaitForStatusAsync<TResource, TStatus>(resourceId, new[] { status }, getResource, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <summary>
        /// Waits for the server to reach the specified status.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="statuses">The status to wait for.</param>
        /// <param name="getResource">Function which retrieves the resource.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task<TResource> WaitForStatusAsync<TResource, TStatus>(string resourceId, IEnumerable<TStatus> statuses, Func<Task<TResource>> getResource, TimeSpan? refreshDelay, TimeSpan? timeout, IProgress<bool> progress, CancellationToken cancellationToken)
            where TStatus : ResourceStatus
        {
            if (string.IsNullOrEmpty(resourceId))
                throw new ArgumentNullException("resourceId");

            if(statuses == null)
                throw new ArgumentNullException("statuses");

            refreshDelay = refreshDelay ?? TimeSpan.FromSeconds(5);
            timeout = timeout ?? TimeSpan.FromMinutes(5);

            using (var timeoutSource = new CancellationTokenSource(timeout.Value))
            using (var rootCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token))
            {
                while (true)
                {
                    dynamic resource = await getResource().ConfigureAwait(false);
                    if (resource.Status?.IsError == true)
                        throw new ResourceErrorException($"The resource ({resourceId}) is in an error state ({resource.Status})");

                    bool complete = statuses.Contains((TStatus)resource.Status);

                    progress?.Report(complete);

                    if (complete)
                        return resource;

                    try
                    {
                        await Task.Delay(refreshDelay.Value, rootCancellationToken.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (timeoutSource.IsCancellationRequested)
                            throw new TimeoutException($"The requested timeout of {timeout.Value.TotalSeconds} seconds has been reached while waiting for the resource ({resourceId}) to reach the {statuses} state.", ex);

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Waits for the resource to be deleted.
        /// <para>Treats a 404 NotFound exception as confirmation that it is deleted.</para>
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="deletedStatus">The deleted status for the specified resource.</param>
        /// <param name="getResource">Function which retrieves the resource.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task WaitUntilDeletedAsync<TStatus>(string resourceId, TStatus deletedStatus, Func<Task<dynamic>> getResource, TimeSpan? refreshDelay, TimeSpan? timeout, IProgress<bool> progress, CancellationToken cancellationToken)
            where TStatus : ResourceStatus
        {
            try
            {
                await WaitForStatusAsync<object, TStatus>(resourceId, deletedStatus, getResource, refreshDelay, timeout, progress, cancellationToken);
            }
            catch (FlurlHttpException httpError) when (httpError.Call.Response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                progress?.Report(true);
            }
        }

        /// <summary>
        /// Waits for the resource to be deleted.
        /// <para>Treats a 404 NotFound exception as confirmation that it is deleted.</para>
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="getResource">Function which retrieves the resource.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public async Task WaitUntilDeletedAsync<TStatus>(string resourceId, Func<Task<dynamic>> getResource, TimeSpan? refreshDelay, TimeSpan? timeout, IProgress<bool> progress, CancellationToken cancellationToken)
            where TStatus : ResourceStatus
        {
            try
            {
                await WaitForStatusAsync<object, TStatus>(resourceId, Enumerable.Empty<TStatus>(), getResource, refreshDelay, timeout, progress, cancellationToken);
            }
            catch (FlurlHttpException httpError) when (httpError.Call.Response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                progress?.Report(true);
            }
        }
    }
}