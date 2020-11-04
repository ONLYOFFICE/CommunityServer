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
    using Mail;

    #endregion

    /// <summary>
    /// This class represents MIME message/rfc822 body. Defined in RFC 2046 5.2.1.
    /// </summary>
    public class MIME_b_MessageRfc822 : MIME_b
    {
        #region Members

        private Mail_Message m_pMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if body has modified.
        /// </summary>
        public override bool IsModified
        {
            get { return m_pMessage.IsModified; }
        }

        /// <summary>
        /// Gets embbed mail message.
        /// </summary>
        public Mail_Message Message
        {
            get { return m_pMessage; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MIME_b_MessageRfc822() : base(new MIME_h_ContentType("message/rfc822"))
        {
            m_pMessage = new Mail_Message();
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

            m_pMessage.ToStream(stream, headerWordEncoder, headerParmetersCharset);
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

            MIME_b_MessageRfc822 retVal = new MIME_b_MessageRfc822();
            if (owner.ContentTransferEncoding != null && owner.ContentTransferEncoding.Equals("base64", StringComparison.OrdinalIgnoreCase))
            {
                Stream decodedDataStream = new MemoryStream();
                using (StreamReader reader = new StreamReader(stream))
                {
                    byte[] decoded = Convert.FromBase64String(reader.ReadToEnd());
                    decodedDataStream.Write(decoded, 0, decoded.Length);
                    decodedDataStream.Seek(0, SeekOrigin.Begin);
                }

                //Create base64 decoder
                stream = new SmartStream(decodedDataStream,true);

            }
            retVal.m_pMessage = Mail_Message.ParseFromStream(stream);

            return retVal;
        }
    }
}