using System;
using OpenStack.Synchronous;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    [Trait("ci","false")] // TODO: Run these with the CI tests once Poppy is installed on our environment
    public class ContentDeliveryNetworkServiceTests : IDisposable
    {
        private readonly ContentDeliveryNetworkService _cdnService;

        public ContentDeliveryNetworkServiceTests(ITestOutputHelper testLog)
        {
            OpenStackNet.Tracing.Http.Listeners.Add(new XunitTraceListener(testLog));

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider();
            _cdnService = new ContentDeliveryNetworkService(authenticationProvider, "DFW");
        }

        public void Dispose()
        {
            OpenStackNet.Tracing.Http.Listeners.Clear();
        }

        [Fact]
        public void Ping()
        {
            _cdnService.Ping();
        }
    }
}
