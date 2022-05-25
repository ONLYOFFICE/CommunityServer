using System.Linq;
using Newtonsoft.Json.Linq;
using OpenStack.Synchronous;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1
{
    public class ComputeServiceTests
    {
        private readonly ComputeService _compute;

        public ComputeServiceTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }

        [Fact]
        public void GetLimits()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(JObject.Parse(@"
{
    'limits': {
        'rate': [
            {
                'limit': [
                    {
                    'next-available': '2012-09-10T20:11:45.146Z',
                    'remaining': 0,
                    'unit': 'DAY',
                    'value': 0,
                    'verb': 'POST'
                },
                {
                    'next-available': '2012-09-10T20:11:45.146Z',
                    'remaining': 0,
                    'unit': 'MINUTE',
                    'value': 0,
                    'verb': 'GET'
                }
            ],
            'regex': '/v[^/]/(\\d+)/(rax-networks)/?.*',
            'uri': '/rax-networks'
            }
        ],
        'absolute': {
            'maxServerMeta': 128,
            'maxPersonality': 5,
            'totalServerGroupsUsed': 0,
            'maxImageMeta': 128,
            'maxPersonalitySize': 10240,
            'maxTotalKeypairs': 100,
            'maxSecurityGroupRules': 20,
            'maxServerGroups': 10,
            'totalCoresUsed': 1,
            'totalRAMUsed': 2048,
            'totalInstancesUsed': 1,
            'maxSecurityGroups': 10,
            'totalFloatingIpsUsed': 0,
            'maxTotalCores': 20,
            'maxServerGroupMembers': 10,
            'maxTotalFloatingIps': 10,
            'totalSecurityGroupsUsed': 1,
            'maxTotalInstances': 10,
            'maxTotalRAMSize': 51200
        }
    }
}").ToString());

                var limits = _compute.GetLimits();

                Assert.NotNull(limits);

                Assert.NotNull(limits.RateLimits);
                Assert.Equal(1, limits.RateLimits.Count);
                var networkLimits = limits.RateLimits.FirstOrDefault(l => l.Name.Contains("rax-networks"));
                Assert.NotNull(networkLimits);
                var networkGetLimit = networkLimits.Limits.FirstOrDefault(l => l.HttpMethod == "GET");
                Assert.NotNull(networkGetLimit);
                
                Assert.NotNull(limits.ResourceLimits);
                Assert.NotNull(limits.ResourceLimits.CoresMax);
            }
        }
    }
}
