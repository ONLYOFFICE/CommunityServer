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
    }
}