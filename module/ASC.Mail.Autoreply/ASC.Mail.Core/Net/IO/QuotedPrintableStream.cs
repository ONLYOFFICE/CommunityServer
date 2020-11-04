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
    using System.Globalization;
    using System.IO;

    #endregion

    /// <summary>
    /// Implements RFC 2045 6.7. Quoted-Printable stream.
    /// </summary>
    public class QuotedPrintableStream : Stream
    {
        #region Members

        private readonly FileAccess m_AccessMode = FileAccess.ReadWrite;
        private readonly byte[] m_pDecodedBuffer;
        private readonly byte[] m_pEncodedBuffer;
        private readonly SmartStream m_pStream;
        private int m_DecodedCount;
        private int m_DecodedOffset;
        private int m_EncodedCount;
        private byte[] line_buf;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool CanRead
        {
            get { return (m_AccessMode & FileAccess.Read) != 0; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool CanWrite
        {
            get { return (m_AccessMode & FileAccess.Write) != 0; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.  This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when this property is accessed.</exception>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets or sets the position within the current stream. This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when this property is accessed.</exception>
        public override long Position
        {
            get { throw new NotSupportedException(); }

            set { throw new NotSupportedException(); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="access">Specifies stream access mode.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public QuotedPrintableStream(SmartStream stream, FileAccess access)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            m_pStream = stream;
            m_AccessMode = access;

            m_pDecodedBuffer = new byte[Workaround.Definitions.MaxStreamLineLength];
            m_pEncodedBuffer = new byte[78];
            line_buf = new byte[Workaround.Definitions.MaxStreamLineLength];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public override void Flush()
        {
            if (m_EncodedCount > 0)
            {
                m_pStream.Write(m_pEncodedBuffer, 0, m_EncodedCount);
                m_EncodedCount = 0;
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
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the current stream. This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when this method is accessed.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="NotSupportedException">Is raised when reading not supported.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentException("Invalid argument 'offset' value.");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("Invalid argument 'count' value.");
            }
            if ((m_AccessMode & FileAccess.Read) == 0)
            {
                throw new NotSupportedException();
            }

            while (true)
            {
                // Read next quoted-printable line and decode it.
                if (m_DecodedOffset >= m_DecodedCount)
                {
                    m_DecodedOffset = 0;
                    m_DecodedCount = 0;
                    SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(line_buf,
                                                                                             SizeExceededAction
                                                                                                 .
                                                                                                 ThrowException);
                    m_pStream.ReadLine(readLineOP, false);
                    // IO error reading line.
                    if (readLineOP.Error != null)
                    {
                        throw readLineOP.Error;
                    }
                        // We reached end of stream.
                    else if (readLineOP.BytesInBuffer == 0)
                    {
                        return 0;
                    }
                        // Decode quoted-printable line.
                    else
                    {
                        // Process bytes.
                        bool softLineBreak = false;
                        int lineLength = readLineOP.LineBytesInBuffer;
                        for (int i = 0; i < readLineOP.LineBytesInBuffer; i++)
                        {
                            byte b = readLineOP.Buffer[i];
                            // We have soft line-break.
                            if (b == '=' && i == (lineLength - 1))
                            {
                                softLineBreak = true;
                            }
                                // We should have =XX char.
                            else if (b == '=')
                            {
                                byte b1 = readLineOP.Buffer[++i];
                                byte b2 = readLineOP.Buffer[++i];

                                string b1b2 = ((char)b1).ToString() + (char)b2;
                                byte b1b2_num;
                                if (byte.TryParse(b1b2, NumberStyles.HexNumber, null, out b1b2_num))
                                {
                                    m_pDecodedBuffer[m_DecodedCount++] = b1b2_num;
                                }
                                else
                                {
                                    m_pDecodedBuffer[m_DecodedCount++] = b;
                                    m_pDecodedBuffer[m_DecodedCount++] = b1;
                                    m_pDecodedBuffer[m_DecodedCount++] = b2;
                                }
                            }
                            // Normal char.
                            else
                            {
                                m_pDecodedBuffer[m_DecodedCount++] = b;
                            }
                        }

                        if (!softLineBreak)
                        {
                            m_pDecodedBuffer[m_DecodedCount++] = (byte) '\r';
                            m_pDecodedBuffer[m_DecodedCount++] = (byte) '\n';
                        }
                    }
                }

                // We some decoded data, return it.
                if (m_DecodedOffset < m_DecodedCount)
                {
                    int countToCopy = Math.Min(count, m_DecodedCount - m_DecodedOffset);
                    Array.Copy(m_pDecodedBuffer, m_DecodedOffset, buffer, offset, countToCopy);
                    m_DecodedOffset += countToCopy;

                    return countToCopy;
                }
            }
        }

        /// <summary>
        /// Encodes a sequence of bytes, writes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="NotSupportedException">Is raised when reading not supported.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentException("Invalid argument 'offset' value.");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("Invalid argument 'count' value.");
            }
            if ((m_AccessMode & FileAccess.Write) == 0)
            {
                throw new NotSupportedException();
            }

            // Process bytes.
            for (int i = 0; i < count; i++)
            {
                byte b = buffer[offset + i];

                // We don't need to encode byte.
                if ((b >= 33 && b <= 60) || (b >= 62 && b <= 126))
                {
                    // Maximum allowed quoted-printable line length reached, do soft line break.
                    if (m_EncodedCount >= 75)
                    {
                        m_pEncodedBuffer[m_EncodedCount++] = (byte) '=';
                        m_pEncodedBuffer[m_EncodedCount++] = (byte) '\r';
                        m_pEncodedBuffer[m_EncodedCount++] = (byte) '\n';

                        // Write encoded data to underlying stream.
                        Flush();
                    }

                    m_pEncodedBuffer[m_EncodedCount++] = b;
                }
                    // We need to encode byte.
                else
                {
                    // Maximum allowed quote-printable line length reached, do soft line break.
                    if (m_EncodedCount >= 73)
                    {
                        m_pEncodedBuffer[m_EncodedCount++] = (byte) '=';
                        m_pEncodedBuffer[m_EncodedCount++] = (byte) '\r';
                        m_pEncodedBuffer[m_EncodedCount++] = (byte) '\n';

                        // Write encoded data to underlying stream.
                        Flush();
                    }

                    // Encode byte.
                    m_pEncodedBuffer[m_EncodedCount++] = (byte) '=';
                    m_pEncodedBuffer[m_EncodedCount++] = (byte) (b >> 4).ToString("x")[0];
                    m_pEncodedBuffer[m_EncodedCount++] = (byte) (b & 0xF).ToString("x")[0];
                }
            }
        }

        #endregion
    }
}