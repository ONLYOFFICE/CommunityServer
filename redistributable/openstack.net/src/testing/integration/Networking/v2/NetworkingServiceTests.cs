using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.Networking.v2
{
    public class NetworkingServiceTests : IDisposable
    {
        private readonly NetworkingService _networkingService;
        private readonly NetworkingTestDataManager _testData;

        public NetworkingServiceTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider();
            _networkingService = new NetworkingService(authenticationProvider, "RegionOne");

            _testData = new NetworkingTestDataManager(_networkingService);
        }

        public void Dispose()
        {
            _testData.Dispose();

            Trace.Listeners.Clear();
            OpenStackNet.Tracing.Http.Listeners.Clear();
        }

        #region Networks

        [Fact]
        public async Task CreateNetworkTest()
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
        public async Task BulkCreateNetworksTest()
        {
            var definitions = new[] { _testData.BuildNetwork(), _testData.BuildNetwork(), _testData.BuildNetwork() };

            Trace.WriteLine(string.Format("Creating networks: {0}", string.Join(", ", definitions.Select(d => d.Name))));
            var networks = await _testData.CreateNetworks(definitions);

            Trace.WriteLine("Verifying networks matches requested definitions...");

            Assert.NotNull(networks);
            Assert.Equal(3, networks.Count());
            Assert.All(networks, n => Assert.True(n.IsUp));
            Assert.All(networks, n => Assert.Equal(NetworkStatus.Active, n.Status));
        }

        [Fact]
        public async Task UpdateNetworkTest()
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
        public async Task ListNetworksTest()
        {
            var networks = await _testData.CreateNetworks();

            Trace.WriteLine("Listing networks...");
            var results = await _networkingService.ListNetworksAsync();

            Trace.WriteLine("Verifying list of networks...");
            Assert.NotNull(results);
            Assert.All(networks, network => Assert.Contains(results, x => x.Id == network.Id));
        }

        [Fact]
        public async Task GetNetworkTest()
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
        public async Task CreateSubnetTest()
        {
            var network = await _testData.CreateNetwork();
            var definition = new SubnetCreateDefinition(network.Id, IPVersion.IPv4, "192.168.3.0/24")
            {
                Name = TestData.GenerateName(),
                IsDHCPEnabled = true,
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
            Assert.Equal(definition.IsDHCPEnabled, subnet.IsDHCPEnabled);
            Assert.Equal(definition.GatewayIP, subnet.GatewayIP);
            Assert.Equal(definition.AllocationPools, subnet.AllocationPools);
            Assert.Equal(definition.Nameservers, subnet.Nameservers);
            Assert.Equal(definition.HostRoutes, subnet.HostRoutes);
        }

        [Fact]
        public async Task BulkCreateSubnetsTest()
        {
            var network = await _testData.CreateNetwork();
            var definitions = new[] {_testData.BuildSubnet(network), _testData.BuildSubnet(network), _testData.BuildSubnet(network) };

            Trace.WriteLine(string.Format("Creating subnets: {0}", string.Join(", ", definitions.Select(d => d.Name))));
            var subnets = await _testData.CreateSubnets(definitions);

            Trace.WriteLine("Verifying subnets matches requested definitions...");
            Assert.NotNull(subnets);
            Assert.Equal(3, subnets.Count());
        }

        [Fact]
        public async Task UpdateSubnetTest()
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
        public async Task ListSubnetsTest()
        {
            var network = await _testData.CreateNetwork();
            var subnets = await _testData.CreateSubnets(network);

            Trace.WriteLine("Listing subnets...");
            var results = await _networkingService.ListSubnetsAsync();

            Trace.WriteLine("Verifying list of subnets...");
            Assert.NotNull(results);
            Assert.All(subnets, subnet => Assert.Contains(results, x => x.Id == subnet.Id));
        }

        [Fact]
        public async Task GetSubnetTest()
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
        public async Task CreatePortTest()
        {
            var network = await _testData.CreateNetwork();
            var subnet = await _testData.CreateSubnet(network);

            var portIdAddress = subnet.CIDR.Replace("0/24", "2");
            var definition = new PortCreateDefinition(network.Id)
            {
                Name = TestData.GenerateName(),
                DHCPOptions =
                {
                    {"server-ip-address", "192.168.1.1"}
                },
                AllowedAddresses =
                {
                    new AllowedAddress("10.0.0.1") { MACAddress = "24:a0:74:f0:1c:66" }
                },
                MACAddress = "24:a0:74:f0:1c:66",
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
            Assert.Equal(definition.DHCPOptions, port.DHCPOptions);
            Assert.Equal(definition.AllowedAddresses, port.AllowedAddresses);
            Assert.Equal(subnet.Id, port.FixedIPs.Single().SubnetId);
            Assert.Equal(portIdAddress, port.FixedIPs.Single().IPAddress);
        }

        [Fact]
        public async Task BulkCreatePortsTest()
        {
            var network = await _testData.CreateNetwork();
            var definitions = new[] { _testData.BuildPort(network), _testData.BuildPort(network), _testData.BuildPort(network) };

            Trace.WriteLine(string.Format("Creating ports: {0}", string.Join(", ", definitions.Select(d => d.Name))));
            var subnets = await _testData.CreatePorts(definitions);

            Trace.WriteLine("Verifying ports matches requested definitions...");
            Assert.NotNull(subnets);
            Assert.Equal(3, subnets.Count());
        }

        [Fact]
        public async Task UpdatePortTest()
        {
            Network network = await _testData.CreateNetwork();
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
        public async Task ListPortsTest()
        {
            var network = await _testData.CreateNetwork();
            var ports = await _testData.CreatePorts(network);

            Trace.WriteLine("Listing ports...");
            var results = await _networkingService.ListPortsAsync();

            Trace.WriteLine("Verifying list of ports...");
            Assert.NotNull(results);
            Assert.All(ports, port => Assert.Contains(results, x => x.Id == port.Id));
        }

        [Fact]
        public async Task GetPortTest()
        {
            var network = await _testData.CreateNetwork();
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
