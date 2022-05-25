namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive.Logic
{
    internal class SkyDriveStorageProvider : GenericStorageProvider
    {
        public SkyDriveStorageProvider()
            : base(new CachedServiceWrapper(new SkyDriveStorageProviderService()))
        {
        }

        //public override string GetFileSystemObjectPath(ICloudFileSystemEntry fsObject)
        //{
        //    return "/" + fsObject.Id;
        //}
    }
}