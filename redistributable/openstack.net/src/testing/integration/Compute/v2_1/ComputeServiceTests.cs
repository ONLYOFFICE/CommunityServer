using System;
using System.Diagnostics;
using OpenStack.Serialization;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace OpenStack.Compute.v2_1
{
    public class ComputeServiceTests : IDisposable
    {
        private readonly ComputeService _compute;

        public ComputeServiceTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider();
            _compute = new ComputeService(authenticationProvider, "RegionOne");
        }

        public void Dispose()
        {
            Trace.Listeners.Clear();
            OpenStackNet.Tracing.Http.Listeners.Clear();
        }

        [Fact]
        public async Task GetLimitsTest()
        {
            var limits = await _compute.GetLimitsAsync();
            Assert.NotNull(limits);
            Assert.Empty(limits.RateLimits);
            Assert.NotNull(limits.ResourceLimits);
            Assert.NotNull(limits.ResourceLimits.ServersMax);
            Assert.NotNull(limits.ResourceLimits.ServersUsed);
            Assert.Empty(((IHaveExtraData)limits).Data);
        }
    }
}
