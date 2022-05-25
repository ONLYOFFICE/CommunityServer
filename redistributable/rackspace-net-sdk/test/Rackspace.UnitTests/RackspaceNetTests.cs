using System;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http;
using OpenStack;
using Rackspace.Testing;
using Xunit;

namespace Rackspace
{
    public class RackspaceNetTests : IDisposable
    {
        public RackspaceNetTests()
        {
            RackspaceNet.ResetDefaults();
        }

        public void Dispose()
        {
            RackspaceNet.ResetDefaults();
        }

        [Fact]
        public async Task UseBothOpenStackAndRackspace_OpenStackConfiguredFirst()
        {
            using (var httpTest = new HttpTest())
            {
                OpenStackNet.Configure();
                RackspaceNet.Configure();

                await "http://api.com".GetAsync();

                var userAgent = httpTest.CallLog[0].Request.Headers.UserAgent.ToString();

                var rackspaceMatches = new Regex("rackspace").Matches(userAgent);
                Assert.Equal(1, rackspaceMatches.Count);

                var openstackMatches = new Regex("openstack").Matches(userAgent);
                Assert.Equal(1, openstackMatches.Count);
            }
        }

        [Fact]
        public async Task UseBothOpenStackAndRackspace_RackspaceConfiguredFirst()
        {
            using (var httpTest = new HttpTest())
            {
                RackspaceNet.Configure();
                OpenStackNet.Configure();

                await "http://api.com".GetAsync();

                var userAgent = httpTest.CallLog[0].Request.Headers.UserAgent.ToString();

                var rackspaceMatches = new Regex("rackspace").Matches(userAgent);
                Assert.Equal(1, rackspaceMatches.Count);

                var openstackMatches = new Regex("openstack").Matches(userAgent);
                Assert.Equal(1, openstackMatches.Count);
            }
        }

        [Fact]
        public void ResetDefaults_ResetsFlurlConfiguration()
        {
            RackspaceNet.Configure();
            Assert.NotNull(FlurlHttp.Configuration.BeforeCall);
            RackspaceNet.ResetDefaults();
            Assert.Null(FlurlHttp.Configuration.BeforeCall);
        }

        [Fact]
        public async Task UserAgentTest()
        {
            using (var httpTest = new HttpTest())
            {
                RackspaceNet.Configure();

                await "http://api.com".GetAsync();

                var userAgent = httpTest.CallLog[0].Request.Headers.UserAgent.ToString();
                Assert.Contains("rackspace.net", userAgent);
                Assert.Contains("openstack.net", userAgent);
            }
        }

        [Fact]
        public async Task UserAgentOnlyListedOnceTest()
        {
            using (var httpTest = new HttpTest())
            {
                RackspaceNet.Configure();
                RackspaceNet.Configure();

                await "http://api.com".GetAsync();

                var userAgent = httpTest.CallLog[0].Request.Headers.UserAgent.ToString();

                var rackspaceMatches = new Regex("rackspace").Matches(userAgent);
                Assert.Equal(1, rackspaceMatches.Count);

                var openstackMatches = new Regex("openstack").Matches(userAgent);
                Assert.Equal(1, openstackMatches.Count);
            }
        }

        [Fact]
        public async Task UserAgentWithApplicationSuffixTest()
        {
            using (var httpTest = new HttpTest())
            {
                RackspaceNet.Configure(configure: options => options.UserAgents.Add(new ProductInfoHeaderValue("(unit-tests)")));

                await "http://api.com".GetAsync();

                var userAgent = httpTest.CallLog[0].Request.Headers.UserAgent.ToString();
                Assert.Contains("rackspace.net", userAgent);
                Assert.Contains("unit-tests", userAgent);
            }
        }
    }
}
