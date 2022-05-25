using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Rackspace.CloudNetworks.v2
{
    public class CloudNetworkServiceTests : IDisposable
    {
        private readonly CloudNetworkService _networkingService;
        private readonly CloudNetworksTestDataManager _testData;

        public CloudNetworkServiceTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            RackspaceNet.Tracing.Http.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider();
            _networkingService = new CloudNetworkService(authenticationProvider, "IAD");

            _testData = new CloudNetworksTestDataManager(_networkingService);
        }

        public void Dispose()
        {
            Trace.Listeners.Clear();
            RackspaceNet.Tracing.Http.Listeners.Clear();

            _testData.Dispose();
        }

        #region Networks

        [Fact]
        public async void CreateNetworkTest()
        {
            var definition = _testData.BuildNetwork();

            Trace.WriteLine(string.Format("Creating network named: {0}", definition.Name));
            var network = await _testData.CreateNetwork(definition);

            Trace.WriteLine("Verifying network matches requested definition...");
            Assert.NotNull(network);
            Assert.Equal(definition.Name, network.Name);
            Assert.True(network.IsUp);
            Assert.False(network.IsShared);
            Assert.Equal(NetworkStatus.Active, network.Status);
        }
        
        [Fact]
        public async void UpdateNetworkTest()
        {
            Network network = await _testData.CreateNetwork();

            var networkUpdate = new NetworkDefinition
            {
                Name = string.Format("{0}-updated", network.Name)
            };

            Trace.WriteLine("Updating the network...");
            network = await _networkingService.UpdateNetworkAsync(network.Id, networkUpdate);

            Trace.WriteLine("Verifying network was updated as requested...");
            Assert.NotNull(network);
            Assert.Equal(networkUpdate.Name, network.Name);
        }

        [Fact]
        public async void ListNetworksTest()
        {
            var networks = await _testData.CreateNetworks();

            Trace.WriteLine("Listing networks...");
            var results = await _networkingService.ListNetworksAsync();

            Trace.WriteLine("Verifying list of networks...");
            Assert.NotNull(results);
            Assert.All(networks, network => Assert.True(results.Any(x => x.Id == network.Id)));

            Trace.WriteLine("Getting the first page of networks...");
            var pagedResults = await _networkingService.ListNetworksAsync(pageSize: 1);
            Assert.Equal(1, pagedResults.Count());
        }

        [Fact]
        public async void GetNetworkTest()
        {
            var network = await _testData.CreateNetwork();

            Trace.WriteLine("Retrieving network...");
            var result = await _networkingService.GetNetworkAsync(network.Id);

            Trace.WriteLine("Verifying network...");
            Assert.NotNull(result);
            Assert.Equal(network.Id, result.Id);
        }
        #endregion

        #region Subnets
        [Fact]
        public async void CreateSubnetTest()
        {
            var network = await _testData.CreateNetwork();
            var definition = new SubnetCreateDefinition(network.Id, IPVersion.IPv4, "192.168.3.0/24")
            {
                Name = TestData.GenerateName(),
                GatewayIP = "192.168.3.1",
                AllocationPools =
                {
                    new AllocationPool("192.168.3.10", "192.168.3.50")
                },
                Nameservers =
                {
                    "8.8.8.8"
                },
                HostRoutes =
                {
                    new HostRoute("1.2.3.4/24", "10.0.0.1")
                }
            };

            Trace.WriteLine(string.Format("Creating subnet named: {0}", definition.Name));
            var subnet = await _testData.CreateSubnet(definition);

            Trace.WriteLine("Verifying subnet matches requested definition...");
            Assert.NotNull(subnet);
            Assert.Equal(definition.NetworkId, subnet.NetworkId);
            Assert.Equal(definition.Name, subnet.Name);
            Assert.Equal(definition.CIDR, subnet.CIDR);
            Assert.Equal(definition.IPVersion, subnet.IPVersion);
            Assert.Equal(definition.GatewayIP, subnet.GatewayIP);
            Assert.Equal(definition.AllocationPools, subnet.AllocationPools);
            Assert.Equal(definition.Nameservers, subnet.Nameservers);
            Assert.Equal(definition.HostRoutes, subnet.HostRoutes);
        }
        
        [Fact]
        public async void UpdateSubnetTest()
        {
            Network network = await _testData.CreateNetwork();
            Subnet subnet = await _testData.CreateSubnet(network);

            var networkUpdate = new SubnetUpdateDefinition
            {
                Name = string.Format("{0}-updated", subnet.Name)
            };

            Trace.WriteLine("Updating the subnet...");
            subnet = await _networkingService.UpdateSubnetAsync(subnet.Id, networkUpdate);

            Trace.WriteLine("Verifying subnet was updated as requested...");
            Assert.NotNull(subnet);
            Assert.Equal(networkUpdate.Name, subnet.Name);
        }

        [Fact]
        public async void ListSubnetsTest()
        {
            var network = await _testData.CreateNetwork();
            var subnets = await _testData.CreateSubnets(network);

            Trace.WriteLine("Listing subnets...");
            var results = await _networkingService.ListSubnetsAsync();

            Trace.WriteLine("Verifying list of subnets...");
            Assert.NotNull(results);
            Assert.All(subnets, subnet => Assert.True(results.Any(x => x.Id == subnet.Id)));

            Trace.WriteLine("Getting the first page of subnets...");
            var pagedResults = await _networkingService.ListSubnetsAsync(pageSize: 1);
            Assert.Equal(1, pagedResults.Count());
        }

        [Fact]
        public async void GetSubnetTest()
        {
            var network = await _testData.CreateNetwork();
            var subnet = await _testData.CreateSubnet(network);

            Trace.WriteLine("Retrieving subnet...");
            var result = await _networkingService.GetSubnetAsync(subnet.Id);

            Trace.WriteLine("Verifying subnet...");
            Assert.NotNull(result);
            Assert.Equal(subnet.Id, result.Id);

        }
        #endregion

        #region Ports
        [Fact]
        public async void CreatePortTests()
        {
            var network = await _testData.CreateNetwork();
            var subnet = await _testData.CreateSubnet(network);

            var portIdAddress = subnet.CIDR.Replace("0/24", "2");
            var definition = new PortCreateDefinition(network.Id)
            {
                Name = TestData.GenerateName(),
                FixedIPs =
                {
                    new IPAddressAssociation(subnet.Id, portIdAddress)
                }
            };

            Trace.WriteLine(string.Format("Creating port named: {0}", definition.Name));
            var port = await _testData.CreatePort(definition);

            Trace.WriteLine("Verifying port matches requested definition...");
            Assert.NotNull(port);
            Assert.NotNull(port.Id);
            Assert.Equal(definition.Name, port.Name);
            Assert.Equal(definition.NetworkId, port.NetworkId);
            Assert.Equal(subnet.Id, port.FixedIPs.Single().SubnetId);
            Assert.Equal(portIdAddress, port.FixedIPs.Single().IPAddress);
        }
        
        [Fact]
        public async void UpdatePortTest()
        {
            Network network = await _testData.CreateNetwork();
            var subnet = await _testData.CreateSubnet(network);
            Port port = await _testData.CreatePort(network);

            var portUpdate = new PortUpdateDefinition
            {
                Name = string.Format("{0}-updated", port.Name)
            };

            Trace.WriteLine("Updating the port...");
            port = await _networkingService.UpdatePortAsync(port.Id, portUpdate);

            Trace.WriteLine("Verifying port was updated as requested...");
            Assert.NotNull(port);
            Assert.Equal(portUpdate.Name, port.Name);
        }

        [Fact]
        public async void ListPortsTest()
        {
            var network = await _testData.CreateNetwork();
            var subnet = await _testData.CreateSubnet(network);
            var ports = await _testData.CreatePorts(network);

            Trace.WriteLine("Listing ports...");
            var results = await _networkingService.ListPortsAsync();

            Trace.WriteLine("Verifying list of ports...");
            Assert.NotNull(results);
            Assert.All(ports, port => Assert.True(results.Any(x => x.Id == port.Id)));

            Trace.WriteLine("Getting the first page of ports...");
            var pagedResults = await _networkingService.ListPortsAsync(pageSize: 1);
            Assert.Equal(1, pagedResults.Count());
        }

        [Fact]
        public async void GetPortTest()
        {
            var network = await _testData.CreateNetwork();
            var subnet = await _testData.CreateSubnet(network);
            var port = await _testData.CreatePort(network);

            Trace.WriteLine("Retrieving port...");
            var result = await _networkingService.GetPortAsync(port.Id);

            Trace.WriteLine("Verifying port...");
            Assert.NotNull(result);
            Assert.Equal(port.Id, result.Id);

        }
        #endregion
    }
}
