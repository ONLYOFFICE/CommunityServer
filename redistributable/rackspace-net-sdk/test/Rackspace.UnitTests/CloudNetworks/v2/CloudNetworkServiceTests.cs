using System;
using Flurl.Http;
using Xunit;

namespace Rackspace.CloudNetworks.v2
{
    public class CloudNetworkServiceTests : IDisposable
    {
        public CloudNetworkServiceTests()
        {
            RackspaceNet.ResetDefaults();
        }

        public void Dispose()
        {
            RackspaceNet.ResetDefaults();
        }

        [Fact]
        public void RackspaceNetIsConfiguredByService()
        {
            Assert.Null(FlurlHttp.Configuration.BeforeCall);
            new CloudNetworkService(Stubs.IdentityService, "region");
            Assert.NotNull(FlurlHttp.Configuration.BeforeCall);
        }
    }
}
