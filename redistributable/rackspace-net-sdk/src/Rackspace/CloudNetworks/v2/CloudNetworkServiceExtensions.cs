using OpenStack.Synchronous.Extensions;
using Rackspace.CloudNetworks.v2;

// ReSharper disable once CheckNamespace
namespace Rackspace.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for an <see cref="CloudNetworkService"/> instance.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class CloudNetworkServiceExtensions
    {
        #region Networks

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.ListNetworks"/>
        public static IPage<Network> ListNetworks(this CloudNetworkService cloudNetworkService, Identifier startNetworkId = null, int? pageSize = null)
        {
            return cloudNetworkService.ListNetworksAsync(startNetworkId, pageSize).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.GetNetwork"/>
        public static Network GetNetwork(this CloudNetworkService cloudNetworkService, Identifier networkId)
        {
            return cloudNetworkService.GetNetworkAsync(networkId).ForceSynchronous();
        }
        
        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.CreateNetwork"/>
        public static Network CreateNetwork(this CloudNetworkService cloudNetworkService, NetworkDefinition network)
        {
            return cloudNetworkService.CreateNetworkAsync(network).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.UpdateNetwork"/>
        public static Network UpdateNetwork(this CloudNetworkService cloudNetworkService, Identifier networkId, NetworkDefinition network)
        {
            return cloudNetworkService.UpdateNetworkAsync(networkId, network).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.DeleteNetwork"/>
        public static void DeleteNetwork(this CloudNetworkService cloudNetworkService, Identifier networkId)
        {
            cloudNetworkService.DeleteNetworkAsync(networkId).ForceSynchronous();
        }
        #endregion

        #region Subnets

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.ListSubnets"/>
        public static IPage<Subnet> ListSubnets(this CloudNetworkService cloudNetworkService, Identifier startSubnetId = null, int? pageSize = null)
        {
            return cloudNetworkService.ListSubnetsAsync(startSubnetId, pageSize).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.CreateSubnet"/>
        public static Subnet CreateSubnet(this CloudNetworkService cloudNetworkService, SubnetCreateDefinition subnet)
        {
            return cloudNetworkService.CreateSubnetAsync(subnet).ForceSynchronous();
        }
        
        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.GetSubnet"/>
        public static Subnet GetSubnet(this CloudNetworkService cloudNetworkService, Identifier subnetId)
        {
            return cloudNetworkService.GetSubnetAsync(subnetId).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.UpdateSubnet"/>
        public static Subnet UpdateSubnet(this CloudNetworkService cloudNetworkService, Identifier subnetId, SubnetUpdateDefinition subnet)
        {
            return cloudNetworkService.UpdateSubnetAsync(subnetId, subnet).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.DeleteSubnet"/>
        public static void DeleteSubnet(this CloudNetworkService cloudNetworkService, Identifier subnetId)
        {
            cloudNetworkService.DeleteSubnetAsync(subnetId).ForceSynchronous();
        }
        #endregion

        #region Ports
        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.ListPorts"/>
        /*public static IPage<Port> ListPorts(this CloudNetworkService cloudNetworkService, Identifier startPortid = null, int? pageSize = null)
        {
            return cloudNetworkService.ListPortsAsync(startPortid, pageSize).ForceSynchronous();
        }*/

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.CreatePort"/>
        public static Port CreatePort(this CloudNetworkService cloudNetworkService, PortCreateDefinition port)
        {
            return cloudNetworkService.CreatePortAsync(port).ForceSynchronous();
        }
        
        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.GetPort"/>
        public static Port GetPort(this CloudNetworkService cloudNetworkService, Identifier portId)
        {
            return cloudNetworkService.GetPortAsync(portId).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.UpdatePort"/>
        public static Port UpdatePort(this CloudNetworkService cloudNetworkService, Identifier portId, PortUpdateDefinition port)
        {
            return cloudNetworkService.UpdatePortAsync(portId, port).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Synchronous.NetworkingServiceExtensions.DeletePort"/>
        public static void DeletePort(this CloudNetworkService cloudNetworkService, Identifier portId)
        {
            cloudNetworkService.DeletePortAsync(portId).ForceSynchronous();
        }
        #endregion
    }
}