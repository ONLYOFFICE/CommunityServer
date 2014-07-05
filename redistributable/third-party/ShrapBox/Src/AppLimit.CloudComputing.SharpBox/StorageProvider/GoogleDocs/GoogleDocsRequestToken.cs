using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs
{
    public class GoogleDocsRequestToken : ICloudStorageAccessToken
    {
        internal OAuthToken RealToken { get; private set; }

        internal GoogleDocsRequestToken(OAuthToken realToken)
        {
            RealToken = realToken;
        }
    }
}