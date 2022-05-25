using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Flurl.Extensions;
using Flurl.Http;
using Newtonsoft.Json;
using OpenStack.Serialization;
using OpenStack.Testing;
using Xunit;
using Xunit.Abstractions;
#pragma warning disable 618 // We can remove this once OpenStackNet.Configure() is made internal and obsolete is removed

namespace OpenStack
{
    public class OpenStackNetTests : IDisposable
    {
        public OpenStackNetTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            Trace.Listeners.Add(testOutput);
        }

        public void Dispose()
        {
            OpenStackNet.ResetDefaults();
        }

        [Fact]
        public void WhenConfigureIsCalled_GlobalFlurlConfiguration_IsNotAltered()
        {
            // User makes their own tweaks to Flurl
            Action<HttpCall> customErrorHandler = call => Debug.WriteLine("I saw an error!");
            FlurlHttp.GlobalSettings.OnError = customErrorHandler;

            // We configure openstack.net
            OpenStackNet.Configure();

            // We shouldn't have clobbered their settings
            Assert.Null(FlurlHttp.GlobalSettings.AfterCall);
            Assert.Null(FlurlHttp.GlobalSettings.AfterCallAsync);
            Assert.Null(FlurlHttp.GlobalSettings.BeforeCall);
            Assert.Null(FlurlHttp.GlobalSettings.BeforeCallAsync);
            Assert.Equal(customErrorHandler, FlurlHttp.GlobalSettings.OnError);
        }

        [Fact]
        public void WhenResetDefaultsIsCalled_GlobalFlurlConfiguration_IsNotAltered()
        {
            // User makes their own tweaks to Flurl
            Action<HttpCall> customErrorHandler = call => Debug.WriteLine("I saw an error!");
            FlurlHttp.GlobalSettings.OnError = customErrorHandler;

            // We configure openstack.net
            OpenStackNet.ResetDefaults();

            // We shouldn't have clobbered their settings
            Assert.Null(FlurlHttp.GlobalSettings.AfterCall);
            Assert.Null(FlurlHttp.GlobalSettings.AfterCallAsync);
            Assert.Null(FlurlHttp.GlobalSettings.BeforeCall);
            Assert.Null(FlurlHttp.GlobalSettings.BeforeCallAsync);
            Assert.Equal(customErrorHandler, FlurlHttp.GlobalSettings.OnError);
        }

        [Fact]
        public void WhenResetDefaultsIsCalled_GlobalJsonConfiguration_IsNotAltered()
        {
            // User makes their own tweaks to the serialization
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings();

            // We configure openstack.net
            OpenStackNet.ResetDefaults();

            // We shouldn't have clobbered their settings
            var serializer = JsonSerializer.CreateDefault();
            Assert.IsNotType<OpenStackContractResolver>(serializer.ContractResolver);
        }

        [Fact]
        public void WhenConfigureIsCalled_GlobalJsonConfiguration_IsNotAltered()
        {
            // User makes their own tweaks to the serialization
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings();

            // We configure openstack.net
            OpenStackNet.Configure();

            // We shouldn't have clobbered their settings
            var serializer = JsonSerializer.CreateDefault();
            Assert.IsNotType<OpenStackContractResolver>(serializer.ContractResolver);
        }

        [Fact]
        public async Task UserAgentTest()
        {
            using (var httpTest = new HttpTest())
            {
                OpenStackNet.Configure();

                await "http://api.com".PrepareRequest().GetAsync();

                var userAgent = httpTest.CallLog[0].Request.Headers.UserAgent.ToString();
                Assert.Contains("openstack.net", userAgent);
            }
        }

        [Fact]
        public async Task UserAgentOnlyListOnceTest()
        {
            using (var httpTest = new HttpTest())
            {
                OpenStackNet.Configure();
                OpenStackNet.Configure(); // Duplicate call to Configure should be ignored

                await "http://api.com".PrepareRequest().GetAsync();

                var userAgent = httpTest.CallLog[0].Request.Headers.UserAgent.ToString();
                Assert.Contains("openstack.net", userAgent);
            }
        }

        [Fact]
        public async Task UserAgentWithApplicationSuffixTest()
        {
            using (var httpTest = new HttpTest())
            {
                // Apply a custom application user agent
                OpenStackNet.Configuring += options =>
                {
                    options.UserAgents.Add(new ProductInfoHeaderValue("(unittests)"));
                };

                await "http://api.com".PrepareRequest().GetAsync();

                var userAgent = httpTest.CallLog[0].Request.Headers.UserAgent.ToString();
                Assert.Contains("openstack.net", userAgent);
                Assert.Contains("unittests", userAgent);
            }
        }
    }
}
