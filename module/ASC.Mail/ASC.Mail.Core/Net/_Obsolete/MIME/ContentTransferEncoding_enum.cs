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