using System;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs
{
    public class GoogleDocsStorageProvider : GenericStorageProvider
    {
        public GoogleDocsStorageProvider()
            : base(new CachedServiceWrapper(new GoogleDocsStorageProviderService()))
        {
        }

        public override Uri GetFileSystemObjectUrl(string path, ICloudDirectoryEntry parent)
        {
            var ph = new PathHelper(path);
            var elements = ph.GetPathElements();
            var current = parent;

            for (int i = 0; i < elements.Length; i++)
            {
                var child = current.GetChild(elements[i], false);
                if (i == elements.Length - 1)
                {
                    if (child == null || child is ICloudDirectoryEntry)
                    {
                        throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);
                    }

                    return new Uri(child.GetPropertyValue(GoogleDocsConstants.DownloadUrlProperty));
                }

                if (child == null || !(child is ICloudDirectoryEntry))
                {
                    throw new SharpBoxException(SharpBoxErrorCodes.ErrorFileNotFound);
                }

                current = (ICloudDirectoryEntry) child;
            }

            return null;
        }

        public override ICloudDirectoryEntry CreateFolder(string name, ICloudDirectoryEntry parent)
        {
            if (parent == null) parent = GetRoot();
            return _Service.CreateResource(_Session, name, parent) as ICloudDirectoryEntry;
        }
    }
}