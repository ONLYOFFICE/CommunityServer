using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox.SyncFramework
{
    /// <summary>
    /// This class generates the differences between two folders, one on the local
    /// filesystem and one in the cloud
    /// </summary>
    internal class DirectoryDiff
    {
        private readonly DirectoryInfo _localPath;
        private readonly ICloudDirectoryEntry _remotePath;

        /// <summary>
        /// ctor for DirectoryDiff
        /// </summary>
        /// <param name="localPath">path of the local directory</param>
        /// <param name="remotePath">path of the remote directory</param>
        public DirectoryDiff(DirectoryInfo localPath, ICloudDirectoryEntry remotePath)
        {
            _localPath = localPath; 
            _remotePath = remotePath;
        }

        /// <summary>
        /// compares a local and a remote directory. The result will be a list of differences!
        /// </summary>
        /// <param name="bRecursive">check also sub directories</param>
        /// <returns></returns>
        public List<DirectoryDiffResultItem> Compare(Boolean bRecursive)
        {
            // 1. create a recursive local file list 
            SortedDictionary<String, FileSystemInfo> localFiles = CreateLocalFileList(_localPath, bRecursive);

            // 2. create a recursive remote file list
            SortedDictionary<String, ICloudFileSystemEntry> remoteFiles = CreateRemoteFileList(_remotePath, bRecursive);

            // 3. create the result list
            var result = new List<DirectoryDiffResultItem>();

            // 4. performe a sorted list comparation
            int i = 0;
            int j = 0;
            int m = Math.Max(localFiles.Keys.Count, remoteFiles.Keys.Count);

            while(i < m)
            {
                String left = null;
                String right = null;

                if (i < localFiles.Keys.Count)
                    left = localFiles.Keys.ElementAt(i);

                if (j < remoteFiles.Keys.Count)
                    right = remoteFiles.Keys.ElementAt(j);

                var ritem = new DirectoryDiffResultItem();

                if (right == null)
                {                    
                    // right list is at end, all left items are missing
                    ritem.localItem = localFiles.Values.ElementAt(i);
                    ritem.remoteItem = null;
                    ritem.compareResult = ComparisonResult.MissingInRemoteFolder;

                    // increase local
                    i++;
                }
                else if (left == null)
                {
                    // left list is at end, all rightitems are missing
                    ritem.localItem = null;
                    ritem.remoteItem = remoteFiles.Values.ElementAt(j);
                    ritem.compareResult = ComparisonResult.MissingInLocalFolder;

                    // increase remote
                    j++;
                }
                else
                {
                    // compare both elements
                    int iRet = left.CompareTo(right);
                    
                    if (iRet == 0)
                    {
                        // are the same elements
                        ritem.localItem = localFiles.Values.ElementAt(i);
                        ritem.remoteItem = remoteFiles.Values.ElementAt(j);

                        // compare the size

                        // set the result
                        ritem.compareResult = ComparisonResult.Identical;

                        // increase both
                        i++;
                        j++;
                    }
                    else if (iRet < 0)
                    {
                        // local missing in remote 
                        ritem.localItem = localFiles.Values.ElementAt(i);
                        ritem.compareResult = ComparisonResult.MissingInRemoteFolder;

                        // increase in local
                        i++;
                    }
                    else
                    {
                        // remote missing in local
                        ritem.remoteItem = remoteFiles.Values.ElementAt(j);
                        ritem.compareResult = ComparisonResult.MissingInLocalFolder;

                        // increase in remote
                        j++;
                    }
                }

                result.Add(ritem);
            }           

            return result;
        }

        private static SortedDictionary<String, ICloudFileSystemEntry> CreateRemoteFileList(ICloudDirectoryEntry start, Boolean bRescusive)
        {
            // result
            var result = new SortedDictionary<string, ICloudFileSystemEntry>();

            // directoryStack
            var directoryStack = new Stack<ICloudDirectoryEntry>();

            // add the start directory to the stack
            directoryStack.Push(start);

            // do enumeration until stack is empty
            while (directoryStack.Count > 0)
            {
                ICloudDirectoryEntry current = directoryStack.Pop();

                foreach (ICloudFileSystemEntry fsinfo in current)
                {
                    if (fsinfo is ICloudDirectoryEntry)
                    {
                        // check if recursion allowed
                        if (bRescusive == false)
                            continue;

                        // push the directory to stack
                        directoryStack.Push(fsinfo as ICloudDirectoryEntry);
                    }

                    // build the path
                    String path = CloudStorage.GetFullCloudPath(fsinfo, Path.DirectorySeparatorChar);
                    String startpath = CloudStorage.GetFullCloudPath(start, Path.DirectorySeparatorChar);
                    path = path.Remove(0, startpath.Length);

                    // add the entry to our output list
                    result.Add(path, fsinfo);
                }
            }

            return result;
        }

        private static SortedDictionary<String, FileSystemInfo> CreateLocalFileList(DirectoryInfo start, Boolean bRescusive)
        {
            // result
            var result = new SortedDictionary<string, FileSystemInfo>();

            // directoryStack
            var directoryStack = new Stack<DirectoryInfo>();

            // add the start directory to the stack
            directoryStack.Push(start);

            // do enumeration until stack is empty
            while (directoryStack.Count > 0)
            {
                DirectoryInfo current = directoryStack.Pop();

                foreach (FileSystemInfo fsinfo in current.GetFileSystemInfos())
                {
                    if ((fsinfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        // check if recursion allowed
                        if (bRescusive == false)
                            continue;

                        // push the directory to stack
                        directoryStack.Push(fsinfo as DirectoryInfo);
                    }
                    
                    // build path
                    String path = fsinfo.FullName;
                    path = path.Remove(0, start.FullName.Length);

                    // add the entry to our output list
                    result.Add(path, fsinfo);
                }
            }

            return result;
        }
    }
}
