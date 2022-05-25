using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenStack.Networking.v2.Layer3;

namespace OpenStack.Networking.v2
{
    public class NetworkingTestDataManager : IDisposable
    {
        private readonly NetworkingService _networkingService;
        private readonly HashSet<object> _testData;
         
        public NetworkingTestDataManager(NetworkingService networkingService)
        {
            _networkingService = networkingService;
            _testData = new HashSet<object>();
        }

        public void Register(IEnumerable<object> testItems)
        {
            foreach (var testItem in testItems)
            {
                Register(testItem);
            }
        }

        public void Register(object testItem)
        {
            _testData.Add(testItem);
        }

        public void Dispose()
        {
            var errors = new List<Exception>();

            try
            {
                DeleteFloatingIPs(_testData.OfType<FloatingIP>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteRouters(_testData.OfType<Router>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeletePorts(_testData.OfType<Port>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteSubnets(_testData.OfType<Subnet>());
            }
            catch (AggregateException ex) { errors.AddRange(ex.InnerExceptions); }

            try
            {
                DeleteNetworks(_testData.OfType<Network>());
            }
            catch (AggregateException ex){ errors.AddRange(ex.InnerExceptions); }

            if (errors.Any())
                throw new AggregateException("Unable to remove all test data!", errors);
        }

        #region Networks
        public Operator.NetworkDefinition BuildNetwork()
        {
            return new Operator.NetworkDefinition
            {
                Name = TestData.GenerateName()
            };
        }

        public async Task<Network> CreateNetwork()
        {
            var definition = BuildNetwork();
            return await CreateNetwork(definition);
        }

        public async Task<Network> CreateNetwork(NetworkDefinition definition)
        {
            var network = await _networkingService.CreateNetworkAsync(definition);
            Register(network);
            return network;
        }

        public async Task<IEnumerable<Network>> CreateNetworks()
        {
            var definitions = new[] { BuildNetwork(), BuildNetwork(), BuildNetwork() };
            return await CreateNetworks(definitions);
        }

        public async Task<IEnumerable<Network>> CreateNetworks(IEnumerable<NetworkDefinition> definitions)
        {
            var networks = await _networkingService.CreateNetworksAsync(definitions);
            Register(networks);
            return networks;
        }

        public void DeleteNetworks(IEnumerable<Network> networks)
        {
            var deletes = networks.Select(x => _networkingService.DeleteNetworkAsync(x.Id)).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Subnets
        private static int _subnetCounter;

        public SubnetCreateDefinition BuildSubnet(Network network)
        {
            var cidr = $"192.168.{_subnetCounter++}.0/24";
            return new SubnetCreateDefinition(network.Id, IPVersion.IPv4, cidr)
            {
                Name = TestData.GenerateName()
            };
        }

        public async Task<Subnet> CreateSubnet(Network network)
        {
            var definition = BuildSubnet(network);
            return await CreateSubnet(definition);
        }

        public async Task<IEnumerable<Subnet>> CreateSubnets(Network network)
        {
            var definitions = new[] {BuildSubnet(network), BuildSubnet(network), BuildSubnet(network) };
            return await CreateSubnets(definitions);
        }

        public async Task<Subnet> CreateSubnet(SubnetCreateDefinition definition)
        {
            var subnet = await _networkingService.CreateSubnetAsync(definition);
            Register(subnet);
            return subnet;
        }

        public async Task<IEnumerable<Subnet>> CreateSubnets(IEnumerable<SubnetCreateDefinition> definitions)
        {
            var subnets = await _networkingService.CreateSubnetsAsync(definitions);
            Register(subnets);
            return subnets;
        }

        public void DeleteSubnets(IEnumerable<Subnet> networks)
        {
            Task[] deletes = networks.Select(n => _networkingService.DeleteSubnetAsync(n.Id)).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Ports
        public PortCreateDefinition BuildPort(Network network)
        {
            return new PortCreateDefinition(network.Id)
            {
                Name = TestData.GenerateName()
            };
        }

        public async Task<Port> CreatePort(Network network)
        {
            var definition = BuildPort(network);
            return await CreatePort(definition);
        }

        public async Task<Port> CreatePort(PortCreateDefinition definition)
        {
            var port = await _networkingService.CreatePortAsync(definition);
            Register(port);
            return port;
        }

        public async Task<IEnumerable<Port>> CreatePorts(Network network)
        {
            var definitions = new[] { BuildPort(network), BuildPort(network), BuildPort(network) };
            return await CreatePorts(definitions);
        }

        public async Task<IEnumerable<Port>> CreatePorts(IEnumerable<PortCreateDefinition> definitions)
        {
            var ports = await _networkingService.CreatePortsAsync(definitions);
            Register(ports);
            return ports;
        }

        public void DeletePorts(IEnumerable<Port> ports)
        {
            Task[] deletes = ports.Select(x => _networkingService.DeletePortAsync(x.Id)).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Routers
        
        public void DeleteRouters(IEnumerable<Router> routers)
        {
            Task[] deletes = routers.Select(router => router.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Floating IPs

        public void DeleteFloatingIPs(IEnumerable<FloatingIP> floatingIPs)
        {
            Task[] deletes = floatingIPs.Select(ip => ip.DeleteAsync()).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion
    }
}
