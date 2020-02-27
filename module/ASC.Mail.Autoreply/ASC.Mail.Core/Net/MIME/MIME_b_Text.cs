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

    #endregion

    /// <summary>
    /// This class represents MIME text/xxx bodies. Defined in RFC 2045.
    /// </summary>
    /// <remarks>
    /// The "text" media type is intended for sending material which is principally textual in form.
    /// </remarks>
    public class MIME_b_Text : MIME_b_SinglepartBase
    {
        #region Properties

        /// <summary>
        /// Gets body decoded text.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when not supported content-type charset or not supported content-transfer-encoding value.</exception>
        /// <exception cref="NotSupportedException">Is raised when body contains not supported Content-Transfer-Encoding.</exception>
        public string Text
        {
            get { return GetCharset().GetString(Data); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mediaType">MIME media type.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>mediaSubType</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public MIME_b_Text(string mediaType) : base(mediaType) {}

        #endregion

        #region Methods

        /// <summary>
        /// Sets text.
        /// </summary>
        /// <param name="transferEncoding">Content transfer encoding.</param>
        /// <param name="charset">Charset to use to encode text. If not sure, utf-8 is recommended.</param>
        /// <param name="text">Text.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>transferEncoding</b>, <b>charset</b> or <b>text</b> is null reference.</exception>
        /// <exception cref="NotSupportedException">Is raised when body contains not supported Content-Transfer-Encoding.</exception>
        public void SetText(string transferEncoding, Encoding charset, string text)
        {
            if (transferEncoding == null)
            {
                throw new ArgumentNullException("transferEncoding");
            }
            if (charset == null)
            {
                throw new ArgumentNullException("charset");
            }
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            SetEncodedData(transferEncoding, new MemoryStream(charset.GetBytes(text)));
            ContentType.Param_Charset = charset.WebName;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Gets charset from Content-Type. If char set isn't specified, "ascii" is defined as default and it will be returned.
        /// </summary>
        /// <returns>Returns content charset.</returns>
        /// <exception cref="ArgumentException">Is raised when Content-Type has not supported charset parameter value.</exception>
        private Encoding GetCharset()
        {
            // RFC 2046 4.1.2. The default character set, US-ASCII.

            if (string.IsNullOrEmpty(ContentType.Param_Charset))
            {
                return Encoding.ASCII;
            }
            else
            {
                return EncodingTools.GetEncodingByCodepageName(ContentType.Param_Charset) ?? Encoding.ASCII;
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

            MIME_b_Text retVal = new MIME_b_Text(mediaType);

            Net_Utils.StreamCopy(stream, retVal.EncodedStream, Workaround.Definitions.MaxStreamLineLength);

            return retVal;
        }
    }
}