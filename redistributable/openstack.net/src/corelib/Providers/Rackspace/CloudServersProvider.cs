using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JSIStudios.SimpleRESTServices.Client;
using JSIStudios.SimpleRESTServices.Client.Json;
using net.openstack.Core.Domain;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Core.Providers;
using net.openstack.Providers.Rackspace.Objects.Request;
using net.openstack.Providers.Rackspace.Objects.Response;

namespace net.openstack.Providers.Rackspace
{
    /// <summary>
    /// <para>DEPRECATED. Use <see cref="OpenStack.Compute.v2_1.ComputeService"/> or Rackspace.CloudServers.v2.CloudServerService (from the Rackspace NuGet package).</para>
    /// <para>The Cloud Servers Provider enables simple access go the Rackspace next generation Cloud Servers powered by OpenStack.
    /// The next generation service is a fast, reliable, and scalable cloud compute solution without the risk of proprietary lock-in. 
    /// It provides the core features of the OpenStack Compute API v2 and also deploys certain extensions as permitted by the OpenStack Compute API contract. 
    /// Some of these extensions are generally available through OpenStack while others implement Rackspace-specific features 
    /// to meet customers’ expectations and for operational compatibility. The OpenStack Compute API and the Rackspace extensions are 
    /// known collectively as API v2.</para>
    /// <para />
    /// <para>Documentation URL: http://docs.rackspace.com/servers/api/v2/cs-gettingstarted/content/overview.html</para>
    /// </summary>
    /// <see cref="IComputeProvider"/>
    /// <inheritdoc />
    /// <threadsafety static="true" instance="false"/>
    [Obsolete("This will be removed in v2.0. Use OpenStack.Compute.v2_1.ComputeService or Rackspace.CloudServers.v2.CloudServerService (from the Rackspace NuGet package).")]
    public class CloudServersProvider : ProviderBase<IComputeProvider>, IComputeProvider
    {
        private readonly HttpStatusCode[] _validServerActionResponseCode = new[] { HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.NonAuthoritativeInformation, HttpStatusCode.NoContent };

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudServersProvider"/> class with
        /// no default identity or region, and the default identity provider and REST
        /// service implementation.
        /// </summary>
        public CloudServersProvider()
            : this(null, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudServersProvider"/> class with
        /// the specified default identity, no default region, and the default identity
        /// provider and REST service implementation.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        public CloudServersProvider(CloudIdentity identity)
            : this(identity, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudServersProvider"/> class with
        /// no default identity or region, the default identity provider, and the specified
        /// REST service implementation.
        /// </summary>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudServersProvider(IRestService restService)
            : this(null, null, null, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudServersProvider"/> class with
        /// no default identity or region, the specified identity provider, and the default
        /// REST service implementation.
        /// </summary>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created with no default identity.</param>
        public CloudServersProvider(IIdentityProvider identityProvider)
            : this(null, null, identityProvider, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudServersProvider"/> class with
        /// the specified default identity and identity provider, no default region, and
        /// the default REST service implementation.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="identity"/> as the default identity.</param>
        public CloudServersProvider(CloudIdentity identity, IIdentityProvider identityProvider)
            : this(identity, null, identityProvider, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudServersProvider"/> class with
        /// the specified default identity and REST service implementation, no default region,
        /// and the default identity provider.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudServersProvider(CloudIdentity identity, IRestService restService)
            : this(identity, null, null, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudServersProvider"/> class with
        /// the specified default identity, no default region, and the specified identity
        /// provider and REST service implementation.
        /// </summary>
        /// <param name="identity">An instance of a <see cref="net.openstack.Core.Domain.CloudIdentity"/> object. <remarks>If not provided, the user will be required to pass a <see cref="net.openstack.Core.Domain.CloudIdentity"/> object to each method individually.</remarks></param>
        /// <param name="identityProvider">An instance of an <see cref="IIdentityProvider"/> to override the default <see cref="CloudIdentity"/></param>
        /// <param name="restService">An instance of an <see cref="IRestService"/> to override the default <see cref="JsonRestServices"/></param>
        public CloudServersProvider(CloudIdentity identity, IIdentityProvider identityProvider, IRestService restService)
            : this(identity, null, identityProvider, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudServersProvider"/> class with
        /// the specified default identity, default region, identity provider, and REST
        /// service implementation.
        /// </summary>
        /// <param name="identity">An instance of a <see cref="net.openstack.Core.Domain.CloudIdentity"/> object. <remarks>If not provided, the user will be required to pass a <see cref="net.openstack.Core.Domain.CloudIdentity"/> object to each method individually.</remarks></param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">An instance of an <see cref="IIdentityProvider"/> to override the default <see cref="CloudIdentity"/></param>
        /// <param name="restService">An instance of an <see cref="IRestService"/> to override the default <see cref="JsonRestServices"/></param>
        public CloudServersProvider(CloudIdentity identity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService)
            : base(identity, defaultRegion, identityProvider, restService) { }

        #endregion

        #region Servers
        
        /// <inheritdoc />
        public IEnumerable<SimpleServer> ListServers(string imageId = null, string flavorId = null, string name = null, ServerState status = null, string markerId = null, int? limit = null, DateTimeOffset? changesSince = null, string region = null, CloudIdentity identity = null)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers", GetServiceEndpoint(identity, region)));

            var parameters = BuildOptionalParameterList(new Dictionary<string, string>
                {
                    {"image", imageId},
                    {"flavor", flavorId},
                    {"name", name},
                    {"status", status != null ? status.Name : null},
                    {"marker", markerId},
                    {"limit", !limit.HasValue ? null : limit.Value.ToString()},
                    {"changes-since", !changesSince.HasValue ? null : changesSince.Value.ToString("yyyy-MM-ddThh:mm:ss")}
                });

            var response = ExecuteRESTRequest<ListServersResponse>(identity, urlPath, HttpMethod.GET, queryStringParameter: parameters);

            if (response == null || response.Data == null)
                return null;

            return BuildCloudServersProviderAwareObject<SimpleServer>(response.Data.Servers, region, identity);
        }

        /// <inheritdoc />
        public IEnumerable<Server> ListServersWithDetails(string imageId = null, string flavorId = null, string name = null, ServerState status = null, string markerId = null, int? limit = null, DateTimeOffset? changesSince = null, string region = null, CloudIdentity identity = null)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/detail", GetServiceEndpoint(identity, region)));

            var parameters = BuildOptionalParameterList(new Dictionary<string, string>
                {
                    {"image", imageId},
                    {"flavor", flavorId},
                    {"name", name},
                    {"status", status != null ? status.Name : null},
                    {"marker", markerId},
                    {"limit", !limit.HasValue ? null : limit.Value.ToString()},
                    {"changes-since", !changesSince.HasValue ? null : changesSince.Value.ToString("yyyy-MM-ddThh:mm:ss")}
                });

            var response = ExecuteRESTRequest<ListServersResponse>(identity, urlPath, HttpMethod.GET, queryStringParameter: parameters);

            if (response == null || response.Data == null)
                return null;

            return BuildCloudServersProviderAwareObject<Server>(response.Data.Servers, region, identity);
        }

        /// <inheritdoc />
        public NewServer CreateServer(string cloudServerName, string imageName, string flavor, DiskConfiguration diskConfig = null, Metadata metadata = null, Personality[] personality = null, bool attachToServiceNetwork = false, bool attachToPublicNetwork = false, IEnumerable<string> networks = null, string region = null, CloudIdentity identity = null)
        {
            if (cloudServerName == null)
                throw new ArgumentNullException("cloudServerName");
            if (imageName == null)
                throw new ArgumentNullException("imageName");
            if (flavor == null)
                throw new ArgumentNullException("flavor");
            if (string.IsNullOrEmpty(cloudServerName))
                throw new ArgumentException("cloudServerName cannot be empty");
            if (string.IsNullOrEmpty(imageName))
                throw new ArgumentException("imageName cannot be empty");
            if (string.IsNullOrEmpty(flavor))
                throw new ArgumentException("flavor cannot be empty");
            if (networks != null && networks.Any(string.IsNullOrEmpty))
                throw new ArgumentException("networks cannot contain any null or empty values");
            if (diskConfig != null && diskConfig != DiskConfiguration.Auto && diskConfig != DiskConfiguration.Manual)
                throw new NotSupportedException("The specified disk configuration is not supported.");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers", GetServiceEndpoint(identity, region)));

            List<string> networksToAttach = new List<string>();
            if (attachToServiceNetwork || attachToPublicNetwork)
            {
                if(attachToPublicNetwork)
                    networksToAttach.Add("00000000-0000-0000-0000-000000000000");

                if(attachToServiceNetwork)
                    networksToAttach.Add("11111111-1111-1111-1111-111111111111");
            }

            if (networks != null)
                networksToAttach.AddRange(networks);

            const string accessIPv4 = null;
            const string accessIPv6 = null;
            var request = new CreateServerRequest(cloudServerName, imageName, flavor, diskConfig, metadata, accessIPv4, accessIPv6, networksToAttach, personality);
            var response = ExecuteRESTRequest<CreateServerResponse>(identity, urlPath, HttpMethod.POST, request);

            if (response == null || response.Data == null || response.Data.Server == null)
                return null;

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
                return null; // throw new ExternalServiceException(response.StatusCode, response.Status, response.RawBody);

            return BuildCloudServersProviderAwareObject<NewServer>(response.Data.Server, region, identity);
        }

        /// <inheritdoc />
        public Server GetDetails(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest<ServerDetailsResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null || response.Data.Server == null)
                return null;

            return BuildCloudServersProviderAwareObject<Server>(response.Data.Server, region, identity);
        }

        /// <inheritdoc />
        public bool UpdateServer(string serverId, string name = null, IPAddress accessIPv4 = null, IPAddress accessIPv6 = null, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (accessIPv4 != null && !IPAddress.None.Equals(accessIPv4) && accessIPv4.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("The specified value for accessIPv4 is not an IP v4 address.", "accessIPv4");
            if (accessIPv6 != null && !IPAddress.None.Equals(accessIPv6) && accessIPv6.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("The specified value for accessIPv6 is not an IP v6 address.", "accessIPv6");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}", GetServiceEndpoint(identity, region), serverId));

            var requestJson = new UpdateServerRequest(name, accessIPv4, accessIPv6);
            var response = ExecuteRESTRequest<ServerDetailsResponse>(identity, urlPath, HttpMethod.PUT, requestJson);

            if (response == null || response.Data == null || response.Data.Server == null)
                return false;

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
                return false; 

            return true;
        }

        /// <inheritdoc />
        public bool DeleteServer(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}", GetServiceEndpoint(identity, region), serverId));

            var defaultSettings = BuildDefaultRequestSettings(new [] {HttpStatusCode.NotFound});
            var response = ExecuteRESTRequest<object>(identity, urlPath, HttpMethod.DELETE, settings: defaultSettings);

            if (response == null || !_validServerActionResponseCode.Contains(response.StatusCode))
                return false; // throw new ExternalServiceException(response.StatusCode, response.Status, response.RawBody);

            return true;
        }

        /// <inheritdoc />
        public Server WaitForServerState(string serverId, ServerState expectedState, ServerState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (expectedState == null)
                throw new ArgumentNullException("expectedState");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            return WaitForServerState(serverId, new[] { expectedState }, errorStates, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, region, identity);
        }

        /// <inheritdoc />
        public Server WaitForServerState(string serverId, ServerState[] expectedStates, ServerState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (expectedStates == null)
                throw new ArgumentNullException("expectedStates");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (expectedStates.Length == 0)
                throw new ArgumentException("expectedStates cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            var serverDetails = GetDetails(serverId, region, identity);

            /*
             * The polling implementation uses triple-checked polling to work around a known bug in Cloud
             * Servers status reporting. Occasionally, for a brief period of time during an asynchronous
             * server operation, the service will return incorrect values in all of the status fields.
             * Polling multiple times allows this SDK to provide reliable wait operations even when the
             * server returns unreliable values.
             */

            Func<bool> exitCondition = () => serverDetails.TaskState == null && (expectedStates.Contains(serverDetails.Status) || errorStates.Contains(serverDetails.Status));
            int count = 0;
            int currentProgress = -1;
            int exitCount = exitCondition() ? 1 : 0;
            while (exitCount < 3 && count < refreshCount)
            {
                if (progressUpdatedCallback != null)
                {
                    if (serverDetails.Progress > currentProgress)
                    {
                        currentProgress = serverDetails.Progress;
                        progressUpdatedCallback(currentProgress);
                    }
                }

                Thread.Sleep(refreshDelay ?? TimeSpan.FromMilliseconds(2400));
                serverDetails = GetDetails(serverId, region, identity);
                count++;
                if (exitCondition())
                    exitCount++;
                else
                    exitCount = 0;
            }

            if (errorStates.Contains(serverDetails.Status))
                throw new ServerEnteredErrorStateException(serverDetails.Status);

            return BuildCloudServersProviderAwareObject<Server>(serverDetails, region, identity);
        }

        /// <inheritdoc />
        public Server WaitForServerActive(string serverId, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            return WaitForServerState(serverId, ServerState.Active, new[] { ServerState.Error, ServerState.Unknown, ServerState.Suspended }, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, region, identity);
        }

        /// <inheritdoc />
        public void WaitForServerDeleted(string serverId, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            try
            {
                WaitForServerState(serverId, ServerState.Deleted,
                                   new[] {ServerState.Error, ServerState.Unknown, ServerState.Suspended},
                                   refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, region, identity);
            }
            catch (Core.Exceptions.Response.ItemNotFoundException){} // there is the possibility that the server can be ACTIVE for one pass and then 
                                                                                   // by the next pass a 404 is returned.  This is due to the VERY limited window in which
                                                                                   // the server goes into the DELETED state before it is removed from the system.
        }

        #endregion

        #region Server Addresses

        /// <inheritdoc />
        public ServerAddresses ListAddresses(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/ips", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest<ListAddressesResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Addresses;
        }

        /// <inheritdoc />
        public IEnumerable<IPAddress> ListAddressesByNetwork(string serverId, string network, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (network == null)
                throw new ArgumentNullException("network");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(network))
                throw new ArgumentException("network cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/ips/{2}", GetServiceEndpoint(identity, region), serverId, network));

            try
            {
                var response = ExecuteRESTRequest<ServerAddresses>(identity, urlPath, HttpMethod.GET);
                if (response == null || response.Data == null)
                    return null;

                return response.Data[network];
            }
            catch (ItemNotFoundException)
            {
                // if the specified server and network exist separately, then the 404 was only caused by server
                // not being connected to the particular network
                // https://github.com/openstacknetsdk/openstack.net/issues/176
                bool foundServer = false;
                try
                {
                    Server details = GetDetails(serverId);
                    foundServer = details != null;
                }
                catch (ResponseException)
                {
                }

                if (!foundServer)
                    throw;

                bool foundNetwork = false;
                try
                {
                    INetworksProvider networksProvider = new CloudNetworksProvider(DefaultIdentity, DefaultRegion, IdentityProvider, RestService);
                    IEnumerable<CloudNetwork> networks = networksProvider.ListNetworks(region, identity);
                    if (networks != null && networks.Any(i => network.Equals(i.Label, StringComparison.OrdinalIgnoreCase)))
                        foundNetwork = true;
                }
                catch (ResponseException)
                {
                }

                if (!foundNetwork)
                    throw;

                return Enumerable.Empty<IPAddress>();
            }
        }

        #endregion

        #region Server Actions

        /// <inheritdoc />
        public bool ChangeAdministratorPassword(string serverId, string password, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (password == null)
                throw new ArgumentNullException("password");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password cannot be empty");
            CheckIdentity(identity);

            var request = new ChangeServerAdminPasswordRequest(password);
            var resp = ExecuteServerAction(serverId, request, region, identity);

            return resp;
        }

        /// <inheritdoc />
        public bool RebootServer(string serverId, RebootType rebootType, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (rebootType == null)
                throw new ArgumentNullException("rebootType");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var request = new ServerRebootRequest(new ServerRebootDetails(rebootType));
            var resp = ExecuteServerAction(serverId, request, region, identity);

            return resp;
        }

        /// <inheritdoc />
        public Server RebuildServer(string serverId, string serverName, string imageName, string flavor, string adminPassword, IPAddress accessIPv4 = null, IPAddress accessIPv6 = null, Metadata metadata = null, DiskConfiguration diskConfig = null, Personality personality = null, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (imageName == null)
                throw new ArgumentNullException("imageName");
            if (flavor == null)
                throw new ArgumentNullException("flavor");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(imageName))
                throw new ArgumentException("imageName cannot be empty");
            if (string.IsNullOrEmpty(flavor))
                throw new ArgumentException("flavor cannot be empty");
            if (accessIPv4 != null && !IPAddress.None.Equals(accessIPv4) && accessIPv4.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("The specified value for accessIPv4 is not an IP v4 address.", "accessIPv4");
            if (accessIPv6 != null && !IPAddress.None.Equals(accessIPv6) && accessIPv6.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("The specified value for accessIPv6 is not an IP v6 address.", "accessIPv6");
            if (diskConfig != null && diskConfig != DiskConfiguration.Auto && diskConfig != DiskConfiguration.Manual)
                throw new NotSupportedException("The specified disk configuration is not supported.");
            CheckIdentity(identity);

            var details = new ServerRebuildDetails(serverName, imageName, flavor, adminPassword, accessIPv4, accessIPv6, metadata, diskConfig, personality);
            var request = new ServerRebuildRequest(details);
            var resp = ExecuteServerAction<ServerDetailsResponse>(serverId, request, region, identity);

            return BuildCloudServersProviderAwareObject<Server>(resp.Server, region, identity);
        }

        /// <inheritdoc />
        public bool ResizeServer(string serverId, string serverName, string flavor, DiskConfiguration diskConfig = null, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (serverName == null)
                throw new ArgumentNullException("serverName");
            if (flavor == null)
                throw new ArgumentNullException("flavor");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(serverName))
                throw new ArgumentException("serverName cannot be empty");
            if (string.IsNullOrEmpty(flavor))
                throw new ArgumentException("flavor cannot be empty");
            if (diskConfig != null && diskConfig != DiskConfiguration.Auto && diskConfig != DiskConfiguration.Manual)
                throw new NotSupportedException("The specified disk configuration is not supported.");
            CheckIdentity(identity);

            var details = new ServerResizeDetails(serverName, flavor, diskConfig);
            var request = new ServerResizeRequest(details);
            var resp = ExecuteServerAction(serverId, request, region, identity);

            return resp;
        }

        /// <inheritdoc />
        public bool ConfirmServerResize(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var request = new ConfirmServerResizeRequest();
            var resp = ExecuteServerAction(serverId, request, region, identity);

            return resp;
        }

        /// <inheritdoc />
        public bool RevertServerResize(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var request = new RevertServerResizeRequest();
            var resp = ExecuteServerAction(serverId, request, region, identity);

            return resp;
        }

        /// <inheritdoc />
        public string RescueServer(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var request = new RescueServerRequest();
            var resp = ExecuteServerAction<RescueServerResponse>(serverId, request, region, identity);

            return resp.AdminPassword;
        }

        /// <inheritdoc />
        public bool UnRescueServer(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var request = new UnrescueServerRequest();
            var resp = ExecuteServerAction(serverId, request, region, identity);

            return resp;
        }

        /// <inheritdoc />
        public bool CreateImage(string serverId, string imageName, Metadata metadata = null, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (imageName == null)
                throw new ArgumentNullException("imageName");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(imageName))
                throw new ArgumentException("imageName cannot be empty");
            CheckIdentity(identity);

            var request = new CreateServerImageRequest(new CreateServerImageDetails(imageName, metadata));
            var resp = ExecuteServerAction(serverId, request, region, identity);

            return resp;
        }

        /// <summary>
        /// Execute a Cloud Servers action which returns a strongly-typed value in the body of the response.
        /// </summary>
        /// <remarks>
        /// This method executes actions using a <see cref="HttpMethod.POST"/> request to the URI
        /// <strong>servers/{serverId}/action</strong>.
        /// </remarks>
        /// <typeparam name="T">The type modeling the JSON representation of the result of executing the action.</typeparam>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id"/>.</param>
        /// <param name="body">The body of the action.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>The result of the web request, as an object of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        protected T ExecuteServerAction<T>(string serverId, object body, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (body == null)
                throw new ArgumentNullException("body");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/action", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest<T>(identity, urlPath, HttpMethod.POST, body);

            if (response == null || response.Data == null || !_validServerActionResponseCode.Contains(response.StatusCode))
                return default(T);

            return response.Data;
        }

        /// <summary>
        /// Execute a Cloud Servers action which does not return a response.
        /// </summary>
        /// <remarks>
        /// This method executes actions using a <see cref="HttpMethod.POST"/> request to the URI
        /// <strong>servers/{serverId}/action</strong>.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id"/>.</param>
        /// <param name="body">The body of the action.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the <see cref="HttpMethod.POST"/> request is executed successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        protected bool ExecuteServerAction(string serverId, object body, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (body == null)
                throw new ArgumentNullException("body");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/action", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, body);

            if (response == null || !_validServerActionResponseCode.Contains(response.StatusCode))
                return false;

            return true;
        }

        #endregion

        #region Volume Attachment Actions

        /// <inheritdoc />
        public ServerVolume AttachServerVolume(string serverId, string volumeId, string storageDevice = null, string region = null,
                                               CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/os-volume_attachments", GetServiceEndpoint(identity, region), serverId));

            var request = new AttachServerVolumeRequest(storageDevice, volumeId);
            var response = ExecuteRESTRequest<ServerVolumeResponse>(identity, urlPath, HttpMethod.POST, request);

            if (response == null || response.Data == null)
                return null;

            return response.Data.ServerVolume;
        }

        /// <inheritdoc />
        public IEnumerable<ServerVolume> ListServerVolumes(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/os-volume_attachments", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest<ServerVolumeListResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.ServerVolumes;
        }

        /// <inheritdoc />
        public ServerVolume GetServerVolumeDetails(string serverId, string volumeId, string region = null,
                                                   CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/os-volume_attachments/{2}", GetServiceEndpoint(identity, region), serverId, volumeId));

            var response = ExecuteRESTRequest<ServerVolumeResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.ServerVolume;
        }

        /// <inheritdoc />
        public bool DetachServerVolume(string serverId, string volumeId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/os-volume_attachments/{2}", GetServiceEndpoint(identity, region), serverId, volumeId));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE);

            if (response == null || !_validServerActionResponseCode.Contains(response.StatusCode))
                return false;

            return true;
        }

        #endregion

        #region Virtual Interfaces

        /// <inheritdoc />
        public IEnumerable<VirtualInterface> ListVirtualInterfaces(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/os-virtual-interfacesv2", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest<ListVirtualInterfacesResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.VirtualInterfaces;
        }

        /// <inheritdoc />
        public VirtualInterface CreateVirtualInterface(string serverId, string networkId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (networkId == null)
                throw new ArgumentNullException("networkId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(networkId))
                throw new ArgumentException("networkId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/os-virtual-interfacesv2", GetServiceEndpoint(identity, region), serverId));

            var request = new CreateVirtualInterfaceRequest(networkId);
            var response = ExecuteRESTRequest<ListVirtualInterfacesResponse>(identity, urlPath, HttpMethod.POST, request);

            if (response == null || response.Data == null || response.Data.VirtualInterfaces == null)
                return null;

            return response.Data.VirtualInterfaces.FirstOrDefault();
        }

        /// <inheritdoc />
        public bool DeleteVirtualInterface(string serverId, string virtualInterfaceId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (virtualInterfaceId == null)
                throw new ArgumentNullException("virtualInterfaceId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(virtualInterfaceId))
                throw new ArgumentException("virtualInterfaceId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/os-virtual-interfacesv2/{2}", GetServiceEndpoint(identity, region), serverId, virtualInterfaceId));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE);

            if (response == null || !_validServerActionResponseCode.Contains(response.StatusCode))
                return false;

            return true;
        }
        #endregion

        #region Flavors

        /// <inheritdoc />
        public IEnumerable<Flavor> ListFlavors(int? minDiskInGB = null, int? minRamInMB = null, string markerId = null, int? limit = null, string region = null, CloudIdentity identity = null)
        {
            if (minDiskInGB < 0)
                throw new ArgumentOutOfRangeException("minDiskInGB");
            if (minRamInMB < 0)
                throw new ArgumentOutOfRangeException("minRamInMB");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/flavors", GetServiceEndpoint(identity, region)));

            var queryStringParameters = BuildListFlavorsQueryStringParameters(minDiskInGB, minRamInMB, markerId, limit);

            var response = ExecuteRESTRequest<ListFlavorsResponse>(identity, urlPath, HttpMethod.GET, queryStringParameter: queryStringParameters);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Flavors;
        }

        /// <inheritdoc />
        public IEnumerable<FlavorDetails> ListFlavorsWithDetails(int? minDiskInGB = null, int? minRamInMB = null, string markerId = null, int? limit = null, string region = null, CloudIdentity identity = null)
        {
            if (minDiskInGB < 0)
                throw new ArgumentOutOfRangeException("minDiskInGB");
            if (minRamInMB < 0)
                throw new ArgumentOutOfRangeException("minRamInMB");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/flavors/detail", GetServiceEndpoint(identity, region)));

            var queryStringParameters = BuildListFlavorsQueryStringParameters(minDiskInGB, minRamInMB, markerId, limit);

            var response = ExecuteRESTRequest<ListFlavorDetailsResponse>(identity, urlPath, HttpMethod.GET, queryStringParameter: queryStringParameters);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Flavors;
        }

        /// <inheritdoc />
        public FlavorDetails GetFlavor(string id, string region = null, CloudIdentity identity = null)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/flavors/{1}", GetServiceEndpoint(identity, region), id));

            var response = ExecuteRESTRequest<FlavorDetailsResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Flavor;
        }

        #endregion

        #region Images

        /// <inheritdoc />
        public IEnumerable<SimpleServerImage> ListImages(string server = null, string imageName = null, ImageState imageStatus = null, DateTimeOffset? changesSince = null, string markerId = null, int? limit = null, ImageType imageType = null, string region = null, CloudIdentity identity = null)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images", GetServiceEndpoint(identity, region)));

            var queryStringParameters = BuildListImagesQueryStringParameters(server, imageName, imageStatus, changesSince, markerId, limit, imageType);

            var response = ExecuteRESTRequest<ListImagesResponse>(identity, urlPath, HttpMethod.GET, queryStringParameter: queryStringParameters);

            if (response == null || response.Data == null)
                return null;

            return BuildCloudServersProviderAwareObject<SimpleServerImage>(response.Data.Images, region, identity);
        }

        /// <inheritdoc />
        public IEnumerable<ServerImage> ListImagesWithDetails(string server = null, string imageName = null, ImageState imageStatus = null, DateTimeOffset? changesSince = null, string markerId = null, int? limit = null, ImageType imageType = null, string region = null, CloudIdentity identity = null)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/detail", GetServiceEndpoint(identity, region)));

            var queryStringParameters = BuildListImagesQueryStringParameters(server, imageName, imageStatus, changesSince, markerId, limit, imageType);

            var response = ExecuteRESTRequest<ListImagesDetailsResponse>(identity, urlPath, HttpMethod.GET, queryStringParameter: queryStringParameters);

            if (response == null || response.Data == null)
                return null;

            return BuildCloudServersProviderAwareObject<ServerImage>(response.Data.Images, region, identity);
        }

        private Dictionary<string, string> BuildListImagesQueryStringParameters(string serverId, string imageName, ImageState imageStatus, DateTimeOffset? changesSince, string markerId, int? limit, ImageType imageType)
        {
            var queryParameters = new Dictionary<string, string>();

            if(!string.IsNullOrEmpty(serverId))
                queryParameters.Add("server", serverId);

            if (!string.IsNullOrEmpty(imageName))
                queryParameters.Add("name", imageName);

            if (imageStatus != null && !string.IsNullOrEmpty(imageStatus.Name))
                queryParameters.Add("status", imageStatus.Name);

            if (changesSince != null)
                queryParameters.Add("changes-since", changesSince.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));

            if (!string.IsNullOrEmpty(markerId))
                queryParameters.Add("marker", markerId);

            if (limit > 0)
                queryParameters.Add("limit", limit.ToString());

            if(imageType != null)
                queryParameters.Add("type", imageType.Name);

            return queryParameters;
        }

        private Dictionary<string, string> BuildListFlavorsQueryStringParameters(int? minDiskInGB, int? minRamInMB, string markerId, int? limit)
        {
            var queryParameters = new Dictionary<string, string>();
            if (minDiskInGB != null)
                queryParameters.Add("minDisk", minDiskInGB.ToString());
            if (minRamInMB != null)
                queryParameters.Add("minRam", minRamInMB.ToString());
            if (!string.IsNullOrEmpty(markerId))
                queryParameters.Add("marker", markerId);
            if (limit != null)
                queryParameters.Add("limit", limit.ToString());

            return queryParameters;
        }

        /// <inheritdoc />
        public ServerImage GetImage(string imageId, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/{1}", GetServiceEndpoint(identity, region), imageId));

            var response = ExecuteRESTRequest<GetImageDetailsResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return BuildCloudServersProviderAwareObject<ServerImage>(response.Data.Image, region, identity);
        }

        /// <inheritdoc />
        public bool DeleteImage(string imageId, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/{1}", GetServiceEndpoint(identity, region), imageId));

            var defaultSettings = BuildDefaultRequestSettings(new[] { HttpStatusCode.NotFound });
            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE, settings: defaultSettings);

            if (response == null || !_validServerActionResponseCode.Contains(response.StatusCode))
                return false; // throw new ExternalServiceException(response.StatusCode, response.Status, response.RawBody);

            return true;
        }

        /// <inheritdoc />
        public ServerImage WaitForImageState(string imageId, ImageState[] expectedStates, ImageState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (expectedStates == null)
                throw new ArgumentNullException("expectedStates");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (expectedStates.Length == 0)
                throw new ArgumentException("expectedStates cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            var details = GetImage(imageId, region, identity);

            int count = 0;
            int currentProgress = -1;
            while (!expectedStates.Contains(details.Status) && !errorStates.Contains(details.Status) && count < refreshCount)
            {
                if (progressUpdatedCallback != null)
                {
                    if (details.Progress > currentProgress)
                    {
                        currentProgress = details.Progress;
                        progressUpdatedCallback(currentProgress);
                    }
                }

                Thread.Sleep(refreshDelay ?? TimeSpan.FromMilliseconds(2400));
                details = GetImage(imageId, region, identity);
                count++;
            }

            if (errorStates.Contains(details.Status))
                throw new ImageEnteredErrorStateException(details.Status);

            return BuildCloudServersProviderAwareObject<ServerImage>(details, region, identity);
        }

        /// <inheritdoc />
        public ServerImage WaitForImageState(string imageId, ImageState expectedState, ImageState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (expectedState == null)
                throw new ArgumentNullException("expectedState");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            return WaitForImageState(imageId, new[] { expectedState }, errorStates, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, region, identity);
        }

        /// <inheritdoc />
        public ServerImage WaitForImageActive(string imageId, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            return WaitForImageState(imageId, ImageState.Active, new[] { ImageState.Error, ImageState.Unknown }, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, region, identity);
        }

        /// <inheritdoc />
        public void WaitForImageDeleted(string imageId, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");
            CheckIdentity(identity);

            try
            {
                WaitForImageState(imageId, ImageState.Deleted,
                                  new[] {ImageState.Error, ImageState.Unknown},
                                  refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, region, identity);
            }
            catch (net.openstack.Core.Exceptions.Response.ItemNotFoundException){} // there is the possibility that the image can be ACTIVE for one pass and then 
                                                                                   // by the next pass a 404 is returned.  This is due to the VERY limited window in which
                                                                                   // the image goes into the DELETED state before it is removed from the system.
        }

        #endregion

        #region Server Metadata

        /// <inheritdoc />
        public Metadata ListServerMetadata(string serverId, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/metadata", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest<MetaDataResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null)
                return null;

            return response.Data.Metadata;
        }

        /// <inheritdoc />
        public bool SetServerMetadata(string serverId, Metadata metadata, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (metadata == null)
                throw new ArgumentNullException("metadata");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (metadata.Any(i => string.IsNullOrEmpty(i.Key)))
                throw new ArgumentException("metadata cannot contain any values with empty keys");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/metadata", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.PUT, new UpdateMetadataRequest(metadata));

            if (response.StatusCode == HttpStatusCode.OK)
                return true;

            return false;
        }

        /// <inheritdoc />
        public bool UpdateServerMetadata(string serverId, Metadata metadata, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (metadata == null)
                throw new ArgumentNullException("metadata");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (metadata.Any(i => string.IsNullOrEmpty(i.Key)))
                throw new ArgumentException("metadata cannot contain any values with empty keys");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/metadata", GetServiceEndpoint(identity, region), serverId));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, new UpdateMetadataRequest(metadata));

            if (response.StatusCode == HttpStatusCode.OK)
                return true;

            return false;
        }

        /// <inheritdoc />
        public string GetServerMetadataItem(string serverId, string key, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (key == null)
                throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/metadata/{2}", GetServiceEndpoint(identity, region), serverId, key));

            var response = ExecuteRESTRequest<MetadataItemResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NonAuthoritativeInformation) || response.Data == null || response.Data.Metadata == null || response.Data.Metadata.Count == 0)
                return null;

            return response.Data.Metadata[key];
        }

        /// <inheritdoc />
        public bool SetServerMetadataItem(string serverId, string key, string value, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/metadata/{2}", GetServiceEndpoint(identity, region), serverId, key));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.PUT, new UpdateMetadataItemRequest(key, value));

            if (response.StatusCode == HttpStatusCode.OK)
                return true;

            return false;
        }

        /// <inheritdoc />
        public bool DeleteServerMetadataItem(string serverId, string key, string region = null, CloudIdentity identity = null)
        {
            if (serverId == null)
                throw new ArgumentNullException("serverId");
            if (key == null)
                throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("serverId cannot be empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/servers/{1}/metadata/{2}", GetServiceEndpoint(identity, region), serverId, key));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE);

            if (response.StatusCode == HttpStatusCode.NoContent)
                return true;

            return false;
        }

        #endregion

        #region Image Metadata

        /// <inheritdoc />
        public Metadata ListImageMetadata(string imageId, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/{1}/metadata", GetServiceEndpoint(identity, region), imageId));

            var response = ExecuteRESTRequest<MetaDataResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null)
                return null;

            return response.Data.Metadata;
        }

        /// <inheritdoc />
        public bool SetImageMetadata(string imageId, Metadata metadata, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (metadata == null)
                throw new ArgumentNullException("metadata");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (metadata.Any(i => string.IsNullOrEmpty(i.Key)))
                throw new ArgumentException("metadata cannot contain any values with empty keys");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/{1}/metadata", GetServiceEndpoint(identity, region), imageId));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.PUT, new UpdateMetadataRequest(metadata));

            if (response.StatusCode == HttpStatusCode.OK)
                return true;

            return false;
        }

        /// <inheritdoc />
        public bool UpdateImageMetadata(string imageId, Metadata metadata, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (metadata == null)
                throw new ArgumentNullException("metadata");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (metadata.Any(i => string.IsNullOrEmpty(i.Key)))
                throw new ArgumentException("metadata cannot contain any values with empty keys");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/{1}/metadata", GetServiceEndpoint(identity, region), imageId));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, new UpdateMetadataRequest(metadata));

            if (response.StatusCode == HttpStatusCode.OK)
                return true;

            return false;
        }

        /// <inheritdoc />
        public string GetImageMetadataItem(string imageId, string key, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (key == null)
                throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/{1}/metadata/{2}", GetServiceEndpoint(identity, region), imageId, key));

            var response = ExecuteRESTRequest<MetadataItemResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null || response.Data.Metadata == null || response.Data.Metadata.Count == 0)
                return null;

            return response.Data.Metadata[key];
        }

        /// <inheritdoc />
        public bool SetImageMetadataItem(string imageId, string key, string value, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/{1}/metadata/{2}", GetServiceEndpoint(identity, region), imageId, key));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.PUT, new UpdateMetadataItemRequest(key, value));

            if (response.StatusCode == HttpStatusCode.OK)
                return true;

            return false;
        }

        /// <inheritdoc />
        public bool DeleteImageMetadataItem(string imageId, string key, string region = null, CloudIdentity identity = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (key == null)
                throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/images/{1}/metadata/{2}", GetServiceEndpoint(identity, region), imageId, key));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE);

            if (response.StatusCode == HttpStatusCode.NoContent)
                return true;

            return false;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Gets the public service endpoint to use for Cloud Servers requests for the specified identity and region.
        /// </summary>
        /// <remarks>
        /// This method uses <c>compute</c> for the service type, and <c>cloudServersOpenStack</c> for the preferred service name.
        /// </remarks>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <param name="region">The preferred region for the service. If this value is <see langword="null"/>, the user's default region will be used.</param>
        /// <returns>The public URL for the requested Cloud Servers endpoint.</returns>
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
            return base.GetPublicServiceEndpoint(identity, "compute", "cloudServersOpenStack", region);
        }

        #endregion
    }
}
