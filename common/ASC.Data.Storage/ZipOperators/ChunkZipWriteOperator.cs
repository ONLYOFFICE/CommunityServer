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
using System.Security.Cryptography;
using System.Text;

using ASC.Common;
using ASC.Core.ChunkedUploader;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace ASC.Data.Storage.ZipOperators
{
    public class ChunkZipWriteOperator : IDataWriteOperator
    {
        private readonly TarOutputStream _tarOutputStream;
        private readonly GZipOutputStream _gZipOutputStream;
        private readonly CommonChunkedUploadSession _chunkedUploadSession;
        private readonly CommonChunkedUploadSessionHolder _sessionHolder;
        private readonly SHA _sha;
        private Stream _fileStream;
        private byte[] _hash;

        public string Hash { get; private set; }
        public string StoragePath { get; private set; }
        public bool NeedUpload
        {
            get
            {
                return false;
            }
        }

        public ChunkZipWriteOperator(
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

            if (_fileStream.Length > 100 * 1024 * 1024)
            {
                Upload(false);
            }
        }

        private void Upload(bool last)
        {
            var chunkUploadSize = _sessionHolder.MaxChunkUploadSize;

            var buffer = new byte[chunkUploadSize];
            int bytesRead;
            _fileStream.Position = 0;
            while ((bytesRead = _fileStream.Read(buffer, 0, (int)chunkUploadSize)) > 0)
            {
                using (var theMemStream = new MemoryStream())
                {
                    theMemStream.Write(buffer, 0, bytesRead);
                    theMemStream.Position = 0;
                    if (bytesRead == chunkUploadSize || last)
                    {
                        if (_fileStream.Position == _fileStream.Length && last)
                        {
                            _chunkedUploadSession.LastChunk = true;
                        }

                        _hash = _sha.ComputeHash(theMemStream, bytesRead != chunkUploadSize);

                        theMemStream.Position = 0;
                        StoragePath = _sessionHolder.UploadChunk(_chunkedUploadSession, theMemStream, theMemStream.Length);
                    }
                    else
                    {
                        _fileStream.Dispose();
                        _fileStream = TempStream.Create();
                        _gZipOutputStream.baseOutputStream_ = _fileStream;

                        theMemStream.CopyTo(_fileStream);
                        _fileStream.Flush();
                    }
                }
            }
        }

        public void Dispose()
        {
            _tarOutputStream.Close();
            _tarOutputStream.Dispose();
            Upload(true);
            _fileStream.Dispose();
            _sha.Dispose();
            Hash = BitConverter.ToString(_hash).Replace("-", string.Empty);
        }
    }

    internal class SHA : SHA256Managed
    {
        public byte[] ComputeHash(Stream inputStream, bool isFinal)
        {
            byte[] array = new byte[4096];
            int num;
            do
            {
                num = inputStream.Read(array, 0, 4096);
                if (num > 0)
                {
                    HashCore(array, 0, num);
                }
            }
            while (num > 0);
            if (isFinal)
            {
                HashValue = HashFinal();
                byte[] result = (byte[])HashValue.Clone();
                Initialize();
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
