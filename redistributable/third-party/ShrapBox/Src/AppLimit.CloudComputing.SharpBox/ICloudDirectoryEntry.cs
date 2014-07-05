using System;
using System.Collections.Generic;

namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// This structure defines the different states which are possible
    /// about the child state
    /// </summary>
    public enum nChildState
    {
        /// <summary>
        /// Indicates that the folder has some childs (folders or files)
        /// </summary>
        HasChilds = 1,

        /// <summary>
        /// Indicates that the folder has no childs (folders or files)
        /// </summary>
        HasNoChilds = 2,

        /// <summary>
        /// Indicates that SharpBox did not evaluated it. To evaluate this 
        /// just call GetChild
        /// </summary>
        HasNotEvaluated = 3
    }

    /// <summary>
    /// Normally file based storage systems are supporting a hierarchical
    /// folder structure. This interface will be used to get access on the 
    /// folders in the cloud storage 
    /// </summary>
    public interface ICloudDirectoryEntry : ICloudFileSystemEntry, IEnumerable<ICloudFileSystemEntry>
    {        
        /// <summary>
        /// This method allows to get a directory entry with a specific folder 
        /// id.
        /// </summary>
        /// <param name="id">The name of the targeted folder</param>
        /// <returns>Reference to the file or folder</returns>
        ICloudFileSystemEntry GetChild(String id);

        /// <summary>
        /// This method allows to get a directory entry with a specific folder 
        /// id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bThrowException"></param>
        /// <returns></returns>
        ICloudFileSystemEntry GetChild(String id, Boolean bThrowException);

        /// <summary>
        /// This method allows to get a directory entry with a specific index
        /// number
        /// </summary>
        /// <param name="idx">The index of the targeted folder</param>
        /// <returns>Reference to the file or folder</returns>
		ICloudFileSystemEntry GetChild(int idx);
        
        /// <summary>
        /// This property allows to access to the number of 
        /// child items
        /// </summary>
		int Count { get; }

        /// <summary>
        /// This property contains the information about the children state. Query this property
        /// will not perform any network operations
        /// </summary>
        nChildState HasChildrens { get; }
	}
}
