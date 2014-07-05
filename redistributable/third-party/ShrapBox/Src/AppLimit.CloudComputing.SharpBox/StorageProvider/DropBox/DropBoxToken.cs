using System;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;
using AppLimit.CloudComputing.SharpBox.Common.Net.Json;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    internal class DropBoxToken : OAuthToken, ICloudStorageAccessToken
    {
        public DropBoxBaseTokenInformation BaseTokenInformation;

        public DropBoxToken(String jsonString)
            : base("", "")
        {
            var jh = new JsonHelper();
            if (jh.ParseJsonMessage(jsonString))
            {
                TokenSecret = jh.GetProperty("secret");
                TokenKey = jh.GetProperty("token");
            }
        }

        public DropBoxToken(OAuthToken token, DropBoxBaseTokenInformation baseCreds)
            : base(token.TokenKey, token.TokenSecret)
        {
            BaseTokenInformation = baseCreds;
        }

        public DropBoxToken(string tokenKey, string tokenSecret, DropBoxBaseTokenInformation baseCreds)
            : base(tokenKey, tokenSecret)
        {
            BaseTokenInformation = baseCreds;
        }

        public ICloudStorageConfiguration ServiceConfiguration
        {
            get { return DropBoxConfiguration.GetStandardConfiguration(); }
        }

        public override string ToString()
        {
            return string.Format("{0}+{1}", TokenKey, TokenSecret);
        }
    }
}