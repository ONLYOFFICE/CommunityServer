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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ASC.Common.Logging;
using ASC.Data.Storage;

namespace ASC.Core.ChunkedUploader
{
    public class CommonChunkedUploadSessionHolder
    {
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(12);

        public IDataStore DataStore { get; set; }
        private string Domain { get; set; }
        public long MaxChunkUploadSize { get; private set; }
        private const string StoragePath = "sessions";
        private readonly object locker = new object();

        public CommonChunkedUploadSessionHolder(IDataStore dataStore, string domain,
            long maxChunkUploadSize = 10 * 1024 * 1024)
        {
            DataStore = dataStore;
            Domain = domain;
            MaxChunkUploadSize = maxChunkUploadSize;
        }

        public void DeleteExpired()
        {
            // clear old sessions
            try
            {
                DataStore.DeleteExpired(Domain, StoragePath, SlidingExpiration);
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC").Error(err);
            }
        }

        public void Store(CommonChunkedUploadSession s)
        {
            using (var stream = s.Serialize())
            {
                DataStore.SavePrivate(Domain, GetPathWithId(s.Id), stream, s.Expired);
            }
        }

        public void Remove(CommonChunkedUploadSession s)
        {
            DataStore.Delete(Domain, GetPathWithId(s.Id));
        }

        public T Get<T>(string sessionId)
        {
            using (var stream = DataStore.GetReadStream(Domain, GetPathWithId(sessionId)))
            {
                return CommonChunkedUploadSession.Deserialize<T>(stream);
            }
        }

        public void Init(CommonChunkedUploadSession chunkedUploadSession)
        {
            if (chunkedUploadSession.BytesTotal < MaxChunkUploadSize && chunkedUploadSession.BytesTotal != -1)
            {
                chunkedUploadSession.UseChunks = false;
                return;
            }

            var tempPath = Guid.NewGuid().ToString();
            var uploadId = DataStore.InitiateChunkedUpload(Domain, tempPath);

            chunkedUploadSession.TempPath = tempPath;
            chunkedUploadSession.UploadId = uploadId;
        }

        public virtual string Finalize(CommonChunkedUploadSession uploadSession)
        {
            var tempPath = uploadSession.TempPath;
            var uploadId = uploadSession.UploadId;
            var eTags = uploadSession.GetItemOrDefault<Dictionary<int, string>>("ETag");

            DataStore.FinalizeChunkedUpload(Domain, tempPath, uploadId, eTags);
            return Path.GetFileName(tempPath);
        }

        public void Move(CommonChunkedUploadSession chunkedUploadSession, string newPath, bool quotaCheckFileSize = true)
        {
            DataStore.Move(Domain, chunkedUploadSession.TempPath, string.Empty, newPath, quotaCheckFileSize);
        }

        public void Abort(CommonChunkedUploadSession uploadSession)
        {
            if (uploadSession.UseChunks)
            {
                var tempPath = uploadSession.TempPath;
                var uploadId = uploadSession.UploadId;

                DataStore.AbortChunkedUpload(Domain, tempPath, uploadId);
            }
            else if (!string.IsNullOrEmpty(uploadSession.ChunksBuffer))
            {
                File.Delete(uploadSession.ChunksBuffer);
            }
        }

        public virtual string UploadChunk(CommonChunkedUploadSession uploadSession, Stream stream, long length)
        {
            var tempPath = uploadSession.TempPath;
            var uploadId = uploadSession.UploadId;

            int chunkNumber;
            int.TryParse(uploadSession.GetItemOrDefault<string>("ChunksUploaded"), out chunkNumber);
            chunkNumber++;

            var eTag = DataStore.UploadChunk(Domain, tempPath, uploadId, stream, MaxChunkUploadSize, chunkNumber, length);

            uploadSession.Items["ChunksUploaded"] = chunkNumber.ToString();
            uploadSession.BytesUploaded += length;

            var eTags = uploadSession.GetItemOrDefault<Dictionary<int, string>>("ETag") ?? new Dictionary<int, string>();
            eTags.Add(chunkNumber, eTag);
            uploadSession.Items["ETag"] = eTags;
            return Path.GetFileName(tempPath);
        }

        public virtual async Task UploadChunkAsync(CommonChunkedUploadSession uploadSession, Stream stream, long length)
        {
            var tempPath = uploadSession.TempPath;
            var uploadId = uploadSession.UploadId;

            int chunkNumber;
            lock (locker)
            {
                int.TryParse(uploadSession.GetItemOrDefault<string>("ChunksUploaded"), out chunkNumber);
                chunkNumber++;
                uploadSession.Items["ChunksUploaded"] = chunkNumber.ToString();
                uploadSession.BytesUploaded += length;
            }

            var eTag = await DataStore.UploadChunkAsync(Domain, tempPath, uploadId, stream, MaxChunkUploadSize, chunkNumber, length);

            lock (locker)
            {
                var eTags = uploadSession.GetItemOrDefault<Dictionary<int, string>>("ETag") ?? new Dictionary<int, string>();
                eTags.Add(chunkNumber, eTag);
                uploadSession.Items["ETag"] = eTags;
            }
        }

        public Stream UploadSingleChunk(CommonChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (uploadSession.BytesTotal == 0)
                uploadSession.BytesTotal = chunkLength;

            if (uploadSession.BytesTotal >= chunkLength)
            {
                //This is hack fixing strange behaviour of plupload in flash mode.

                if (string.IsNullOrEmpty(uploadSession.ChunksBuffer))
                {
                    uploadSession.ChunksBuffer = TempPath.GetTempFileName();
                }

                using (var bufferStream = new FileStream(uploadSession.ChunksBuffer, FileMode.Append))
                {
                    stream.CopyTo(bufferStream);
                }

                uploadSession.BytesUploaded += chunkLength;

                if (uploadSession.BytesTotal == uploadSession.BytesUploaded)
                {
                    return new FileStream(uploadSession.ChunksBuffer, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,
                        4096, FileOptions.DeleteOnClose);
                }
            }

            return Stream.Null;
        }

        private string GetPathWithId(string id)
        {
            return Path.Combine(StoragePath, id + ".session");
        }
    }
}