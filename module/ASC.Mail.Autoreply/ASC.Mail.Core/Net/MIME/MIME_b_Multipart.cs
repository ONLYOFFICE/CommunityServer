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


namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.IO;
    using System.Text;
    using IO;

    #endregion

    /// <summary>
    /// This class represents MIME application/xxx bodies. Defined in RFC 2046 5.1.
    /// </summary>
    /// <remarks>
    /// The "multipart" represents single MIME body containing multiple child MIME entities.
    /// The "multipart" body must contain at least 1 MIME entity.
    /// </remarks>
    public class MIME_b_Multipart : MIME_b
    {
        #region Nested type: _MultipartReader

        /// <summary>
        /// Implements  multipart "body parts" reader.
        /// </summary>
        public class _MultipartReader : Stream
        {
            #region Nested type: State

            /// <summary>
            /// This enum specified multipart reader sate.
            /// </summary>
            private enum State
            {
                /// <summary>
                /// First boundary must be seeked.
                /// </summary>
                SeekFirst = 0,

                /// <summary>
                /// Read next boundary.
                /// </summary>
                ReadNext = 1,

                /// <summary>
                /// All boundraies readed.
                /// </summary>
                Done = 2,
            }

            #endregion

            #region Members

            private readonly string m_Boundary = "";
            private readonly SmartStream.ReadLineAsyncOP m_pReadLineOP;
            private readonly SmartStream m_pStream;
            private readonly StringBuilder m_pTextEpilogue;
            private readonly StringBuilder m_pTextPreamble;
            private State m_State = State.SeekFirst;

            #endregion

            #region Properties

            /// <summary>
            /// Gets a value indicating whether the current stream supports reading.
            /// </summary>
            public override bool CanRead
            {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the current stream supports seeking.
            /// </summary>
            public override bool CanSeek
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating whether the current stream supports writing.
            /// </summary>
            public override bool CanWrite
            {
                get { return false; }
            }

            /// <summary>
            /// Gets the length in bytes of the stream.  This method is not supported and always throws a NotSupportedException.
            /// </summary>
            /// <exception cref="NotSupportedException">Is raised when this property is accessed.</exception>
            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Gets or sets the position within the current stream. This method is not supported and always throws a NotSupportedException.
            /// </summary>
            /// <exception cref="NotSupportedException">Is raised when this property is accessed.</exception>
            public override long Position
            {
                get { throw new NotSupportedException(); }

                set { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Gets "preamble" text. Defined in RFC 2046 5.1.1.
            /// </summary>
            /// <remarks>Preamble text is text between MIME entiy headers and first boundary.</remarks>
            public string TextPreamble
            {
                get { return m_pTextPreamble.ToString(); }
            }

            /// <summary>
            /// Gets "epilogue" text. Defined in RFC 2046 5.1.1.
            /// </summary>
            /// <remarks>Epilogue text is text after last boundary end.</remarks>
            public string TextEpilogue
            {
                get { return m_pTextEpilogue.ToString(); }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="stream">Stream from where to read body part.</param>
            /// <param name="boundary">Boundry ID what separates body parts.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> or <b>boundary</b> is null reference.</exception>
            public _MultipartReader(SmartStream stream, string boundary)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }
                if (boundary == null)
                {
                    throw new ArgumentNullException("boundary");
                }

                m_pStream = stream;
                m_Boundary = boundary;

                m_pReadLineOP = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                SizeExceededAction.ThrowException);
                m_pTextPreamble = new StringBuilder();
                m_pTextEpilogue = new StringBuilder();
            }

            #endregion

            #region Methods

            /// <summary>
            /// Moves to next "body part". Returns true if moved to next "body part" or false if there are no more parts.
            /// </summary>
            /// <returns>Returns true if moved to next "body part" or false if there are no more body parts.</returns>
            public bool Next()
            {
                if (m_State == State.Done)
                {
                    return false;
                }
                else if (m_State == State.SeekFirst)
                {
                    while (true)
                    {
                        m_pStream.ReadLine(m_pReadLineOP, false);
                        if (m_pReadLineOP.Error != null)
                        {
                            throw m_pReadLineOP.Error;
                        }
                            // We reached end of stream. Bad boundary: boundary end tag missing.
                        else if (m_pReadLineOP.BytesInBuffer == 0)
                        {
                            m_State = State.Done;

                            return false;
                        }
                        else
                        {
                            // Check if we have boundary start/end.
                            if (m_pReadLineOP.Buffer[0] == '-')
                            {
                                string boundary = m_pReadLineOP.LineUtf8;
                                // We have readed all MIME entity body parts.
                                if ("--" + m_Boundary + "--" == boundary)
                                {
                                    m_State = State.Done;

                                    return false;
                                }
                                    // We have next boundary.
                                else if ("--" + m_Boundary == boundary)
                                {
                                    m_State = State.ReadNext;

                                    return true;
                                }
                                // Not boundary or not boundary we want.
                                //else{
                            }

                            m_pTextPreamble.Append(m_pReadLineOP.LineUtf8 + "\r\n");
                        }
                    }
                }
                else if (m_State == State.ReadNext)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
            /// </summary>
            public override void Flush() {}

            /// <summary>
            /// Sets the position within the current stream. This method is not supported and always throws a NotSupportedException.
            /// </summary>
            /// <param name="offset">A byte offset relative to the <b>origin</b> parameter.</param>
            /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
            /// <returns>The new position within the current stream.</returns>
            /// <exception cref="NotSupportedException">Is raised when this method is accessed.</exception>
            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Sets the length of the current stream. This method is not supported and always throws a NotSupportedException.
            /// </summary>
            /// <param name="value">The desired length of the current stream in bytes.</param>
            /// <exception cref="Seek">Is raised when this method is accessed.</exception>
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
            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }

                if (m_State == State.Done)
                {
                    return 0;
                }

                m_pStream.ReadLine(m_pReadLineOP, false);
                if (m_pReadLineOP.Error != null)
                {
                    throw m_pReadLineOP.Error;
                }
                    // We reached end of stream. Bad boundary: boundary end tag missing.
                else if (m_pReadLineOP.BytesInBuffer == 0)
                {
                    m_State = State.Done;

                    return 0;
                }
                else
                {
                    // Check if we have boundary start/end.
                    if (m_pReadLineOP.Buffer[0] == '-')
                    {
                        string boundary = m_pReadLineOP.LineUtf8;
                        // We have readed all MIME entity body parts.
                        if ("--" + m_Boundary + "--" == boundary)
                        {
                            m_State = State.Done;

                            // Read "epilogoue" if any.
                            while (true)
                            {
                                m_pStream.ReadLine(m_pReadLineOP, false);

                                if (m_pReadLineOP.Error != null)
                                {
                                    throw m_pReadLineOP.Error;
                                }
                                    // We reached end of stream. Epilogue reading completed.
                                else if (m_pReadLineOP.BytesInBuffer == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    m_pTextEpilogue.Append(m_pReadLineOP.LineUtf8 + "\r\n");
                                }
                            }

                            return 0;
                        }
                            // We have next boundary.
                        else if ("--" + m_Boundary == boundary)
                        {
                            return 0;
                        }
                        // Not boundary or not boundary we want.
                        //else{
                    }
                    // We have body part data line
                    //else{

                    if (count < m_pReadLineOP.BytesInBuffer)
                    {
                        //throw new ArgumentException("Argument 'buffer' is to small. This should never happen.");
                    }
                    Array.Copy(m_pReadLineOP.Buffer, 0, buffer, offset, m_pReadLineOP.BytesInBuffer);

                    return m_pReadLineOP.BytesInBuffer;
                }
            }

            /// <summary>
            /// Writes sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
            /// </summary>
            /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
            /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
            /// <param name="count">The number of bytes to be written to the current stream.</param>
            /// <exception cref="NotSupportedException">Is raised when this method is accessed.</exception>
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        #endregion

        #region Members

        private readonly MIME_EntityCollection m_pBodyParts;
        private string m_TextEpilogue = "";
        private string m_TextPreamble = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets if body has modified.
        /// </summary>
        public override bool IsModified
        {
            get { return m_pBodyParts.IsModified; }
        }

        /// <summary>
        /// Gets default body part Content-Type. For more info see RFC 2046 5.1.
        /// </summary>
        public virtual MIME_h_ContentType DefaultBodyPartContentType
        {
            /* RFC 2026 5.1.
                The absence of a Content-Type header usually indicates that the corresponding body has
                a content-type of "text/plain; charset=US-ASCII".
            */

            get
            {
                MIME_h_ContentType retVal = new MIME_h_ContentType("text/plain");
                retVal.Param_Charset = "US-ASCII";

                return retVal;
            }
        }

        /// <summary>
        /// Gets multipart body body-parts collection.
        /// </summary>
        /// <remarks>Multipart entity child entities are called "body parts" in RFC 2045.</remarks>
        public MIME_EntityCollection BodyParts
        {
            get { return m_pBodyParts; }
        }

        /// <summary>
        /// Gets or sets "preamble" text. Defined in RFC 2046 5.1.1.
        /// </summary>
        /// <remarks>Preamble text is text between MIME entiy headers and first boundary.</remarks>
        public string TextPreamble
        {
            get { return m_TextPreamble; }

            set { m_TextPreamble = value; }
        }

        /// <summary>
        /// Gets or sets "epilogue" text. Defined in RFC 2046 5.1.1.
        /// </summary>
        /// <remarks>Epilogue text is text after last boundary end.</remarks>
        public string TextEpilogue
        {
            get { return m_TextEpilogue; }

            set { m_TextEpilogue = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="contentType">Content type.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>contentType</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public MIME_b_Multipart(MIME_h_ContentType contentType) : base(contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            if (string.IsNullOrEmpty(contentType.Param_Boundary))
            {
                throw new ArgumentException(
                    "Argument 'contentType' doesn't contain required boundary parameter.");
            }

            m_pBodyParts = new MIME_EntityCollection();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Stores MIME entity body to the specified stream.
        /// </summary>
        /// <param name="stream">Stream where to store body data.</param>
        /// <param name="headerWordEncoder">Header 8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="headerParmetersCharset">Charset to use to encode 8-bit header parameters. Value null means parameters not encoded.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        protected internal override void ToStream(Stream stream,
                                                  MIME_Encoding_EncodedWord headerWordEncoder,
                                                  Encoding headerParmetersCharset)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Set "preamble" text if any.
            if (!string.IsNullOrEmpty(m_TextPreamble))
            {
                byte[] preableBytes = null;
                if (m_TextPreamble.EndsWith("\r\n"))
                {
                    preableBytes = Encoding.UTF8.GetBytes(m_TextPreamble);
                }
                else
                {
                    preableBytes = Encoding.UTF8.GetBytes(m_TextPreamble + "\r\n");
                }
                stream.Write(preableBytes, 0, preableBytes.Length);
            }

            for (int i = 0; i < m_pBodyParts.Count; i++)
            {
                MIME_Entity bodyPart = m_pBodyParts[i];
                // Start new body part.
                byte[] bStart = Encoding.UTF8.GetBytes("--" + ContentType.Param_Boundary + "\r\n");
                stream.Write(bStart, 0, bStart.Length);

                bodyPart.ToStream(stream, headerWordEncoder, headerParmetersCharset);

                // Last body part, close boundary.
                if (i == (m_pBodyParts.Count - 1))
                {
                    byte[] bEnd = Encoding.UTF8.GetBytes("--" + ContentType.Param_Boundary + "--\r\n");
                    stream.Write(bEnd, 0, bEnd.Length);
                }
            }

            // Set "epilogoue" text if any.
            if (!string.IsNullOrEmpty(m_TextEpilogue))
            {
                byte[] epilogoueBytes = null;
                if (m_TextEpilogue.EndsWith("\r\n"))
                {
                    epilogoueBytes = Encoding.UTF8.GetBytes(m_TextEpilogue);
                }
                else
                {
                    epilogoueBytes = Encoding.UTF8.GetBytes(m_TextEpilogue + "\r\n");
                }
                stream.Write(epilogoueBytes, 0, epilogoueBytes.Length);
            }
        }

        #endregion

        /// <summary>
        /// Parses body from the specified stream
        /// </summary>
        /// <param name="owner">Owner MIME entity.</param>
        /// <param name="mediaType">MIME media type. For example: text/plain.</param>
        /// <param name="stream">Stream from where to read body.</param>
        /// <returns>Returns parsed body.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b>, <b>mediaType</b> or <b>stream</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when any parsing errors.</exception>
        protected new static MIME_b Parse(MIME_Entity owner, string mediaType, SmartStream stream)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (owner.ContentType == null || owner.ContentType.Param_Boundary == null)
            {
                throw new ParseException("Multipart entity has not required 'boundary' paramter.");
            }

            MIME_b_Multipart retVal = new MIME_b_Multipart(owner.ContentType);
            ParseInternal(owner, mediaType, stream, retVal);

            return retVal;
        }

        /// <summary>
        /// Internal body parsing.
        /// </summary>
        /// <param name="owner">Owner MIME entity.</param>
        /// <param name="mediaType">MIME media type. For example: text/plain.</param>
        /// <param name="stream">Stream from where to read body.</param>
        /// <param name="body">Multipart body instance.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b>, <b>mediaType</b>, <b>stream</b> or <b>body</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when any parsing errors.</exception>
        protected static void ParseInternal(MIME_Entity owner,
                                            string mediaType,
                                            SmartStream stream,
                                            MIME_b_Multipart body)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (owner.ContentType == null || owner.ContentType.Param_Boundary == null)
            {
                throw new ParseException("Multipart entity has not required 'boundary' parameter.");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            _MultipartReader multipartReader = new _MultipartReader(stream, owner.ContentType.Param_Boundary);
            while (multipartReader.Next())
            {
                MIME_Entity entity = new MIME_Entity();
                entity.Parse(new SmartStream(multipartReader, false), body.DefaultBodyPartContentType);
                body.m_pBodyParts.Add(entity);
                entity.SetParent(owner);
            }

            body.m_TextPreamble = multipartReader.TextPreamble;
            body.m_TextEpilogue = multipartReader.TextEpilogue;
        }
    }
}