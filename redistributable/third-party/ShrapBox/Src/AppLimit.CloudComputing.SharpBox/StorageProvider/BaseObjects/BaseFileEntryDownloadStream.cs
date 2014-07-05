using System;
using System.Collections.Generic;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects
{
    internal class BaseFileEntryDownloadStream : Stream, IDisposable
    {
        private Stream _baseStream;
        private ICloudFileSystemEntry _fsEntry;

        public Stack<IDisposable> _DisposableObjects = new Stack<IDisposable>();


        public BaseFileEntryDownloadStream(Stream baseStream, ICloudFileSystemEntry fsEntry)
        {
            // save the information
            _baseStream = baseStream;
            _fsEntry = fsEntry;
        }

        public override void Close()
        {
            // 1. stream close
            _baseStream.Close();
        }

        public override bool CanRead
        {
            get { return _baseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _baseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _baseStream.CanWrite; }
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Length
        {
            get { return _fsEntry.Length; }
        }

        public override long Position
        {
            get { return _baseStream.Position; }
            set { _baseStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            // dispose my basestream
            _baseStream.Dispose();

            // dispose my base calss
            base.Dispose();

            // dispose other objects
            while (_DisposableObjects.Count > 0)
                _DisposableObjects.Pop().Dispose();
        }

        #endregion
    }
}