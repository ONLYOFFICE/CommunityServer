using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    internal class WebResponseStream : Stream, IDisposable
    {
        private Stream               _responseStream;
        private WebResponse          _response;
        private WebRequestService   _service;

        public WebResponseStream(Stream srcStream, WebResponse response, WebRequestService service)
        {
            _responseStream = srcStream;
            _response = response;
            _service = service;
        }

        public override bool CanRead
        {
            get { return _responseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _responseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _responseStream.CanWrite; }
        }

        public override void Flush()
        {
            _responseStream.Flush();
        }

        public override long Length
        {
            get { return _responseStream.Length; }
        }

        public override long Position
        {
            get
            {
                return _responseStream.Position;
            }
            set
            {
                _responseStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _responseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _responseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _responseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _responseStream.Write(buffer, offset, count);
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            // dispose our request
            _service.DisposeWebResponseStreams(_response, this);

            // to all dispose stuff from base
            base.Dispose();
        }

        #endregion
    }
}
