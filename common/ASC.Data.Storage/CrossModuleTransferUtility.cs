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
