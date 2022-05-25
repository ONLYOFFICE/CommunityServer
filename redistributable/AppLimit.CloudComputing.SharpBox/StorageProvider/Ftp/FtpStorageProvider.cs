using AppLimit.CloudComputing.SharpBox.StorageProvider.FTP.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.FTP
{
    internal class FtpStorageProvider : GenericStorageProvider
    {
        public FtpStorageProvider()
            : base(new CachedServiceWrapper(new FtpStorageProviderService()))
        {
        }
    }
}