using System;
using System.Linq;
using System.Net;
using OpenStack.Networking.v2.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Networking.v2
{
    public class NetworkTests
    {
        private readonly NetworkingService _networkingService;

        public NetworkTests()
        {
            _networkingService = new NetworkingService(Stubs.AuthenticationProvider, "region");
        }

        [Fact]
        public void ListNetworks()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                httpTest.RespondWithJson(new NetworkCollection(new[] {new Network {Id = networkId}}));

                var networks = _networkingService.ListNetworks();

                httpTest.ShouldHaveCalled("*/networks");
                Assert.NotNull(networks);
                Assert.Single(networks);
                Assert.Equal(networkId, networks.First().Id);
            }
        }

        [Fact]
        public void CreateNetwork()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                httpTest.RespondWithJson(new Network{Id = networkId});

                var definition = new NetworkDefinition();
                var network = _networkingService.CreateNetwork(definition);

                httpTest.ShouldHaveCalled("*/networks");
                Assert.NotNull(network);
                Assert.Equal(networkId, network.Id);
            }
        }

        [Fact]
        public void CreateNetworks()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new NetworkCollection(new[] { new Network { Name = "network-1"}, new Network{Name = "network-2"} }));

                var definitions = new[] {new NetworkDefinition(), new NetworkDefinition()};
                var networks = _networkingService.CreateNetworks(definitions);

                httpTest.ShouldHaveCalled("*/networks");
                Assert.NotNull(networks);
                Assert.Equal(2, networks.Count());
                Assert.Equal("network-1", networks.First().Name);
                Assert.Equal("network-2", networks.Last().Name);
            }
        }

        [Fact]
        public void GetNetwork()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                httpTest.RespondWithJson(new Network { Id = networkId });

                var network = _networkingService.GetNetwork(networkId);

                httpTest.ShouldHaveCalled("*/networks/" + networkId);
                Assert.NotNull(network);
                Assert.Equal(networkId, network.Id);
            }
        }

        [Fact]
        public void DeleteNetwork()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");

                _networkingService.DeleteNetwork(networkId);

                httpTest.ShouldHaveCalled("*/networks/" + networkId);
            }
        }

        [Fact]
        public void WhenDeleteNetwork_Returns404NotFound_ShouldConsiderRequestSuccessful()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Not here, boss...");

                _networkingService.DeleteNetwork(networkId);

                httpTest.ShouldHaveCalled("*/networks/" + networkId);
            }
        }

        [Fact]
        public void UpdateNetwork()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                httpTest.RespondWithJson(new Network {Id = networkId});

                var definition = new NetworkDefinition { Name = "new network name" };
                var network = _networkingService.UpdateNetwork(networkId, definition);

                httpTest.ShouldHaveCalled("*/networks/" + networkId);
                Assert.NotNull(network);
                Assert.Equal(networkId, network.Id);
            }
        }
    }
}
