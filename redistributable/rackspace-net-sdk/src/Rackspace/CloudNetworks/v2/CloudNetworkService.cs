using System.Threading;
using System.Threading.Tasks;

using Flurl.Extensions;
using Flurl.Http;

using Rackspace.CloudNetworks.v2.Serialization;

using NetworkingApiBuilder = OpenStack.Networking.v2.NetworkingApiBuilder;

namespace Rackspace.CloudNetworks.v2
{
    /// <summary>
    /// The Rackspace Cloud Networks service.
    /// </summary>
    /// <seealso href="http://api.rackspace.com/api-ref-networks.html">Cloud Networks API v2 Reference</seealso>
    /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-gettingstarted/content/ch_preface.html">Cloud Networks Getting Started</seealso>
    /// <threadsafety static="true" instance="false"/>
    public class CloudNetworkService
    {
        private readonly NetworkingApiBuilder _networkingApiBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudNetworkService"/> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        public CloudNetworkService(OpenStack.Authentication.IAuthenticationProvider authenticationProvider, string region)
        {
            RackspaceNet.Configure();
            _networkingApiBuilder = new NetworkingApiBuilder(ServiceType.CloudNetworks, authenticationProvider, region, true);
        }

        #region Networks
        /// <inheritdoc cref="NetworkingApiBuilder.ListNetworksAsync" />
        public async Task<IPage<Network>> ListNetworksAsync(Identifier startNetworkId = null, int? pageSize = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            PreparedRequest request = await _networkingApiBuilder.ListNetworksAsync(cancellationToken);

            request.Url.SetQueryParams(new
            {
                marker = startNetworkId,
                limit = pageSize
            });

            return await request.SendAsync()
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
        public async Task<IPage<Subnet>> ListSubnetsAsync(Identifier startSubnetId = null, int? pageSize = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            PreparedRequest request = await _networkingApiBuilder.ListSubnetsAsync(cancellationToken);

            request.Url.SetQueryParams(new
            {
                marker = startSubnetId,
                limit = pageSize
            });

            return await request
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

     /*   /// <inheritdoc cref="NetworkingApiBuilder.ListPortsAsync" />
        public async Task<IPage<Port>> ListPortsAsync(Identifier startPortId = null, int? pageSize = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            //IEnumerable<IServiceResource>
            PortCollection request = await _networkingApiBuilder.ListPortsAsync<PortCollection>(null, cancellationToken);

            request.Url.SetQueryParams(new
            {
                marker = startPortId,
                limit = pageSize
            });

            return await request.SendAsync()
                .ReceiveJson<PortCollection>();
        }*/

        /// <inheritdoc cref="NetworkingApiBuilder.CreatePortAsync" />
        public Task<Port> CreatePortAsync(PortCreateDefinition port, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _networkingApiBuilder
                .CreatePortAsync(port, cancellationToken)
                .SendAsync()
                .ReceiveJson<Port>();
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
