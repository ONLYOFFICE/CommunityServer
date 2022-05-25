using System;
using System.Collections.Generic;
using System.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1
{
    public class ServerAddressTests
    {
        private readonly ComputeService _compute;

        public ServerAddressTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }

        [Fact]
        public void GetServerAddress()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new ServerCollection { new Server { Id = serverId } });
                httpTest.RespondWithJson(new Dictionary<string, IList<ServerAddress>>
                {
                    ["private"] = new List<ServerAddress>
                    {
                        new ServerAddress {IP = "1.2.3.4"}
                    }
                });

                var serverReferences = _compute.ListServerSummaries();
                var result = (serverReferences.First().GetAddress("private")).First();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/ips/private");
                Assert.NotNull(result);
                Assert.Equal("1.2.3.4", result.IP);
            }
        }

        [Fact]
        public void ListServerAddresses()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new ServerCollection { new Server { Id = serverId } });
                httpTest.RespondWithJson(new ServerAddressCollection {["ServiceNet"] = new List<ServerAddress> {new ServerAddress {IP = "192.168.1.189"}}});

                var serverReferences = _compute.ListServerSummaries();
                var results = serverReferences.First().ListAddresses();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/ips");
                Assert.NotNull(results);
                Assert.True(results.ContainsKey("ServiceNet"));

                var result = results["ServiceNet"].First();
                Assert.Equal("192.168.1.189", result.IP);
            }
        }
    }
}
