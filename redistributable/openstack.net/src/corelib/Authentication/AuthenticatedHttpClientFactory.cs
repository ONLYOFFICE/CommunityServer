using System.Net.Http;

using Flurl.Http;
using Flurl.Http.Configuration;

namespace OpenStack.Authentication
{
    /// <summary>
    /// Instructs Flurl to use an <see cref="AuthenticatedMessageHandler"/> for all requests.
    /// </summary>
    /// <exclude />
    public class AuthenticatedHttpClientFactory : DefaultHttpClientFactory
    {
        /// <inheritdoc/>
        public HttpClient CreateClient(HttpMessageHandler handler)
        {
            return new HttpClient(handler)
            {
                Timeout = (System.TimeSpan)FlurlHttp.GlobalSettings.Defaults.Timeout
            };
        }

        /// <inheritdoc/>
        public HttpMessageHandler CreateMessageHandler()
        {
            return new AuthenticatedMessageHandler(base.CreateMessageHandler());
        }

    }
}