using System;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.API
{
    /// <summary>
    /// This class allows to create the needed object which are internally used to create
    /// extern storage providers
    /// </summary>
    public class GenericStorageProviderFactory
    {
        /// <summary>
        /// This method builds an object 
        /// </summary>        
        /// <param name="session"></param>
        /// <param name="Name"></param>
        /// <param name="parent"></param>        
        /// <returns></returns>
        public static ICloudFileSystemEntry CreateFileSystemEntry(IStorageProviderSession session, string Name, ICloudDirectoryEntry parent)
        {
            return CreateFileSystemEntry(session, Name, DateTime.Now, 0, parent);
        }

        /// <summary>
        /// This method creates a filesystem entry 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="Name"></param>
        /// <param name="modified"></param>
        /// <param name="fileSize"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static ICloudFileSystemEntry CreateFileSystemEntry(IStorageProviderSession session, string Name, DateTime modified, long fileSize, ICloudDirectoryEntry parent)
        {
            // build up query url
            var newObj = new BaseFileEntry(Name, fileSize, modified, session.Service, session);

            // case the parent if possible
            if (parent != null)
            {
                var objparent = parent as BaseDirectoryEntry;
                objparent.AddChild(newObj);
            }

            return newObj;
        }

        /// <summary>
        /// This method clears all childs of a directory entry
        /// </summary>
        /// <param name="dir"></param>
        public static void ClearAllChilds(ICloudDirectoryEntry dir)
        {

            (dir as BaseDirectoryEntry).ClearChilds();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="Name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static ICloudDirectoryEntry CreateDirectoryEntry(IStorageProviderSession session, string Name, ICloudDirectoryEntry parent)
        {
            return CreateDirectoryEntry(session, Name, DateTime.Now, parent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="Name"></param>
        /// <param name="modifiedDate"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static ICloudDirectoryEntry CreateDirectoryEntry(IStorageProviderSession session, string Name, DateTime modifiedDate, ICloudDirectoryEntry parent)
        {
            // build up query url
            var newObj = new BaseDirectoryEntry(Name, 0, modifiedDate, session.Service, session);

            // case the parent if possible
            if (parent != null)
            {
                var objparent = parent as BaseDirectoryEntry;
                objparent.AddChild(newObj);
            }

            return newObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="objectToRemove"></param>
        public static void DeleteFileSystemEntry(IStorageProviderSession session, ICloudFileSystemEntry objectToRemove)
        {
            // get the parent dir
            var parentDir = objectToRemove.Parent as BaseDirectoryEntry;

            // remove from parent 
            parentDir.RemoveChild(objectToRemove as BaseFileEntry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="objectToMove"></param>
        /// <param name="newParent"></param>
        public static void MoveFileSystemEntry(IStorageProviderSession session, ICloudFileSystemEntry objectToMove, ICloudDirectoryEntry newParent)
        {
            // readjust parent
            var oldParent = objectToMove.Parent as BaseDirectoryEntry;
            oldParent.RemoveChild(objectToMove as BaseFileEntry);

            var newParentObject = newParent as BaseDirectoryEntry;
            newParentObject.AddChild(objectToMove as BaseFileEntry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="objectToRename"></param>
        /// <param name="newName"></param>
        public static void RenameFileSystemEntry(IStorageProviderSession session, ICloudFileSystemEntry objectToRename, String newName)
        {
            // rename the fsentry
            var fentry = objectToRename as BaseFileEntry;
            fentry.Name = newName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsentry"></param>
        /// <param name="newLength"></param>
        public static void ModifyFileSystemEntryLength(ICloudFileSystemEntry fsentry, long newLength)
        {
            var fs = (BaseFileEntry) fsentry;
            fs.Length = newLength;
        }
    }
}