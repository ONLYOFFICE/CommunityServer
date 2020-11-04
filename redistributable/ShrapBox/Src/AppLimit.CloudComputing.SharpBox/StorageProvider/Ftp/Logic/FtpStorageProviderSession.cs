using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.FTP.Logic
{
    internal class FtpStorageProviderSession : IStorageProviderSession
    {
        public FtpStorageProviderSession(ICloudStorageAccessToken token, FtpConfiguration config, IStorageProviderService service)
        {
            SessionToken = token;
            ServiceConfiguration = config;
            Service = service;
        }

        #region IStorageProviderSession Members

        public ICloudStorageAccessToken SessionToken { get; private set; }

        public IStorageProviderService Service { get; private set; }

        public ICloudStorageConfiguration ServiceConfiguration { get; private set; }

        #endregion
    }
}