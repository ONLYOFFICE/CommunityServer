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