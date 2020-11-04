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


namespace ASC.Mail.Net.IO
{
    #region usings

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// Implements read-only stream what operates on specified range of source stream
    /// </summary>
    public class PartialStream : Stream
    {
        #region Members

        private readonly long m_Length;
        private readonly Stream m_pStream;
        private readonly long m_Start;
        private bool m_IsDisposed;
        private long m_Position;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool CanRead
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool CanSeek
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool CanWrite
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="Seek">Is raised when this property is accessed.</exception>
        public override long Length
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                return m_Length;
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override long Position
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                return m_Position;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }
                if (value < 0 || value > Length)
                {
                    throw new ArgumentException("Property 'Position' value must be >= 0 and <= this.Length.");
                }

                m_Position = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="start">Zero based start positon in source stream.</param>
        /// <param name="length">Length of stream.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public PartialStream(Stream stream, long start, long length)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Argument 'stream' does not support seeking.");
            }
            if (start < 0)
            {
                throw new ArgumentException("Argument 'start' value must be >= 0.");
            }
            if ((start + length) > stream.Length)
            {
                throw new ArgumentException("Argument 'length' value will exceed source stream length.");
            }

            m_pStream = stream;
            m_Start = start;
            m_Length = length;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public new void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;

            base.Dispose();
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public override void Flush()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("SmartStream");
            }
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <b>origin</b> parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("SmartStream");
            }

            if (origin == SeekOrigin.Begin)
            {
                m_Position = 0;
            }
            else if (origin == SeekOrigin.Current) {}
            else if (origin == SeekOrigin.End)
            {
                m_Position = m_Length;
            }

            return m_Position;
        }

        /// <summary>
        /// Sets the length of the current stream. This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when this method is accessed.</exception>
        public override void SetLength(long value)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("SmartStream");
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("SmartStream");
            }

            if (m_pStream.Position != (m_Start + m_Position))
            {
                m_pStream.Position = m_Start + m_Position;
            }
            int readedCount = m_pStream.Read(buffer, offset, Math.Min(count, (int) (Length - m_Position)));
            m_Position += readedCount;

            return readedCount;
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when this method is accessed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("SmartStream");
            }

            throw new NotSupportedException();
        }

        #endregion
    }
}