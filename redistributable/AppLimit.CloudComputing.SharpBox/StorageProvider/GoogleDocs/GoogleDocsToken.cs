using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs
{
    internal class GoogleDocsToken : OAuthToken, ICloudStorageAccessToken
    {
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }

        public GoogleDocsToken(OAuthToken token, string consumerKey, string consumerSecret)
            : this(token.TokenKey, token.TokenSecret, consumerKey, consumerSecret)
        {
        }

        public GoogleDocsToken(string tokenKey, string tokenSecret, string consumerKey, string consumerSecret)
            : base(tokenKey, tokenSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }

        public override string ToString()
        {
            return string.Format("{0}+{1}", TokenKey, TokenSecret);
        }
    }
}