using System;
using System.Linq;
using OpenStack.Synchronous;
using Xunit;
using HttpTest = OpenStack.Testing.HttpTest;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    public class FlavorTests
    {
        private const string DefaultRegion = "DFW";

        [Fact]
        public void ListFlavors()
        {
            using (var httpTest = new HttpTest())
            {
                var cdnService = new ContentDeliveryNetworkService(Stubs.AuthenticationProvider, DefaultRegion);
                httpTest.RespondWithJson(new FlavorCollection(new[] {new Flavor()}));

                var flavors = cdnService.ListFlavors();

                Assert.NotNull(flavors);
                Assert.Single(flavors);
            }
        }

        [Fact]
        public void GetFlavor()
        {
            using (var httpTest = new HttpTest())
            {
                var cdnService = new ContentDeliveryNetworkService(Stubs.AuthenticationProvider, DefaultRegion);
                httpTest.RespondWithJson(new Flavor {Id = "flavor-id"});

                var flavor = cdnService.GetFlavor("flavor-id");

                Assert.NotNull(flavor);
                Assert.Equal("flavor-id", flavor.Id);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WhenGetFlavor_IsCalledWithoutAnId_ItShoudldThrowAnException(string flavorId)
        {
            using (new HttpTest())
            {
                var cdnService = new ContentDeliveryNetworkService(Stubs.AuthenticationProvider, DefaultRegion);

                Assert.Throws<ArgumentNullException>(() => cdnService.GetFlavor(flavorId));
            }
        }
    }
}
