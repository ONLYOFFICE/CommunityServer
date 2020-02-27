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
    using IO;

    #endregion

    /// <summary>
    /// This class represents MIME multipart/digest body. Defined in RFC 2046 5.1.5.
    /// </summary>
    /// <remarks>
    /// The "multipart/digest" Content-Type is intended to be used to send collections of messages.
    /// </remarks>
    public class MIME_b_MultipartDigest : MIME_b_Multipart
    {
        #region Properties

        /// <summary>
        /// Gets default body part Content-Type. For more info see RFC 2046 5.1.
        /// </summary>
        public override MIME_h_ContentType DefaultBodyPartContentType
        {
            /* RFC 2046 5.1.6.
                The absence of a Content-Type header usually indicates that the corresponding body has
                a content-type of "message/rfc822".
            */

            get
            {
                MIME_h_ContentType retVal = new MIME_h_ContentType("message/rfc822");

                return retVal;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="contentType">Content type.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>contentType</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public MIME_b_MultipartDigest(MIME_h_ContentType contentType) : base(contentType)
        {
            if (
                !string.Equals(contentType.TypeWithSubype,
                               "multipart/digest",
                               StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ArgumentException(
                    "Argument 'contentType.TypeWithSubype' value must be 'multipart/digest'.");
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

            MIME_b_MultipartDigest retVal = new MIME_b_MultipartDigest(owner.ContentType);
            ParseInternal(owner, mediaType, stream, retVal);

            return retVal;
        }
    }
}