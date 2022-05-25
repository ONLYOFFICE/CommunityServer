using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using OpenStack.Compute.v2_1;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;
using OpenStack.Networking.v2.Layer3.Synchronous;
using OpenStack.Networking.v2.Serialization;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;
using SecurityGroupCollection = OpenStack.Networking.v2.Serialization.SecurityGroupCollection;

namespace OpenStack.Networking.v2.Layer3
{
    public class Layer3Tests
    {
        private readonly NetworkingService _networking;

        public Layer3Tests()
        {
            _networking = new NetworkingService(Stubs.AuthenticationProvider, "region");
        }

        #region Routers
        [Fact]
        public void CreateRouter()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                Identifier routerId = Guid.NewGuid();
                httpTest.RespondWithJson(new Router { Id = routerId, ExternalGateway = new ExternalGateway { ExternalNetworkId = networkId } });

                var definition = new RouterCreateDefinition();
                var result = _networking.CreateRouter(definition);

                httpTest.ShouldHaveCalled("*/routers");
                Assert.NotNull(result);
                Assert.Equal(routerId, result.Id);
                Assert.Equal(networkId, result.ExternalGateway.ExternalNetworkId);
                Assert.IsType<NetworkingApiBuilder>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void GetRouter()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier routerId = Guid.NewGuid();
                httpTest.RespondWithJson(new Router { Id = routerId });

                var result = _networking.GetRouter(routerId);

                httpTest.ShouldHaveCalled($"*/routers/{routerId}");
                Assert.NotNull(result);
                Assert.Equal(routerId, result.Id);
                Assert.IsType<NetworkingApiBuilder>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void ListRouters()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier routerId = Guid.NewGuid();
                httpTest.RespondWithJson(new RouterCollection
                {
                    new Router { Id = routerId }
                });

                var results = _networking.ListRouters(new RouterListOptions { Status = RouterStatus.Active });

                httpTest.ShouldHaveCalled("*/routers?status=ACTIVE");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(routerId, result.Id);
                Assert.IsType<NetworkingApiBuilder>(((IServiceResource)result).Owner);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NotFound)]
        public void DeleteRouter(HttpStatusCode responseCode)
        {
            using (var httpTest = new HttpTest())
            {
                Identifier portId = Guid.NewGuid();
                Identifier routerId = Guid.NewGuid();
                httpTest.RespondWithJson(new Router { Id = routerId});
                httpTest.RespondWithJson(new PortCollection
                {
                    new Port {Id = portId}
                });
                httpTest.RespondWith((int)responseCode, "All gone!");

                var router = _networking.GetRouter(routerId);
                router.Delete();

                httpTest.ShouldHaveCalled($"*/routers/{routerId}/remove_router_interface");
                httpTest.ShouldHaveCalled($"*/routers/{routerId}");
            }
        }

        [Fact]
        public void AttachPort()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier portId = Guid.NewGuid();
                Identifier routerId = Guid.NewGuid();
                httpTest.RespondWithJson(new Router { Id = routerId});

                var router = _networking.GetRouter(routerId);
                router.AttachPort(portId);

                httpTest.ShouldHaveCalled($"*/routers/{routerId}/add_router_interface");
                var capturedRequest = httpTest.CallLog.Last();
                Assert.Equal(HttpMethod.Put, capturedRequest.Request.Method);
            }
        }

        [Fact]
        public void AttachSubnet()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier portId = Guid.NewGuid();
                Identifier subnetId = Guid.NewGuid();
                Identifier routerId = Guid.NewGuid();
                httpTest.RespondWithJson(new Router { Id = routerId });
                httpTest.RespondWithJson(new {port_id = portId, subnet_id = subnetId});

                var router = _networking.GetRouter(routerId);
                var result =router.AttachSubnet(subnetId);

                httpTest.ShouldHaveCalled($"*/routers/{routerId}/add_router_interface");
                var capturedRequest = httpTest.CallLog.Last();
                Assert.Equal(HttpMethod.Put, capturedRequest.Request.Method);
                Assert.NotNull(result);
                Assert.Equal(portId, result);
            }
        }
        #endregion

        #region Floating IPs
        [Fact]
        public void CreateFloatingIP()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier networkId = Guid.NewGuid();
                Identifier floatingIPId = Guid.NewGuid();
                httpTest.RespondWithJson(new FloatingIP { Id = floatingIPId });

                var definition = new FloatingIPCreateDefinition(networkId);
                var result = _networking.CreateFloatingIP(definition);

                httpTest.ShouldHaveCalled("*/floatingips");
                Assert.NotNull(result);
                Assert.Equal(floatingIPId, result.Id);
                Assert.IsType<NetworkingApiBuilder>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void GetFloatingIP()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier floatingIPId = Guid.NewGuid();
                httpTest.RespondWithJson(new FloatingIP { Id = floatingIPId });

                var result = _networking.GetFloatingIP(floatingIPId);

                httpTest.ShouldHaveCalled($"*/floatingips/{floatingIPId}");
                Assert.NotNull(result);
                Assert.Equal(floatingIPId, result.Id);
                Assert.IsType<NetworkingApiBuilder>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void ListFloatingIPs()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier floatingIPId = Guid.NewGuid();
                httpTest.RespondWithJson(new FloatingIPCollection
                {
                    new FloatingIP { Id = floatingIPId }
                });

                var results = _networking.ListFloatingIPs(new FloatingIPListOptions {Status = FloatingIPStatus.Active});

                httpTest.ShouldHaveCalled("*/floatingips?status=ACTIVE");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(floatingIPId, result.Id);
                Assert.IsType<NetworkingApiBuilder>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void AssociateFloatingIP()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier portId = Guid.NewGuid();
                Identifier floatingIPId = Guid.NewGuid();
                httpTest.RespondWithJson(new FloatingIP { Id = floatingIPId });
                httpTest.RespondWithJson(new FloatingIP { Id = floatingIPId, PortId = portId});

                var floatingIP = _networking.GetFloatingIP(floatingIPId);
                floatingIP.Associate(portId);

                httpTest.ShouldHaveCalled($"*/floatingips/{floatingIPId}");
                Assert.Equal(floatingIP.PortId, portId);
            }
        }

        [Fact]
        public void AssociateFloatingIPToServer()
        {
            var compute = new ComputeService(Stubs.AuthenticationProvider, "region");

            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                const string ip = "10.0.0.1";
                httpTest.RespondWithJson(new Server { Id = serverId });
                httpTest.RespondWith((int)HttpStatusCode.OK, "ip associated!");
                httpTest.RespondWithJson(new ServerAddressCollection
                {
                    ["network1"] = new List<ServerAddress>
                    {
                        new ServerAddress {IP = ip, Type = AddressType.Floating}
                    }
                });

                var server = compute.GetServer(serverId);
                server.AssociateFloatingIP(new AssociateFloatingIPRequest(ip));

                Assert.NotNull(server.Addresses["network1"].Single(a => a.IP == ip && a.Type == AddressType.Floating));
            }
        }

        [Fact]
        public void DisassociateFloatingIP()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier portId = Guid.NewGuid();
                Identifier floatingIPId = Guid.NewGuid();
                httpTest.RespondWithJson(new FloatingIP { Id = floatingIPId, PortId =  portId});
                httpTest.RespondWithJson(new FloatingIP { Id = floatingIPId });

                var floatingIP = _networking.GetFloatingIP(floatingIPId);
                floatingIP.Disassociate();

                httpTest.ShouldHaveCalled($"*/floatingips/{floatingIPId}");
                Assert.Null(floatingIP.PortId);
            }
        }

        [Fact]
        public void DisassociateFloatingIPFromServer()
        {
            var compute = new ComputeService(Stubs.AuthenticationProvider, "region");

            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                const string ip = "10.0.0.1";
                httpTest.RespondWithJson(new Server
                {
                    Id = serverId,
                    Addresses =
                    {
                        ["network1"] = new List<ServerAddress>
                        {
                            new ServerAddress {IP = ip, Type = AddressType.Floating}
                        }
                    }
                });
                httpTest.RespondWith((int)HttpStatusCode.OK, "ip disassociated!");

                var server = compute.GetServer(serverId);
                server.DisassociateFloatingIP(ip);

                Assert.Null(server.Addresses["network1"].FirstOrDefault(a => a.IP == ip && a.Type == AddressType.Floating));
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NotFound)]
        public void DeleteFloatingIP(HttpStatusCode responseCode)
        {
            using (var httpTest = new HttpTest())
            {
                Identifier floatingIPId = Guid.NewGuid();
                httpTest.RespondWith((int)responseCode, "All gone!");

                _networking.DeleteFloatingIP(floatingIPId);

                httpTest.ShouldHaveCalled($"*/floatingips/{floatingIPId}");
            }
        }
        #endregion

        #region  Security Groups
        [Fact]
        public void ListSecurityGroups()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                Identifier securityGroupRuleId = Guid.NewGuid();
                SecurityGroupRule rule = new SecurityGroupRule { Id = securityGroupRuleId };
                List<SecurityGroupRule> rules = new List<SecurityGroupRule> { rule };

                httpTest.RespondWithJson(new SecurityGroupCollection
                {
                    new SecurityGroup { Id = securityGroupId, SecurityGroupRules = rules }
                });

                var results = _networking.ListSecurityGroups();

                httpTest.ShouldHaveCalled("*/security-groups");
                Assert.Single(results);
                var result = results.First();
                var resultRule = result.SecurityGroupRules.First();
                Assert.Equal(securityGroupId, result.Id);
                Assert.Equal(rule.Id, resultRule.Id);
            }
        }

        [Fact]
        public void ListSecurityGroupRules()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                Identifier securityGroupRuleId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroupRuleCollection
                {
                    new SecurityGroupRule {Id = securityGroupRuleId, SecurityGroupId = securityGroupId}
                });

                var results = _networking.ListSecurityGroupRules();

                httpTest.ShouldHaveCalled("*/security-group-rules");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(securityGroupRuleId, result.Id);
                Assert.Equal(securityGroupId, result.SecurityGroupId);
            }
        }
        #endregion

    }
}
