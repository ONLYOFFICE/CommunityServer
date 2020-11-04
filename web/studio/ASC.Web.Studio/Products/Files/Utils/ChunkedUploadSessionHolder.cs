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


using ASC.Files.Core;
using ASC.Web.Files.Classes;
using System;
using System.IO;
using ASC.Core.ChunkedUploader;
using ASC.Web.Studio.Core;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Utils
{
    static class ChunkedUploadSessionHolder
    {
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(12);

        private static CommonChunkedUploadSessionHolder CommonSessionHolder(bool currentTenant = true)
        {
            return new CommonChunkedUploadSessionHolder(Global.GetStore(currentTenant), FileConstant.StorageDomainTmp, SetupInfo.ChunkUploadSize);
        }

        static ChunkedUploadSessionHolder()
        {
            // clear old sessions
            try
            {
                CommonSessionHolder(false).DeleteExpired();
            }
            catch (Exception err)
            {
                Global.Logger.Error(err);
            }
        }

        public static void StoreSession(ChunkedUploadSession s)
        {
            CommonSessionHolder(false).Store(s);
        }

        public static void RemoveSession(ChunkedUploadSession s)
        {
            CommonSessionHolder(false).Remove(s);
        }

        public static ChunkedUploadSession GetSession(string sessionId)
        {
            return (ChunkedUploadSession)CommonSessionHolder(false).Get(sessionId);
        }

        public static ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            var result = new ChunkedUploadSession(file, contentLength);
            CommonSessionHolder().Init(result);
            return result;
        }

        public static void UploadChunk(ChunkedUploadSession uploadSession, Stream stream, long length)
        {
            CommonSessionHolder().UploadChunk(uploadSession, stream, length);
        }

        public static void FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            CommonSessionHolder().Finalize(uploadSession);
        }

        public static void Move(ChunkedUploadSession chunkedUploadSession, string newPath)
        {
            CommonSessionHolder().Move(chunkedUploadSession, newPath);
        }

        public static void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            CommonSessionHolder().Abort(uploadSession);
        }

        public static Stream UploadSingleChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            return CommonSessionHolder().UploadSingleChunk(uploadSession, stream, chunkLength);
        }
    }
}