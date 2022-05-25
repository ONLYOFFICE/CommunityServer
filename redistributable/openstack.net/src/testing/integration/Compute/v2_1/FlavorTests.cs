using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.Compute.v2_1
{
    public class FlavorTests : IDisposable
    {
        private readonly ComputeService _compute;
        private readonly ComputeTestDataManager _testData;

        public FlavorTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider();
            _compute = new ComputeService(authenticationProvider, "RegionOne");

            _testData = new ComputeTestDataManager(_compute);
        }

        public void Dispose()
        {
            _testData.Dispose();

            Trace.Listeners.Clear();
            OpenStackNet.Tracing.Http.Listeners.Clear();
        }
        
        [Fact]
        public async Task ListFlavorSummariesTest()
        {
            var results = await _compute.ListFlavorSummariesAsync();

            Assert.NotNull(results);
            Assert.All(results, result => Assert.NotNull(result.Id));
            Assert.All(results, result => Assert.NotNull(result.Name));
        }

        [Fact]
        public async Task GetFlavorTest()
        {
            var results = await _compute.ListFlavorSummariesAsync();

            var flavorRef = results.FirstOrDefault(x => x.Name == "m1.tiny");
            var flavor = await flavorRef.GetFlavorAsync();
            Assert.NotNull(flavor);
            Assert.Equal(flavorRef.Id, flavor.Id);
            Assert.Equal(flavorRef.Name, flavor.Name);
            Assert.True(flavor.DiskSize > 0);
            Assert.True(flavor.MemorySize > 0);
            Assert.True(flavor.VirtualCPUs > 0);
            Assert.NotNull(flavor.EphemeralDiskSize);
            Assert.Null(flavor.SwapSize);
        }

        [Fact]
        public async Task ListFlavorsTest()
        {
            var results = await _compute.ListFlavorsAsync();

            Assert.NotNull(results);
            Assert.True(results.Any());

            Assert.All(results, result => Assert.NotNull(result.Id));
            Assert.All(results, result => Assert.NotNull(result.Name));
            Assert.All(results, result => Assert.True(result.DiskSize > 0));
            Assert.All(results, result => Assert.True(result.MemorySize > 0));
            Assert.All(results, result => Assert.True(result.VirtualCPUs > 0));
        }
    }
}
