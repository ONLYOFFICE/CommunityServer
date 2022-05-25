namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using System;

    /// <summary>
    /// Represents an error which occurred while extracting a file during an Extract Archive operation.
    /// </summary>
    /// <seealso cref="CloudFilesProvider.ExtractArchive"/>
    /// <seealso cref="CloudFilesProvider.ExtractArchiveFromFile"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class ExtractArchiveError
    {
        /// <summary>
        /// This is the backing field for the <see cref="Path"/> property.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        private readonly string _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractArchiveError"/> class
        /// with the specified path and status.
        /// </summary>
        /// <param name="path">The path of the file affected by this error.</param>
        /// <param name="status">The specific status for the error affecting the file.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="path"/> is empty.</exception>
        public ExtractArchiveError(string path, string status)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path cannot be empty");

            _path = path;
            _status = status;
        }

        /// <summary>
        /// Gets the path of the file affected by this error.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// Gets the specific status for the error affecting the file.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
        }
    }
}
