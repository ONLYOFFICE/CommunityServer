using SelectelSharp.Requests;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SelectelSharp
{
    public class SelectelClient
    {
        public string StorageUrl { get; private set; }
        public string CDNUrl { get; private set; }
             
        public string AuthToken { get; private set; }
        public long ExpireAuthToken { get; private set; }

        public SelectelClient() { }

        public SelectelClient(string proxyUrl = null, string proxyUser = null, string proxyPassword = null)
        {
            var proxy = new WebProxy(proxyUrl, true);
            proxy.Credentials = new NetworkCredential(proxyUser, proxyPassword);
            WebRequest.DefaultWebProxy = proxy;
        }

        public SelectelClient(WebProxy proxy = null)
        {
            WebRequest.DefaultWebProxy = proxy;
        }

        public async Task AuthorizeAsync(string user, string key)
        {
            var result = await ExecuteAsync(new AuthRequest(user, key));

            this.StorageUrl = result.StorageUrl;
            this.AuthToken = result.AuthToken;
            this.ExpireAuthToken = result.ExpireAuthToken;
            this.CDNUrl = this.StorageUrl.Replace("https:", "http:").Replace(".ru", ".com");          

        }

        public void Authorize(string user, string key)
        {
            AuthorizeAsync(user, key).Wait();
        }

        public async Task<T> ExecuteAsync<T>(BaseRequest<T> request)
        {
            if (!request.AllowAnonymously)
            {
                CheckTokenNotNull();
            }

            await request.Execute(StorageUrl, AuthToken);
            return request.Result;
        }

        private void CheckTokenNotNull()
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                throw new Exception("You should first authorize this client. Call AuthorizeAsync method.");
            }
        }
    }
}
