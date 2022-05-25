using System;
using System.Collections.Generic;
using System.Net;
using Flurl.Http;
using Marvin.JsonPatch;
using OpenStack.ContentDeliveryNetworks.v1;
using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for an <see cref="IContentDeliveryNetworkService"/> instance.
    /// </summary>
    public static class ContentDeliveryNetworkServiceExtensions
    {
        /// <summary>
        /// Gets the specified flavor.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="flavorId">The flavor identifier.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="flavorId"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>The flavor associated with the specified identifer.</returns>
        public static Flavor GetFlavor(this IContentDeliveryNetworkService cdnService, string flavorId)
        {
            return cdnService.GetFlavorAsync(flavorId).ForceSynchronous();
        }

        /// <summary>
        /// Lists basic information for all available flavors.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>A collection of <see cref="Flavor"/> objects describing the available flavors.</returns>
        public static IEnumerable<Flavor> ListFlavors(this IContentDeliveryNetworkService cdnService)
        {
            return cdnService.ListFlavorsAsync().ForceSynchronous();
        }

        /// <summary>
        /// Pings the service.
        /// <para>
        /// If no exception is thrown, the service is considered up/healthy.
        /// </para>
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode" />.</exception>
        public static void Ping(this IContentDeliveryNetworkService cdnService)
        {
            cdnService.PingAsync().ForceSynchronous();
        }

        /// <summary>
        /// Lists all Content Delivery Network services associated with the account
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="startServiceId">The id of the first service from which to start paging the results.</param>
        /// <param name="pageSize">The numer of services to return per page.</param>
        /// <exception cref="FlurlHttpException">If the service call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>
        /// A collection of <see cref="Service" /> associated wit the acccount./>
        /// </returns>
        public static IPage<Service> ListServices(this IContentDeliveryNetworkService cdnService, string startServiceId = null, int? pageSize = null)
        {
            return cdnService.ListServicesAsync(startServiceId, pageSize).ForceSynchronous();
        }

        /// <summary>
        /// Gets the specified service.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="serviceId">The service identifier.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        /// <returns>
        /// The service associated with the specified identifer.
        /// </returns>
        public static Service GetService(this IContentDeliveryNetworkService cdnService, string serviceId)
        {
            return cdnService.GetServiceAsync(serviceId).ForceSynchronous();
        }

        /// <summary>
        /// Creates a Content Delivery Network service.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="service">The service definition.</param>
        /// <returns>
        /// The identifier of the newly created service.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service" /> is <see langword="null" />.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode" />.</exception>
        public static string CreateService(this IContentDeliveryNetworkService cdnService, ServiceDefinition service)
        {
            return cdnService.CreateServiceAsync(service).ForceSynchronous();
        }

        /// <summary>
        /// Deletes the specified Content Delivery Network service.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="serviceId">The service identifier.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> is <see langword="null"/> or empty.</exception>
        public static void DeleteService(this IContentDeliveryNetworkService cdnService, string serviceId)
        {
            cdnService.DeleteServiceAsync(serviceId).ForceSynchronous();
        }

        /// <summary>
        /// Updates the specified Content DeliveryNetwork service
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="patch">The patch containing updated properties.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> or <paramref name="patch"/> is <see langword="null"/>.</exception>
        public static void UpdateService(this IContentDeliveryNetworkService cdnService, string serviceId, JsonPatchDocument<ServiceDefinition> patch)
        {
            cdnService.UpdateServiceAsync(serviceId, patch).ForceSynchronous();
        }

        /// <summary>
        /// Removes the current version of the specified asset that has been cached at the edge.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="url">The asset URL.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public static void PurgeCachedAsset(this IContentDeliveryNetworkService cdnService, string serviceId, string url)
        {
            cdnService.PurgeCachedAssetAsync(serviceId, url).ForceSynchronous();
        }

        /// <summary>
        /// Removes the current version of all assets that has been cached at the edge.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="serviceId">The service identifier.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public static void PurgeCachedAssets(this IContentDeliveryNetworkService cdnService, string serviceId)
        {
            cdnService.PurgeCachedAssetsAsync(serviceId).ForceSynchronous();
        }

        /// <summary>
        /// Waits for the service to be deployed.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <exception cref="ServiceOperationFailedException">If the previous service operation failed.</exception>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public static Service WaitForServiceDeployed(this IContentDeliveryNetworkService cdnService, string serviceId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            return cdnService.WaitForServiceDeployedAsync(serviceId, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <summary>
        /// Waits for the service to be deleted.
        /// </summary>
        /// <param name="cdnService">The <see cref="IContentDeliveryNetworkService"/> service instance.</param>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="refreshDelay">The amount of time to wait between requests.</param>
        /// <param name="timeout">The amount of time to wait before throwing a <see cref="TimeoutException"/>.</param>
        /// <param name="progress">The progress callback.</param>
        /// <exception cref="ServiceOperationFailedException">If the service deletion failed.</exception>
        /// <exception cref="TimeoutException">If the <paramref name="timeout"/> value is reached.</exception>
        /// <exception cref="FlurlHttpException">If the API call returns a bad <see cref="HttpStatusCode"/>.</exception>
        public static void WaitForServiceDeleted(this IContentDeliveryNetworkService cdnService, string serviceId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            cdnService.WaitForServiceDeletedAsync(serviceId, refreshDelay, timeout, progress).ForceSynchronous();
        }
    }
}
