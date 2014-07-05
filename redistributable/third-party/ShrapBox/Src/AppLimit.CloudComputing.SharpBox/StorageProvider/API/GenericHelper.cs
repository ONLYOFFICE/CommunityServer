using System;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.API
{
    internal class GenericHelper
    {
        public static String GetResourcePath(ICloudFileSystemEntry entry)
        {
            var current = entry;
            var path = "";

            while (current != null)
            {
                if (current.Name != "/")
                {
                    if (path == String.Empty)
                        path = current.Id;
                    else
                        path = current.Id + "/" + path;
                }
                else
                    path = "/" + path;

                current = current.Parent;
            }

            return path;
        }
    }
}