namespace AppLimit.CloudComputing.SharpBox.StorageProvider.API
{
    internal class GenericHelper
    {
        public static string GetResourcePath(ICloudFileSystemEntry entry)
        {
            var current = entry;
            var path = "";

            while (current != null)
            {
                if (current.Name != "/")
                {
                    if (path == string.Empty)
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