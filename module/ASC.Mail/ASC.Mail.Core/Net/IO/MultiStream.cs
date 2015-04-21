/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.IO
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.IO;

    #endregion

    /// <summary>
    /// This class combines multiple stream into one stream for reading.
    /// The most common usage for that stream is when you need to insert some data to the beginning of some stream.
    /// </summary>
    public class MultiStream : Stream
    {
        #region Members

        private bool m_IsDisposed;
        private Queue<Stream> m_pStreams;

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

                return false;
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
        /// <exception cref="NotSupportedException">Is raised when one of the source streams won't support <b>Length</b> property.</exception>
        public override long Length
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                long length = 0;
                foreach (Stream stream in m_pStreams.ToArray())
                {
                    length += stream.Length;
                }

                return length;
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream. This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when this property is accessed.</exception>
        public override long Position
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                throw new NotSupportedException();
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                throw new NotSupportedException();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultiStream()
        {
            m_pStreams = new Queue<Stream>();
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
            m_pStreams = null;

            base.Dispose();
        }

        /// <summary>
        /// Appends this stream to read queue.
        /// </summary>
        /// <param name="stream">Stream to add.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        public void AppendStream(Stream stream)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            m_pStreams.Enqueue(stream);
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
        /// Sets the position within the current stream. This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <b>origin</b> parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when this method is accessed.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("SmartStream");
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the current stream. This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="Seek">Is raised when this method is accessed.</exception>
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

            while (true)
            {
                // We have readed all streams data, no data left.
                if (m_pStreams.Count == 0)
                {
                    return 0;
                }
                else
                {
                    int readedCount = m_pStreams.Peek().Read(buffer, offset, count);
                    // We have readed all current stream data.
                    if (readedCount == 0)
                    {
                        // Move to next stream .
                        m_pStreams.Dequeue();

                        // Next while loop will process "read".
                    }
                    else
                    {
                        return readedCount;
                    }
                }
            }
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