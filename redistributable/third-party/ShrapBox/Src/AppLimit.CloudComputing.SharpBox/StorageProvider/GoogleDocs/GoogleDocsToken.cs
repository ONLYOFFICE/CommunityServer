using System;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs
{
    internal class GoogleDocsToken : OAuthToken, ICloudStorageAccessToken
    {
        public String ConsumerKey { get; private set; }
        public String ConsumerSecret { get; private set; }

        public GoogleDocsToken(OAuthToken token, String consumerKey, String consumerSecret)
            : this(token.TokenKey, token.TokenSecret, consumerKey, consumerSecret)
        {
        }

        public GoogleDocsToken(String tokenKey, String tokenSecret, String consumerKey, String consumerSecret)
            : base(tokenKey, tokenSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }

        public override string ToString()
        {
            return String.Format("{0}+{1}", TokenKey, TokenSecret);
        }
    }
}