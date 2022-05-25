using Flurl.Http.Testing;
using OpenStack.Synchronous;
using Xunit;
using HttpTest = OpenStack.Testing.HttpTest;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    public class ContentDeliveryNetworkServiceTests
    {
        private const string Region = "DFW";

        [Fact]
        public void Ping()
        {
            using (new HttpTest())
            {
                var service = new ContentDeliveryNetworkService(Stubs.AuthenticationProvider, Region);

                service.Ping();
            }
        }
    }
}
