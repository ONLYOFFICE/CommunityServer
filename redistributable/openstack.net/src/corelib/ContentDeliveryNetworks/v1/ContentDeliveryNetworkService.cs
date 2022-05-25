using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Extensions;
using Flurl.Http;
using Marvin.JsonPatch;
using OpenStack.Authentication;
using OpenStack.Serialization;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// The default provider for the OpenStack (Poppy) Content Delivery Network Service.
    /// </summary>
    /// <preliminary/>
    /// <seealso href="http://docs.cloudcdn.apiary.io/">OpenStack (Poppy) Content Delivery Network Service API v1 Reference</seealso>
    public class ContentDeliveryNetworkService : IContentDeliveryNetworkService
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly ServiceEndpoint _endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentDeliveryNetworkService"/> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The cloud region.</param>
        /// <param name="useInternalUrl">if set to <c>true</c> uses the internal URLs specified in the ServiceCatalog, otherwise the public URLs are used.</param>
        /// <exception cref="ArgumentNullException">If the <paramref name="authenticationProvider"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If the <paramref name="region"/> is <see langword="null"/> or empty.</exception>
        public ContentDeliveryNetworkService(IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl = false)
        {
            if (authenticationProvider == null)
                throw new ArgumentNullException("authenticationProvider");
            if (string.IsNullOrEmpty(region))
                throw new ArgumentException("region cannot be null or empty", "region");

            _authenticationProvider = authenticationProvider;
            _endpoint = new ServiceEndpoint(ServiceType.ContentDeliveryNetwork, authenticationProvider, region, useInternalUrl);
        }

        /// <inheritdoc />
        public async Task<Flavor> GetFlavorAsync(string flavorId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(flavorId))
                throw new ArgumentNullException("flavorId");

            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return await endpoint
                .AppendPathSegments("flavors", flavorId)
                .Authenticate(_authenticationProvider)
                .GetJsonAsync<Flavor>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Flavor>> ListFlavorsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return await endpoint
                .AppendPathSegments("flavors")
                .Authenticate(_authenticationProvider)
                .GetJsonAsync<FlavorCollection>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task PingAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            await endpoint
                .AppendPathSegments("ping")
                .Authenticate(_authenticationProvider)
                .GetAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IPage<Service>> ListServicesAsync(string startServiceId = null, int? pageSize = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);
            string url = endpoint
                .AppendPathSegments("services")
                .SetQueryParams(new
                {
                    marker = startServiceId,
                    limit = pageSize
                });

            return await ListServicesAsync(url, cancellationToken).ConfigureAwait(false);
        }

        // TODO: move into a class that we can use via composition so that we aren't implementing this for everything that is paged?
        private async Task<ServiceCollection> ListServicesAsync(Url url, CancellationToken cancellationToken)
        {
            ServiceCollection result = await url
                .Authenticate(_authenticationProvider)
                .GetJsonAsync<ServiceCollection>(cancellationToken)
                .ConfigureAwait(false);

            ((IPageBuilder<ServiceCollection>)result).SetNextPageHandler(ListServicesAsync);

            return result;
        }

        /// <inheritdoc />
        public async Task<Service> GetServiceAsync(string serviceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(serviceId))
                throw new ArgumentNullException("serviceId");

            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return await endpoint
                .AppendPathSegments("services", serviceId)
                .Authenticate(_authenticationProvider)
                .GetJsonAsync<Service>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<string> CreateServiceAsync(ServiceDefinition service, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (service == null)
                throw new ArgumentNullException("service");

            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            var response = await endpoint
                .AppendPathSegments("services")
                .Authenticate(_authenticationProvider)
                .PostJsonAsync(service, cancellationToken)
                .ConfigureAwait(false);

            response.Headers.TryGetFirst("Location", out var location);
            return new Uri(location).Segments.Last();
        }

        /// <inheritdoc />
        public async Task DeleteServiceAsync(string serviceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(serviceId))
                throw new ArgumentNullException("serviceId");

            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            await endpoint
                .AppendPathSegments("services", serviceId)
                .Authenticate(_authenticationProvider)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .DeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateServiceAsync(string serviceId, JsonPatchDocument<ServiceDefinition> patch, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(serviceId))
                throw new ArgumentNullException("serviceId");
            if (patch == null)
                throw new ArgumentNullException("patch");

            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            await endpoint
                .AppendPathSegments("services", serviceId)
                .Authenticate(_authenticationProvider)
                .PatchJsonAsync(patch, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task PurgeCachedAssetAsync(string serviceId, string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(serviceId))
                throw new ArgumentNullException("serviceId");
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            await endpoint
                .AppendPathSegments("services", serviceId, "assets")
                .SetQueryParam("url", url)
                .Authenticate(_authenticationProvider)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .DeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task PurgeCachedAssetsAsync(string serviceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(serviceId))
                throw new ArgumentNullException("serviceId");

            string endpoint = await _endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            await endpoint
                .AppendPathSegments("services", serviceId, "assets")
                .SetQueryParam("all", true)
                .Authenticate(_authenticationProvider)
                .DeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Service> WaitForServiceDeployedAsync(string serviceId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(serviceId))
                throw new ArgumentNullException("serviceId");

            refreshDelay = refreshDelay ?? TimeSpan.FromSeconds(5);
            timeout = timeout ?? TimeSpan.FromMinutes(5);

            using (var timeoutSource = new CancellationTokenSource(timeout.Value))
            using (var rootCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token))
            {
                while (true)
                {
                    var service = await GetServiceAsync(serviceId, cancellationToken).ConfigureAwait(false);
                    if (service.Status == ServiceStatus.Failed)
                        throw new ServiceOperationFailedException(service.Errors);

                    bool complete = service.Status == ServiceStatus.Deployed;
                    
                    if (progress != null)
                        progress.Report(complete);

                    if (complete)
                        return service;

                    try
                    {
                        await Task.Delay(refreshDelay.Value, rootCancellationToken.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (timeoutSource.IsCancellationRequested)
                            throw new TimeoutException(string.Format("The requested timeout of {0} seconds has been reached while waiting for the service ({1}) to be deployed.", timeout.Value.TotalSeconds, serviceId), ex);

                        throw;
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task WaitForServiceDeletedAsync(string serviceId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(serviceId))
                throw new ArgumentNullException("serviceId");

            refreshDelay = refreshDelay ?? TimeSpan.FromSeconds(5);
            timeout = timeout ?? TimeSpan.FromMinutes(5);

            using (var timeoutSource = new CancellationTokenSource(timeout.Value))
            using (var rootCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token))
            {
                while (true)
                {
                    bool complete = false;
                    try
                    {
                        var service = await GetServiceAsync(serviceId, cancellationToken).ConfigureAwait(false);
                        if (service.Status == ServiceStatus.Failed)
                            throw new ServiceOperationFailedException(service.Errors);
                    }
                    catch (FlurlHttpException httpError)
                    {
                        if (httpError.Call.Response.StatusCode == (int)HttpStatusCode.NotFound)
                            complete = true;
                        else
                            throw;
                    }

                    if (progress != null)
                        progress.Report(complete);

                    if (complete)
                        return;

                    try
                    {
                        await Task.Delay(refreshDelay.Value, rootCancellationToken.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (timeoutSource.IsCancellationRequested)
                            throw new TimeoutException(string.Format("The requested timeout of {0} seconds has been reached while waiting for the service ({1}) to be deleted.", timeout.Value.TotalSeconds, serviceId), ex);

                        throw;
                    }
                }
            }
        }
    }
}
