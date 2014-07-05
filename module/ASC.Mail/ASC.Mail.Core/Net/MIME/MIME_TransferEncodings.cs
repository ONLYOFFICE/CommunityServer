/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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