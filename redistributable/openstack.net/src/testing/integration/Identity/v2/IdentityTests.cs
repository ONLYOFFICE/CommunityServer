using System.Diagnostics;
using net.openstack.Core.Providers;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.Identity.v2
{
    public class IdentityTests
    {
        private readonly OpenStackIdentityProvider _identityService;

        public IdentityTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);

            _identityService = (OpenStackIdentityProvider) TestIdentityProvider.GetIdentityProvider();
        }

        [Fact]
        public void AuthenticateTest()
        {
            var userAccess = _identityService.Authenticate();
            Assert.NotNull(userAccess);
            Assert.NotNull(userAccess.Token);
            Assert.NotNull(userAccess.Token.Id);
            Assert.NotNull(userAccess.ServiceCatalog);
            Assert.NotEmpty(userAccess.ServiceCatalog);
        }
    }
}
