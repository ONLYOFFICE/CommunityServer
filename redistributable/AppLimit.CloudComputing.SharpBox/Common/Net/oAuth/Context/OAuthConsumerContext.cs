
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context
{
    internal class OAuthConsumerContext
    {
        public readonly string ConsumerKey;
        public readonly string ConsumerSecret;
        public readonly SignatureTypes SignatureMethod = SignatureTypes.HMACSHA1;

        public OAuthConsumerContext(string consumerKey, string consumerSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }
    }
}