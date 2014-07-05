using System;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.CIFS
{
    /// <summary>
    /// This class implements all BoxNet specific configurations
    /// </summary>
    public class CIFSConfiguration : ICloudStorageConfiguration
    {
        private bool _TrustUnsecureSSLConnections = false;

        /// <summary>
        /// ctor of the Box.Net configuration
        /// </summary>
        public CIFSConfiguration(String path)
        {
            ServiceLocator = new Uri(path);
        }

        /// <summary>
        /// Specifies if we allow not secure ssl connections
        /// </summary>
        public bool TrustUnsecureSSLConnections
        {
            get { return _TrustUnsecureSSLConnections; }
            set { _TrustUnsecureSSLConnections = value; }
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
        public Uri ServiceLocator { get; set; }

        #endregion
    }
}