using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Extensions;
using Flurl.Http;
using OpenStack.Authentication;
using OpenStack.Networking.v2.Serialization;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// The OpenStack Networking Service.
    /// </summary>
    /// <seealso href="https://wiki.openstack.org/wiki/Neutron/APIv2-specification">OpenStack Networking API v2 Overview</seealso>
    /// <seealso href="http://developer.openstack.org/api-ref-networking-v2.html">OpenStack Networking API v2 Reference</seealso>
    public class NetworkingService
    {
        internal readonly NetworkingApiBuilder _networkingApiBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkingService"/> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        /// <param name="useInternalUrl">if set to <c>true</c> uses the internal URLs specified in the ServiceCatalog, otherwise the public URLs are used.</param>
        public NetworkingService(IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl = false)
        {
            _networkingApiBuilder = new NetworkingApiBuilder(ServiceType.Networking, authenticationProvider, region, useInternalUrl);
        }

        #region Networks
        /// <inheritdoc cref="NetworkingApiBuilder.ListNetworksAsync" />
        public async Task<IEnumerable<Network>> ListNetworksAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _networkingApiBuilder
                .ListNetworksAsync(cancellationToken)
                .SendAsync()
                .ReceiveJson<NetworkCollection>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.GetNetworkAsync" />
        public Task<Network> GetNetworkAsync(Identifier networkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .GetNetworkAsync(networkId, cancellationToken)
                .SendAsync()
                .ReceiveJson<Network>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.CreateNetworkAsync" />
        public Task<Network> CreateNetworkAsync(NetworkDefinition network, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .CreateNetworkAsync(network, cancellationToken)
                .SendAsync()
                .ReceiveJson<Network>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.CreateNetworksAsync" />
        public async Task<IEnumerable<Network>> CreateNetworksAsync(IEnumerable<NetworkDefinition> networks, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _networkingApiBuilder
                .CreateNetworksAsync(networks, cancellationToken)
                .SendAsync()
                .ReceiveJson<NetworkCollection>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.UpdateNetworkAsync" />
        public Task<Network> UpdateNetworkAsync(Identifier networkId, NetworkDefinition network, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .UpdateNetworkAsync(networkId, network, cancellationToken)
                .SendAsync()
                .ReceiveJson<Network>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DeleteNetworkAsync" />
        public Task DeleteNetworkAsync(Identifier networkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .DeleteNetworkAsync(networkId, cancellationToken)
                .SendAsync();
        }
        #endregion

        #region Subnets

        /// <inheritdoc cref="NetworkingApiBuilder.ListSubnetsAsync" />
        public async Task<IEnumerable<Subnet>> ListSubnetsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _networkingApiBuilder
                .ListSubnetsAsync(cancellationToken)
                .SendAsync()
                .ReceiveJson<SubnetCollection>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.CreateSubnetAsync" />
        public Task<Subnet> CreateSubnetAsync(SubnetCreateDefinition subnet, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .CreateSubnetAsync(subnet, cancellationToken)
                .SendAsync()
                .ReceiveJson<Subnet>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.CreateSubnetsAsync" />
        public async Task<IEnumerable<Subnet>> CreateSubnetsAsync(IEnumerable<SubnetCreateDefinition> subnets, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _networkingApiBuilder
                .CreateSubnetsAsync(subnets, cancellationToken)
                .SendAsync()
                .ReceiveJson<SubnetCollection>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.GetSubnetAsync" />
        public Task<Subnet> GetSubnetAsync(Identifier subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .GetSubnetAsync(subnetId, cancellationToken)
                .SendAsync()
                .ReceiveJson<Subnet>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.UpdateSubnetAsync" />
        public Task<Subnet> UpdateSubnetAsync(Identifier subnetId, SubnetUpdateDefinition subnet, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .UpdateSubnetAsync(subnetId, subnet, cancellationToken)
                .SendAsync()
                .ReceiveJson<Subnet>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DeleteSubnetAsync" />
        public Task DeleteSubnetAsync(Identifier subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .DeleteSubnetAsync(subnetId, cancellationToken)
                .SendAsync();
        }
        #endregion

        #region Ports

        /// <inheritdoc cref="NetworkingApiBuilder.ListPortsAsync{T}" />
        public async Task<IEnumerable<Port>> ListPortsAsync(PortListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _networkingApiBuilder.ListPortsAsync<PortCollection>(options, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.CreatePortAsync" />
        public Task<Port> CreatePortAsync(PortCreateDefinition port, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .CreatePortAsync(port, cancellationToken)
                .SendAsync()
                .ReceiveJson<Port>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.CreatePortsAsync" />
        public async Task<IEnumerable<Port>> CreatePortsAsync(IEnumerable<PortCreateDefinition> ports, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _networkingApiBuilder
                .CreatePortsAsync(ports, cancellationToken)
                .SendAsync()
                .ReceiveJson<PortCollection>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.GetPortAsync" />
        public Task<Port> GetPortAsync(Identifier portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .GetPortAsync(portId, cancellationToken)
                .SendAsync()
                .ReceiveJson<Port>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.UpdatePortAsync" />
        public Task<Port> UpdatePortAsync(Identifier portId, PortUpdateDefinition port, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .UpdatePortAsync(portId, port, cancellationToken)
                .SendAsync()
                .ReceiveJson<Port>();
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DeletePortAsync" />
        public Task DeletePortAsync(Identifier portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .DeletePortAsync(portId, cancellationToken)
                .SendAsync();
        }
        #endregion
    }
}
