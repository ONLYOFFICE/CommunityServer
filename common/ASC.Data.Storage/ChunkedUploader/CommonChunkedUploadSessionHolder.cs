/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Data.Storage;

namespace ASC.Core.ChunkedUploader
{
    public class CommonChunkedUploadSessionHolder
    {
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(12);

        private IDataStore DataStore { get; set; }
        private string Domain { get; set; }
        private long MaxChunkUploadSize { get; set; }
        private const string StoragePath = "sessions";

        public CommonChunkedUploadSessionHolder(IDataStore dataStore, string domain, long maxChunkUploadSize = 5 * 1024 * 1024)
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
                log4net.LogManager.GetLogger("ASC").Error(err);
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

        public CommonChunkedUploadSession Get(string sessionId)
        {
            using (var stream = DataStore.GetReadStream(Domain, GetPathWithId(sessionId)))
            {
                return CommonChunkedUploadSession.Deserialize(stream);
            }
        }

        public void Init(CommonChunkedUploadSession chunkedUploadSession)
        {
            if (chunkedUploadSession.BytesTotal < MaxChunkUploadSize)
            {
                chunkedUploadSession.UseChunks = false;
                return;
            }

            var tempPath = Guid.NewGuid().ToString();
            var uploadId = DataStore.InitiateChunkedUpload(Domain, tempPath);

            chunkedUploadSession.TempPath = tempPath;
            chunkedUploadSession.UploadId = uploadId;
        }

        public void Finalize(CommonChunkedUploadSession uploadSession)
        {
            var tempPath = uploadSession.TempPath;
            var uploadId = uploadSession.UploadId;
            var eTags = uploadSession.GetItemOrDefault<List<string>>("ETag")
                                     .Select((x, i) => new KeyValuePair<int, string>(i + 1, x))
                                     .ToDictionary(x => x.Key, x => x.Value);

            DataStore.FinalizeChunkedUpload(Domain, tempPath, uploadId, eTags);
        }

        public void Move(CommonChunkedUploadSession chunkedUploadSession, string newPath)
        {
            DataStore.Move(Domain, chunkedUploadSession.TempPath, string.Empty, newPath);
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

        public void UploadChunk(CommonChunkedUploadSession uploadSession, Stream stream, long length)
        {
            var tempPath = uploadSession.TempPath;
            var uploadId = uploadSession.UploadId;
            var chunkNumber = uploadSession.GetItemOrDefault<int>("ChunksUploaded") + 1;

            var eTag = DataStore.UploadChunk(Domain, tempPath, uploadId, stream, chunkNumber, length);

            uploadSession.Items["ChunksUploaded"] = chunkNumber;
            uploadSession.BytesUploaded += length;

            var eTags = uploadSession.GetItemOrDefault<List<string>>("ETag") ?? new List<string>();
            eTags.Add(eTag);
            uploadSession.Items["ETag"] = eTags;
        }

        public Stream UploadSingleChunk(CommonChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (uploadSession.BytesTotal == 0)
                uploadSession.BytesTotal = chunkLength;

            if (uploadSession.BytesTotal >= chunkLength)
            {
                //This is hack fixing strange behaviour of plupload in flash mode.

                if (string.IsNullOrEmpty(uploadSession.ChunksBuffer))
                    uploadSession.ChunksBuffer = Path.GetTempFileName();

                using (var bufferStream = new FileStream(uploadSession.ChunksBuffer, FileMode.Append))
                {
                    stream.StreamCopyTo(bufferStream);
                }

                uploadSession.BytesUploaded += chunkLength;

                if (uploadSession.BytesTotal == uploadSession.BytesUploaded)
                {
                    return new FileStream(uploadSession.ChunksBuffer, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
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