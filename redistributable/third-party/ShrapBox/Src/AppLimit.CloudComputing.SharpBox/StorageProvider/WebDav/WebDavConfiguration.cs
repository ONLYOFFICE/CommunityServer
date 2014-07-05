using System;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav
{
    /// <summary>
    /// This class implements all BoxNet specific configurations
    /// </summary>
    public class WebDavConfiguration : ICloudStorageConfiguration
    {
        private bool _TrustUnsecureSSLConnections = true;

        /// <summary>
        /// The url of webserver which has to be used for access to a specific 
        /// webdav share.
        /// </summary>
        private Uri webServer { get; set; }

        /// <summary>
        /// ctor of the Box.Net configuration
        /// </summary>
        public WebDavConfiguration(Uri uriWebDavServer)
        {
            webServer = uriWebDavServer;
            UploadDataStreambuffered = false;
        }

        /// <summary>
        /// Specifies if we allow not secure ssl connections
        /// </summary>
        public bool TrustUnsecureSSLConnections
        {
            get { return _TrustUnsecureSSLConnections; }
            set { _TrustUnsecureSSLConnections = value; }
        }

        /// <summary>
        /// Specifies if the webdavclient should upload the data
        /// stream buffered. Detailed is fales to reduce the 
        /// memory foodprint!
        /// </summary>
        public bool UploadDataStreambuffered { get; set; }

        /// <summary>
        /// This method returns a standard configuration for 1and1 
        /// </summary>
        /// <returns></returns>
        public static WebDavConfiguration Get1and1Configuration()
        {
            // set the right url
            WebDavConfiguration config = new WebDavConfiguration(new Uri("https://sd2dav.1und1.de"));
            config.Limits = new CloudStorageLimits();
            config.Limits.MaxDownloadFileSize = 500*1024*1024;
            config.Limits.MaxUploadFileSize = config.Limits.MaxDownloadFileSize;

            // 1and1 does not support a valid ssl
            config.TrustUnsecureSSLConnections = true;

            // go ahead
            return config;
        }

        /// <summary>
        /// This method returns a standard cofiguration for StoreGate
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static WebDavConfiguration GetStoreGateConfiguration(NetworkCredential credentials)
        {
            // set the right url
            WebDavConfiguration config = new WebDavConfiguration(new Uri("https://webdav1.storegate.com/" + credentials.UserName + "/home/" + credentials.UserName));
            config.Limits = new CloudStorageLimits();
            config.Limits.MaxDownloadFileSize = -1;
            config.Limits.MaxUploadFileSize = -1;

            // box.net does not support a valid ssl
            config.TrustUnsecureSSLConnections = false;

            // go ahead
            return config;
        }

        /// <summary>
        /// This method returns a standard cofiguration for CloudMe
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static WebDavConfiguration GetCloudMeConfiguration(NetworkCredential credentials)
        {
            // set the right url
            WebDavConfiguration config = new WebDavConfiguration(new Uri("http://webdav.cloudme.com/" + credentials.UserName + "/xios"));
            config.Limits = new CloudStorageLimits();
            config.Limits.MaxDownloadFileSize = -1;
            config.Limits.MaxUploadFileSize = -1;

            // box.net does not support a valid ssl
            config.TrustUnsecureSSLConnections = false;

            // set streambuffered transfer
            config.UploadDataStreambuffered = true;

            // go ahead
            return config;
        }

        /// <summary>
        /// This method returns a standard configuration for Strato HiDrive 
        /// </summary>
        /// <returns></returns>
        public static WebDavConfiguration GetHiDriveConfiguration()
        {
            // set the right url
            WebDavConfiguration config = new WebDavConfiguration(new Uri("https://webdav.hidrive.strato.com"));
            config.Limits = new CloudStorageLimits();
            config.Limits.MaxDownloadFileSize = -1;
            config.Limits.MaxUploadFileSize = config.Limits.MaxDownloadFileSize;

            // box.net does not support a valid ssl
            config.TrustUnsecureSSLConnections = false;

            // go ahead
            return config;
        }

        public static WebDavConfiguration GetYandexConfiguration()
        {
            var config = new WebDavConfiguration(new Uri("https://webdav.yandex.ru")) {TrustUnsecureSSLConnections = false};
            return config;
        }

        #region ICloudStorageConfiguration Members

        private CloudStorageLimits _Limits = new CloudStorageLimits() {MaxDownloadFileSize = -1, MaxUploadFileSize = -1};

        /// <summary>
        /// Sets or gets the limits of a webdav configuration
        /// </summary>
        public CloudStorageLimits Limits
        {
            get { return _Limits; }

            set { _Limits = value; }
        }

        /// <summary>
        /// Gets the webdav service url
        /// </summary>
        public Uri ServiceLocator
        {
            get { return webServer; }

            set { webServer = value; }
        }

        #endregion
    }
}