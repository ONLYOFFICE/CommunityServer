using System;
using System.IO;

namespace ASC.Web.Files.Core.Compress
{
    ///<summary>Archiving Class Unification Interface</summary>
    public interface ICompress : IDisposable
    {
        void SetStream(Stream stream);

        /// <summary>
        /// The record name is created (the name of a separate file in the archive)
        /// </summary>
        /// <param name="title">File name with extension, this name will have the file in the archive</param>
        void CreateEntry(string title);

        /// <summary>
        /// Transfer the file itself to the archive
        /// </summary>
        /// <param name="readStream">File data</param>
        void PutStream(Stream readStream);

        /// <summary>
        /// Put an entry on the output stream.
        /// </summary>
        void PutNextEntry();

        /// <summary>
        /// Closes the current entry.
        /// </summary>
        void CloseEntry();

        /// <summary>
        /// Resource title (does not affect the work of the class)
        /// </summary>
        /// <returns></returns>
        string Title { get; }

        /// <summary>
        /// Extension the archive (does not affect the work of the class)
        /// </summary>
        /// <returns></returns>
        string ArchiveExtension { get; }
    }
}
