/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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