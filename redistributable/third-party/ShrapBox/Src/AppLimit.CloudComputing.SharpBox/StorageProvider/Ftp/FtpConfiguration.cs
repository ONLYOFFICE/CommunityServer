using System;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.FTP
{
    /// <summary>
    /// This class implements all FTP specific configurations
    /// </summary>
    public class FtpConfiguration : ICloudStorageConfiguration
    {
        private bool _TrustUnsecureSSLConnections = false;

        /// <summary>
        /// ctor of the FTP configuration
        /// </summary>
        public FtpConfiguration(Uri uriFtpServer)
        {
            ServiceLocator = uriFtpServer;
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

        private CloudStorageLimits _Limits = new CloudStorageLimits();

        /// <summary>
        /// Sets or gets the limits of a webdav configuration
        /// </summary>
        public CloudStorageLimits Limits
        {
            get { return _Limits; }

            set { _Limits = value; }
        }

        /// <summary>
        /// Gets the FTP service url
        /// </summary>
        public Uri ServiceLocator { get; set; }

        #endregion
    }
}