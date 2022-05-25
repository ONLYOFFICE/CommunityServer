using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using JSIStudios.SimpleRESTServices.Client;
using JSIStudios.SimpleRESTServices.Client.Json;
using net.openstack.Core.Domain;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Core.Providers;
using net.openstack.Core.Validators;
using net.openstack.Providers.Rackspace.Objects.Request;
using net.openstack.Providers.Rackspace.Objects.Response;
using net.openstack.Providers.Rackspace.Validators;

namespace net.openstack.Providers.Rackspace
{
    /// <summary>
    /// Provides an implementation of <see cref="IBlockStorageProvider"/>
    /// for operating with Rackspace's Cloud Block Storage product.
    /// </summary>
    /// <remarks>
    /// <para>The Cloud Block Storage Provider enables simple access to the Rackspace Cloud Block Storage Volumes as well as Cloud Block Storage Volume Snapshot services.
    /// Rackspace Cloud Block Storage is a block level storage solution that allows customers to mount drives or volumes to their Rackspace Next Generation Cloud Servers.
    /// The two primary use cases are (1) to allow customers to scale their storage independently from their compute resources,
    /// and (2) to allow customers to utilize high performance storage to serve database or I/O-intensive applications.</para>
    ///
    /// <para>Highlights of Rackspace Cloud Block Storage include:</para>
    /// <list type="bullet">
    /// <item>Mount a drive to a Cloud Server to scale storage without paying for more compute capability.</item>
    /// <item>A high performance option for databases and high performance applications, leveraging solid state drives for speed.</item>
    /// <item>A standard speed option for customers who just need additional storage on their Cloud Server.</item>
    /// </list>
    ///
    /// <note>
    /// <list type="bullet">
    /// <item>Cloud Block Storage is an add-on feature to Next Generation Cloud Servers.  Customers may not attach Cloud Block Storage volumes to other instances, like first generation Cloud Servers.</item>
    /// <item>Cloud Block Storage is multi-tenant rather than dedicated.</item>
    /// <item>When volumes are destroyed, Rackspace keeps that disk space unavailable until zeros have been written to the space to ensure that data is not accessible by any other customers.</item>
    /// <item>Cloud Block Storage allows you to create snapshots that you can save, list, and restore.</item>
    /// </list>
    /// </note>
    /// </remarks>
    /// <see cref="IBlockStorageProvider"/>
    /// <inheritdoc />
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/">OpenStack Block Storage Service API v2 Reference</seealso>
    /// <seealso href="http://docs.rackspace.com/cbs/api/v1.0/cbs-devguide/content/overview.html">Rackspace Cloud Block Storage Developer Guide - API v1.0</seealso>
    /// <threadsafety static="true" instance="false"/>
    public class CloudBlockStorageProvider : ProviderBase<IBlockStorageProvider>, IBlockStorageProvider
    {
        /// <summary>
        /// The HTTP response codes indicating a successful result from an API call.
        /// </summary>
        private readonly HttpStatusCode[] _validResponseCode = new[] { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted };

        /// <summary>
        /// The <see cref="IBlockStorageValidator"/> to use for validating requests to
        /// this service. The default value is <see cref="CloudBlockStorageValidator.Default"/>.
        /// </summary>
        private readonly IBlockStorageValidator _cloudBlockStorageValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// no default identity or region, and the default identity provider and REST
        /// service implementation.
        /// </summary>
        public CloudBlockStorageProvider()
            : this(null, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// the specified default identity, no default region, and the default identity
        /// provider and REST service implementation.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        public CloudBlockStorageProvider(CloudIdentity identity)
            : this(identity, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// no default identity or region, the default identity provider, and the specified
        /// REST service implementation.
        /// </summary>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudBlockStorageProvider(IRestService restService)
            : this(null, null, null, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// no default identity or region, the specified identity provider, and the default
        /// REST service implementation.
        /// </summary>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created with no default identity.</param>
        public CloudBlockStorageProvider(IIdentityProvider identityProvider)
            : this(null, null, identityProvider, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// the specified default identity and identity provider, no default region, and
        /// the default REST service implementation.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="identity"/> as the default identity.</param>
        public CloudBlockStorageProvider(CloudIdentity identity, IIdentityProvider identityProvider)
            : this(identity, null, identityProvider, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// the specified default identity and REST service implementation, no default region,
        /// and the default identity provider.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudBlockStorageProvider(CloudIdentity identity, IRestService restService)
            : this(identity, null, null, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// the specified default identity, no default region, and the specified identity
        /// provider and REST service implementation.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="identity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudBlockStorageProvider(CloudIdentity identity, IIdentityProvider identityProvider, IRestService restService)
            : this(identity, null, identityProvider, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// the specified default identity, default region, identity provider, and REST service implementation.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="identity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudBlockStorageProvider(CloudIdentity identity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService)
            : this(identity, defaultRegion, identityProvider, restService, CloudBlockStorageValidator.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlockStorageProvider"/> class with
        /// the specified default identity, default region, identity provider, REST service
        /// implementation, and block storage validator.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created with no default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="cloudBlockStorageValidator">The <see cref="IBlockStorageValidator"/> to use for validating requests to this service.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="cloudBlockStorageValidator"/> is <see langword="null"/>.</exception>
        internal CloudBlockStorageProvider(CloudIdentity identity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService, IBlockStorageValidator cloudBlockStorageValidator)
            : base(identity, defaultRegion, identityProvider, restService)
        {
            if (cloudBlockStorageValidator == null)
                throw new ArgumentNullException("cloudBlockStorageValidator");

            _cloudBlockStorageValidator = cloudBlockStorageValidator;
        }

        #region Volumes

        /// <inheritdoc />
        public Volume CreateVolume(int size, string displayDescription = null, string displayName = null, string snapshotId = null, string volumeType = null, string region = null, CloudIdentity identity = null)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException("size");
            CheckIdentity(identity);

            _cloudBlockStorageValidator.ValidateVolumeSize(size);

            var urlPath = new Uri(string.Format("{0}/volumes", GetServiceEndpoint(identity, region)));
            var requestBody = new CreateCloudBlockStorageVolumeRequest(new CreateCloudBlockStorageVolumeDetails(size, displayDescription, displayName, snapshotId, volumeType));
            var response = ExecuteRESTRequest<GetCloudBlockStorageVolumeResponse>(identity, urlPath, HttpMethod.POST, requestBody);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Volume;
        }

        /// <inheritdoc />
        public IEnumerable<Volume> ListVolumes(string region = null, CloudIdentity identity = null)
        {
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/volumes", GetServiceEndpoint(identity, region)));
            var response = ExecuteRESTRequest<ListVolumeResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Volumes;
        }

        /// <inheritdoc />
        public Volume ShowVolume(string volumeId, string region = null, CloudIdentity identity = null)
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/volumes/{1}", GetServiceEndpoint(identity, region), volumeId));
            var response = ExecuteRESTRequest<GetCloudBlockStorageVolumeResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Volume;
        }

        /// <inheritdoc />
        public bool DeleteVolume(string volumeId, string region = null, CloudIdentity identity = null)
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/volumes/{1}", GetServiceEndpoint(identity, region), volumeId));
            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE);

            return response != null && _validResponseCode.Contains(response.StatusCode);
        }

        /// <inheritdoc />
        public IEnumerable<VolumeType> ListVolumeTypes(string region = null, CloudIdentity identity = null)
        {
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/types", GetServiceEndpoint(identity, region)));
            var response = ExecuteRESTRequest<ListVolumeTypeResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.VolumeTypes;
        }

        /// <inheritdoc />
        public VolumeType DescribeVolumeType(string volumeTypeId, string region = null, CloudIdentity identity = null)
        {
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/types/{1}", GetServiceEndpoint(identity, region), volumeTypeId));
            var response = ExecuteRESTRequest<GetCloudBlockStorageVolumeTypeResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.VolumeType;
        }

        /// <inheritdoc />
        public Volume WaitForVolumeAvailable(string volumeId, int refreshCount = 600, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null)
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            return WaitForVolumeState(volumeId, VolumeState.Available, new[] { VolumeState.Error, VolumeState.ErrorDeleting }, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), region, identity);
        }

        /// <inheritdoc />
        public bool WaitForVolumeDeleted(string volumeId, int refreshCount = 360, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null)
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            return WaitForItemToBeDeleted(ShowVolume, volumeId, refreshCount, refreshDelay ?? TimeSpan.FromSeconds(10), region, identity);
        }

        /// <inheritdoc />
        public Volume WaitForVolumeState(string volumeId, VolumeState expectedState, VolumeState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null)
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (expectedState == null)
                throw new ArgumentNullException("expectedState");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            var volumeInfo = ShowVolume(volumeId, region, identity);

            var count = 0;
            while (!volumeInfo.Status.Equals(expectedState) && !errorStates.Contains(volumeInfo.Status) && count < refreshCount)
            {
                Thread.Sleep(refreshDelay ?? TimeSpan.FromMilliseconds(2400));
                volumeInfo = ShowVolume(volumeId, region, identity);
                count++;
            }

            if (errorStates.Contains(volumeInfo.Status))
                throw new VolumeEnteredErrorStateException(volumeInfo.Status);

            return volumeInfo;
        }

        #endregion

        #region Snapshots

        /// <inheritdoc />
        public Snapshot CreateSnapshot(string volumeId, bool force = false, string displayName = "None", string displayDescription = "None", string region = null, CloudIdentity identity = null)
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/snapshots", GetServiceEndpoint(identity, region)));
            var requestBody = new CreateCloudBlockStorageSnapshotRequest(new CreateCloudBlockStorageSnapshotDetails(volumeId, force, displayName, displayDescription));
            var response = ExecuteRESTRequest<GetCloudBlockStorageSnapshotResponse>(identity, urlPath, HttpMethod.POST, requestBody);
            if (response == null || response.Data == null)
                return null;

            return response.Data.Snapshot;
        }

        /// <inheritdoc />
        public IEnumerable<Snapshot> ListSnapshots(string region = null, CloudIdentity identity = null)
        {
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/snapshots", GetServiceEndpoint(identity, region)));
            var response = ExecuteRESTRequest<ListSnapshotResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Snapshots;
        }

        /// <inheritdoc />
        public Snapshot ShowSnapshot(string snapshotId, string region = null, CloudIdentity identity = null)
        {
            if (snapshotId == null)
                throw new ArgumentNullException("snapshotId");
            if (string.IsNullOrEmpty(snapshotId))
                throw new ArgumentException("snapshotId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/snapshots/{1}", GetServiceEndpoint(identity, region), snapshotId));
            var response = ExecuteRESTRequest<GetCloudBlockStorageSnapshotResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Snapshot;
        }

        /// <inheritdoc />
        public bool DeleteSnapshot(string snapshotId, string region = null, CloudIdentity identity = null)
        {
            if (snapshotId == null)
                throw new ArgumentNullException("snapshotId");
            if (string.IsNullOrEmpty(snapshotId))
                throw new ArgumentException("snapshotId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/snapshots/{1}", GetServiceEndpoint(identity, region), snapshotId));
            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE);

            return response != null && _validResponseCode.Contains(response.StatusCode);
        }

        /// <inheritdoc />
        public Snapshot WaitForSnapshotAvailable(string snapshotId, int refreshCount = 360, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null)
        {
            if (snapshotId == null)
                throw new ArgumentNullException("snapshotId");
            if (string.IsNullOrEmpty(snapshotId))
                throw new ArgumentException("snapshotId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            return WaitForSnapshotState(snapshotId, SnapshotState.Available, new[] { SnapshotState.Error, SnapshotState.ErrorDeleting }, refreshCount, refreshDelay ?? TimeSpan.FromSeconds(10), region, identity);
        }

        /// <inheritdoc />
        public bool WaitForSnapshotDeleted(string snapshotId, int refreshCount = 180, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null)
        {
            if (snapshotId == null)
                throw new ArgumentNullException("snapshotId");
            if (string.IsNullOrEmpty(snapshotId))
                throw new ArgumentException("snapshotId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            return WaitForItemToBeDeleted(ShowSnapshot, snapshotId, refreshCount, refreshDelay ?? TimeSpan.FromSeconds(10), region, identity);
        }

        /// <inheritdoc />
        public Snapshot WaitForSnapshotState(string snapshotId, SnapshotState expectedState, SnapshotState[] errorStates, int refreshCount = 60, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null)
        {
            if (snapshotId == null)
                throw new ArgumentNullException("snapshotId");
            if (expectedState == null)
                throw new ArgumentNullException("expectedState");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (string.IsNullOrEmpty(snapshotId))
                throw new ArgumentException("snapshotId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            var snapshotInfo = ShowSnapshot(snapshotId, region, identity);

            var count = 0;
            while (!snapshotInfo.Status.Equals(expectedState) && !errorStates.Contains(snapshotInfo.Status) && count < refreshCount)
            {
                Thread.Sleep(refreshDelay ?? TimeSpan.FromSeconds(10));
                snapshotInfo = ShowSnapshot(snapshotId, region, identity);
                count++;
            }

            if (errorStates.Contains(snapshotInfo.Status))
                throw new SnapshotEnteredErrorStateException(snapshotInfo.Status);

            return snapshotInfo;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the public service endpoint to use for Cloud Block Storage requests for the specified identity and region.
        /// </summary>
        /// <remarks>
        /// This method uses <c>volume</c> for the service type, and <c>cloudBlockStorage</c> for the preferred service name.
        /// </remarks>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <param name="region">The preferred region for the service. If this value is <see langword="null"/>, the user's default region will be used.</param>
        /// <returns>The public URL for the requested Cloud Block Storage endpoint.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// </exception>
        /// <exception cref="NoDefaultRegionSetException">If <paramref name="region"/> is <see langword="null"/> and no default region is available for the identity or provider.</exception>
        /// <exception cref="UserAuthenticationException">If no service catalog is available for the user.</exception>
        /// <exception cref="UserAuthorizationException">If no endpoint is available for the requested service.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        protected string GetServiceEndpoint(CloudIdentity identity, string region)
        {
            return base.GetPublicServiceEndpoint(identity, "volume", "cloudBlockStorage", region);
        }

        private bool WaitForItemToBeDeleted<T>(Func<string, string, CloudIdentity, T> retrieveItemMethod, string id, int refreshCount = 360, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null)
        {
            try
            {
                retrieveItemMethod(id, region, identity);

                var count = 0;
                while (count < refreshCount)
                {
                    Thread.Sleep(refreshDelay ?? TimeSpan.FromSeconds(10));
                    retrieveItemMethod(id, region, identity);
                    count++;
                }
            }
            catch (ItemNotFoundException)
            {
                return true;
            }

            return false;
        }

        #endregion

        
    }
}
