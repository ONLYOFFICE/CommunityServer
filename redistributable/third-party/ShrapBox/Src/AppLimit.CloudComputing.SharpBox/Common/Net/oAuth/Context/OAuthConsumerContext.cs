using System;

using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context
{
    internal class OAuthConsumerContext
    {
        public readonly String ConsumerKey;
        public readonly String ConsumerSecret;
        public readonly SignatureTypes SignatureMethod = SignatureTypes.HMACSHA1;

        public OAuthConsumerContext(String consumerKey, String consumerSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }
    }
}
