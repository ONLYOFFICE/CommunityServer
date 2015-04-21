using AppLimit.CloudComputing.SharpBox.StorageProvider.CIFS.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.CIFS
{
    internal class CIFSStorageProvider : GenericStorageProvider
    {
        public CIFSStorageProvider()
            : base(new CachedServiceWrapper(new CIFSStorageProviderService()))
        {
        }
    }
}