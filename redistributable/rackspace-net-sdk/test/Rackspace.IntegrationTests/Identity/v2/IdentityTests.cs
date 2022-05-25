using System.Diagnostics;
using net.openstack.Providers.Rackspace;
using Xunit;
using Xunit.Abstractions;

namespace Rackspace.Identity.v2
{
    public class IdentityTests
    {
        private readonly CloudIdentityProvider _identityService;

        public IdentityTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            RackspaceNet.Tracing.Http.Listeners.Add(testOutput);

            _identityService = TestIdentityProvider.GetIdentityProvider();
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
