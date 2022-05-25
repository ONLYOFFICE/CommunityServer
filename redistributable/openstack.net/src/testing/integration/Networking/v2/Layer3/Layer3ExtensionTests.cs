using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenStack.Compute.v2_1;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.Networking.v2.Layer3
{
    public class Layer3ExtensionTests : IDisposable
    {
        private readonly NetworkingService _networkingService;
        private readonly NetworkingTestDataManager _testData;
        private readonly ComputeTestDataManager _computeTestData;

        public Layer3ExtensionTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);

            // The operator identity is necessary to create external networks
            var authenticationProvider = TestIdentityProvider.GetOperatorIdentity();
            _networkingService = new NetworkingService(authenticationProvider, "RegionOne");

            _testData = new NetworkingTestDataManager(_networkingService);
            var compute = new ComputeService(authenticationProvider, "RegionOne");
            _computeTestData = new ComputeTestDataManager(compute);
        }

        public void Dispose()
        {
            _computeTestData.Dispose();
            _testData.Dispose();

            Trace.Listeners.Clear();
            OpenStackNet.Tracing.Http.Listeners.Clear();
        }

        [Fact]
        public async Task AssignFloatingIP()
        {
            // 1. Make an internal network and subnet.
            var internalNetwork = await _testData.CreateNetwork();
            var internalSubnet = await _testData.CreateSubnet(internalNetwork);

            // 2. Make an external network and subnet.
            Operator.NetworkDefinition externalNetworkDefinition = _testData.BuildNetwork();
            externalNetworkDefinition.IsExternal = true;
            var externalNetwork = await _testData.CreateNetwork(externalNetworkDefinition);
            var externalSubnet = await _testData.CreateSubnet(externalNetwork);

            // 3. Make a router on the external network.
            var routerDefinition = new RouterCreateDefinition
            {
                Name = TestData.GenerateName(),
                ExternalGateway = new ExternalGatewayDefinition(externalNetwork.Id)
            };
            var router = await _networkingService.CreateRouterAsync(routerDefinition);
            _testData.Register(router);

            // 4. Attach the router to the internal network.
            var internalPortId = await router.AttachSubnetAsync(internalSubnet.Id);
            _testData.Register(new Port {Id = internalPortId});
            
            // 5. Make a floating ip on the external network.
            var floatingIPDefinition = new FloatingIPCreateDefinition(externalNetwork.Id);
            var floatingIP = await _networkingService.CreateFloatingIPAsync(floatingIPDefinition);
            _testData.Register(floatingIP);

            // 6. Make a server on the internal network.
            var serverDefinition = _computeTestData.BuildServer();
            serverDefinition.Networks.Add(new ServerNetworkDefinition {NetworkId = internalNetwork.Id });
            var server = await _computeTestData.CreateServer(serverDefinition);
            await server.WaitUntilActiveAsync();

            // 7.Associate the floating ip to the server.
            await server.AssociateFloatingIPAsync(new AssociateFloatingIPRequest(floatingIP.FloatingIPAddress));

            // 8. Disassociate the floating ip from the server
            await server.DisassociateFloatingIPAsync(floatingIP.FloatingIPAddress);
        }

        [Fact]
        public async Task ListSecurityGroups()
        {
            var groups = await _networkingService.ListSecurityGroupsAsync(new SecurityGroupListOptions {Name = "default"});

            Assert.NotEmpty(groups);

            var defaultGroup = groups.First();
            Assert.NotNull(defaultGroup);
            Assert.NotNull(defaultGroup.Name);
            Assert.NotNull(defaultGroup.Description);
            Assert.NotNull(defaultGroup.Id);
            Assert.NotEmpty(defaultGroup.SecurityGroupRules);

            var defaultRule = defaultGroup.SecurityGroupRules.First();
            Assert.NotNull(defaultRule.Id);
            Assert.NotNull(defaultRule.Direction);
#pragma warning disable xUnit2002 // Do not use null check on value type
            Assert.NotNull(defaultRule.Ethertype);
#pragma warning restore xUnit2002 // Do not use null check on value type
            Assert.NotNull(defaultRule.SecurityGroupId);
        }
    }
}