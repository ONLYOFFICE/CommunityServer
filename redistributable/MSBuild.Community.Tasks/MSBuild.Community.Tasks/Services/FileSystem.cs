//$Id$
using System.IO;

namespace MSBuild.Community.Tasks.Services
{
    /// <summary>
    /// The contract for a service that will provide access to the file system.
    /// </summary>
    /// <exclude />
    public interface IFilesSystem
    {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <returns><c>True</c> if the file exists on the filesystem.</returns>
        bool FileExists(string path);
        /// <summary>
        /// Returns the contents of a file.
        /// </summary>
        /// <param name="fileName">The path of the file to read.</param>
        /// <returns>The text with the specified file.</returns>
        string  ReadTextFromFile(string fileName);
        /// <summary>
        /// Writes text to a file.
        /// </summary>
        /// <param name="fileName">The path of the file to write.</param>
        /// <param name="contents">The text to write to the file.</param>
        void WriteTextToFile(string fileName, string contents);
    }

    /// <summary>
    /// Provides access to the file system.
    /// </summary>
    /// <exclude />
    public class FileSystem : IFilesSystem
    {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <returns><c>True</c> if the file exists on the filesystem.</returns>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Returns the contents of a file.
        /// </summary>
        /// <param name="fileName">The path of the file to read.</param>
        /// <returns>The text with the specified file.</returns>
        public string ReadTextFromFile(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        /// <summary>
        /// Writes text to a file.
        /// </summary>
        /// <param name="fileName">The path of the file to write.</param>
        /// <param name="contents">The text to write to the file.</param>
        public void WriteTextToFile(string fileName, string contents)
        {
            File.WriteAllText(fileName, contents);
        }
    }
}
