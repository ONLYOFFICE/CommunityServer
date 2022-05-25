using System.Net.Http.Headers;

namespace System.Net.Http
{
    internal static class HttpHeadersExtensions
    {
        public const string AuthTokenHeader = "X-Auth-Token";

        public static void SetAuthToken(this HttpHeaders headers, string value)
        {
            if (headers.Contains(AuthTokenHeader))
                headers.Remove(AuthTokenHeader);

            headers.Add(AuthTokenHeader, value);
        }
    }
}