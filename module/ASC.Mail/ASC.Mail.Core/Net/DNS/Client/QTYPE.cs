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