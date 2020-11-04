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