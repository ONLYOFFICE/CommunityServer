using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav.Logic;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav
{
    internal class WebDavStorageProvider : GenericStorageProvider
    {
        public WebDavStorageProvider()
            : base(new CachedServiceWrapper(new WebDavStorageProviderService()))
        {
        }

        public WebDavStorageProvider(IStorageProviderService svc)
            : base(new CachedServiceWrapper(svc))
        {
        }
    }
}