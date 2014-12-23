/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.IO
{
    #region usings

    using System;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// This class implements "line" reader, LF and CRLF lines are supported.
    /// </summary>
    public class LineReader : IDisposable
    {
        #region Members

        private readonly int m_BufferSize = Workaround.Definitions.MaxStreamLineLength;
        private readonly bool m_Owner;
        private readonly byte[] m_pLineBuffer;
        private readonly byte[] m_pReadBuffer;
        private readonly Stream m_pSource;
        private bool m_IsDisposed;
        private int m_OffsetInBuffer;
        private Encoding m_pCharset;
        private int m_StoredInBuffer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_IsDisposed; }
        }

        /// <summary>
        /// Gets source stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Stream Stream
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSource;
            }
        }

        /// <summary>
        /// Gets if line reader is <b>Stream</b> owner.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public bool IsStreamOwner
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_Owner;
            }
        }

        /// <summary>
        /// Gets or sets charset to us for deocoding bytes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when null reference is passed.</exception>
        public Encoding Charset
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pCharset;
            }

            set
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_pCharset = value;
            }
        }

        /// <summary>
        /// Gets number of bytes in read buffer.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int AvailableInBuffer
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_StoredInBuffer - m_OffsetInBuffer;
            }
        }

        /// <summary>
        /// Gets if line reader can synchronize source stream to actual readed data position.
        /// </summary>
        public bool CanSyncStream
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pSource.CanSeek;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Stream from where to read lines.</param>
        /// <param name="owner">Specifies if <b>LineReader</b> is owner of <b>stream</b>. 
        /// <param name="bufferSize">Read buffer size, value 1 means no buffering.</param>
        /// If this value is true, closing reader will close <b>stream</b>.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        public LineReader(Stream stream, bool owner, int bufferSize)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (bufferSize < 1)
            {
                throw new ArgumentException("Argument 'bufferSize' value must be >= 1.");
            }

            m_pSource = stream;
            m_Owner = owner;
            m_BufferSize = bufferSize;

            m_pReadBuffer = new byte[bufferSize];
            m_pLineBuffer = new byte[Workaround.Definitions.MaxStreamLineLength];
            m_pCharset = Encoding.UTF8;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }
            m_IsDisposed = true;

            if (m_Owner)
            {
                m_pSource.Dispose();
            }
        }

        /// <summary>
        /// Reads line from source stream. Returns null if end of stream(EOS) reached.
        /// </summary>
        /// <returns>Returns readed line or null if end of stream reached.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public string ReadLine()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            int storedCount = ReadLine(m_pLineBuffer,
                                       0,
                                       m_pLineBuffer.Length,
                                       SizeExceededAction.ThrowException);
            if (storedCount == -1)
            {
                return null;
            }
            else
            {
                return m_pCharset.GetString(m_pLineBuffer, 0, storedCount);
            }
        }

        /// <summary>
        /// Reads binary line and stores it to the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store line data.</param>
        /// <param name="offset">Start offset in the buffer.</param>
        /// <param name="count">Maximum number of bytes store to the buffer.</param>
        /// <param name="exceededAction">Specifies how reader acts when line buffer too small.</param>
        /// <returns>Returns number of bytes stored to <b>buffer</b> or -1 if end of stream reached.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="LineSizeExceededException">Is raised when line is bigger than <b>buffer</b> can store.</exception>
        public int ReadLine(byte[] buffer, int offset, int count, SizeExceededAction exceededAction)
        {
            int rawBytesReaded = 0;

            return ReadLine(buffer, offset, count, exceededAction, out rawBytesReaded);
        }

        /// <summary>
        /// Reads binary line and stores it to the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer where to store line data.</param>
        /// <param name="offset">Start offset in the buffer.</param>
        /// <param name="count">Maximum number of bytes store to the buffer.</param>
        /// <param name="exceededAction">Specifies how reader acts when line buffer too small.</param>
        /// <param name="rawBytesReaded">Gets raw number of bytes readed from source.</param>
        /// <returns>Returns number of bytes stored to <b>buffer</b> or -1 if end of stream reached.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="LineSizeExceededException">Is raised when line is bigger than <b>buffer</b> can store.</exception>
        public virtual int ReadLine(byte[] buffer,
                                    int offset,
                                    int count,
                                    SizeExceededAction exceededAction,
                                    out int rawBytesReaded)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentException("Argument 'offset' value must be >= 0.");
            }
            if (count < 0)
            {
                throw new ArgumentException("Argument 'count' value must be >= 0.");
            }
            if (buffer.Length < (count + offset))
            {
                throw new ArgumentException(
                    "Argument 'count' value is bigger than specified 'buffer' can store.");
            }

            int maxStoreCount = buffer.Length - offset;
            int storedCount = 0;
            rawBytesReaded = 0;
            bool sizeExceeded = false;
            while (true)
            {
                // No data in buffer, buffer next block.
                if (m_StoredInBuffer == m_OffsetInBuffer)
                {
                    m_OffsetInBuffer = 0;
                    m_StoredInBuffer = m_pSource.Read(m_pReadBuffer, 0, m_BufferSize);
                    // We reached end of stream, no more data.
                    if (m_StoredInBuffer == 0)
                    {
                        break;
                    }
                }

                byte currentByte = m_pReadBuffer[m_OffsetInBuffer++];
                rawBytesReaded++;
                // We have LF, we got a line.
                if (currentByte == '\n')
                {
                    break;
                }
                    // We just skip CR, because CR must be with LF, otherwise it's invalid CR.
                else if (currentByte == '\r') {}
                    // Normal byte.
                else
                {
                    // Line buffer full.
                    if (storedCount == maxStoreCount)
                    {
                        sizeExceeded = true;

                        if (exceededAction == SizeExceededAction.ThrowException)
                        {
                            throw new LineSizeExceededException();
                        }
                    }
                    else
                    {
                        buffer[offset + storedCount] = currentByte;
                        storedCount++;
                    }
                }
            }

            // Line buffer is not big enough to store whole line data.
            if (sizeExceeded)
            {
                throw new LineSizeExceededException();
            }
                // We haven't readed nothing, we are end of stream.
            else if (rawBytesReaded == 0)
            {
                return -1;
            }
            else
            {
                return storedCount;
            }
        }

        /// <summary>
        /// Sets stream position to the place we have consumed from stream and clears buffer data.
        /// For example if we have 10 byets in buffer, stream position is actually +10 bigger than 
        /// we readed, the result is that stream.Position -= 10 and buffer is cleared.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when source stream won't support seeking.</exception>
        public virtual void SyncStream()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!m_pSource.CanSeek)
            {
                throw new InvalidOperationException("Source stream does not support seeking, can't sync.");
            }

            if (AvailableInBuffer > 0)
            {
                m_pSource.Position -= AvailableInBuffer;
                m_OffsetInBuffer = 0;
                m_StoredInBuffer = 0;
            }
        }

        #endregion
    }
}