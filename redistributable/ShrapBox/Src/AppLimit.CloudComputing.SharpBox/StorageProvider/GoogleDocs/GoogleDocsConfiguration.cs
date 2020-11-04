using System;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs
{
    public class GoogleDocsConfiguration : ICloudStorageConfiguration
    {
        public GoogleDocsConfiguration()
            : this(new Uri(GoogleDocsConstants.GoogleDocsBaseUrl))
        {
        }

        public GoogleDocsConfiguration(Uri serviceLocator)
        {
            ServiceLocator = serviceLocator;
            AuthorizationCallBack = new Uri(GoogleDocsConstants.CallbackDefaultUrl);
        }

        public static GoogleDocsConfiguration GetStandartConfiguration()
        {
            return new GoogleDocsConfiguration();
        }

        public Uri OAuthGetRequestTokenUrl
        {
            get { return new Uri(GoogleDocsConstants.OAuthGetRequestUrl); }
        }

        public Uri OAuthAuthorizeTokenUrl
        {
            get { return new Uri(GoogleDocsConstants.OAuthAuthorizeUrl); }
        }

        public Uri OAuthGetAccessTokenUrl
        {
            get { return new Uri(GoogleDocsConstants.OAuthGetAccessUrl); }
        }

        public String AccessUrlScope
        {
            get { return GoogleDocsConstants.BasicScopeUrl; }
        }

        public Uri AuthorizationCallBack { get; set; }

        public Uri ServiceLocator { get; private set; }

        public bool TrustUnsecureSSLConnections
        {
            get { return false; }
        }

        public CloudStorageLimits Limits
        {
            get { return new CloudStorageLimits(2L*1024L*1024L*1024L, -1) { MaxChunkedUploadFileSize = -1 }; }
        }

        public String GDataVersion
        {
            get { return "3.0"; }
        }
    }
}