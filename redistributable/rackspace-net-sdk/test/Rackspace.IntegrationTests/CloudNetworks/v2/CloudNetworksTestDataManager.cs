using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenStack;

namespace Rackspace.CloudNetworks.v2
{
    public class CloudNetworksTestDataManager : IDisposable
    {
        private readonly CloudNetworkService _cloudNetworkService;
        private readonly HashSet<object> _testData;
         
        public CloudNetworksTestDataManager(CloudNetworkService cloudNetworkService)
        {
            _cloudNetworkService = cloudNetworkService;
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
        public NetworkDefinition BuildNetwork()
        {
            return new NetworkDefinition
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
            var network = await _cloudNetworkService.CreateNetworkAsync(definition);
            Register(network);
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            return network;
        }

        public async Task<IEnumerable<Network>> CreateNetworks()
        {
            var definitions = new[] { BuildNetwork(), BuildNetwork(), BuildNetwork() };
            return await CreateNetworks(definitions);
        }

        public async Task<IEnumerable<Network>> CreateNetworks(IEnumerable<NetworkDefinition> definitions)
        {
            Task<Network>[] creates = definitions.Select(CreateNetwork).ToArray();
            return await Task.WhenAll(creates);
        }

        private void DeleteNetworks(IEnumerable<Network> networks)
        {
            Task[] deletes = networks.Select(x => _cloudNetworkService.DeleteNetworkAsync(x.Id)).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion

        #region Subnets
        private static int _subnetCounter;

        public SubnetCreateDefinition BuildSubnet(Network network)
        {
            var cidr = string.Format("192.168.{0}.0/24", _subnetCounter++);
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
            // Rackspace only allows 1 ipv4 and ipv6 subnet per network
            SubnetCreateDefinition ipv6Subnet = new SubnetCreateDefinition(network.Id, IPVersion.IPv6, "2001:db8::/32");
            var definitions = new[] {BuildSubnet(network), ipv6Subnet };
            return await CreateSubnets(definitions);
        }

        public async Task<Subnet> CreateSubnet(SubnetCreateDefinition definition)
        {
            var subnet = await _cloudNetworkService.CreateSubnetAsync(definition);
            Register(subnet);
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            return subnet;
        }

        public async Task<IEnumerable<Subnet>> CreateSubnets(IEnumerable<SubnetCreateDefinition> definitions)
        {
            Task<Subnet>[] creates = definitions.Select(CreateSubnet).ToArray();
            return await Task.WhenAll(creates);
        }

        private void DeleteSubnets(IEnumerable<Subnet> networks)
        {
            Task[] deletes = networks.Select(x => _cloudNetworkService.DeleteSubnetAsync(x.Id)).ToArray();
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
            var port = await _cloudNetworkService.CreatePortAsync(definition);
            Register(port);
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            return port;
        }

        public async Task<IEnumerable<Port>> CreatePorts(Network network)
        {
            var definitions = new[] { BuildPort(network), BuildPort(network), BuildPort(network) };
            return await CreatePorts(definitions);
        }

        public async Task<IEnumerable<Port>> CreatePorts(IEnumerable<PortCreateDefinition> definitions)
        {
            var ports = new List<Port>();
            foreach (var definition in definitions)
            {
                var port = await CreatePort(definition);
                ports.Add(port);
            }
            return ports;
        }

        public void DeletePorts(IEnumerable<Port> ports)
        {
            Task[] deletes = ports.Select(x => _cloudNetworkService.DeletePortAsync(x.Id)).ToArray();
            Task.WaitAll(deletes);
        }
        #endregion
    }
}
