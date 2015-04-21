using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSBuild.Community.Tasks
{
    internal static class PathUtil
    {
        private static readonly char[] _invalidPathChars;
        private static readonly char[] _invalidFileChars;

        static PathUtil()
        {
            _invalidPathChars = Path.GetInvalidPathChars();
            Array.Sort(_invalidPathChars);
            _invalidFileChars = Path.GetInvalidFileNameChars();
            Array.Sort(_invalidFileChars);
        }

        /// <summary>  
        /// Creates a relative path from one file  
        /// or folder to another.  
        /// </summary>  
        /// <param name="fromDirectory">  
        /// Contains the directory that defines the   
        /// start of the relative path.  
        /// </param>  
        /// <param name="toPath">  
        /// Contains the path that defines the  
        /// endpoint of the relative path.  
        /// </param>  
        /// <returns>  
        /// The relative path from the start  
        /// directory to the end path.  
        /// </returns>  
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="fromDirectory"/> or <paramref name="toPath"/> are null.
        /// </exception>  
        public static string RelativePathTo(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
                throw new ArgumentNullException("fromDirectory");

            if (toPath == null)
                throw new ArgumentNullException("toPath");

            bool isRooted = Path.IsPathRooted(fromDirectory)
                && Path.IsPathRooted(toPath);

            if (isRooted)
            {
                bool isDifferentRoot = !string.Equals(
                    Path.GetPathRoot(fromDirectory),
                    Path.GetPathRoot(toPath),
                    StringComparison.OrdinalIgnoreCase);

                if (isDifferentRoot)
                    return toPath;
            }

            var relativePath = new List<string>();
            string[] fromDirectories = fromDirectory.Split(Path.DirectorySeparatorChar);
            string[] toDirectories = toPath.Split(Path.DirectorySeparatorChar);

            int length = System.Math.Min(fromDirectories.Length, toDirectories.Length);

            int lastCommonRoot = -1;

            // find common root  
            for (int x = 0; x < length; x++)
            {
                if (!string.Equals(fromDirectories[x], toDirectories[x],
                    StringComparison.OrdinalIgnoreCase))
                    break;

                lastCommonRoot = x;
            }
            if (lastCommonRoot == -1)
                return toPath;

            // add relative folders in from path  
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
                if (fromDirectories[x].Length > 0)
                    relativePath.Add("..");

            // add to folders to path  
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
                relativePath.Add(toDirectories[x]);

            // create relative path  
            string newPath = string.Join(Path.DirectorySeparatorChar.ToString(), relativePath.ToArray());
            return newPath;
        }

        public static bool IsPathValid(string path)
        {
            for (int i = 0; i < path.Length; i++)
                if (Array.BinarySearch(_invalidPathChars, 0, _invalidPathChars.Length, path[i]) >= 0)
                    return false;

            string fileName = Path.GetFileName(path);
            for (int i = 0; i < fileName.Length; i++)
                if (Array.BinarySearch(_invalidFileChars, 0, _invalidFileChars.Length, fileName[i]) >= 0)
                    return false;


            return true;
        }
    }
}
