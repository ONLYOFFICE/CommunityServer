using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.Compute.v2_1
{
    public class ImageTests : IDisposable
    {
        private readonly ComputeService _compute;
        private readonly ComputeTestDataManager _testData;

        public ImageTests(ITestOutputHelper testLog)
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
        public async Task ListImageSummariesTest()
        {
            var results = await _compute.ListImageSummariesAsync(new ImageListOptions {PageSize = 1});

            while (results.Any())
            {
                var result = results.First();
                Assert.NotNull(result.Name);

                Trace.WriteLine("Getting next page...");
                results = await results.GetNextPageAsync();
            }
            Assert.NotNull(results);
        }

        [Fact]
        public async Task FindSnapshotsTest()
        {
            Trace.WriteLine("Creating a test server...");
            var server = await _testData.CreateServer();
            await server.WaitUntilActiveAsync();
            Trace.WriteLine("Snapshotting server...");
            var snapshot = await server.SnapshotAsync(new SnapshotServerRequest(server.Name + "SNAPSHOT"));
            _testData.Register(snapshot);

            Trace.WriteLine("Getting snapshot details...");
            var results = await _compute.ListImagesAsync(new ImageListOptions {Type = ImageType.Snapshot});
            Assert.NotNull(results);
            Assert.All(results, x => Assert.Equal(ImageType.Snapshot, x.Type));
            Assert.Contains(results, image => image.Id == snapshot.Id);
        }

        [Fact]
        public async Task ListImagesTest()
        {
            var results = await _compute.ListImagesAsync(new ImageListOptions { PageSize = 1 });

            while (results.Any())
            {
                var result = results.First();
                Assert.NotNull(result.Size);
                results = await results.GetNextPageAsync();
            }
            Assert.NotNull(results);
        }

        [Fact]
        public async Task EditImageMetadataTest()
        {
            Trace.WriteLine("Creating a test server...");
            var server = await _testData.CreateServer();
            await server.WaitUntilActiveAsync();
            Trace.WriteLine("Snapshotting server...");
            var snapshot = await server.SnapshotAsync(new SnapshotServerRequest(server.Name + "SNAPSHOT")
            {
                Metadata =
                {
                    ["category"] = "ci_test",
                    ["bad_key"] = "value"
                }
            });
            _testData.Register(snapshot);

            Assert.True(snapshot.Metadata.ContainsKey("category"));

            // Edit immediately
            Trace.WriteLine("Adding a key...");
            await snapshot.Metadata.CreateAsync("new_key", "value");
            Assert.True(snapshot.Metadata.ContainsKey("new_key"));

            Trace.WriteLine("Removing a key...");
            await snapshot.Metadata.DeleteAsync("bad_key");
            Assert.False(snapshot.Metadata.ContainsKey("bad_key"));

            // Verify edits were persisted
            Trace.WriteLine("Retrieving metadata...");
            var metadata = await snapshot.GetMetadataAsync();
            Assert.True(metadata.ContainsKey("category"));
            Assert.True(metadata.ContainsKey("new_key"));
            Assert.False(metadata.ContainsKey("bad_key"));

            // Batch edit
            metadata.Remove("new_key");
            metadata["category"] = "updated";
            Trace.WriteLine("Updating edited metadata...");
            await metadata.UpdateAsync(overwrite: true);

            Assert.Equal("updated", metadata["category"]);
            Assert.False(metadata.ContainsKey("new_key"));
        }

        [Fact]
        public async Task DeleteImageTest()
        {
            Trace.WriteLine("Creating a server...");
            var server = await _testData.CreateServer();
            await server.WaitUntilActiveAsync();

            Trace.WriteLine("Snapshotting the server...");
            var snapshot = await server.SnapshotAsync(new SnapshotServerRequest(server.Name + "SNAPSHOT"));
            await snapshot.WaitUntilActiveAsync();

            Trace.WriteLine($"Deleting snapshot image {snapshot.Name}");

            await snapshot.DeleteAsync();
            await snapshot.WaitUntilDeletedAsync();
        }
    }
}
