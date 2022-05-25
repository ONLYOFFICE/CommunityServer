using System;
using System.Linq;
using System.Net;
using net.openstack.Core.Domain;
using Newtonsoft.Json;
using Rackspace.CloudNetworks.v2.Serialization;
using Rackspace.Synchronous;
using Rackspace.Testing;
using Xunit;

namespace Rackspace.CloudNetworks.v2
{
    public class SubnetTests
    {
        private readonly CloudNetworkService _cloudNetworkService;

        public SubnetTests()
        {
            _cloudNetworkService = new CloudNetworkService(Stubs.IdentityService, "region");
        }

        [Fact]
        public void SubnetCollectionSerialization()
        {
            var subnets = new SubnetCollection
            {
                Subnets = { new Subnet { Id = Guid.NewGuid() } },
                SubnetsLinks = { new Link("next", "http://api.com/next") }
            };
            string json = JsonConvert.SerializeObject(subnets, Formatting.None);
            Assert.Contains("\"subnets\"", json);
            Assert.DoesNotContain("\"subnet\"", json);

            var result = JsonConvert.DeserializeObject<SubnetCollection>(json);
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
            Assert.Equal(1, result.Subnets.Count());
            Assert.Equal(1, result.SubnetsLinks.Count());
        }

        [Fact]
        public void ListSubnets()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWithJson(new SubnetCollection {Subnets = {new Subnet {Id = subnetId}}});

                var subnets = _cloudNetworkService.ListSubnets();

                httpTest.ShouldHaveCalled("*/subnets");
                Assert.NotNull(subnets);
                Assert.Equal(1, subnets.Count());
                Assert.Equal(subnetId, subnets.First().Id);
            }
        }

        [Fact]
        public void PageSubnets()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWithJson(new SubnetCollection
                {
                    Subnets = { new Subnet { Id = subnetId } },
                    SubnetsLinks = { new Link("next", "http://api.com/next") }
                });

                IPage<Subnet> subnets = _cloudNetworkService.ListSubnets(subnetId, 10);

                httpTest.ShouldHaveCalled(string.Format($"*/subnets?marker={subnetId}&limit=10"));
                Assert.NotNull(subnets);
                Assert.Equal(1, subnets.Count());
                Assert.Equal(subnetId, subnets.First().Id);
                Assert.True(subnets.HasNextPage);
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
                var subnet = _cloudNetworkService.CreateSubnet(definition);
                
                httpTest.ShouldHaveCalled("*/subnets");
                Assert.NotNull(subnet);
                Assert.Equal(subnetId, subnet.Id);
            }
        }
        
        [Fact]
        public void GetSubnets()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier subnetId = Guid.NewGuid();
                httpTest.RespondWithJson(new Subnet { Id = subnetId });

                var subnet = _cloudNetworkService.GetSubnet(subnetId);

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

                _cloudNetworkService.DeleteSubnet(subnetId);

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

                _cloudNetworkService.DeleteSubnet(subnetId);

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
                var subnet = _cloudNetworkService.UpdateSubnet(subnetId, definition);

                httpTest.ShouldHaveCalled("*/subnets/" + subnetId);
                Assert.NotNull(subnet);
                Assert.Equal(subnetId, subnet.Id);
            }
        }
    }
}
