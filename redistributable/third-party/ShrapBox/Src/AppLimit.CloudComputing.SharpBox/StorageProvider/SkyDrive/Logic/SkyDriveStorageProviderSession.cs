using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive.Logic
{
    internal class SkyDriveStorageProviderSession : IStorageProviderSession
    {
        public ICloudStorageAccessToken SessionToken { get; set; }

        public IStorageProviderService Service { get; set; }

        public ICloudStorageConfiguration ServiceConfiguration { get; set; }

        public SkyDriveStorageProviderSession(ICloudStorageAccessToken token, IStorageProviderService service, ICloudStorageConfiguration configuration)
        {
            SessionToken = token;
            Service = service;
            ServiceConfiguration = configuration;
        }
    }
}
