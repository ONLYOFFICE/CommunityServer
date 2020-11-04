/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Diagnostics;
using ASC.Core.ChunkedUploader;

namespace ASC.Files.Core
{
    [DebuggerDisplay("{Id} into {FolderId}")]
    [Serializable]
    public class ChunkedUploadSession : CommonChunkedUploadSession
    {
        public string FolderId { get; set; }

        public File File { get; set; }

        public bool Encrypted { get; set; }

        public ChunkedUploadSession(File file, long bytesTotal) : base(bytesTotal)
        {
            File = file;
        }

        public override object Clone()
        {
            var clone = (ChunkedUploadSession) MemberwiseClone();
            clone.File = (File) File.Clone();
            return clone;
        }
    }
}
