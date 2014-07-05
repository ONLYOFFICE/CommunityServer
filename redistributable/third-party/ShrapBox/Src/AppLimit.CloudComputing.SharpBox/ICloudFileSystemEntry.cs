using System;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// Normally file based storage systems are supporting file system
    /// entries. This interface will be used to get access on the 
    /// file system entry in the cloud storage 
    /// </summary>
    public interface ICloudFileSystemEntry
    {
        /// <summary>
        /// The name of the associated folder
        /// </summary>
        String Name { get; }

        /// <summary>
        /// This attribute contains uniq id of the entry. Id = Name for most providers
        /// </summary>
        String Id { get; }

        /// <summary>
        /// This attribute contains the size of a file or the count of childs
        /// of the associated directory
        /// </summary>
        long Length { get; }

        /// <summary>
        /// This attribute contains the modification date of the object
        /// </summary>
        DateTime Modified { get; }

        /// <summary>
        /// ID of the parent folder.
        /// </summary>
        String ParentID { get; set; }

        /// <summary>
        /// The parent folder of the associated folder, can be null if it's 
        /// the root folder
        /// </summary>
        ICloudDirectoryEntry Parent { get; set;  }

        /// <summary>
        /// This method return an implementation of an content data transfer interface
        /// </summary>
        /// <returns></returns>
        ICloudFileDataTransfer GetDataTransferAccessor();

        /// <summary>
        /// This method gives raw access to the properties of the specific
        /// protocol provider
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        String GetPropertyValue(String key);
    }
}
