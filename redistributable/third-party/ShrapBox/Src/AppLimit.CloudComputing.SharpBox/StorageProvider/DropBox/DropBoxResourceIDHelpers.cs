using System;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    internal static class DropBoxResourceIDHelpers
    {
        public static String GetResourcePath(ICloudFileSystemEntry resource)
        {
            return GetResourcePath(resource, null);
        }

        public static String GetResourcePath(ICloudFileSystemEntry parent, String nameOrId)
        {
            nameOrId = !String.IsNullOrEmpty(nameOrId) ? nameOrId.Trim('/') : String.Empty;
            var parentId = parent != null ? parent.Id.Trim('/') : String.Empty;
            if (String.IsNullOrEmpty(parentId))
                return nameOrId;
            if (String.IsNullOrEmpty(nameOrId))
                return parentId;
            return parentId + "/" + nameOrId;
        }

        public static String GetParentID(String path)
        {
            path = path.Trim('/');
            int index = path.LastIndexOf('/');
            return index != -1 ? path.Substring(0, index) : "/";
        }
    }
}
