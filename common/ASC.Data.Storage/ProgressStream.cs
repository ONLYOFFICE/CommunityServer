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

namespace ASC.Data.Storage
{
    public class ProgressStream : Stream
    {
        private readonly Stream stream;
        private long length = long.MaxValue;

        public ProgressStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            this.stream = stream;
            try
            {
                length = stream.Length;
            }
            catch (Exception) { }
        }

        public override bool CanRead
        {
            get { return stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return stream.CanWrite; }
        }

        public override long Length
        {
            get { return stream.Length; }
        }

        public override long Position
        {
            get { return stream.Position; }
            set { stream.Position = value; }
        }

        public event Action<ProgressStream, int> OnReadProgress;

        public void InvokeOnReadProgress(int progress)
        {
            var handler = OnReadProgress;
            if (handler != null)
            {
                handler(this, progress);
            }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
            length = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readed = stream.Read(buffer, offset, count);
            OnReadProgress(this, (int)(stream.Position / (double)length * 100));
            return readed;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }
    }
}