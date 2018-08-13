/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Rfc 2045 6. Content-Transfer-Encoding. Specified how entity data is encoded.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public enum ContentTransferEncoding_enum
    {
        /// <summary>
        /// Rfc 2045 2.7. 7bit data.
        /// "7bit data" refers to data that is all represented as relatively
        ///	short lines with 998 octets or less between CRLF line separation
        ///	sequences [RFC-821].  No octets with decimal values greater than 127
        ///	are allowed and neither are NULs (octets with decimal value 0).  CR
        ///	(decimal value 13) and LF (decimal value 10) octets only occur as
        ///	part of CRLF line separation sequences.
        /// </summary>
        _7bit = 1,

        /// <summary>
        /// Rfc 2045 2.8. 8bit data.
        /// "8bit data" refers to data that is all represented as relatively
        ///	short lines with 998 octets or less between CRLF line separation
        ///	sequences [RFC-821]), but octets with decimal values greater than 127
        ///	may be used.  As with "7bit data" CR and LF octets only occur as part
        ///	of CRLF line separation sequences and no NULs are allowed.
        /// </summary>
        _8bit = 2,

        /// <summary>
        /// Rfc 2045 2.9. Binary data.
        /// "Binary data" refers to data where any sequence of octets whatsoever is allowed.
        /// </summary>
        Binary = 3,

        /// <summary>
        /// Rfc 2045 6.7 Quoted-Printable Content-Transfer-Encoding.
        /// </summary>
        QuotedPrintable = 4,

        /// <summary>
        /// Rfc 2045 6.8 Base64 Content-Transfer-Encoding.
        /// </summary>
        Base64 = 5,

        /// <summary>
        /// Content-Transfer-Encoding field isn't available(doesn't exist).
        /// </summary>
        NotSpecified = 30,

        /// <summary>
        /// Content transfer encoding is unknown.
        /// </summary>
        Unknown = 40,
    }
}