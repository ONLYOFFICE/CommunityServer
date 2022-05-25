using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Xunit;
using Xunit.Abstractions;
using VolumeState = net.openstack.Core.Domain.VolumeState;

namespace OpenStack.Compute.v2_1.Operator
{
    public class ServerTests : IDisposable
    {
        private readonly ComputeService _compute;
        private readonly ComputeTestDataManager _testData;

        public ServerTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetOperatorIdentity();
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
        [Trait("ci", "false")]
        public async Task EvacuateServerTest()
        {
            var server = await _testData.CreateServer();
            await server.WaitUntilActiveAsync();
            Trace.WriteLine($"Created server named: {server.Name}");

            Trace.WriteLine("Evacuating the server to a new host...");
            var request = new EvacuateServerRequest(false)
            {
                AdminPassword = "top-secret-password"
            };

            // Our service isn't down, so we can't really evacuate.
            // Just check that the request would have gone through (i.e. was valid)
            // and only was rejected because the compute service is healthy
            try
            {
                await server.EvacuateAsync(request);
            }
            catch (FlurlHttpException httpError) when (httpError.Call.ErrorResponseBody.Contains("is still in use"))
            {
                // Hurray! the test passed
            }
        }
    }
}
