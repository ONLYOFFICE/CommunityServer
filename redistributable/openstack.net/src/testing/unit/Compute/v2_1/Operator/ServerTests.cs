using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1.Operator
{
    public class ServerTests
    {
        private readonly ComputeService _compute;

        public ServerTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }
        
        [Fact]
        public async Task EvacuateServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, boss");

                var server = _compute.GetServer(serverId);
                await server.EvacuateAsync(new EvacuateServerRequest(false));

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                string lastRequest = httpTest.CallLog.Last().RequestBody;
                Assert.Contains("evacuate", lastRequest);
            }
        }
    }
}
