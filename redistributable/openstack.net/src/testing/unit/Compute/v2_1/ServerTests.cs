using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1
{
    public class ServerTests
    {
        private readonly ComputeService _compute;

        public ServerTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }

        [Fact]
        public void SerializeServerWithSchedulerHints()
        {
            string expectedJson = JObject.Parse(@"{'server':{'name':'name','imageRef':'00000000-0000-0000-0000-000000000000','flavorRef':'00000000-0000-0000-0000-000000000000'},'os:scheduler_hints':{'group':'groupId'}}")
                .ToString(Formatting.None);
            var server = new ServerCreateDefinition("name", Guid.Empty, Guid.Empty);
            server.SchedulerHints = new SchedulerHints();
            server.SchedulerHints.Add("group", "groupId");

            var json = OpenStackNet.Serialize(server);
            Assert.Equal(expectedJson, json);
        }

        [Fact]
        public void SerializeServerWithoutSchedulerHints()
        {
            string expectedJson = JObject.Parse(@"{'server':{'name':'name','imageRef':'00000000-0000-0000-0000-000000000000','flavorRef':'00000000-0000-0000-0000-000000000000'}}")
                .ToString(Formatting.None);
            var server = new ServerCreateDefinition("name", Guid.Empty, Guid.Empty);

            var json = OpenStackNet.Serialize(server);
            Assert.Equal(expectedJson, json);
        }

        [Fact]
        public void SerializeListServerOptionsInUrl()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new ServerSummaryCollection());
                _compute.ListServerSummaries(new ServerListOptions());
                httpTest.ShouldNotHaveCalled("*metadata*");
            }   
        }

        [Fact]
        public void CreateServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server {Id = serverId});

                var definition = new ServerCreateDefinition("{name}", Guid.NewGuid(), "{flavor-id}");
                var result = _compute.CreateServer(definition);

                httpTest.ShouldHaveCalled("*/servers");
                Assert.NotNull(result);
                Assert.Equal(serverId,result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void CreateServerMetadata()
        {
            using (var httpTest = new HttpTest())
            {
                const string key = "stuff";
                Identifier serverId = "1";
                httpTest.RespondWithJson(new Server { Id = serverId });

                var server = _compute.GetServer(serverId);
                server.Metadata.Create(key, "things");

                Assert.True(server.Metadata.ContainsKey(key));
                httpTest.ShouldHaveCalled($"*/servers/{serverId}/metadata/{key}");
            }
        }

        [Fact]
        public void GetServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });

                var result = _compute.GetServer(serverId);

                httpTest.ShouldHaveCalled($"*/servers/{serverId}");
                Assert.NotNull(result);
                Assert.Equal(serverId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void GetServerExtension()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new ServerSummaryCollection
                {
                    Items = { new ServerSummary { Id = serverId } }
                });
                httpTest.RespondWithJson(new Server { Id = serverId });

                var serverRef = _compute.ListServerSummaries().First();
                var result = serverRef.GetServer();
                
                Assert.NotNull(result);
                Assert.Equal(serverId, result.Id);
            }
        }

        [Fact]
        public void GetServerMetadata()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = "1";
                httpTest.RespondWithJson(new ServerSummaryCollection { new ServerSummary { Id = serverId } });
                httpTest.RespondWithJson(new ServerMetadata { ["stuff"] = "things" });

                var servers = _compute.ListServerSummaries();
                ServerMetadata result = servers.First().GetMetadata();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/metadata");
                Assert.NotNull(result);
                Assert.Single(result);
                Assert.True(result.ContainsKey("stuff"));
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void GetServerMetadataItem()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = "1";
                httpTest.RespondWithJson(new ServerSummaryCollection { new ServerSummary { Id = serverId } });
                httpTest.RespondWithJson(new
                {
                    meta = new
                    {
                        stuff = "things"
                    }

                });

                var servers = _compute.ListServerSummaries();
                string result = servers.First().GetMetadataItem("stuff");

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/metadata");
                Assert.NotNull(result);
                Assert.Equal("things", result);
            }
        }

        [Fact]
        public void WaitForServerActive()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId, Status = ServerStatus.Building});
                httpTest.RespondWithJson(new Server { Id = serverId, Status = ServerStatus.Active });

                var result = _compute.GetServer(serverId);
                result.WaitUntilActive();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}");
                Assert.NotNull(result);
                Assert.Equal(serverId, result.Id);
                Assert.Equal(ServerStatus.Active, result.Status);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void ListServerSummaries()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new ServerSummaryCollection
                {
                   Items = { new ServerSummary { Id = serverId}},
                   Links = { new PageLink("next", "http://api.com/next") }
                });

                var results = _compute.ListServerSummaries();

                httpTest.ShouldHaveCalled("*/servers");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(serverId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void ListServerSummariesWithFilter()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new ServerCollection());

                const string name = "foo";
                const string flavorId = "1";
                Identifier imageId = Guid.NewGuid();
                var lastModified = DateTimeOffset.Now.AddDays(-1);
                ServerStatus status = ServerStatus.Active;

                _compute.ListServerSummaries(new ServerListOptions { Name = name, FlavorId = flavorId, ImageId = imageId, UpdatedAfter = lastModified, Status = status});

                httpTest.ShouldHaveCalled($"*name={name}");
                httpTest.ShouldHaveCalled($"*flavor={flavorId}");
                httpTest.ShouldHaveCalled($"*image={imageId}");
                httpTest.ShouldHaveCalled($"*status={status}");
                httpTest.ShouldHaveCalled("*changes-since=");
            }
        }

        [Fact]
        public void ListServerSummariesWithPaging()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new ServerCollection
                {
                    Items = {new Server()},
                    Links = {new PageLink("next", "http://example.com")}
                });

                Identifier startingAt = Guid.NewGuid();
                const int pageSize = 10;
                _compute.ListServerSummaries(new ServerListOptions { PageSize = pageSize, StartingAt = startingAt });

                httpTest.ShouldHaveCalled($"*marker={startingAt}*");
                httpTest.ShouldHaveCalled($"*limit={pageSize}*");
            }
        }

        [Fact]
        public void UpdateServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId, Name = "{new-name}"});

                var request = new ServerUpdateDefinition {Name = "{new-name}"};
                var result = _compute.UpdateServer(serverId, request);

                httpTest.ShouldHaveCalled($"*/servers/{serverId}");
                Assert.NotNull(result);
                Assert.Equal(serverId, result.Id);
                Assert.Equal(request.Name, result.Name);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void UpdateServerExtension()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId, Name = "{old-name}" });
                var lastModified = DateTimeOffset.Now;
                httpTest.RespondWithJson(new Server { Id = serverId, Name = "{new-name}", LastModified = lastModified});

                var server = _compute.GetServer(serverId);
                server.Name = "{new-name}";
                server.Update();

                Assert.Equal(serverId, server.Id);
                Assert.Equal("{new-name}", server.Name);
                Assert.Equal(lastModified, server.LastModified);
                Assert.IsType<ComputeApi>(((IServiceResource)server).Owner);
            }
        }

        [Theory]
        [InlineData(false, "POST")]
        [InlineData(true, "PUT")]
        public void UpdateServerMetadata(bool overwrite, string expectedHttpVerb)
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = "1";
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWithJson(new ServerMetadata { ["stuff"] = "things" });

                var server = _compute.GetServer(serverId);
                server.Metadata["color"] = "blue";
                server.Metadata.Update(overwrite);

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/metadata");
                Assert.Equal(expectedHttpVerb, httpTest.CallLog.Last().Request.Method.Method);
                Assert.True(server.Metadata.ContainsKey("stuff"));
            }
        }

        [Fact]
        public void DeleteServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");

                _compute.DeleteServer(serverId);

                httpTest.ShouldHaveCalled($"*/servers/{serverId}");
            }
        }

        [Fact]
        public void DeleteServerExtension()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server {Id = serverId});
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");
                httpTest.RespondWithJson(new Server { Id = serverId, Status = ServerStatus.Deleted});

                var server =_compute.GetServer(serverId);
                server.Delete();
                Assert.NotEqual(server.Status, ServerStatus.Deleted);

                server.WaitUntilDeleted();
                Assert.Equal(server.Status, ServerStatus.Deleted);
            }
        }

        [Fact]
        public void WhenDeleteServer_Returns404NotFound_ShouldConsiderRequestSuccessful()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Not here, boss...");

                _compute.DeleteServer(serverId);

                httpTest.ShouldHaveCalled($"*/servers/{serverId}");
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NotFound)]
        public void DeleteServerMetadata(HttpStatusCode responseCode)
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                const string key = "stuff";
                httpTest.RespondWithJson(new Server
                {
                    Id = serverId,
                    Metadata =
                    {
                        [key] = "things"
                    }
                });
                httpTest.RespondWith((int)responseCode, "All gone!");

                var server = _compute.GetServer(serverId);

                server.Metadata.Delete(key);
                Assert.False(server.Metadata.ContainsKey(key));
                httpTest.ShouldHaveCalled($"*/servers/{serverId}/metadata/{key}");
            }
        }

        [Fact]
        public void WaitForServerDeleted()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId, Status = ServerStatus.Active });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");
                httpTest.RespondWithJson(new Server { Id = serverId, Status = ServerStatus.Deleted });

                var result = _compute.GetServer(serverId);
                result.Delete();
                result.WaitUntilDeleted();
                
                Assert.Equal(ServerStatus.Deleted, result.Status);
            }
        }

        [Fact]
        public void WaitForServerDeleted_Returns404NotFound_ShouldConsiderRequestSuccessful()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId, Status = ServerStatus.Active });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Nothing here, boss");

                var result = _compute.GetServer(serverId);
                result.Delete();
                result.WaitUntilDeleted();

                Assert.Equal(ServerStatus.Deleted, result.Status);
            }
        }

        [Fact]
        public void SnapshotServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                Identifier imageId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, boss");
                httpTest.ResponseQueue.Last().Headers.Location = new Uri($"http://api.example.com/images/{imageId}");
                httpTest.RespondWithJson(new Image { Id = imageId });

                var server = _compute.GetServer(serverId);
                Image result = server.Snapshot(new SnapshotServerRequest("{image-name"));

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("createImage", httpTest.CallLog.First(x => x.Url.EndsWith("/action")).RequestBody);
                Assert.NotNull(result);
                Assert.Equal(imageId, result.Id);
            }
        }

        [Fact]
        public void StartServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, boss");

                var server = _compute.GetServer(serverId);
                server.Start();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("os-start", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void StopServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, boss");

                var server = _compute.GetServer(serverId);
                server.Stop();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("os-stop", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void SuspendServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, boss");

                var server = _compute.GetServer(serverId);
                server.Suspend();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("suspend", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void ResumeServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, boss");

                var server = _compute.GetServer(serverId);
                server.Resume();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("resume", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void RebootServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, boss");

                var server = _compute.GetServer(serverId);
                server.Reboot(new RebootServerRequest {Type = RebootType.Hard});

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                string lastRequest = httpTest.CallLog.Last().RequestBody;
                Assert.Contains("reboot", lastRequest);
                Assert.Contains("HARD", lastRequest);
            }
        }
        
        [Fact]
        public void AttachVolume()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWithJson(new ServerVolume {Id = volumeId, DeviceName = "/dev/vdd"});

                var server = _compute.GetServer(serverId);
                var result = server.AttachVolume(new ServerVolumeDefinition(volumeId));

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/os-volume_attachments");
                Assert.NotNull(result);
                Assert.Equal(volumeId, result.Id);
                Assert.Contains(server.AttachedVolumes, v => v.Id == volumeId);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void DetachVolume()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server
                {
                    Id = serverId,
                    AttachedVolumes = { new ServerVolume { Id = volumeId, DeviceName = "/dev/vdd" } }
                });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, good buddy");

                var server = _compute.GetServer(serverId);
                ServerVolumeReference attachedVolume = server.AttachedVolumes[0];
                attachedVolume.Detach();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/os-volume_attachments/{volumeId}");
                Assert.DoesNotContain(server.AttachedVolumes, v => v.Id == volumeId);
            }
        }

        [Fact]
        public void GetVolume()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server {Id = serverId, AttachedVolumes = {new ServerVolumeReference {Id = volumeId}}});
                httpTest.RespondWithJson(new ServerVolume {Id = volumeId, DeviceName = "/dev/vdd"});

                var server = _compute.GetServer(serverId);
                var result = server.AttachedVolumes[0].GetServerVolume();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/os-volume_attachments/{volumeId}");
                Assert.NotNull(result);
                Assert.Equal(volumeId, result.Id);
            }
        }

        [Fact]
        public void ListVolumes()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier volumeId = Guid.NewGuid();
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server {Id = serverId});
                httpTest.RespondWithJson(new ServerVolumeCollection
                {
                    new ServerVolume
                    {
                        Id = volumeId, DeviceName = "/dev/vdd"
                    }
                });

                var server = _compute.GetServer(serverId);
                var results = server.ListVolumes();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/os-volume_attachments");
                Assert.NotNull(results);
                Assert.Single(results);
                Assert.Equal(volumeId, results.First().Id);
            }
        }

        [Theory]
        [InlineData("novnc")]
        [InlineData("xvpvnc")]
        public void GetVncConsole(string typeName)
        {
            var type = StringEnumeration.FromDisplayName<RemoteConsoleType>(typeName);
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server {Id = serverId});
                httpTest.RespondWithJson(new RemoteConsole {Type = type});

                var server = _compute.GetServer(serverId);
                var result = server.GetVncConsole(RemoteConsoleType.NoVnc);
                
                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("os-getVNCConsole", httpTest.CallLog.Last().RequestBody);
                Assert.NotNull(result);
                Assert.Equal(type, result.Type);
            }
        }

        [Fact]
        public void GetSpiceConsole()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWithJson(new RemoteConsole { Type = RemoteConsoleType.SpiceHtml5 });

                var server = _compute.GetServer(serverId);
                var result = server.GetSpiceConsole();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("os-getSPICEConsole", httpTest.CallLog.Last().RequestBody);
                Assert.NotNull(result);
                Assert.Equal(RemoteConsoleType.SpiceHtml5, result.Type);
            }
        }

        [Fact]
        public void GetSerialConsole()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWithJson(new RemoteConsole { Type = RemoteConsoleType.Serial });

                var server = _compute.GetServer(serverId);
                var result = server.GetSerialConsole();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("os-getSerialConsole", httpTest.CallLog.Last().RequestBody);
                Assert.NotNull(result);
                Assert.Equal(RemoteConsoleType.Serial, result.Type);
            }
        }

        [Fact]
        public void GetRdpConsole()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWithJson(new RemoteConsole { Type = RemoteConsoleType.RdpHtml5 });

                var server = _compute.GetServer(serverId);
                var result = server.GetRdpConsole();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("os-getRDPConsole", httpTest.CallLog.Last().RequestBody);
                Assert.NotNull(result);
                Assert.Equal(RemoteConsoleType.RdpHtml5, result.Type);
            }
        }

        [Fact]
        public void GetConsoleOutput()
        {
            const string output = "FAKE CONSOLE OUTPUT\nANOTHER\nLAST LINE";
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith(JObject.Parse("{'output': '" + output + "'}").ToString());

                var server = _compute.GetServer(serverId);
                var result = server.GetConsoleOutput();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("os-getConsoleOutput", httpTest.CallLog.Last().RequestBody);
                Assert.Equal(output, result);
            }
        }

        [Fact]
        public void RescueServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server {Id = serverId});
                httpTest.RespondWithJson(new {adminPass = "top-secret"});

                var server = _compute.GetServer(serverId);
                server.Rescue();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("rescue", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void UnrescueServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWithJson(new { adminPass = "top-secret" });

                var server = _compute.GetServer(serverId);
                server.UnrescueAsync();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("unrescue", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void ResizeServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier flavorId = "1";
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, good buddy!");

                var server = _compute.GetServer(serverId);
                server.Resize(flavorId);

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("resize", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void ConfirmResizeServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier flavorId = "1";
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, good buddy!");
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, good buddy!");

                var server = _compute.GetServer(serverId);
                server.Resize(flavorId);
                server.ConfirmResize();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("confirmResize", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void CancelResizeServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier flavorId = "1";
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, good buddy!");
                httpTest.RespondWith((int)HttpStatusCode.Accepted, "Roger that, good buddy!");

                var server = _compute.GetServer(serverId);
                server.Resize(flavorId);
                server.CancelResize();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/action");
                Assert.Contains("revertResize", httpTest.CallLog.Last().RequestBody);
            }
        }

        [Fact]
        public void ListServerActions()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                Identifier actionId = Guid.NewGuid();
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWithJson(new ServerActionSummaryCollection {new ServerActionSummary {Id = actionId, ServerId = serverId, Name = "create"}});
                httpTest.RespondWithJson(new ServerAction
                {
                    Id = actionId,
                    Name = "create",
                    Events = {new ServerEvent {Name = "create_instance"}}
                });

                var server = _compute.GetServer(serverId);
                var results = server.ListActionSummaries();

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/os-instance-actions");
                Assert.NotNull(results);
                Assert.Single(results);

                var actionRef = results.First();
                Assert.Equal(actionId, actionRef.Id);
                Assert.NotNull(actionRef.Name);

                var action = actionRef.GetAction();
                httpTest.ShouldHaveCalled($"*/servers/{serverId}/os-instance-actions/{actionId}");
                Assert.NotNull(action);
                Assert.NotNull(action.Name);
                Assert.NotNull(action.Events);
                Assert.Equal(1, action.Events.Count);
                Assert.NotNull(action.Events.First().Name);
            }
        }
    }
}
