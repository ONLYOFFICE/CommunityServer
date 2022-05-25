using System;
using System.Net.Http;

using Flurl.Http.Configuration;

using OpenStack.Authentication;

namespace OpenStack.Testing
{
    /// <summary>
    /// Use this instead of <see cref="Flurl.Http.Testing.HttpTest"/> for any OpenStack.NET unit tests.
    /// <para>
    /// This extends Flurl's default HttpTest to use <see cref="AuthenticatedMessageHandler"/> in unit tests. 
    /// If you use the default HttpTest, then any tests which rely upon authentication handling (e.g retrying a request when a token expires) will fail.
    /// </para>
    /// </summary>
    public class HttpTest : Flurl.Http.Testing.HttpTest, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTest"/> class.
        /// </summary>
        public HttpTest()
        {
            OpenStackNet.ResetDefaults();
            OpenStackNet.Configuring += SetTestMode;
        }

        private void SetTestMode(OpenStackNetConfigurationOptions options)
        {
            options.FlurlHttpSettings.HttpClientFactory = new TestHttpClientFactory();
        }

        /// <inheritdoc />
        public new void Dispose()
        {
            OpenStackNet.ResetDefaults();
            base.Dispose();
        }

        class TestHttpClientFactory : IHttpClientFactory
        {
            private readonly Flurl.Http.Testing.TestHttpClientFactory _testMessageHandler;
            private readonly AuthenticatedHttpClientFactory _authenticatedClientFactory;

            public TestHttpClientFactory()
            {
                _testMessageHandler = new Flurl.Http.Testing.TestHttpClientFactory();
                _authenticatedClientFactory = new AuthenticatedHttpClientFactory();
            }

            public HttpClient CreateHttpClient(HttpMessageHandler handler)
            {
                return _authenticatedClientFactory.CreateClient(handler);
            }

            public HttpMessageHandler CreateMessageHandler()
            {
                return new AuthenticatedMessageHandler(_testMessageHandler.CreateMessageHandler());
            }
        }
    }
}