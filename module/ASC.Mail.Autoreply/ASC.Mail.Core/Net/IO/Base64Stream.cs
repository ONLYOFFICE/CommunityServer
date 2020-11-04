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
    using System.Collections.Generic;
    using System.IO;

    #endregion

    /// <summary>
    /// This class implements base64 encoder/decoder. Defined in RFC 4648.
    /// </summary>
    public class Base64Stream : Stream, IDisposable
    {
        #region Members

        private static readonly short[] BASE64_DECODE_TABLE = new short[]
                                                                  {
                                                                      -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                                                                      // 0 -    9
                                                                      -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                                                                      //10 -   19
                                                                      -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                                                                      //20 -   29
                                                                      -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                                                                      //30 -   39
                                                                      -1, -1, -1, 62, -1, -1, -1, 63, 52, 53,
                                                                      //40 -   49
                                                                      54, 55, 56, 57, 58, 59, 60, 61, -1, -1,
                                                                      //50 -   59
                                                                      -1, -1, -1, -1, -1, 0, 1, 2, 3, 4,
                                                                      //60 -   69
                                                                      5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
                                                                      //70 -   79
                                                                      15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                                      //80 -   89
                                                                      25, -1, -1, -1, -1, -1, -1, 26, 27, 28,
                                                                      //90 -   99
                                                                      29, 30, 31, 32, 33, 34, 35, 36, 37, 38,
                                                                      //100 - 109
                                                                      39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
                                                                      //110 - 119
                                                                      49, 50, 51, -1, -1, -1, -1, -1 //120 - 127
                                                                  };

        private static readonly byte[] BASE64_ENCODE_TABLE = new[]
                                                                 {
                                                                     (byte) 'A', (byte) 'B', (byte) 'C',
                                                                     (byte) 'D', (byte) 'E', (byte) 'F',
                                                                     (byte) 'G', (byte) 'H', (byte) 'I',
                                                                     (byte) 'J', (byte) 'K', (byte) 'L',
                                                                     (byte) 'M', (byte) 'N', (byte) 'O',
                                                                     (byte) 'P', (byte) 'Q', (byte) 'R',
                                                                     (byte) 'S', (byte) 'T', (byte) 'U',
                                                                     (byte) 'V', (byte) 'W', (byte) 'X',
                                                                     (byte) 'Y', (byte) 'Z', (byte) 'a',
                                                                     (byte) 'b', (byte) 'c', (byte) 'd',
                                                                     (byte) 'e', (byte) 'f', (byte) 'g',
                                                                     (byte) 'h', (byte) 'i', (byte) 'j',
                                                                     (byte) 'k', (byte) 'l', (byte) 'm',
                                                                     (byte) 'n', (byte) 'o', (byte) 'p',
                                                                     (byte) 'q', (byte) 'r', (byte) 's',
                                                                     (byte) 't', (byte) 'u', (byte) 'v',
                                                                     (byte) 'w', (byte) 'x', (byte) 'y',
                                                                     (byte) 'z', (byte) '0', (byte) '1',
                                                                     (byte) '2', (byte) '3', (byte) '4',
                                                                     (byte) '5', (byte) '6', (byte) '7',
                                                                     (byte) '8', (byte) '9', (byte) '+',
                                                                     (byte) '/'
                                                                 };

        private readonly FileAccess m_AccessMode = FileAccess.ReadWrite;
        private readonly bool m_AddLineBreaks = true;
        private readonly bool m_IsOwner;
        private readonly Queue<byte> m_pDecodeReminder;
        private readonly byte[] m_pEncode3x8Block = new byte[3];
        private readonly byte[] m_pEncodeBuffer = new byte[78];
        private readonly Stream m_pStream;
        private int m_EncodeBufferOffset;
        private bool m_IsDisposed;
        private bool m_IsFinished;
        private int m_OffsetInEncode3x8Block;

        private static string[] padding_tails = new string[4] { "", "===", "==", "=" };

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
        /// Gets the length in bytes of the stream.  This method is not supported and always throws a NotSupportedException.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when this property is accessed.</exception>
        public override long Length
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException("SmartStream");
                }

                throw new NotSupportedException();
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

        private MemoryStream decodedDataStream = null;
        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Stream which to encode/decode.</param>
        /// <param name="owner">Specifies if Base64Stream is owner of <b>stream</b>.</param>
        /// <param name="addLineBreaks">Specifies if encoder inserts CRLF after each 76 bytes.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public Base64Stream(Stream stream, bool owner, bool addLineBreaks)
            : this(stream, owner, addLineBreaks, FileAccess.ReadWrite) {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Stream which to encode/decode.</param>
        /// <param name="owner">Specifies if Base64Stream is owner of <b>stream</b>.</param>
        /// <param name="addLineBreaks">Specifies if encoder inserts CRLF after each 76 bytes.</param>
        /// <param name="access">This stream access mode.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public Base64Stream(Stream stream, bool owner, bool addLineBreaks, FileAccess access)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            // Parse all stream
            if (stream.Length > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
                decodedDataStream = new MemoryStream();
                using (StreamReader reader = new StreamReader(stream))
                {
                    List<string> lines = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        string next_line = reader.ReadLine();
                        lines.Add(next_line);
                        int exc_bytes = next_line.Length & 0x03;
                        // If the line length is not multiple of 4 then we need to process possible wrong message content (it really happens)
                        if (exc_bytes != 0)
                        {
                            // Process even part of the line first
                            string even_part = next_line.Substring(0, next_line.Length - exc_bytes);
                            byte[] decoded = Convert.FromBase64String(even_part);
                            decodedDataStream.Write(decoded, 0, decoded.Length);

                            // then try to recover the odd part by appending "=" to it
                            try
                            {
                                string rest = next_line.Substring(next_line.Length - exc_bytes, exc_bytes);
                                byte[] rest_decoded = Convert.FromBase64String(rest + padding_tails[exc_bytes]);
                                decodedDataStream.Write(decoded, 0, decoded.Length);
                            }
                            catch (System.FormatException) {}
                        }
                        else
                        {
                            byte[] decoded = Convert.FromBase64String(next_line);
                            decodedDataStream.Write(decoded, 0, decoded.Length);
                        }
                    }
                    decodedDataStream.Seek(0, SeekOrigin.Begin);
                }
            }

            m_pStream = stream;
            m_IsOwner = owner;
            m_AddLineBreaks = addLineBreaks;
            m_AccessMode = access;

            m_pDecodeReminder = new Queue<byte>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Celans up any resources being used.
        /// </summary>
        public new void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }
            try
            {
                Finish();
            }
            catch {}
            m_IsDisposed = true;

            if (m_IsOwner)
            {
                m_pStream.Close();
            }
            if (decodedDataStream!=null)
            {
                decodedDataStream.Close();
                decodedDataStream.Dispose();
            }
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public override void Flush()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("Base64Stream");
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
                throw new ObjectDisposedException("Base64Stream");
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
                throw new ObjectDisposedException("Base64Stream");
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
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null reference.</exception>
        /// <exception cref="NotSupportedException">Is raised when reading not supported.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException("Base64Stream");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((m_AccessMode & FileAccess.Read) == 0)
            {
                throw new NotSupportedException();
            }

            if (decodedDataStream!=null)
            {
                //read from it
                return decodedDataStream.Read(buffer, offset, count);
            }

            /* RFC 4648.
			
				Base64 is processed from left to right by 4 6-bit byte block, 4 6-bit byte block 
				are converted to 3 8-bit bytes.
				If base64 4 byte block doesn't have 3 8-bit bytes, missing bytes are marked with =. 
							
				Value Encoding  Value Encoding  Value Encoding  Value Encoding
					0 A            17 R            34 i            51 z
					1 B            18 S            35 j            52 0
					2 C            19 T            36 k            53 1
					3 D            20 U            37 l            54 2
					4 E            21 V            38 m            55 3
					5 F            22 W            39 n            56 4
					6 G            23 X            40 o            57 5
					7 H            24 Y            41 p            58 6
					8 I            25 Z            42 q            59 7
					9 J            26 a            43 r            60 8
					10 K           27 b            44 s            61 9
					11 L           28 c            45 t            62 +
					12 M           29 d            46 u            63 /
					13 N           30 e            47 v
					14 O           31 f            48 w         (pad) =
					15 P           32 g            49 x
					16 Q           33 h            50 y
					
				NOTE: 4 base64 6-bit bytes = 3 8-bit bytes				
					// |    6-bit    |    6-bit    |    6-bit    |    6-bit    |
					// | 1 2 3 4 5 6 | 1 2 3 4 5 6 | 1 2 3 4 5 6 | 1 2 3 4 5 6 |
					// |    8-bit         |    8-bit        |    8-bit         |
			*/

            int storedInBuffer = 0;

            // If we have decoded-buffered bytes, use them first.
            while (m_pDecodeReminder.Count > 0)
            {
                buffer[offset++] = m_pDecodeReminder.Dequeue();
                storedInBuffer++;
                count--;

                // We filled whole "buffer", no more room.
                if (count == 0)
                {
                    return storedInBuffer;
                }
            }

            // 1) Calculate as much we can decode to "buffer". !!! We need to read as 4x7-bit blocks.
            int rawBytesToRead = (int) Math.Ceiling(count/3.0)*4;

            byte[] readBuffer = new byte[rawBytesToRead];
            short[] decodeBlock = new short[4];
            byte[] decodedBlock = new byte[3];
            int decodeBlockOffset = 0;
            int paddedCount = 0;
            // Decode while we have room in "buffer".
            while (storedInBuffer < count)
            {
                int readedCount = m_pStream.Read(readBuffer, 0, rawBytesToRead);
                // We reached end of stream, no more data.
                if (readedCount == 0)
                {
                    // We have last block without padding 1 char.
                    if (decodeBlockOffset == 3)
                    {
                        buffer[offset + storedInBuffer++] = (byte) (decodeBlock[0] << 2 | decodeBlock[1] >> 4);
                        // See if "buffer" can accomodate 2 byte.
                        if (storedInBuffer < count)
                        {
                            buffer[offset + storedInBuffer++] =
                                (byte) ((decodeBlock[1] & 0xF) << 4 | decodeBlock[2] >> 2);
                        }
                        else
                        {
                            m_pDecodeReminder.Enqueue(
                                (byte) ((decodeBlock[1] & 0xF) << 4 | decodeBlock[2] >> 2));
                        }
                    }
                        // We have last block without padding 2 chars.
                    else if (decodeBlockOffset == 2)
                    {
                        buffer[offset + storedInBuffer++] = (byte) (decodeBlock[0] << 2 | decodeBlock[1] >> 4);
                    }
                        // We have invalid base64 data.
                    else if (decodeBlockOffset == 1)
                    {
                        throw new InvalidDataException("Incomplete base64 data..");
                    }

                    return storedInBuffer;
                }

                // Process readed bytes.
                for (int i = 0; i < readedCount; i++)
                {
                    byte b = readBuffer[i];

                    // If padding char.
                    if (b == '=')
                    {
                        decodeBlock[decodeBlockOffset++] = (byte) '=';
                        paddedCount++;
                        rawBytesToRead--;
                    }
                        // If base64 char.
                    else if (BASE64_DECODE_TABLE[b] != -1)
                    {
                        decodeBlock[decodeBlockOffset++] = BASE64_DECODE_TABLE[b];
                        rawBytesToRead--;
                    }
                        // Non-base64 char, skip it.
                    else {}

                    // Decode block full, decode bytes.
                    if (decodeBlockOffset == 4)
                    {
                        // Decode 3x8-bit block.
                        decodedBlock[0] = (byte) (decodeBlock[0] << 2 | decodeBlock[1] >> 4);
                        decodedBlock[1] = (byte) ((decodeBlock[1] & 0xF) << 4 | decodeBlock[2] >> 2);
                        decodedBlock[2] = (byte) ((decodeBlock[2] & 0x3) << 6 | decodeBlock[3] >> 0);

                        // Invalid base64 data. Base64 final quantum may have max 2 padding chars.
                        if (paddedCount > 2)
                        {
                            throw new InvalidDataException(
                                "Invalid base64 data, more than 2 padding chars(=).");
                        }

                        for (int n = 0; n < (3 - paddedCount); n++)
                        {
                            // We have room in "buffer", store byte there.
                            if (storedInBuffer < count)
                            {
                                buffer[offset + storedInBuffer++] = decodedBlock[n];
                            }
                                //No room in "buffer", store reminder.
                            else
                            {
                                m_pDecodeReminder.Enqueue(decodedBlock[n]);
                            }
                        }

                        decodeBlockOffset = 0;
                        paddedCount = 0;
                    }
                }
            }

            return storedInBuffer;
        }

        /// <summary>
        /// Encodes a sequence of bytes, writes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this.Finish has been called and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="NotSupportedException">Is raised when reading not supported.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (m_IsFinished)
            {
                throw new InvalidOperationException("Stream is marked as finished by calling Finish method.");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentException("Invalid argument 'offset' value.");
            }
            if (count < 0 || count > (buffer.Length - offset))
            {
                throw new ArgumentException("Invalid argument 'count' value.");
            }
            if ((m_AccessMode & FileAccess.Write) == 0)
            {
                throw new NotSupportedException();
            }

            /* RFC 4648.
			
				Base64 is processed from left to right by 4 6-bit byte block, 4 6-bit byte block 
				are converted to 3 8-bit bytes.
				If base64 4 byte block doesn't have 3 8-bit bytes, missing bytes are marked with =. 
							
				Value Encoding  Value Encoding  Value Encoding  Value Encoding
					0 A            17 R            34 i            51 z
					1 B            18 S            35 j            52 0
					2 C            19 T            36 k            53 1
					3 D            20 U            37 l            54 2
					4 E            21 V            38 m            55 3
					5 F            22 W            39 n            56 4
					6 G            23 X            40 o            57 5
					7 H            24 Y            41 p            58 6
					8 I            25 Z            42 q            59 7
					9 J            26 a            43 r            60 8
					10 K           27 b            44 s            61 9
					11 L           28 c            45 t            62 +
					12 M           29 d            46 u            63 /
					13 N           30 e            47 v
					14 O           31 f            48 w         (pad) =
					15 P           32 g            49 x
					16 Q           33 h            50 y
					
				NOTE: 4 base64 6-bit bytes = 3 8-bit bytes				
					// |    6-bit    |    6-bit    |    6-bit    |    6-bit    |
					// | 1 2 3 4 5 6 | 1 2 3 4 5 6 | 1 2 3 4 5 6 | 1 2 3 4 5 6 |
					// |    8-bit         |    8-bit        |    8-bit         |
			*/

            int encodeBufSize = m_pEncodeBuffer.Length;

            // Process all bytes.
            for (int i = 0; i < count; i++)
            {
                m_pEncode3x8Block[m_OffsetInEncode3x8Block++] = buffer[offset + i];

                // 3x8-bit encode block is full, encode it.
                if (m_OffsetInEncode3x8Block == 3)
                {
                    m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[m_pEncode3x8Block[0] >> 2];
                    m_pEncodeBuffer[m_EncodeBufferOffset++] =
                        BASE64_ENCODE_TABLE[(m_pEncode3x8Block[0] & 0x03) << 4 | m_pEncode3x8Block[1] >> 4];
                    m_pEncodeBuffer[m_EncodeBufferOffset++] =
                        BASE64_ENCODE_TABLE[(m_pEncode3x8Block[1] & 0x0F) << 2 | m_pEncode3x8Block[2] >> 6];
                    m_pEncodeBuffer[m_EncodeBufferOffset++] =
                        BASE64_ENCODE_TABLE[(m_pEncode3x8Block[2] & 0x3F)];

                    // Encode buffer is full, write buffer to underlaying stream (we reserved 2 bytes for CRLF).
                    if (m_EncodeBufferOffset >= (encodeBufSize - 2))
                    {
                        if (m_AddLineBreaks)
                        {
                            m_pEncodeBuffer[m_EncodeBufferOffset++] = (byte) '\r';
                            m_pEncodeBuffer[m_EncodeBufferOffset++] = (byte) '\n';
                        }

                        m_pStream.Write(m_pEncodeBuffer, 0, m_EncodeBufferOffset);
                        m_EncodeBufferOffset = 0;
                    }

                    m_OffsetInEncode3x8Block = 0;
                }
            }
        }

        /// <summary>
        /// Completes encoding. Call this method if all data has written and no more data. 
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public void Finish()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (m_IsFinished)
            {
                return;
            }
            m_IsFinished = true;

            // PADD left-over, if any. Write encode buffer to underlaying stream.
            if (m_OffsetInEncode3x8Block == 1)
            {
                m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[m_pEncode3x8Block[0] >> 2];
                m_pEncodeBuffer[m_EncodeBufferOffset++] =
                    BASE64_ENCODE_TABLE[(m_pEncode3x8Block[0] & 0x03) << 4];
                m_pEncodeBuffer[m_EncodeBufferOffset++] = (byte) '=';
                m_pEncodeBuffer[m_EncodeBufferOffset++] = (byte) '=';
            }
            else if (m_OffsetInEncode3x8Block == 2)
            {
                m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[m_pEncode3x8Block[0] >> 2];
                m_pEncodeBuffer[m_EncodeBufferOffset++] =
                    BASE64_ENCODE_TABLE[(m_pEncode3x8Block[0] & 0x03) << 4 | m_pEncode3x8Block[1] >> 4];
                m_pEncodeBuffer[m_EncodeBufferOffset++] =
                    BASE64_ENCODE_TABLE[(m_pEncode3x8Block[1] & 0x0F) << 2];
                m_pEncodeBuffer[m_EncodeBufferOffset++] = (byte) '=';
            }

            if (m_EncodeBufferOffset > 0)
            {
                m_pStream.Write(m_pEncodeBuffer, 0, m_EncodeBufferOffset);
            }
        }

        #endregion
    }
}