using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.Compute.v2_1
{
    public class ServerGroupTests : IDisposable
    {
        private readonly ComputeService _compute;
        private readonly ComputeTestDataManager _testData;

        public ServerGroupTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider();
            _compute = new ComputeService(authenticationProvider, "RegionOne");

            _testData = new ComputeTestDataManager(_compute);
        }

        public void Dispose()
        {
            _testData.Dispose();

            Trace.Listeners.Clear();
            OpenStackNet.Tracing.Http.Listeners.Clear();
        }

        [Fact]
        public async Task CreateServerGroup_AndAssociateAServerTest()
        {
            ServerGroupDefinition definition = _testData.BuildServerGroup();
            Trace.WriteLine($"Creating server group named: {definition.Name}");
            ServerGroup serverGroup = await _testData.CreateServerGroup(definition);
            
            Trace.WriteLine("Verifying server group matches requested definition...");
            Assert.NotNull(serverGroup);
            Assert.Equal(definition.Name, serverGroup.Name);
            Assert.Equal(definition.Policies, serverGroup.Policies);

            Trace.WriteLine("Creating a server associated with the group...");
            ServerCreateDefinition serverDefinition = _testData.BuildServer();
            serverDefinition.SchedulerHints = new SchedulerHints();
            serverDefinition.SchedulerHints.Add("group", serverGroup.Id);
            var server = await _testData.CreateServer(serverDefinition);
            await server.WaitUntilActiveAsync();

            Trace.WriteLine("Verifying the server is a member of the group...");
            serverGroup = await _compute.GetServerGroupAsync(serverGroup.Id);
            Assert.Contains(server.Id, serverGroup.Members);

            Trace.WriteLine("Deleting the server group...");
            await serverGroup.DeleteAsync();

            var groups = await _compute.ListServerGroupsAsync();
            Assert.DoesNotContain(serverGroup.Id, groups.Select(x => x.Id));
        }
    }
}
