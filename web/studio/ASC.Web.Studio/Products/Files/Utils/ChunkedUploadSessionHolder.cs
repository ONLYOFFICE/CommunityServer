/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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