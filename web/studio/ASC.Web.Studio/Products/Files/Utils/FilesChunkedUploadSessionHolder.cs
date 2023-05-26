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
using System.Threading.Tasks;

using ASC.Core.ChunkedUploader;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Utils
{
    public class FilesChunkedUploadSessionHolder : CommonChunkedUploadSessionHolder
    {
        public FilesChunkedUploadSessionHolder(IDataStore dataStore, string domain, long maxChunkUploadSize = 10485760)
            : base(dataStore, domain, maxChunkUploadSize)
        {
        }

        public override string UploadChunk(CommonChunkedUploadSession commonChunkedUploadSession, Stream stream, long chunkLength)
        {
            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var chunkedUploadSession = commonChunkedUploadSession as ChunkedUploadSession;
                chunkedUploadSession.File.ContentLength += stream.Length;
                return Convert.ToString(fileDao.UploadChunk(chunkedUploadSession, stream, chunkLength).ID);
            }
        }

        public override async Task UploadChunkAsync(CommonChunkedUploadSession commonChunkedUploadSession, Stream stream, long chunkLength)
        {
            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var chunkedUploadSession = commonChunkedUploadSession as ChunkedUploadSession;
                chunkedUploadSession.File.ContentLength += stream.Length;
                await fileDao.UploadChunkAsync(chunkedUploadSession, stream, stream.Length);
            }
        }

        public override string Finalize(CommonChunkedUploadSession commonChunkedUploadSession)
        {
            using (var fileDao = GetFileDao())
            {
                var chunkedUploadSession = commonChunkedUploadSession as ChunkedUploadSession;
                chunkedUploadSession.BytesTotal = chunkedUploadSession.BytesUploaded;
                return Convert.ToString(fileDao.FinalizeUploadSession(chunkedUploadSession).ID);
            }
        }

        private IFolderDao GetFolderDao()
        {
            return Global.DaoFactory.GetFolderDao();
        }

        private IFileDao GetFileDao()
        {
            return Global.DaoFactory.GetFileDao();
        }
    }
}