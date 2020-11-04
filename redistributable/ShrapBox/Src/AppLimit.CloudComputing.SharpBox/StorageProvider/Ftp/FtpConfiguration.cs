using System;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.FTP
{
    /// <summary>
    /// This class implements all FTP specific configurations
    /// </summary>
    public class FtpConfiguration : ICloudStorageConfiguration
    {
        private bool _trustUnsecureSslConnections;

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
            get { return _trustUnsecureSslConnections; }
            set { _trustUnsecureSslConnections = value; }
        }

        #region ICloudStorageConfiguration Members

        private CloudStorageLimits _limits = new CloudStorageLimits();

        /// <summary>
        /// Sets or gets the limits of a webdav configuration
        /// </summary>
        public CloudStorageLimits Limits
        {
            get { return _limits; }

            set { _limits = value; }
        }

        /// <summary>
        /// Gets the FTP service url
        /// </summary>
        public Uri ServiceLocator { get; set; }

        #endregion
    }
}