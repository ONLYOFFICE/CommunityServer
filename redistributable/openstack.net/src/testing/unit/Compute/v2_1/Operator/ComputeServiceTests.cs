using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenStack.Testing;
using Xunit;

namespace OpenStack.Compute.v2_1.Operator
{
    public class ComputeServiceTests
    {
        private readonly ComputeService _compute;

        public ComputeServiceTests()
        {
            _compute = new ComputeService(Stubs.AuthenticationProvider, "region");
        }

        [Fact]
        public async Task GetCurrentQuotas()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(JObject.Parse(@"
{
  'quota_set': {
    'injected_file_content_bytes': 10240,
    'metadata_items': 128,
    'server_group_members': 10,
    'server_groups': 10,
    'ram': 51200,
    'floating_ips': 10,
    'key_pairs': 100,
    'id': 'details',
    'instances': 10,
    'security_group_rules': 20,
    'injected_files': 5,
    'cores': 20,
    'fixed_ips': -1,
    'injected_file_path_bytes': 255,
    'security_groups': 10
  }
}").ToString());

                var quotas = await _compute.GetCurrentQuotasAsync();

                httpTest.ShouldHaveCalled("*/os-quota-sets/detail");
                Assert.NotNull(quotas);

                Assert.Equal(100, quotas.KeyPairs);
                Assert.Equal(-1, quotas.FixedIPs);
            }
        }

        [Fact]
        public async Task GetDefaultQuotas()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(JObject.Parse(@"
{
  'quota_set': {
    'injected_file_content_bytes': 10240,
    'metadata_items': 128,
    'server_group_members': 10,
    'server_groups': 10,
    'ram': 51200,
    'floating_ips': 10,
    'key_pairs': 100,
    'id': 'defaults',
    'instances': 10,
    'security_group_rules': 20,
    'injected_files': 5,
    'cores': 20,
    'fixed_ips': -1,
    'injected_file_path_bytes': 255,
    'security_groups': 10
  }
}").ToString());

                var quotas = await _compute.GetDefaultQuotasAsync();

                httpTest.ShouldHaveCalled("*/os-quota-sets/defaults");
                Assert.NotNull(quotas);

                Assert.Equal(100, quotas.KeyPairs);
                Assert.Equal(-1, quotas.FixedIPs);
            }
        }
    }
}
