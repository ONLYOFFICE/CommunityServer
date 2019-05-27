/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.IO;
using ASC.Common.Logging;
using ASC.Core.ChunkedUploader;

namespace ASC.Data.Storage
{
    public class CrossModuleTransferUtility
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC.CrossModuleTransferUtility");
        private readonly IDataStore source;
        private readonly IDataStore destination;
        private readonly long maxChunkUploadSize;
        private readonly int chunksize;

        public CrossModuleTransferUtility(IDataStore source, IDataStore destination)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");

            this.source = source;
            this.destination = destination;
            maxChunkUploadSize = 10*1024*1024;
            chunksize = 5*1024*1024;
        }

        public void CopyFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            if (srcDomain == null) throw new ArgumentNullException("srcDomain");
            if (srcPath == null) throw new ArgumentNullException("srcPath");
            if (destDomain == null) throw new ArgumentNullException("destDomain");
            if (destPath == null) throw new ArgumentNullException("destPath");

            using (var stream = source.GetReadStream(srcDomain, srcPath))
            {
                if (stream.Length < maxChunkUploadSize)
                {
                    destination.Save(destDomain, destPath, stream);
                }
                else
                {
                    var session = new CommonChunkedUploadSession(stream.Length);
                    var holder = new CommonChunkedUploadSessionHolder(destination, destDomain);
                    holder.Init(session);
                    try
                    {
                        Stream memstream = null;
                        try
                        {
                            while (GetStream(stream, out memstream))
                            {
                                memstream.Seek(0, SeekOrigin.Begin);
                                holder.UploadChunk(session, memstream, chunksize);
                                memstream.Dispose();
                                memstream = null;
                            }
                        }
                        finally
                        {
                            if (memstream != null)
                            {
                                memstream.Dispose();
                            }
                        }

                        holder.Finalize(session);
                        destination.Move(destDomain, session.TempPath, destDomain, destPath);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Copy File", ex);
                        holder.Abort(session);
                    }
                }
            }
        }

        private bool GetStream(Stream stream, out Stream memstream)
        {
            memstream = TempStream.Create();
            var total = 0;
            int readed;
            const int portion = 2048;
            var buffer = new byte[portion];

            while ((readed = stream.Read(buffer, 0, portion)) > 0)
            {
                memstream.Write(buffer, 0, readed);
                total += readed;
                if (total >= chunksize) break;
            }

            return total > 0;
        }
    }
}
