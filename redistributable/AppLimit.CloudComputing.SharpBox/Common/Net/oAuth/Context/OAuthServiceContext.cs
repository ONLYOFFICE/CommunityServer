namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context
{
    internal class OAuthServiceContext
    {
        public readonly string RequestTokenUrl;
        public readonly string AuthorizationUrl;
        public readonly string CallbackUrl;
        public readonly string AccessTokenUrl;

        public OAuthServiceContext(string requestTokenUrl, string authorizationUrl, string callbackUrl, string accessTokenUrl)
        {
            RequestTokenUrl = requestTokenUrl;
            AuthorizationUrl = authorizationUrl;
            CallbackUrl = callbackUrl;
            AccessTokenUrl = accessTokenUrl;
        }
    }
}