using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using OpenStack.BlockStorage.v2;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1
{
    public class VolumeTests
    {
        private readonly ComputeService _compute;

        public VolumeTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }

        [Fact]
        public void DeserializeVolumeWithEmptyAttachment()
        {
            var json = JObject.Parse(@"{'volume': {'attachments': [{}]}}").ToString();
            var result = OpenStackNet.Deserialize<Volume>(json);
            Assert.Empty(result.Attachments);
        }

        [Fact]
        public void GetVolume()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                httpTest.RespondWithJson(new Volume { Id = volumeId });

                var result = _compute.GetVolume(volumeId);

                httpTest.ShouldHaveCalled($"*/os-volumes/{volumeId}");
                Assert.NotNull(result);
                Assert.Equal(volumeId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void GetVolumeSnapshot()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier snapshotId = Guid.NewGuid();
                httpTest.RespondWithJson(new VolumeSnapshot { Id = snapshotId });

                var result = _compute.GetVolumeSnapshot(snapshotId);

                httpTest.ShouldHaveCalled($"*/os-snapshots/{snapshotId}");
                Assert.NotNull(result);
                Assert.Equal(snapshotId, result.Id);
            }
        }

        [Fact]
        public void CreateVolume()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                httpTest.RespondWithJson(new Volume {Id = volumeId});

                var request = new VolumeDefinition(size: 1);
                var result = _compute.CreateVolume(request);

                httpTest.ShouldHaveCalled("*/os-volumes");
                Assert.Equal(volumeId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void WaitForVolumeAvailable()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                httpTest.RespondWithJson(new Volume { Id = volumeId, Status = VolumeStatus.Creating });
                httpTest.RespondWithJson(new Volume { Id = volumeId, Status = VolumeStatus.Available });

                var result = _compute.GetVolume(volumeId);
                result.WaitUntilAvailable();

                httpTest.ShouldHaveCalled($"*/os-volumes/{volumeId}");
                Assert.NotNull(result);
                Assert.Equal(volumeId, result.Id);
                Assert.Equal(VolumeStatus.Available, result.Status);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void SnapshotVolume()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                httpTest.RespondWithJson(new Volume() { Id = volumeId });
                httpTest.RespondWithJson(new VolumeSnapshot { VolumeId = volumeId });

                var volume = _compute.GetVolume(volumeId);
                var result = volume.Snapshot();

                httpTest.ShouldHaveCalled("*/os-snapshots");
                Assert.Contains(volumeId, httpTest.CallLog.Last().RequestBody);
                Assert.Equal(volumeId, result.VolumeId);
                Assert.IsType<ComputeApi>(((IServiceResource)volume).Owner);
            }
        }

        [Fact]
        public void ListVolumes()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                httpTest.RespondWithJson(new VolumeCollection
                {
                    new Volume {Id = volumeId}
                });

                var results = _compute.ListVolumes();

                httpTest.ShouldHaveCalled("*/os-volumes");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(volumeId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        //[Fact]
        //public void ListVolumeTypes()
        //{
        //    using (var httpTest = new HttpTest())
        //    {
        //        Identifier volumeTypeId = Guid.NewGuid();
        //        httpTest.RespondWithJson(new VolumeTypeCollection
        //        {
        //            new VolumeType {Id = volumeTypeId}
        //        });

        //        var results = _compute.ListVolumeTypes();

        //        httpTest.ShouldHaveCalled("*/os-volume-types");
        //        Assert.Equal(1, results.Count());
        //        var result = results.First();
        //        Assert.Equal(volumeTypeId, result.Id);
        //    }
        //}

        [Fact]
        public void ListVolumeSnapshots()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier snapshotId = Guid.NewGuid();
                httpTest.RespondWithJson(new VolumeSnapshotCollection
                {
                    new VolumeSnapshot {Id = snapshotId}
                });

                var results = _compute.ListVolumeSnapshots();

                httpTest.ShouldHaveCalled("*/os-snapshots");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(snapshotId, result.Id);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NotFound)]
        public void DeleteVolume(HttpStatusCode responseCode)
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                httpTest.RespondWithJson(new Volume { Id = volumeId });
                httpTest.RespondWith((int)responseCode, "All gone!");

                var volume = _compute.GetVolume(volumeId);

                volume.Delete();
                httpTest.ShouldHaveCalled($"*/os-volumes/{volumeId}");
            }
        }

        [Fact]
        public void WaitForVolumeDeleted()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                httpTest.RespondWithJson(new Volume { Id = volumeId, Status = VolumeStatus.Available });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "All gone!");
                httpTest.RespondWithJson(new Volume { Id = volumeId, Status = VolumeStatus.Deleting });
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Not here, boss!");

                var result = _compute.GetVolume(volumeId);
                result.Delete();
                result.WaitUntilDeleted();
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NotFound)]
        public void DeleteVolumeSnapshot(HttpStatusCode responseCode)
        {
            using (var httpTest = new HttpTest())
            {
                Identifier snapshotId = Guid.NewGuid();
                httpTest.RespondWithJson(new VolumeSnapshot { Id = snapshotId });
                httpTest.RespondWith((int)responseCode, "All gone!");

                var snapshot = _compute.GetVolumeSnapshot(snapshotId);

                snapshot.Delete();
                httpTest.ShouldHaveCalled($"*/os-snapshots/{snapshotId}");
            }
        }
    }
}
