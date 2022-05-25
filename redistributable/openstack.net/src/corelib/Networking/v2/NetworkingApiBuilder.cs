using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Extensions;
using Flurl.Http;
using OpenStack.Authentication;
using OpenStack.Networking.v2.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Builds requests to the Networking API which can be further customized and then executed.
    /// <para>Intended for custom implementations.</para>
    /// </summary>
    /// <seealso href="http://developer.openstack.org/api-ref-networking-v2.html">OpenStack Networking API v2 Reference</seealso>
    public class NetworkingApiBuilder
    {
        /// <summary />
        protected readonly IAuthenticationProvider AuthenticationProvider;

        /// <summary />
        protected readonly ServiceEndpoint Endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkingApiBuilder"/> class.
        /// </summary>
        /// <param name="serviceType">The service type for the desired networking provider.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        /// <param name="useInternalUrl">if set to <c>true</c> uses the internal URLs specified in the ServiceCatalog, otherwise the public URLs are used.</param>
        public NetworkingApiBuilder(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl)
        {
            if(serviceType == null)
                throw new ArgumentNullException("serviceType");
            if (authenticationProvider == null)
                throw new ArgumentNullException("authenticationProvider");
            if (string.IsNullOrEmpty(region))
                throw new ArgumentException("region cannot be null or empty", "region");

            AuthenticationProvider = authenticationProvider;
            Endpoint = new ServiceEndpoint(serviceType, authenticationProvider, region, useInternalUrl);
        }

        #region Networks
        /// <summary>
        /// Lists all networks associated with the account.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection of network resources associated with the account.
        /// </returns>
        public async Task<PreparedRequest> ListNetworksAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);
 
            return endpoint
                .AppendPathSegments("networks")
                .Authenticate(AuthenticationProvider)
                .PrepareGet(cancellationToken);
        }
        
        /// <summary>
        /// Gets the specified network.
        /// </summary>
        /// <param name="networkId">The network identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The network associated with the specified identifier.
        /// </returns>
        public virtual async Task<PreparedRequest> GetNetworkAsync(string networkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("networks", networkId)
                .Authenticate(AuthenticationProvider)
                .PrepareGet(cancellationToken);
        }

        /// <summary>
        /// Creates a network.
        /// </summary>
        /// <param name="network">The network definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The created network.
        /// </returns>
        public virtual async Task<PreparedRequest> CreateNetworkAsync(object network, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("networks")
                .Authenticate(AuthenticationProvider)
                .PreparePostJson(network, cancellationToken);
        }

        /// <summary>
        /// Bulk creates multiple networks.
        /// </summary>
        /// <param name="networks">The network definitions.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The created networks.
        /// </returns>
        public virtual async Task<PreparedRequest> CreateNetworksAsync(IEnumerable<object> networks, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("networks")
                .Authenticate(AuthenticationProvider)
                .PreparePostJson(new NetworkDefinitionCollection(networks), cancellationToken);
        }

        /// <summary>
        /// Updates the specified network.
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="network">The updated network definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The updated network.
        /// </returns>
        public virtual async Task<PreparedRequest> UpdateNetworkAsync(string networkId, object network, CancellationToken cancellationToken = default(CancellationToken))
        {
            string endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("networks", networkId)
                .Authenticate(AuthenticationProvider)
                .PreparePutJson(network, cancellationToken);
        }

        /// <summary>
        /// Deletes the specified network.
        /// </summary>
        /// <param name="networkId">The network identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<PreparedRequest> DeleteNetworkAsync(string networkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return (PreparedRequest)endpoint
                .AppendPathSegments("networks", networkId)
                .Authenticate(AuthenticationProvider)
                .PrepareDelete(cancellationToken)
                .AllowHttpStatus(HttpStatusCode.NotFound);
        }
        #endregion

        #region Subnets

        /// <summary>
        /// Lists all subnets associated with the account.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection of subnet resources associated with the account.
        /// </returns>
        public virtual async Task<PreparedRequest> ListSubnetsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegment("subnets")
                .Authenticate(AuthenticationProvider)
                .PrepareGet(cancellationToken);
        }

        /// <summary>
        /// Creates a subnet.
        /// </summary>
        /// <param name="subnet">The subnet definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The created subnet.
        /// </returns>
        public virtual async Task<PreparedRequest> CreateSubnetAsync(object subnet, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("subnets")
                .Authenticate(AuthenticationProvider)
                .PreparePostJson(subnet, cancellationToken);
        }

        /// <summary>
        /// Bulk creates multiple subnets.
        /// </summary>
        /// <param name="subnets">The subnet definitions.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The created subnets.
        /// </returns>
        public virtual async Task<PreparedRequest> CreateSubnetsAsync(IEnumerable<object> subnets, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("subnets")
                .Authenticate(AuthenticationProvider)
                .PreparePostJson(new SubnetDefinitionCollection(subnets), cancellationToken);
        }

        /// <summary>
        /// Gets the specified subnet.
        /// </summary>
        /// <param name="subnetId">The subnet identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The subnet associated with the specified identifier.
        /// </returns>
        public virtual async Task<PreparedRequest> GetSubnetAsync(string subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("subnets", subnetId)
                .Authenticate(AuthenticationProvider)
                .PrepareGet(cancellationToken);
        }

        /// <summary>
        /// Updates the specified subnet.
        /// </summary>
        /// <param name="subnetId"></param>
        /// <param name="subnet">The updated subnet definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The updated subnet.
        /// </returns>
        public virtual async Task<PreparedRequest> UpdateSubnetAsync(string subnetId, object subnet, CancellationToken cancellationToken = default(CancellationToken))
        {
            string endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("subnets", subnetId)
                .Authenticate(AuthenticationProvider)
                .PreparePutJson(subnet, cancellationToken);
        }

        /// <summary>
        /// Deletes the specified subnet.
        /// </summary>
        /// <param name="subnetId">The subnet identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<PreparedRequest> DeleteSubnetAsync(string subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return (PreparedRequest)endpoint
                .AppendPathSegments("subnets", subnetId)
                .Authenticate(AuthenticationProvider)
                .PrepareDelete(cancellationToken)
                .AllowHttpStatus(HttpStatusCode.NotFound);
        }
        #endregion

        #region Ports
        /// <summary>
        /// Lists all ports associated with the account.
        /// </summary>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection of port resources associated with the account.
        /// </returns>
        public virtual async Task<T> ListPortsAsync<T>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListPortsRequest(queryString, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListPortsAsync{T}"/> request.
        /// </summary>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<PreparedRequest> BuildListPortsRequest(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            PreparedRequest request = await Endpoint.PrepareGetResourceRequest("ports", cancellationToken).ConfigureAwait(false);

            request.Url.SetQueryParams(queryString?.Build());

            return request;
        }

        /// <summary>
        /// Creates a port.
        /// </summary>
        /// <param name="port">The port definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The created port.
        /// </returns>
        public virtual async Task<PreparedRequest> CreatePortAsync(object port, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("ports")
                .Authenticate(AuthenticationProvider)
                .PreparePostJson(port, cancellationToken);
        }

        /// <summary>
        /// Bulk creates multiple ports.
        /// </summary>
        /// <param name="ports">The port definitions.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The created subnets.
        /// </returns>
        public virtual async Task<PreparedRequest> CreatePortsAsync(IEnumerable<object> ports, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("ports")
                .Authenticate(AuthenticationProvider)
                .PreparePostJson(new PortDefinitionCollection(ports), cancellationToken);
        }

        /// <summary>
        /// Gets the specified port.
        /// </summary>
        /// <param name="portId">The port identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The port associated with the specified identifier.
        /// </returns>
        public virtual async Task<PreparedRequest> GetPortAsync(string portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("ports", portId)
                .Authenticate(AuthenticationProvider)
                .PrepareGet(cancellationToken);
        }

        /// <summary>
        /// Updates the specified port.
        /// </summary>
        /// <param name="portId">The port identifier.</param>
        /// <param name="port">The updated port definition.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// The updated port.
        /// </returns>
        public virtual async Task<PreparedRequest> UpdatePortAsync(string portId, object port, CancellationToken cancellationToken = default(CancellationToken))
        {
            string endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return endpoint
                .AppendPathSegments("ports", portId)
                .Authenticate(AuthenticationProvider)
                .PreparePutJson(port, cancellationToken);
        }

        /// <summary>
        /// Deletes the specified port.
        /// </summary>
        /// <param name="portId">The port identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<PreparedRequest> DeletePortAsync(string portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            return (PreparedRequest)endpoint
                .AppendPathSegments("ports", portId)
                .Authenticate(AuthenticationProvider)
                .PrepareDelete(cancellationToken)
                .AllowHttpStatus(HttpStatusCode.NotFound);
        }
        #endregion

        #region Layer 3 Extension

        #region Routers
        /// <summary>
        /// Shows details for a server group.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<T> GetRouterAsync<T>(string routerId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetRouterRequest(routerId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetRouterAsync{T}"/> request.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetRouterRequest(string routerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routerId == null)
                throw new ArgumentNullException("routerId");

            return Endpoint.PrepareGetResourceRequest($"routers/{routerId}", cancellationToken);
        }

        /// <summary>
        /// Creates a router.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="router">The router.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<T> CreateRouterAsync<T>(object router, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildCreateRouterRequest(router, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="CreateRouterAsync{T}"/> request.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildCreateRouterRequest(object router, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (router == null)
                throw new ArgumentNullException("router");

            return Endpoint.PrepareCreateResourceRequest("routers", router, cancellationToken);
        }

        /// <summary>
        /// Updates a router.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="router">The router.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<T> UpdateRouterAsync<T>(object router, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildUpdateRouterRequest(router, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="UpdateRouterAsync{T}"/> request.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildUpdateRouterRequest(object router, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (router == null)
                throw new ArgumentNullException("router");

            return Endpoint.PrepareUpdateResourceRequest("routers", router, cancellationToken);
        }

        /// <summary>
        /// Lists all routers for the account. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListRoutersAsync<T>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListRoutersRequest(queryString, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListRoutersAsync{T}"/> request.
        /// </summary>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<PreparedRequest> BuildListRoutersRequest(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            PreparedRequest request = await Endpoint.PrepareGetResourceRequest("routers", cancellationToken).ConfigureAwait(false);

            request.Url.SetQueryParams(queryString?.Build());

            return request;
        }

        /// <summary>
        /// Deletes a router.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task DeleteRouterAsync(string routerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteRouterRequest(routerId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteRouterAsync"/> request.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildDeleteRouterRequest(string routerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routerId == null)
                throw new ArgumentNullException("routerId");

            return Endpoint.PrepareDeleteResourceRequest($"routers/{routerId}", cancellationToken);
        }

        /// <summary>
        /// Attaches an existing port to the specified router.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="portId">The port identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task AttachPortToRouterAsync(string routerId, string portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildAttachPortToRouterRequest(routerId, portId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="AttachPortToRouterAsync"/> request.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="portId">The port identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<PreparedRequest> BuildAttachPortToRouterRequest(string routerId, string portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routerId == null)
                throw new ArgumentNullException("routerId");

            if (portId == null)
                throw new ArgumentNullException("portId");

            var request = await Endpoint.PrepareRequest($"routers/{routerId}/add_router_interface", cancellationToken).ConfigureAwait(false);
            var requestBody = new {port_id = portId};
            return request.PreparePutJson(requestBody, cancellationToken);
        }

        /// <summary>
        /// Creates a new port on the subnet and attaches it to the specified router.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="subnetId">The subnet identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>The newly created port identifier.</returns>
        public virtual async Task<string> AttachSubnetToRouterAsync(string routerId, string subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
             dynamic result = await BuildAttachSubnetToRouterRequest(routerId, subnetId, cancellationToken)
                .SendAsync()
                .ReceiveJson().ConfigureAwait(false);

            return result.port_id;
        }

        /// <summary>
        /// Builds the <see cref="AttachSubnetToRouterAsync"/> request.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="subnetId">The subnet identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<PreparedRequest> BuildAttachSubnetToRouterRequest(string routerId, string subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routerId == null)
                throw new ArgumentNullException("routerId");

            if (subnetId == null)
                throw new ArgumentNullException("subnetId");

            var request = await Endpoint.PrepareRequest($"routers/{routerId}/add_router_interface", cancellationToken).ConfigureAwait(false);
            var requestBody = new { subnet_id = subnetId };
            return request.PreparePutJson(requestBody, cancellationToken);
        }

        /// <summary>
        /// Detaches a port from the specified router.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="portId">The port identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task DetachPortFromRouterAsync(string routerId, string portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDetachPortFromRouterRequest(routerId, portId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="AttachPortToRouterAsync"/> request.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="portId">The port identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<PreparedRequest> BuildDetachPortFromRouterRequest(string routerId, string portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routerId == null)
                throw new ArgumentNullException("routerId");

            if (portId == null)
                throw new ArgumentNullException("portId");

            var request = await Endpoint.PrepareRequest($"routers/{routerId}/remove_router_interface", cancellationToken).ConfigureAwait(false);
            request.AllowHttpStatus(HttpStatusCode.NotFound);
            var requestBody = new { port_id = portId };
            return request.PreparePutJson(requestBody, cancellationToken);
        }

        /// <summary>
        /// Finds the port on the subnet attached to the specified router, detaches then deletes it. 
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="subnetId">The subnet identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>The newly created port identifier.</returns>
        public virtual Task DetachSubnetFromRouterAsync(string routerId, string subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDetachSubnetFromRouterRequest(routerId, subnetId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DetachSubnetFromRouterAsync"/> request.
        /// </summary>
        /// <param name="routerId">The router identifier.</param>
        /// <param name="subnetId">The subnet identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<PreparedRequest> BuildDetachSubnetFromRouterRequest(string routerId, string subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routerId == null)
                throw new ArgumentNullException("routerId");

            if (subnetId == null)
                throw new ArgumentNullException("subnetId");

            var request = await Endpoint.PrepareRequest($"routers/{routerId}/remove_router_interface", cancellationToken).ConfigureAwait(false);
            var requestBody = new { subnet_id = subnetId };
            return request.PreparePutJson(requestBody, cancellationToken);
        }
        #endregion

        #region SecurityGroup
        /// <summary>
        /// Lists all network security groups associated with the account.
        /// </summary>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection of network security group resources associated with the account.
        /// </returns>
        public async Task<T> ListSecurityGroupsAsync<T>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListSecurityGroupsRequest(queryString, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds a <see cref="ListSecurityGroupsAsync{T}"/> request.
        /// </summary>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<PreparedRequest> BuildListSecurityGroupsRequest(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            var request = endpoint
                .AppendPathSegments("security-groups")
                .Authenticate(AuthenticationProvider)
                .PrepareGet(cancellationToken);

            request.Url.SetQueryParams(queryString?.Build());

            return request;
        }

        /// <summary>
        /// Lists all network security group rules associated with the account.
        /// </summary>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>
        /// A collection of network security group rule resources associated with the account.
        /// </returns>
        public async Task<T> ListSecurityGroupRulesAsync<T>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListSecurityGroupRulesRequest(queryString, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds a <see cref="ListSecurityGroupRulesAsync{T}"/> request.
        /// </summary>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<PreparedRequest> BuildListSecurityGroupRulesRequest(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            Url endpoint = await Endpoint.GetEndpoint(cancellationToken).ConfigureAwait(false);

            var request = endpoint
                .AppendPathSegments("security-group-rules")
                .Authenticate(AuthenticationProvider)
                .PrepareGet(cancellationToken);

            request.Url.SetQueryParams(queryString?.Build());

            return request;
        }
        #endregion

        #region Floating IPs
        /// <summary>
        /// Shows details for a server group.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="floatingIPId">The floating IP identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> GetFloatingIPAsync<T>(string floatingIPId, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildGetFloatingIPRequest(floatingIPId, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="GetFloatingIPAsync{T}"/> request.
        /// </summary>
        /// <param name="floatingIPId">The floating IP identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildGetFloatingIPRequest(string floatingIPId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (floatingIPId == null)
                throw new ArgumentNullException("floatingIPId");

            return Endpoint.PrepareGetResourceRequest($"floatingips/{floatingIPId}", cancellationToken);
        }

        /// <summary>
        /// Creates a floating IP.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="floatingIP">The floating IP.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> CreateFloatingIPAsync<T>(object floatingIP, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildCreateFloatingIPRequest(floatingIP, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="CreateFloatingIPAsync{T}"/> request.
        /// </summary>
        /// <param name="floatingIP">The floating IP.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildCreateFloatingIPRequest(object floatingIP, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (floatingIP == null)
                throw new ArgumentNullException("floatingIP");

            return Endpoint.PrepareCreateResourceRequest("floatingips", floatingIP, cancellationToken);
        }

        /// <summary>
        /// Updates a floating IP.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="floatingIPId">The floating IP identifier.</param>
        /// <param name="floatingIP">The floating IP.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> UpdateFloatingIPAsync<T>(string floatingIPId, object floatingIP, CancellationToken cancellationToken = default(CancellationToken))
            where T : IServiceResource
        {
            return await BuildUpdateFloatingIPRequest(floatingIPId, floatingIP, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwner(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="UpdateFloatingIPAsync{T}"/> request.
        /// </summary>
        /// <param name="floatingIPId">The floating IP identifier.</param>
        /// <param name="floatingIP">The floating IP.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildUpdateFloatingIPRequest(string floatingIPId, object floatingIP, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (floatingIP == null)
                throw new ArgumentNullException("floatingIP");

            return Endpoint.PrepareUpdateResourceRequest($"floatingips/{floatingIPId}", floatingIP, cancellationToken);
        }

        /// <summary>
        /// Lists all floating IPs for the account. 
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<T> ListFloatingIPsAsync<T>(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
            where T : IEnumerable<IServiceResource>
        {
            return await BuildListFloatingIPsRequest(queryString, cancellationToken)
                .SendAsync()
                .ReceiveJson<T>()
                .PropogateOwnerToChildren(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the <see cref="ListFloatingIPsAsync{T}"/> request.
        /// </summary>
        /// <param name="queryString">Options for filtering.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual async Task<PreparedRequest> BuildListFloatingIPsRequest(IQueryStringBuilder queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            PreparedRequest request = await Endpoint.PrepareGetResourceRequest("floatingips", cancellationToken).ConfigureAwait(false);

            request.Url.SetQueryParams(queryString?.Build());

            return request;
        }

        /// <summary>
        /// Deletes a floating IP.
        /// </summary>
        /// <param name="floatingIPId">The floating IP identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task DeleteFloatingIPAsync(string floatingIPId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildDeleteFloatingIPRequest(floatingIPId, cancellationToken).SendAsync();
        }

        /// <summary>
        /// Builds the <see cref="DeleteFloatingIPAsync"/> request.
        /// </summary>
        /// <param name="floatingIPId">The floating IP identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task<PreparedRequest> BuildDeleteFloatingIPRequest(string floatingIPId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (floatingIPId == null)
                throw new ArgumentNullException("floatingIPId");

            return Endpoint.PrepareDeleteResourceRequest($"floatingips/{floatingIPId}", cancellationToken);
        }
        #endregion

        #endregion
    }
}
