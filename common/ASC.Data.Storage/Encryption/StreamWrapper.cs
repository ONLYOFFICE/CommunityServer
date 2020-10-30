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
using System.Security.Cryptography;

namespace ASC.Data.Storage.Encryption
{
    internal sealed class StreamWrapper : Stream
    {
        private readonly Stream stream;
        private readonly CryptoStream cryptoStream;
        private readonly SymmetricAlgorithm symmetricAlgorithm;
        private readonly ICryptoTransform cryptoTransform;
        private readonly long fileSize;
        private readonly long metadataLength;

        public StreamWrapper(Stream fileStream, IMetadata metadata)
        {
            stream = fileStream;
            symmetricAlgorithm = metadata.GetCryptographyAlgorithm();
            cryptoTransform = symmetricAlgorithm.CreateDecryptor();
            cryptoStream = new CryptoStreamWrapper(stream, cryptoTransform, CryptoStreamMode.Read);
            fileSize = metadata.GetFileSize();
            metadataLength = metadata.GetMetadataLength();
        }

        public override bool CanRead
        {
            get { return stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return fileSize; }
        }

        public override long Position
        {
            get
            {
                return stream.Position - metadataLength;
            }
            set
            {
                if (value < 0 || value > fileSize)
                    throw new ArgumentOutOfRangeException();

                stream.Position = value + metadataLength;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return cryptoStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            cryptoStream.Dispose();
            stream.Dispose();
            symmetricAlgorithm.Dispose();
            cryptoTransform.Dispose();
        }
    }
}
