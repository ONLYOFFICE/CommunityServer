using System;
using System.Linq;
using Rackspace.Synchronous;
using Rackspace.Testing;
using Xunit;

namespace Rackspace.RackConnect.v3
{
    public class NetworkTests
    {
        private readonly RackConnectService _rackConnectService;

        public NetworkTests()
        {
            _rackConnectService = new RackConnectService(Stubs.IdentityService, "region");
        }

        [Fact]
        public void ListNetworks()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier id = Guid.NewGuid();
                httpTest.RespondWithJson(new[] { new NetworkReference { Id = id } });

                var results = _rackConnectService.ListNetworks();

                httpTest.ShouldHaveCalled($"*/cloud_networks");
                Assert.NotNull(results);
                Assert.Equal(1, results.Count());
                Assert.Equal(id, results.First().Id);
            }
        }

        [Fact]
        public void GetNetwork()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier id = Guid.NewGuid();
                httpTest.RespondWithJson(new NetworkReference { Id = id });

                var result = _rackConnectService.GetNetwork(id);

                httpTest.ShouldHaveCalled($"*/cloud_networks/{id}");
                Assert.NotNull(result);
                Assert.Equal(id, result.Id);
            }
        }
    }
}
