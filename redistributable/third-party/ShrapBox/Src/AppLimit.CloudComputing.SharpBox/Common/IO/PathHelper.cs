using System;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox.Common.IO
{
    /// <summary>
    /// A class with reimplements some aspects if the Path clas
    /// to make the handling of path names easier
    /// </summary>
    public class PathHelper
    {
        private readonly String _path;

        /// <summary>
        /// The used path delimiter
        /// </summary>
        public const char Delimiter = '/';

        /// <summary>
        /// ctor with access path
        /// </summary>
        /// <param name="path"></param>
        public PathHelper(String path)
        {
            _path = path;
        }

        /// <summary>
        /// Checks if the given path is rooted
        /// </summary>
        /// <returns></returns>
        public Boolean IsPathRooted()
        {
        	return _path.Length != 0 && _path[0] == Delimiter;
        }

        /// <summary>
        /// Returns all path elements in a path
        /// </summary>
        /// <returns></returns>
    	public String[] GetPathElements()
        {            
            String workingPath;

            // remove heading and trailing /
            workingPath = IsPathRooted() ? _path.Remove(0, 1) : _path;
            
            workingPath = workingPath.TrimEnd(Delimiter);
            
            return workingPath.Length == 0 ? new String[0] : workingPath.Split(Delimiter);
        }
        
        /// <summary>
        /// Returns the directory name
        /// </summary>
        /// <returns></returns>
        public String GetDirectoryName()
        {
        	int idx = _path.LastIndexOf(Delimiter);
        	return idx == 0 ? "" : _path.Substring(0, idx);
        }

        /// <summary>
        /// Returns the filename
        /// </summary>
        /// <returns></returns>
    	public String GetFileName()
        {
            return Path.GetFileName(_path);
        }

        /// <summary>
        /// Combines several path elements
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static String Combine(String left, String right)
        {
            // remove delimiter
            right = right.TrimStart(Delimiter);
            left = left.TrimEnd(Delimiter);

            // build the path
            if (right.Length == 0)
                return left;
            else
                return left + Delimiter + right;
        }
    }
}
