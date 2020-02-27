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


namespace ASC.Mail.Net.Dns.Client
{
    /// <summary>
    /// HINFO record.
    /// </summary>
    public class DNS_rr_HINFO : DNS_rr_base
    {
        #region Members

        private readonly string m_CPU = "";
        private readonly string m_OS = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets host's CPU.
        /// </summary>
        public string CPU
        {
            get { return m_CPU; }
        }

        /// <summary>
        /// Gets host's OS.
        /// </summary>
        public string OS
        {
            get { return m_OS; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="cpu">Host CPU.</param>
        /// <param name="os">Host OS.</param>
        /// <param name="ttl">TTL value.</param>
        public DNS_rr_HINFO(string cpu, string os, int ttl) : base(QTYPE.HINFO, ttl)
        {
            m_CPU = cpu;
            m_OS = os;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses resource record from reply data.
        /// </summary>
        /// <param name="reply">DNS server reply data.</param>
        /// <param name="offset">Current offset in reply data.</param>
        /// <param name="rdLength">Resource record data length.</param>
        /// <param name="ttl">Time to live in seconds.</param>
        public static DNS_rr_HINFO Parse(byte[] reply, ref int offset, int rdLength, int ttl)
        {
            /* RFC 1035 3.3.2. HINFO RDATA format

			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			/                      CPU                      /
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			/                       OS                      /
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			
			CPU     A <character-string> which specifies the CPU type.

			OS      A <character-string> which specifies the operating
					system type.
					
					Standard values for CPU and OS can be found in [RFC-1010].

			*/

            // CPU
            string cpu = Dns_Client.ReadCharacterString(reply, ref offset);

            // OS
            string os = Dns_Client.ReadCharacterString(reply, ref offset);

            return new DNS_rr_HINFO(cpu, os, ttl);
        }

        #endregion
    }
}