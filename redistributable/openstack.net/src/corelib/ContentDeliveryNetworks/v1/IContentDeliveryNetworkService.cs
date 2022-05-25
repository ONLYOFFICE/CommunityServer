using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Marvin.JsonPatch;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a provider for the OpenStack (Poppy) Content Delivery Network Service.
    /// </summary>
    /// <preliminary/>
    /// <seealso href="http://docs.cloudcdn.apiary.io/">OpenStack (Poppy) Content Delivery Network Service API v1 Reference</seealso>
    public interface IContentDeliveryNetworkService
    {
        /// <summary>
        /// Gets the specified flavor.
        /// </summary>
        /// <param name="flavorId">The flavor identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="flavorId"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>
        /// The flavor associated with the specified identifer.
        /// </returns>
        Task<Flavor> GetFlavorAsync(string flavorId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists basic information for all available flavors.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="FlurlHttpException">If the service call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>
        /// A collection of <see cref="Flavor"/> objects describing the available flavors.
        /// </returns>
        Task<IEnumerable<Flavor>> ListFlavorsAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Pings the service.
        /// <para>
        /// If no exception is thrown, the service is considered up/healthy.
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        Task PingAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists all Content Delivery Network services associated with the account.
        /// </summary>
        /// <param name="startServiceId">The id of the first service from which to start paging the results.</param>
        /// <param name="pageSize">The numer of services to return per page.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="FlurlHttpException">If the service call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>
        /// A collection of <see cref="Service" /> resources associated with the acccount./>
        /// </returns>
        Task<IPage<Service>> ListServicesAsync(string startServiceId = null, int? pageSize = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets the specified Content Delivery Network service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>
        /// The service associated with the specified identifer.
        /// </returns>
        Task<Service> GetServiceAsync(string serviceId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a Content Delivery Network service.
        /// </summary>
        /// <param name="service">The service definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>
        /// The identifier of the newly created service.
        /// </returns>
        Task<string> CreateServiceAsync(ServiceDefinition service, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes the specified Content Delivery Network service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        Task DeleteServiceAsync(string serviceId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Updates the specified Content DeliveryNetwork service
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="patch">The patch containing updated properties.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> or <paramref name="patch"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        Task UpdateServiceAsync(string serviceId, JsonPatchDocument<ServiceDefinition> patch, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Removes the current version of the specified asset that has been cached at the edge.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="url">The asset URL.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        Task PurgeCachedAssetAsync(string serviceId, string url, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Removes the current version of all assets that has been cached at the edge.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        Task PurgeCachedAssetsAsync(string serviceId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Waits for the service to be deployed.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ServiceOperationFailedException">If the previous service operation failed.</exception>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        Task<Service> WaitForServiceDeployedAsync(string serviceId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Waits for the service to be deleted.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ServiceOperationFailedException">If the service deletion failed.</exception>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        Task WaitForServiceDeletedAsync(string serviceId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
