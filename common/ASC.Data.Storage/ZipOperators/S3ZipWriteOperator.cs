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
using System.Text;
using System.Threading.Tasks;

using ASC.Common;
using ASC.Core.ChunkedUploader;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace ASC.Data.Storage.ZipOperators
{
    public class S3ZipWriteOperator : IDataWriteOperator
    {
        private readonly TarOutputStream _tarOutputStream;
        private readonly GZipOutputStream _gZipOutputStream;
        private CommonChunkedUploadSession _chunkedUploadSession;
        private readonly CommonChunkedUploadSessionHolder _sessionHolder;
        private readonly SHA _sha;
        private Stream _fileStream;
        private byte[] _hash;
        protected const int TasksLimit = 10;
        private readonly List<Task> tasks = new List<Task>(TasksLimit);
        private readonly List<Stream> streams = new List<Stream>(TasksLimit);

        public string Hash { get; private set; }
        public string StoragePath { get; private set; }
        public bool NeedUpload
        {
            get
            {
                return false;
            }
        }

        public S3ZipWriteOperator(
            CommonChunkedUploadSession chunkedUploadSession,
            CommonChunkedUploadSessionHolder sessionHolder)
        {
            _chunkedUploadSession = chunkedUploadSession;
            _sessionHolder = sessionHolder;


            _fileStream = TempStream.Create();
            _gZipOutputStream = new GZipOutputStream(_fileStream)
            {
                IsStreamOwner = false
            };
            _tarOutputStream = new TarOutputStream(_gZipOutputStream, Encoding.UTF8);
            _sha = new SHA();
        }

        public void WriteEntry(string key, Stream stream)
        {
            if (_fileStream == null)
            {
                _fileStream = TempStream.Create();
                _gZipOutputStream.baseOutputStream_ = _fileStream;
            }
            using (var buffered = stream.GetBuffered())
            {
                var entry = TarEntry.CreateTarEntry(key);
                entry.Size = buffered.Length;
                _tarOutputStream.PutNextEntry(entry);
                buffered.Position = 0;
                buffered.CopyTo(_tarOutputStream);
                _tarOutputStream.Flush();
                _tarOutputStream.CloseEntry();
            }

            if (_fileStream.Length > _sessionHolder.MaxChunkUploadSize)
            {
                var fs = _fileStream;
                _fileStream = null;
                SplitAndUpload(fs);
            }
        }

        private void SplitAndUpload(Stream stream, bool last = false)
        {
            stream.Position = 0;
            var buffer = new byte[_sessionHolder.MaxChunkUploadSize];

            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, (int)_sessionHolder.MaxChunkUploadSize)) > 0)
            {
                var tempStream = TempStream.Create();

                tempStream.Write(buffer, 0, bytesRead);
                if (tempStream.Length == _sessionHolder.MaxChunkUploadSize)
                {
                    tempStream.Position = 0;
                    ComputeHash(tempStream, false);
                    Upload(tempStream);
                }
                else
                {
                    if (last)
                    {
                        ComputeHash(tempStream, last);
                        Upload(tempStream);
                    }
                    else
                    {
                        tempStream.Position = tempStream.Length;
                        _fileStream = tempStream;
                        _gZipOutputStream.baseOutputStream_ = _fileStream;
                    }
                }
            }
            stream.Dispose();
        }

        private void ComputeHash(Stream stream, bool isLast)
        {
            stream.Position = 0;
            _hash = _sha.ComputeHash(stream, isLast);
        }

        private void Upload(Stream stream)
        {
            stream.Position = 0;
            if (tasks.Count == TasksLimit)
            {
                Task.WaitAny(tasks.ToArray());
                for (int i = 0; i < tasks.Count; i++)
                {
                    if (tasks[i].IsCompleted)
                    {
                        tasks.RemoveAt(i);
                        streams[i].Dispose();
                        streams.RemoveAt(i);
                    }
                }
            }
            streams.Add(stream);
            tasks.Add(_sessionHolder.UploadChunkAsync(_chunkedUploadSession, stream, stream.Length));
        }

        public void Dispose()
        {
            _tarOutputStream.Close();
            _tarOutputStream.Dispose();

            SplitAndUpload(_fileStream, true);
            Task.WaitAll(tasks.ToArray());

            StoragePath = _sessionHolder.Finalize(_chunkedUploadSession);

            _sha.Dispose();
            Hash = BitConverter.ToString(_hash).Replace("-", string.Empty);

            streams.ForEach(s => s.Dispose());
        }
    }
}
