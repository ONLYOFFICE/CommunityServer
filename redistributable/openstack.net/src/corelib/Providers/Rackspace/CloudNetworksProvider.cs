using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// <para>DEPRECATED. Use <see cref="OpenStack.Networking.v2.NetworkingService"/> or Rackspace.CloudNetworks.v2.CloudNetworkService (from the Rackspace NuGet package).</para>
    /// <para>The Cloud Networks Provider enable simple access to the Rackspace Cloud Network Services.
    /// Cloud Networks lets you create a virtual Layer 2 network, known as an isolated network, 
    /// which gives you greater control and security when you deploy web applications.</para>
    /// <para />
    /// <para>Documentation URL: http://docs.rackspace.com/servers/api/v2/cn-gettingstarted/content/ch_overview.html</para>
    /// </summary>
    /// <see cref="INetworksProvider"/>
    /// <inheritdoc />
    /// <threadsafety static="true" instance="false"/>
    [Obsolete("This will be removed in v2.0. Use OpenStack.Networking.v2.NetworkingService or Rackspace.CloudNetworks.v2.CloudNetworkService (from the Rackspace NuGet package).")]
    public class CloudNetworksProvider : ProviderBase<INetworksProvider>, INetworksProvider
    {
        private readonly HttpStatusCode[] _validResponseCode = new[] { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted };

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworksProvider"/> class with
        /// no default identity or region, and the default identity provider and REST
        /// service implementation.
        /// </summary>
        public CloudNetworksProvider()
            : this(null, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworksProvider"/> class with
        /// the specified default identity, no default region, and the default identity
        /// provider and REST service implementation.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        public CloudNetworksProvider(CloudIdentity identity)
            : this(identity, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworksProvider"/> class with
        /// no default identity or region, the default identity provider, and the specified
        /// REST service implementation.
        /// </summary>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudNetworksProvider(IRestService restService)
            : this(null, null, null, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworksProvider"/> class with
        /// no default identity or region, the specified identity provider, and the default
        /// REST service implementation.
        /// </summary>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created with no default identity.</param>
        public CloudNetworksProvider(IIdentityProvider identityProvider)
            : this(null, null, identityProvider, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworksProvider"/> class with
        /// the specified default identity and identity provider, no default region, and
        /// the default REST service implementation.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="identity"/> as the default identity.</param>
        public CloudNetworksProvider(CloudIdentity identity, IIdentityProvider identityProvider)
            : this(identity, null, identityProvider, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworksProvider"/> class with
        /// the specified default identity and REST service implementation, no default region,
        /// and the default identity provider.
        /// </summary>
        /// <param name="identity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudNetworksProvider(CloudIdentity identity, IRestService restService)
            : this(identity, null, null, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworksProvider"/> class with
        /// the specified default identity, no default region, and the specified identity
        /// provider and REST service implementation.
        /// </summary>
        /// <param name="identity">An instance of a <see cref="net.openstack.Core.Domain.CloudIdentity"/> object. <remarks>If not provided, the user will be required to pass a <see cref="net.openstack.Core.Domain.CloudIdentity"/> object to each method individually.</remarks></param>
        /// <param name="identityProvider">An instance of an <see cref="IIdentityProvider"/> to override the default <see cref="CloudIdentity"/></param>
        /// <param name="restService">An instance of an <see cref="IRestService"/> to override the default <see cref="JsonRestServices"/></param>
        public  CloudNetworksProvider(CloudIdentity identity, IIdentityProvider identityProvider, IRestService restService)
            : this(identity, null, identityProvider, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworksProvider"/> class with
        /// the specified default identity, default region, identity provider, and REST
        /// service implementation.
        /// </summary>
        /// <param name="identity">An instance of a <see cref="net.openstack.Core.Domain.CloudIdentity"/> object. <remarks>If not provided, the user will be required to pass a <see cref="net.openstack.Core.Domain.CloudIdentity"/> object to each method individually.</remarks></param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">An instance of an <see cref="IIdentityProvider"/> to override the default <see cref="CloudIdentity"/></param>
        /// <param name="restService">An instance of an <see cref="IRestService"/> to override the default <see cref="JsonRestServices"/></param>
        public CloudNetworksProvider(CloudIdentity identity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService)
            : base(identity, defaultRegion, identityProvider, restService) { }

        #endregion


        #region Networks

        /// <inheritdoc />
        public IEnumerable<CloudNetwork> ListNetworks(string region = null, CloudIdentity identity = null)
        {
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/os-networksv2", GetServiceEndpoint(identity, region)));
            var response = ExecuteRESTRequest<ListCloudNetworksResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Networks;
        }

        /// <inheritdoc />
        public CloudNetwork CreateNetwork(string cidr, string label, string region = null, CloudIdentity identity = null)
        {
            if (cidr == null)
                throw new ArgumentNullException("cidr");
            if (label == null)
                throw new ArgumentNullException("label");
            if (string.IsNullOrEmpty(cidr))
                throw new ArgumentException("cidr cannot be empty");
            if (string.IsNullOrEmpty(label))
                throw new ArgumentException("label cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/os-networksv2", GetServiceEndpoint(identity, region)));
            var cloudNetworkRequest = new CreateCloudNetworkRequest(new CreateCloudNetworksDetails(cidr, label));

            var response = ExecuteRESTRequest<CloudNetworkResponse>(identity, urlPath, HttpMethod.POST, cloudNetworkRequest);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Network;
        }

        /// <inheritdoc />
        public CloudNetwork ShowNetwork(string networkId, string region = null, CloudIdentity identity = null)
        {
            if (networkId == null)
                throw new ArgumentNullException("networkId");
            if (string.IsNullOrEmpty(networkId))
                throw new ArgumentException("networkId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/os-networksv2/{1}", GetServiceEndpoint(identity, region), networkId));
            var response = ExecuteRESTRequest<CloudNetworkResponse>(identity, urlPath, HttpMethod.GET);

            if (response == null || response.Data == null)
                return null;

            return response.Data.Network;
        }

        /// <inheritdoc />
        public bool DeleteNetwork(string networkId, string region = null, CloudIdentity identity = null)
        {
            if (networkId == null)
                throw new ArgumentNullException("networkId");
            if (string.IsNullOrEmpty(networkId))
                throw new ArgumentException("networkId cannot be empty");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}/os-networksv2/{1}", GetServiceEndpoint(identity, region), networkId));

            Response response = null;
            try
            {
                response = ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE);                
            } 
            catch(UserNotAuthorizedException ex)
            {
                if(ex.Response.StatusCode == HttpStatusCode.Forbidden)
                    throw new UserAuthorizationException("ERROR: Cannot delete network. Ensure that all servers are removed from this network first.");
            }

            return response != null && _validResponseCode.Contains(response.StatusCode);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Gets the public service endpoint to use for Cloud Networks requests for the specified identity and region.
        /// </summary>
        /// <remarks>
        /// This method uses <c>compute</c> for the service type, and <c>cloudServersOpenStack</c> for the preferred service name.
        /// </remarks>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <param name="region">The preferred region for the service. If this value is <see langword="null"/>, the user's default region will be used.</param>
        /// <returns>The public URL for the requested Cloud Networks endpoint.</returns>
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
