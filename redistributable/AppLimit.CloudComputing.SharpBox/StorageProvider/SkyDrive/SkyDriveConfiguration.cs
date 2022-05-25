using System;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive
{
    public class SkyDriveConfiguration : ICloudStorageConfiguration
    {
        public Uri ServiceLocator
        {
            get { return new Uri(SkyDriveConstants.BaseAccessUrl); }
        }

        public bool TrustUnsecureSSLConnections
        {
            get { return false; }
        }

        public CloudStorageLimits Limits
        {
            get { return new CloudStorageLimits(100*1024*1024, -1); }
        }
    }
}