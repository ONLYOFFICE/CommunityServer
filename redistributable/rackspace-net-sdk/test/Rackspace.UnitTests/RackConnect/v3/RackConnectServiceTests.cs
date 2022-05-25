using System;
using Flurl.Http;
using Xunit;

namespace Rackspace.RackConnect.v3
{
    public class RackConnectServiceTests : IDisposable
    {
        public RackConnectServiceTests()
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
            new RackConnectService(Stubs.IdentityService, "region");
            Assert.NotNull(FlurlHttp.Configuration.BeforeCall);
        }
    }
}
