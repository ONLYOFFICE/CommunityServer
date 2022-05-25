/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Collections.Generic;
using System.IO;

using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Core.Compress
{
    /// <summary>
    /// Archives the data stream in the format selected in the settings
    /// </summary>
    public class CompressToArchive : ICompress
    {
        private readonly ICompress compress;

        internal static string TarExt = ".tar.gz";
        internal static string ZipExt = ".zip";
        private static List<string> Exts = new List<string>(2) { TarExt, ZipExt };

        public static ICompress Instance => new CompressToArchive();

        private CompressToArchive()
        {
            compress = FilesSettings.DownloadTarGz
                ? (ICompress)new CompressToTarGz()
                : new CompressToZip();
        }

        public CompressToArchive(Stream stream) : this()
        {
            compress.SetStream(stream);
        }

        public static string GetExt(string ext)
        {
            return Exts.Contains(ext) ? ext : Instance.ArchiveExtension;
        }

        public void SetStream(Stream stream) { }

        /// <summary>
        /// The record name is created (the name of a separate file in the archive)
        /// </summary>
        /// <param name="title">File name with extension, this name will have the file in the archive</param>
        /// <param name="lastModification">Set the datetime of last modification of the entry.</param>
        public void CreateEntry(string title, DateTime? lastModification = null) => compress.CreateEntry(title, lastModification);

        /// <summary>
        /// Transfer the file itself to the archive
        /// </summary>
        /// <param name="readStream">File data</param>
        public void PutStream(Stream readStream) => compress.PutStream(readStream);

        /// <summary>
        /// Put an entry on the output stream.
        /// </summary>
        public void PutNextEntry() => compress.PutNextEntry();

        /// <summary>
        /// Closes the current entry.
        /// </summary>
        public void CloseEntry() => compress.CloseEntry();

        /// <summary>
        /// Resource title (does not affect the work of the class)
        /// </summary>
        /// <returns></returns>
        public string Title => compress.Title;

        /// <summary>
        /// Extension the archive (does not affect the work of the class)
        /// </summary>
        /// <returns></returns>
        public string ArchiveExtension => compress.ArchiveExtension;

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => compress.Dispose();
    }
}