using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Rackspace.CloudServers.v2;
using Xunit;
using Xunit.Abstractions;

namespace Rackspace.RackConnect.v3
{
    public class RackConnectServiceTests : IDisposable
    {
        private readonly RackConnectService _rackConnectService;
        private readonly RackConnectTestDataManager _testData;

        public RackConnectServiceTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            RackspaceNet.Tracing.Http.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider("RackConnect");
            _rackConnectService = new RackConnectService(authenticationProvider, "LON");

            _testData = new RackConnectTestDataManager(_rackConnectService, authenticationProvider);
        }

        public void Dispose()
        {
            Trace.Listeners.Clear();
            RackspaceNet.Tracing.Http.Listeners.Clear();

            _testData.Dispose();
        }

        [Fact]
        public async Task CreateUnassignedPublicIPTest()
        {
            Trace.Write("Provisioning a public ip address... ");
            var ip = await _testData.CreatePublicIP(new PublicIPCreateDefinition { ShouldRetain = true });

            await ip.WaitUntilActiveAsync();
            Trace.WriteLine(ip.PublicIPv4Address);

            Assert.NotNull(ip);
            Assert.Null(ip.Server);
            Assert.True(ip.ShouldRetain);
            Assert.NotNull(ip.PublicIPv4Address);
            Assert.Equal(PublicIPStatus.Active, ip.Status);
        }

        [Fact]
        public async Task CreatePublicIPTest()
        {
            Trace.WriteLine("Looking up the RackConnect network...");
            var network = (await _rackConnectService.ListNetworksAsync()).First();

            Trace.Write("Creating a test cloud server...");
            var server = _testData.CreateServer(network.Id);
            Trace.WriteLine(server.Id);

            Trace.Write("Assigning a public ip address to the test cloud server... ");
            var ipRequest = new PublicIPCreateDefinition {ServerId = server.Id, ShouldRetain = true};
            var ip = await _testData.CreatePublicIP(ipRequest);
            await ip.WaitUntilActiveAsync();
            Trace.WriteLine(ip.PublicIPv4Address);

            Assert.NotNull(ip);
            Assert.Equal(server.Id, ip.Server.ServerId);
            Assert.NotNull(ip.PublicIPv4Address);
            Assert.Equal(PublicIPStatus.Active, ip.Status);

            Trace.WriteLine("Retrieving public IPs assigned to the test cloud server...");
            var filterByServer = new ListPublicIPsFilter {ServerId = server.Id};
            var ips = await _rackConnectService.ListPublicIPsAsync(filterByServer);
            Assert.NotNull(ips);
            Assert.True(ips.Any(x => x.Id == ip.Id));

            Trace.WriteLine("Update the IP address to not be retained...");
            ip = await _rackConnectService.UpdatePublicIPAsync(ip.Id, new PublicIPUpdateDefinition { ShouldRetain = false });
            await ip.WaitUntilActiveAsync();
            Assert.NotNull(ip);
            Assert.False(ip.ShouldRetain);

            Trace.WriteLine("Removing public IP from test cloud server...");
            await ip.DeleteAsync();
            await ip.WaitUntilDeletedAsync();

            Trace.WriteLine($"Verifying that {ip.PublicIPv4Address} is no longer assigned...");
            ips = await _rackConnectService.ListPublicIPsAsync(filterByServer);
            Assert.NotNull(ips);
            Assert.False(ips.Any(x => x.Id == ip.Id));
        }

        [Fact]
        public async Task ListNetworksTest()
        {
            Trace.WriteLine("Listing RackConnect networks...");
            var networks = await _rackConnectService.ListNetworksAsync();

            Assert.NotNull(networks);
            var network = networks.FirstOrDefault();
            Assert.NotNull(network);
            Assert.NotNull(network.Id);
            Assert.NotNull(network.Name);
            Assert.NotNull(network.CIDR);
            Assert.NotNull(network.Created);
        }

        [Fact]
        public async Task GetNetworkTest()
        {
            Trace.WriteLine("Listing RackConnect networks...");
            var networks = await _rackConnectService.ListNetworksAsync();
            Assert.NotNull(networks);
            var network = networks.FirstOrDefault();
            Assert.NotNull(network);

            Trace.WriteLine($"Retrieving RackConnect network ${network.Name}...");
            network = await _rackConnectService.GetNetworkAsync(network.Id);
            Assert.NotNull(network.Id);
            Assert.NotNull(network.Name);
            Assert.NotNull(network.CIDR);
            Assert.NotNull(network.Created);
        }
    }
}