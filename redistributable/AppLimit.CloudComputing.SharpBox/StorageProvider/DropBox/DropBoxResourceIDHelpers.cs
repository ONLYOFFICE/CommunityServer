namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    internal static class DropBoxResourceIDHelpers
    {
        public static string GetResourcePath(ICloudFileSystemEntry resource)
        {
            return GetResourcePath(resource, null);
        }

        public static string GetResourcePath(ICloudFileSystemEntry parent, string nameOrId, string parentId = null)
        {
            nameOrId = !string.IsNullOrEmpty(nameOrId) ? nameOrId.Trim('/') : string.Empty;
            if (parent != null)
                parentId = parent.Id;
            parentId = (parentId ?? "").Trim('/');
            if (string.IsNullOrEmpty(parentId))
                return nameOrId;
            if (string.IsNullOrEmpty(nameOrId))
                return parentId;
            return parentId + "/" + nameOrId;
        }

        public static string GetParentID(string path)
        {
            path = path.Trim('/');
            int index = path.LastIndexOf('/');
            return index != -1 ? path.Substring(0, index) : "/";
        }
    }
}