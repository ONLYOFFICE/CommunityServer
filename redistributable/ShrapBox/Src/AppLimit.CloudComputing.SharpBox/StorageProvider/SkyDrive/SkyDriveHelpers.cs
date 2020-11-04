using System;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive
{
    internal static class SkyDriveHelpers
    {
        public static bool HasResourceID(String nameOrID)
        {
            return GetResourceID(nameOrID) != String.Empty;
        }

        public static String GetResourceID(String nameOrID)
        {
            if (String.IsNullOrEmpty(nameOrID))
                return String.Empty;

            var index = nameOrID.LastIndexOf('/');
            if (index != -1 && index != nameOrID.Length)
            {
                nameOrID = nameOrID.Substring(index + 1);
            }
            return SkyDriveConstants.ResourceIDRegex.IsMatch(nameOrID) ? nameOrID : String.Empty;
        }

        public static bool HasParentID(String nameOrID)
        {
            return GetParentID(nameOrID) != String.Empty;
        }

        public static String GetParentID(String nameOrID)
        {
            if (String.IsNullOrEmpty(nameOrID))
                return String.Empty;

            var index = nameOrID.LastIndexOf('/');
            if (index != -1 && index != nameOrID.Length)
            {
                nameOrID = nameOrID.Remove(index);
            }
            return GetResourceID(nameOrID);
        }

        public static bool IsFolderID(String id)
        {
            return id.StartsWith("folder") || id.StartsWith("album") || id == String.Empty; //empty if root folder with id like "/"
        }

        public static void CopyProperties(ICloudFileSystemEntry src, ICloudFileSystemEntry dest)
        {
            if (!(dest is BaseFileEntry) || !(src is BaseFileEntry)) return;

            var destBase = dest as BaseFileEntry;
            var srcBase = src as BaseFileEntry;
            destBase.Name = srcBase.Name;
            destBase.Id = srcBase.Id;
            destBase[SkyDriveConstants.InnerIDKey] = srcBase[SkyDriveConstants.InnerIDKey];
            destBase.Modified = srcBase.Modified;
            destBase.Length = srcBase.Length;
            destBase[SkyDriveConstants.UploadLocationKey] = srcBase[SkyDriveConstants.UploadLocationKey];
            destBase.ParentID = srcBase.ParentID;
        }
    }
}