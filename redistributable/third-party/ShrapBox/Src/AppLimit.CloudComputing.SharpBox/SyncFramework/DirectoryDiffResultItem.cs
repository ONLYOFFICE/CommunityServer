using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox.SyncFramework
{
    /// <summary>
    /// Describes the comparison results between scanned files and directories.
    /// </summary>
    internal enum ComparisonResult
    {
        /// <summary>
        /// The two files are identical. That means, they have the same size. There
        /// is nothing such as a CRC check.
        /// </summary>
        Identical = 0,
        /// <summary>
        /// The file is present in the local folder but is missing in the remote one.
        /// This is also raised for missing folders.
        /// </summary>
        MissingInLocalFolder,
        /// <summary>
        /// The file is present in the local folder but is missing in the remote one.
        /// This is also raised for missing folders.
        /// </summary>
        MissingInRemoteFolder,
        /// <summary>
        /// The two files have different sizes.
        /// </summary>
        SizeDifferent
    }

    internal class DirectoryDiffResultItem
    {
        public FileSystemInfo          localItem;
        public ICloudFileSystemEntry   remoteItem;
        public ComparisonResult        compareResult;
    }
}
