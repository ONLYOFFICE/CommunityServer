using System;
using System.Linq;
using System.Net;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Images.v2;
using OpenStack.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1
{
    public class ImageTests
    {
        private readonly ComputeService _compute;

        public ImageTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }
        
        [Fact]
        public void GetImage()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = "1";
                httpTest.RespondWithJson(new Image { Id = imageId });

                var result = _compute.GetImage(imageId);

                httpTest.ShouldHaveCalled($"*/images/{imageId}");
                Assert.NotNull(result);
                Assert.Equal(imageId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void GetImageMetadata()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = "1";
                httpTest.RespondWithJson(new ImageSummaryCollection { new ImageSummary { Id = imageId } });
                httpTest.RespondWithJson(new ImageMetadata { ["stuff"] = "things" });

                var imageReferences = _compute.ListImageSummaries();
                ImageMetadata result = imageReferences.First().GetMetadata();

                httpTest.ShouldHaveCalled($"*/images/{imageId}/metadata");
                Assert.NotNull(result);
                Assert.Single(result);
                Assert.True(result.ContainsKey("stuff"));
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void GetImageMetadataItem()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = "1";
                httpTest.RespondWithJson(new ImageSummaryCollection { new ImageSummary { Id = imageId } });
                httpTest.RespondWithJson(new
                {
                    meta = new
                    {
                        stuff = "things"
                    }

                });

                var imageReferences = _compute.ListImageSummaries();
                string result = imageReferences.First().GetMetadataItem("stuff");

                httpTest.ShouldHaveCalled($"*/images/{imageId}/metadata");
                Assert.NotNull(result);
                Assert.Equal("things", result);
            }
        }

        [Fact]
        public void GetImageExtension()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWithJson(new ImageSummaryCollection
                {
                    new ImageSummary {Id = imageId}
                });
                httpTest.RespondWithJson(new Image { Id = imageId });

                var results = _compute.ListImageSummaries();
                var flavorRef = results.First();
                var result = flavorRef.GetImage();

                Assert.NotNull(result);
                Assert.Equal(imageId, result.Id);
            }
        }

        [Fact]
        public void WaitForImageActive()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWithJson(new Image { Id = imageId, Status = ImageStatus.Unknown });
                httpTest.RespondWithJson(new Image { Id = imageId, Status = ImageStatus.Saving });
                httpTest.RespondWithJson(new Image { Id = imageId, Status = ImageStatus.Active });

                var result = _compute.GetImage(imageId);
                result.WaitUntilActive();

                httpTest.ShouldHaveCalled($"*/images/{imageId}");
                Assert.NotNull(result);
                Assert.Equal(imageId, result.Id);
                Assert.Equal(ImageStatus.Active, result.Status);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void CreateImageMetadata()
        {
            using (var httpTest = new HttpTest())
            {
                const string key = "stuff";
                Identifier imageId = "1";
                httpTest.RespondWithJson(new Image { Id = imageId });

                var image = _compute.GetImage(imageId);
                image.Metadata.Create(key, "things");

                Assert.True(image.Metadata.ContainsKey(key));
                httpTest.ShouldHaveCalled($"*/images/{imageId}/metadata/{key}");
            }
        }

        [Fact]
        public void ListImages()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWithJson(new ImageSummaryCollection
                {
                    Items = { new ImageSummary { Id = imageId } },
                    Links = { new PageLink("next", "http://api.com/next") }
                });

                var results = _compute.ListImageSummaries();

                httpTest.ShouldHaveCalled("*/images");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(imageId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void ListImagesWithFilter()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new ImageCollection());

                const string name = "foo";
                const int minRam = 2;
                const int minDisk = 1;
                Identifier serverId = Guid.NewGuid();
                var lastModified = DateTimeOffset.Now.AddDays(-1);
                var imageType = ImageType.Snapshot;
                
                _compute.ListImageSummaries(new ImageListOptions { Name = name, ServerId = serverId, UpdatedAfter = lastModified, MininumDiskSize = minDisk, MininumMemorySize = minRam, Type = imageType});

                httpTest.ShouldHaveCalled($"*name={name}");
                httpTest.ShouldHaveCalled($"*server={serverId}");
                httpTest.ShouldHaveCalled($"*minRam={minRam}");
                httpTest.ShouldHaveCalled($"*minDisk={minDisk}");
                httpTest.ShouldHaveCalled($"*type={imageType}");
                httpTest.ShouldHaveCalled("*changes-since=");
            }
        }

        [Fact]
        public void ListImagesWithPaging()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new ImageCollection());

                Identifier startingAt = Guid.NewGuid();
                const int pageSize = 10;
                _compute.ListImageSummaries(new ImageListOptions { PageSize = pageSize, StartingAt = startingAt });

                httpTest.ShouldHaveCalled($"*marker={startingAt}*");
                httpTest.ShouldHaveCalled($"*limit={pageSize}*");
            }
        }

        [Theory]
        [InlineData(false, "POST")]
        [InlineData(true, "PUT")]
        public void UpdateImageMetadata(bool overwrite, string expectedHttpVerb)
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = "1";
                httpTest.RespondWithJson(new Image {Id = imageId});
                httpTest.RespondWithJson(new ImageMetadata {["stuff"] = "things" });

                var image = _compute.GetImage(imageId);
                image.Metadata["color"] = "blue";
                image.Metadata.Update(overwrite);

                httpTest.ShouldHaveCalled($"*/images/{imageId}/metadata");
                Assert.Equal(expectedHttpVerb, httpTest.CallLog.Last().Request.Method.Method);
                Assert.True(image.Metadata.ContainsKey("stuff"));
            }
        }

        [Fact]
        public void DeleteImage()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");

                _compute.DeleteImage(imageId);

                httpTest.ShouldHaveCalled($"*/images/{imageId}");
            }
        }

        [Fact]
        public void DeleteImageExtension()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWithJson(new Image { Id = imageId });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");
                httpTest.RespondWithJson(new Image { Id = imageId, Status = ImageStatus.Deleted });

                var image = _compute.GetImage(imageId);

                image.Delete();
                image.WaitUntilDeleted();
                Assert.Equal(image.Status, ImageStatus.Deleted);
            }
        }

        [Fact]
        public void WhenDeleteImage_Returns404NotFound_ShouldConsiderRequestSuccessful()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Not here, boss...");

                _compute.DeleteImage(imageId);

                httpTest.ShouldHaveCalled($"*/images/{imageId}");
            }
        }

        [Fact]
        public void WaitForImageDeleted()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWithJson(new Image { Id = imageId, Status = ImageStatus.Active });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");
                httpTest.RespondWithJson(new Image { Id = imageId, Status = ImageStatus.Deleted });

                var result = _compute.GetImage(imageId);
                result.Delete();
                result.WaitUntilDeleted();

                Assert.Equal(ImageStatus.Deleted, result.Status);
            }
        }

        [Fact]
        public void WaitForImageDeleted_Returns404NotFound_ShouldConsiderRequestSuccessful()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWithJson(new Image { Id = imageId, Status = ImageStatus.Active });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Nothing here, boss");

                var result = _compute.GetImage(imageId);
                result.Delete();
                result.WaitUntilDeleted();

                Assert.Equal(ImageStatus.Deleted, result.Status);
            }
        }

        [Fact]
        public void DeleteImageMetadata()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                const string key = "stuff";
                httpTest.RespondWithJson(new Image
                {
                    Id = imageId,
                    Metadata =
                    {
                        [key] = "things"
                    }
                });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");

                var image = _compute.GetImage(imageId);

                image.Metadata.Delete(key);
                Assert.False(image.Metadata.ContainsKey(key));
                httpTest.ShouldHaveCalled($"*/images/{imageId}/metadata/{key}");
            }
        }

        [Fact]
        public void WhenDeleteImageMetadata_Returns404NotFound_ShouldConsiderRequestSuccessful()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Not here, boss...");

                _compute.DeleteImageMetadataAsync(imageId, "{invalid-key}");
            }
        }
    }
}
