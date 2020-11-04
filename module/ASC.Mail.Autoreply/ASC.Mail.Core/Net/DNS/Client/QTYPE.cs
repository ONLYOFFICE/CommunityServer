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