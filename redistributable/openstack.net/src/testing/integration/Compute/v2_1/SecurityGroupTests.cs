using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Xunit;
using Xunit.Abstractions;
using VolumeState = net.openstack.Core.Domain.VolumeState;

namespace OpenStack.Compute.v2_1
{
    public class SecurityGroupTests : IDisposable
    {
        private readonly ComputeService _compute;
        private readonly ComputeTestDataManager _testData;

        public SecurityGroupTests(ITestOutputHelper testLog)
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
        public async Task SecurityGroupTest()
        {
            var definition = _testData.BuildSecurityGroup();
            Trace.WriteLine($"Creating security group named: {definition.Name}");
            var securityGroup = await _testData.CreateSecurityGroup(definition);
            
            Trace.WriteLine("Verifying security group matches requested definition...");
            Assert.NotNull(securityGroup);
            Assert.Equal(definition.Name, securityGroup.Name);
            Assert.Equal(definition.Description, securityGroup.Description);

            Trace.WriteLine("Creatinga server associated with the security group...");
            var serverDefinition = _testData.BuildServer();
            serverDefinition.SecurityGroups.Add(new SecurityGroupReference());
            Trace.WriteLine("Updating the security group...");
            string updatedName = securityGroup.Name + "UPDATED";
            securityGroup.Name = updatedName;
            await securityGroup.UpdateAsync();
            Assert.Equal(updatedName, securityGroup.Name);

            Trace.WriteLine("Verifying the updated security group matches...");
            securityGroup = await _compute.GetSecurityGroupAsync(securityGroup.Id);
            Assert.NotNull(securityGroup);
            Assert.Equal(updatedName, securityGroup.Name);
            Assert.Equal(definition.Description, securityGroup.Description);
            
            Trace.WriteLine("Deleting the security group...");
            await securityGroup.DeleteAsync();

            await Assert.ThrowsAsync<FlurlHttpException>(() => _compute.GetSecurityGroupAsync(securityGroup.Id));

        }

        [Fact]
        public async Task ListSecurityGroupsTest()
        {
            var groups = await _compute.ListSecurityGroupsAsync();
            Assert.NotEmpty(groups);
            Assert.Contains(groups, x => x.Name == "default");
        }

        [Fact]
        public async Task SecurityGroupRuleTest()
        {
            Trace.WriteLine("Creating security group...");
            var securityGroup = await _testData.CreateSecurityGroup();
            
            Trace.WriteLine("Adding a rule...");
            var ruleDefinition = new SecurityGroupRuleDefinition(IPProtocol.TCP, 22, 22, "0.0.0.0/24");
            var rule = await securityGroup.AddRuleAsync(ruleDefinition);

            Trace.WriteLine("Verifying rule matches requested definition...");
            Assert.NotNull(rule);
            Assert.Equal(ruleDefinition.Protocol, rule.Protocol);
            Assert.Equal(ruleDefinition.ToPort, rule.ToPort);
            Assert.Equal(ruleDefinition.FromPort, rule.FromPort);
            Assert.Equal(ruleDefinition.CIDR, rule.CIDR);
            Assert.Equal(securityGroup.Id, rule.GroupId);

            Trace.WriteLine("Deleting the rule...");
            await rule.DeleteAsync();

            securityGroup = await _compute.GetSecurityGroupAsync(securityGroup.Id);
            Assert.DoesNotContain(securityGroup.Rules, x => x.Id == rule.Id);
        }
    }
}
