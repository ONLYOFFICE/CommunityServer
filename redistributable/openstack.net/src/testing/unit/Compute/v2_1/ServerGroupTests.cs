using System;
using System.Linq;
using System.Net;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1
{
    public class ServerGroupTests
    {
        private readonly ComputeService _compute;

        public ServerGroupTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }
        
        [Fact]
        public void GetServerGroup()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new ServerGroup { Id = serverGroupId });

                var result = _compute.GetServerGroup(serverGroupId);

                httpTest.ShouldHaveCalled($"*/os-server-groups/{serverGroupId}");
                Assert.NotNull(result);
                Assert.Equal(serverGroupId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void CreateServerGroup()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new ServerGroup {Id = serverGroupId});

                var request = new ServerGroupDefinition("{name}", "{policy-name}");
                var result = _compute.CreateServerGroup(request);

                httpTest.ShouldHaveCalled("*/os-server-groups");
                Assert.Equal(serverGroupId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void ListServerGroups()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new ServerGroupCollection
                {
                    new ServerGroup {Id = serverGroupId}
                });

                var results = _compute.ListServerGroups();

                httpTest.ShouldHaveCalled("*/os-server-groups");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(serverGroupId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NotFound)]
        public void DeleteServerGroup(HttpStatusCode responseCode)
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new ServerGroup { Id = serverGroupId });
                httpTest.RespondWith((int)responseCode, "All gone!");

                var serverGroup = _compute.GetServerGroup(serverGroupId);

                serverGroup.Delete();
                httpTest.ShouldHaveCalled($"*/os-server-groups/{serverGroupId}");
            }
        }
    }
}
