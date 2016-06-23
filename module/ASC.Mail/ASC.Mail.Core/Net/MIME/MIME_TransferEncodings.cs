/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
    /// <summary>
    /// This class holds MIME content transfer encodings. Defined in RFC 2045 6.
    /// </summary>
    public class MIME_TransferEncodings
    {
        #region Members

        /// <summary>
        /// Used to encode arbitrary octet sequences into a form that satisfies the rules of 7bit. Has a fixed overhead and is 
        /// intended for non text data and text that is not ASCII heavy.
        /// Defined in RFC 2045 6.8.
        /// </summary>
        public static readonly string Base64 = "base64";

        /// <summary>
        /// Any sequence of octets. This type is not widely used. Defined in RFC 3030.
        /// </summary>
        public static readonly string Binary = "binary";

        /// <summary>
        /// Up to 998 octets per line with CR and LF (codes 13 and 10 respectively) only allowed to appear as part of a CRLF line ending.
        /// Defined in RFC 2045 6.2.
        /// </summary>
        public static readonly string EightBit = "8bit";

        /// <summary>
        /// Used to encode arbitrary octet sequences into a form that satisfies the rules of 7bit. 
        /// Designed to be efficient and mostly human readable when used for text data consisting primarily of US-ASCII characters 
        /// but also containing a small proportion of bytes with values outside that range.
        /// Defined in RFC 2045 6.7.
        /// </summary>
        public static readonly string QuotedPrintable = "quoted-printable";

        /// <summary>
        /// Up to 998 octets per line of the code range 1..127 with CR and LF (codes 13 and 10 respectively) only allowed to 
        /// appear as part of a CRLF line ending. This is the default value.
        /// Defined in RFC 2045 6.2.
        /// </summary>
        public static readonly string SevenBit = "7bit";

        #endregion
    }
}