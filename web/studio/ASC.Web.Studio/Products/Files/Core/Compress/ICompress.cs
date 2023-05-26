/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
        /// <param name="lastModification">Set the datetime of last modification of the entry.</param>
        void CreateEntry(string title, DateTime? lastModification = null);
                
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
