using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    /// <summary>
    /// This token will be issued during the web based token generation 
    /// process for dropbox
    /// </summary>
    public class DropBoxRequestToken : ICloudStorageAccessToken
    {
        internal OAuthToken RealToken;

        internal DropBoxRequestToken(OAuthToken realToken)
        {
            RealToken = realToken;
        }

        public override string ToString()
        {
            return RealToken.TokenKey + "&" + RealToken.TokenSecret;
        }

        public static DropBoxRequestToken Parse(string token)
        {
            var splited = token.Split('&');
            return new DropBoxRequestToken(new OAuthToken(splited[0], splited[1]));
        }
    }
}