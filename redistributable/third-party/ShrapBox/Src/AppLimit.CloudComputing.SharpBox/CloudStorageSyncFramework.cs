using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using AppLimit.CloudComputing.SharpBox.Common;
using AppLimit.CloudComputing.SharpBox.SyncFramework;

using AppLimit.CloudComputing.SharpBox.Exceptions;

namespace AppLimit.CloudComputing.SharpBox
{
    public partial class CloudStorage
    {
        /// <summary>
        /// This flags are used to control the folder
        /// synchronization.
        /// </summary>
        [Flags]
        public enum SyncFolderFlags
        {
            /// <summary>
            /// Place holder for empty
            /// </summary>
            Empty = 0x00000000,

            /// <summary>
            /// Just upload file from local to remote
            /// </summary>
            UploadItems = 0x00000001,

            /// <summary>
            /// Just download files from remote to local
            /// </summary>
            DownloadItems = 0x00000002,

            /// <summary>
            /// Remove files which are not available in remote
            /// </summary>
            RemoveMissingFiles = 0x00000004,

            /// <summary>
            /// Performe the action recursively
            /// </summary>
            Recursive = 0x00000008,

            /// <summary>
            /// Synchronize all, which combines the right flags
            /// </summary>
            Synchronize = UploadItems | DownloadItems | RemoveMissingFiles | Recursive
        }

        /// <summary>
        /// This function allows to synchronize a local folder with an folder exists in the cloud storage 
        /// and vice versa. The local folder and the target folder has to be created before.       
        /// </summary>
        /// <param name="srcFolder"></param>
        /// <param name="tgtFolder"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Boolean SynchronizeFolder(DirectoryInfo srcFolder, ICloudDirectoryEntry tgtFolder, SyncFolderFlags flags)
        {
            // init ret value 
            Boolean bRet = true;

            // init helper parameter
            Boolean bRecursive = ((flags & SyncFolderFlags.Recursive) != 0);

            // init the differ
            DirectoryDiff diff = new DirectoryDiff(srcFolder, tgtFolder);

            // build the diff results
            List<DirectoryDiffResultItem> res = diff.Compare(bRecursive);

            // process the diff result
            foreach (DirectoryDiffResultItem item in res)
            {
                switch (item.compareResult)
                {
                    case ComparisonResult.Identical:
                        {
                            continue;
                        }
                    case ComparisonResult.MissingInLocalFolder:
                        {
                            // check of the upload flag was set
                            if ((flags & SyncFolderFlags.DownloadItems) != SyncFolderFlags.DownloadItems)
                                continue;

                            // copy remote to local or create path

                            // 1. get the rel path 
                            String relPath;
                            if (item.remoteItem is ICloudDirectoryEntry)
                                relPath = CloudStorage.GetFullCloudPath(tgtFolder, item.remoteItem, '\\');
                            else
                                relPath = CloudStorage.GetFullCloudPath(tgtFolder, item.remoteItem.Parent, '\\');

                            // 2. ensure the directory exists
                            String tgtPath = Path.Combine(srcFolder.FullName, relPath);
                            if (!Directory.Exists(tgtPath))
                                Directory.CreateDirectory(tgtPath);

                            // 3. download file if needed  
                            if (!(item.remoteItem is ICloudDirectoryEntry))
                            {
                                DownloadFile(item.remoteItem.Parent, item.remoteItem.Name, tgtPath);
                            }
                            break;
                        }
                    case ComparisonResult.MissingInRemoteFolder:
                        {
                            // check of the upload flag was set
                            if ((flags & SyncFolderFlags.UploadItems) != SyncFolderFlags.UploadItems)
                                continue;

                            // copy local to remote

                            // 1. get the rel path 
                            String relPath;
                            if ((item.localItem.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                                relPath = item.localItem.FullName.Remove(0, srcFolder.FullName.Length);
                            else
                                relPath = Path.GetDirectoryName(item.localItem.FullName).Remove(0, srcFolder.FullName.Length);

                            // 2. convert delimit
                            relPath = relPath.Replace(Path.DirectorySeparatorChar, '/');

                            // 3. ensure the directory exists
                            ICloudDirectoryEntry realTarget = null;

                            if (relPath.Length == 0)
                                realTarget = tgtFolder;
                            else
                            {
                                if (relPath[0] == '/')
                                    relPath = relPath.Remove(0, 1);

                                //check if subfolder exists, if it doesn't, create it
                                realTarget = GetFolder(relPath, tgtFolder) ?? CreateFolderEx(relPath, tgtFolder);
                            }

                            // 4. check target
                            if (realTarget == null)
                            {
                                bRet = false;
                                continue;
                            }

                            // 5. upload file if needed  
                            if ((item.localItem.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                            {
                                if (UploadFile(item.localItem.FullName, realTarget) == null)
                                    bRet = false;
                            }
                            break;
                        }

                    case ComparisonResult.SizeDifferent:
                        {
                            throw new NotImplementedException();
                        }
                }
            }

            return bRet;
        }

        /// <summary>
        /// This function allows to synchronize a local folder with an folder exists in the cloud storage 
        /// and vice versa. The local folder and the target folder has to be created before.       
        /// </summary>
        /// <param name="srcFolder"></param>
        /// <param name="tgtFolder"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Boolean SynchronizeFolder(string srcFolder, string tgtFolder, SyncFolderFlags flags)
        {
            // check parameters
            if (srcFolder == null || tgtFolder == null || flags == SyncFolderFlags.Empty)
                return false;

            // build directory info for local folder
            DirectoryInfo srcFolderInfo = new DirectoryInfo(srcFolder);

            // build the target folder
            ICloudDirectoryEntry tgtFolderEntry = GetFolder(tgtFolder);
            if (tgtFolderEntry == null)
                return false;

            // call the sync job
            return SynchronizeFolder(srcFolderInfo, tgtFolderEntry, flags);
        }

        /// <summary>
        /// Returns a path to the file object
        /// </summary>
        /// <param name="fsentry"></param>
        /// <returns></returns>
        public static String GetFullCloudPath(ICloudFileSystemEntry fsentry)
        {
            return GetFullCloudPath(fsentry, '/');
        }

        /// <summary>
        /// Returns a path to the file object
        /// </summary>
        /// <param name="fsentry">start entry</param>
        /// <param name="cDelimiter">delimiter</param>
        /// <returns></returns>
        public static String GetFullCloudPath(ICloudFileSystemEntry fsentry, char cDelimiter)
        {
            // create string builder
            StringBuilder sb = new StringBuilder();

            // add the object as self
            sb.Insert(0, fsentry.Name);

            // add the delimiter
            sb.Insert(0, cDelimiter);

            // visit every parent
            ICloudDirectoryEntry current = fsentry.Parent;
            while (current != null)
            {
                // add the item 
                sb.Insert(0, current.Name);

                // add the delimiter
                sb.Insert(0, cDelimiter);

                // go up
                current = current.Parent;
            }

            // return result
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="fsentry"></param>
        /// <param name="cDelimiter"></param>
        /// <returns></returns>
        public static String GetFullCloudPath(ICloudDirectoryEntry start, ICloudFileSystemEntry fsentry, char cDelimiter)
        {
            String strfsentry = GetFullCloudPath(fsentry, cDelimiter);
            String strStart = GetFullCloudPath(start, cDelimiter);

            return strfsentry.Remove(0, strStart.Length);
        }
    }
}
