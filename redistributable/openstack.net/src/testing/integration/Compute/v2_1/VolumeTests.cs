using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using OpenStack.BlockStorage.v2;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.Compute.v2_1
{
    public class VolumeTests : IDisposable
    {
        private readonly ComputeService _compute;
        private readonly ComputeTestDataManager _testData;

        public VolumeTests(ITestOutputHelper testLog)
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

        // Endpoint is documented but doesn't exist
        //[Fact]
        //public async Task ListVolumeTypes()
        //{
        //    var types = await _compute.ListVolumeTypesAsync();
        //    Assert.NotNull(types);
        //    Assert.Contains("lvm", types.Select(x => x.Name));
        //}

        [Fact]
        public async Task VolumesTest()
        {
            var volume = await _testData.CreateVolume();
            await volume.WaitUntilAvailableAsync();
            Assert.NotNull(volume.Id);
            Assert.NotNull(volume.Name);
            Assert.Null(volume.Description);
            Assert.Null(volume.SourceSnapshotId);
            Assert.Empty(volume.Attachments);

            var volumeDetails = await _compute.GetVolumeAsync(volume.Id);
            Assert.Equal(volume.Id, volumeDetails.Id);
            Assert.Equal(volume.Name, volumeDetails.Name);
            Assert.Equal(volume.Description, volumeDetails.Description);
            Assert.Equal(volumeDetails.SourceSnapshotId, volumeDetails.SourceSnapshotId);
            Assert.Empty(volumeDetails.Attachments);

            var results = await _compute.ListVolumesAsync();
            
            Assert.NotNull(results);
            Assert.Contains(volume.Id, results.Select(x => x.Id));
        }

        [Fact]
        public async Task AttachVolumeTest()
        {
            var server = await _testData.CreateServer();
            await server.WaitUntilActiveAsync();

            var volume = await _testData.CreateVolume();
            await volume.WaitUntilAvailableAsync();

            var serverVolume = await server.AttachVolumeAsync(new ServerVolumeDefinition(volume.Id));
            _testData.Register(serverVolume);
            await volume.WaitForStatusAsync(VolumeStatus.InUse);

            volume = await _compute.GetVolumeAsync(volume.Id);
            serverVolume = volume.Attachments.FirstOrDefault();
            
            Assert.NotNull(serverVolume);
            Assert.Equal(server.Id, serverVolume.ServerId);
            Assert.Equal(volume.Id, serverVolume.VolumeId);
        }

        [Fact]
        public async Task SnapshotsTest()
        {
            var volume = await _testData.CreateVolume();
            await volume.WaitUntilAvailableAsync();

            var snapshot = await volume.SnapshotAsync();
            _testData.Register(snapshot);
            await snapshot.WaitUntilAvailableAsync();
            Assert.NotNull(snapshot.Id);
            Assert.Equal(volume.Id, snapshot.VolumeId);
            Assert.Null(snapshot.Name);
            Assert.Null(snapshot.Description);

            var snapshotDetails = await _compute.GetVolumeSnapshotAsync(snapshot.Id);
            Assert.Equal(snapshot.Id, snapshotDetails.Id);
            Assert.Equal(snapshot.Name, snapshotDetails.Name);
            Assert.Equal(snapshot.Description, snapshotDetails.Description);
            Assert.Equal(snapshot.VolumeId, snapshotDetails.VolumeId);

            var results = await _compute.ListVolumeSnapshotsAsync();

            Assert.NotNull(results);
            Assert.Contains(snapshot.Id, results.Select(x => x.Id));
        }
    }
}
