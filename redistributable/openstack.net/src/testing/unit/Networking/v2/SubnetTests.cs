using System;
using System.Linq;
using System.Net;
using OpenStack.Networking.v2.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Networking.v2
{
    public class SubnetTests
    {
        private readonly NetworkingService _networkingService;

        public SubnetTests()
        {
            _networkingService = new NetworkingService(Stubs.AuthenticationProvider, "region");
        }

        [Fact]
        public void ListSubnets()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWithJson(new SubnetCollection(new[] {new Subnet {Id = subnetId}}));

                var subnets = _networkingService.ListSubnets();

                httpTest.ShouldHaveCalled("*/subnets");
                Assert.NotNull(subnets);
                Assert.Single(subnets);
                Assert.Equal(subnetId, subnets.First().Id);
            }
        }

        [Fact]
        public void CreateSubnet()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWithJson(new Subnet { Id = subnetId });

                var definition = new SubnetCreateDefinition(networkId, IPVersion.IPv4, "10.0.0.0/24");
                var subnet = _networkingService.CreateSubnet(definition);
                
                httpTest.ShouldHaveCalled("*/subnets");
                Assert.NotNull(subnet);
                Assert.Equal(subnetId, subnet.Id);
            }
        }

        [Fact]
        public void CreateSubnets()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                httpTest.RespondWithJson(new SubnetCollection(new[] { new Subnet { Name = "subnet-1" }, new Subnet { Name = "subnet-2" } }));

                var definitions = new[]
                {
                    new SubnetCreateDefinition(networkId, IPVersion.IPv4, "{cidr-1}"),
                    new SubnetCreateDefinition(networkId, IPVersion.IPv6, "{cidr-2}")
                };
                var subnets = _networkingService.CreateSubnets(definitions);

                httpTest.ShouldHaveCalled("*/subnets");
                Assert.NotNull(subnets);
                Assert.Equal(2, subnets.Count());
                Assert.Equal("subnet-1", subnets.First().Name);
                Assert.Equal("subnet-2", subnets.Last().Name);
            }
        }

        [Fact]
        public void GetSubnets()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWithJson(new Subnet { Id = subnetId });

                var subnet = _networkingService.GetSubnet(subnetId);

                httpTest.ShouldHaveCalled("*/subnets/" + subnetId);
                Assert.NotNull(subnet);
                Assert.Equal(subnetId, subnet.Id);
            }
        }

        [Fact]
        public void DeleteSubnet()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");

                _networkingService.DeleteSubnet(subnetId);

                httpTest.ShouldHaveCalled("*/subnets/" + subnetId);
            }
        }

        [Fact]
        public void WhenDeleteSubnet_Returns404NotFound_ShouldConsiderRequestSuccessful()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Not here, boss...");

                _networkingService.DeleteSubnet(subnetId);

                httpTest.ShouldHaveCalled("*/subnets/" + subnetId);
            }
        }

        [Fact]
        public void UpdateSubnet()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWithJson(new Subnet { Id = subnetId });

                var definition = new SubnetUpdateDefinition { Name = "new subnet name" };
                var subnet = _networkingService.UpdateSubnet(subnetId, definition);

                httpTest.ShouldHaveCalled("*/subnets/" + subnetId);
                Assert.NotNull(subnet);
                Assert.Equal(subnetId, subnet.Id);
            }
        }
    }
}
