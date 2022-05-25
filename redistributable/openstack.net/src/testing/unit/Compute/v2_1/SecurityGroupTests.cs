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
    public class SecurityGroupTests
    {
        private readonly ComputeService _compute;

        public SecurityGroupTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }
        
        [Fact]
        public void GetSecurityGroup()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroup { Id = securityGroupId });

                var result = _compute.GetSecurityGroup(securityGroupId);

                httpTest.ShouldHaveCalled($"*/os-security-groups/{securityGroupId}");
                Assert.NotNull(result);
                Assert.Equal(securityGroupId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void GetSecurityGroupFromReferenceTest()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                Identifier securityGroupId = Guid.NewGuid();
                const string securityGroupName = "{group-name}";
                httpTest.RespondWithJson(new Server
                {
                    SecurityGroups = {new SecurityGroupReference {Name = securityGroupName}}
                });
                httpTest.RespondWithJson(new SecurityGroupCollection
                {
                    new SecurityGroup {Id = Guid.NewGuid(), Name = "default"},
                    new SecurityGroup {Id = securityGroupId, Name = securityGroupName}
                });
                httpTest.RespondWithJson(new SecurityGroup { Id = securityGroupId });

                var server = _compute.GetServer(serverId);
                var securityGroupRef = server.SecurityGroups.First();

                SecurityGroup result = securityGroupRef.GetSecurityGroup();

                Assert.NotNull(result);
                Assert.Equal(securityGroupId, result.Id);
            }
        }

        [Fact]
        public void CreateSecurityGroup()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroup {Id = securityGroupId});

                var request = new SecurityGroupDefinition("{name}", "{description}");
                var result = _compute.CreateSecurityGroup(request);

                httpTest.ShouldHaveCalled("*/os-security-groups");
                Assert.Equal(securityGroupId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void DeserializeSecurityGroupRule()
        {
            const string json = @"{
'security_group_rule': {
    'from_port': 22,
    'group': {},
    'ip_protocol': 'tcp',
    'to_port': 22,
    'parent_group_id': 'bfa0c0ed-274b-4711-b3ee-37d35660fb06',
    'ip_range': {
        'cidr': '0.0.0.0 / 24'
    },
    'id': '55d75417-37df-48e2-96aa-20ba53a82900'
}}";
            var result = OpenStackNet.Deserialize<SecurityGroupRule>(JObject.Parse(json).ToString());
            Assert.Equal("0.0.0.0 / 24", result.CIDR);
        }

        [Fact]
        public void AddSecurityGroupRule()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                Identifier securityGroupRuleId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroup { Id = securityGroupId });
                httpTest.RespondWithJson(new SecurityGroupRule {Id = securityGroupRuleId});

                var request = new SecurityGroupRuleDefinition(IPProtocol.TCP, 0, 0, "{cidr}");
                var securityGroup = _compute.GetSecurityGroup(securityGroupId);
                var result = securityGroup.AddRule(request);

                httpTest.ShouldHaveCalled("*/os-security-group-rules");
                Assert.Contains(securityGroupId, httpTest.CallLog.Last().RequestBody); // Make sure that we automatically set the parent group id

                Assert.NotNull(result);
                Assert.Contains(result, securityGroup.Rules);
                Assert.Equal(securityGroupRuleId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void ListSecurityGroups()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroupCollection
                {
                    new SecurityGroup {Id = securityGroupId}
                });

                var results = _compute.ListSecurityGroups();

                httpTest.ShouldHaveCalled("*/os-security-groups");
                Assert.Single(results);
                var result = results.First();
                Assert.Equal(securityGroupId, result.Id);
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }

        [Fact]
        public void ListSecurityGroupForServer()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier serverId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroupCollection
                {
                    new SecurityGroup {Id = Guid.NewGuid()}
                });

                var results = _compute.ListSecurityGroups(serverId);

                httpTest.ShouldHaveCalled($"*/servers/{serverId}/os-security-groups");
                Assert.Single(results);
                var result = results.First();
                Assert.IsType<ComputeApi>(((IServiceResource)result).Owner);
            }
        }
        
        [Fact]
        public void DeleteSecurityGroup()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroup { Id = securityGroupId });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");

                var securityGroup = _compute.GetSecurityGroup(securityGroupId);

                securityGroup.Delete();
                httpTest.ShouldHaveCalled($"*/os-security-groups/{securityGroupId}");
            }
        }

        [Fact]
        public void DeleteSecurityGroupReference()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroup { Id = securityGroupId });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");

                SecurityGroupReference securityGroup = _compute.GetSecurityGroup(securityGroupId);

                securityGroup.Delete();
                httpTest.ShouldHaveCalled($"*/os-security-groups/{securityGroupId}");
            }
        }

        [Fact]
        public void DeleteSecurityGroupRule()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                Identifier securityGroupRuleId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroup
                {
                    Id = securityGroupId,
                    Rules =
                    {
                        new SecurityGroupRule {Id = securityGroupRuleId}
                    }
                });
                httpTest.RespondWith((int)HttpStatusCode.NoContent, "All gone!");

                var securityGroup = _compute.GetSecurityGroup(securityGroupId);
                var rule = securityGroup.Rules.First();
                rule.Delete();

                httpTest.ShouldHaveCalled($"*/os-security-groups/{securityGroupId}");
            }
        }

        [Fact]
        public void WhenDeleteSecurityGroup_Returns404NotFound_ShouldConsiderRequestSuccessful()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                httpTest.RespondWith((int)HttpStatusCode.NotFound, "Not here, boss...");

                _compute.DeleteSecurityGroup(securityGroupId);

                httpTest.ShouldHaveCalled($"*/os-security-groups/{securityGroupId}");
            }
        }

        [Fact]
        public void UpdateSecurityGroup()
        {
            using (var httpTest = new HttpTest())
            {
                Identifier securityGroupId = Guid.NewGuid();
                httpTest.RespondWithJson(new SecurityGroup {Id = securityGroupId});

                var securityGroup = _compute.GetSecurityGroup(securityGroupId);
                securityGroup.Name = "new-name";
                securityGroup.Update();

                httpTest.ShouldHaveCalled($"*/os-security-groups/{securityGroupId}");
            }
        }
    }
}
