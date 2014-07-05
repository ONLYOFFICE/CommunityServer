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

namespace ASC.Mail.Net.Dns.Client
{
    /// <summary>
    /// Query type.
    /// </summary>
    public enum QTYPE
    {
        /// <summary>
        /// IPv4 host address
        /// </summary>
        A = 1,

        /// <summary>
        /// An authoritative name server.
        /// </summary>
        NS = 2,

        //	MD    = 3,  Obsolete
        //	MF    = 4,  Obsolete

        /// <summary>
        /// The canonical name for an alias.
        /// </summary>
        CNAME = 5,

        /// <summary>
        /// Marks the start of a zone of authority.
        /// </summary>
        SOA = 6,

        //	MB    = 7,  EXPERIMENTAL
        //	MG    = 8,  EXPERIMENTAL
        //  MR    = 9,  EXPERIMENTAL
        //	NULL  = 10, EXPERIMENTAL

        /*	/// <summary>
		/// A well known service description.
		/// </summary>
		WKS   = 11, */

        /// <summary>
        /// A domain name pointer.
        /// </summary>
        PTR = 12,

        /// <summary>
        /// Host information.
        /// </summary>
        HINFO = 13,
        /*
		/// <summary>
		/// Mailbox or mail list information.
		/// </summary>
		MINFO = 14, */

        /// <summary>
        /// Mail exchange.
        /// </summary>
        MX = 15,

        /// <summary>
        /// Text strings.
        /// </summary>
        TXT = 16,

        /// <summary>
        /// IPv6 host address.
        /// </summary>
        AAAA = 28,

        /// <summary>
        /// SRV record specifies the location of services.
        /// </summary>
        SRV = 33,

        /// <summary>
        /// NAPTR(Naming Authority Pointer) record.
        /// </summary>
        NAPTR = 35,

        /// <summary>
        /// All records what server returns.
        /// </summary>
        ANY = 255,

        /*	/// <summary>
		/// UnKnown
		/// </summary>
		UnKnown = 9999, */
    }
}