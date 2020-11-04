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
    /// Base class for DNS records.
    /// </summary>
    public abstract class DNS_rr_base
    {
        #region Members

        private readonly int m_TTL = -1;
        private readonly QTYPE m_Type = QTYPE.A;

        #endregion

        #region Properties

        /// <summary>
        /// Gets record type (A,MX,...).
        /// </summary>
        public QTYPE RecordType
        {
            get { return m_Type; }
        }

        /// <summary>
        /// Gets TTL (time to live) value in seconds.
        /// </summary>
        public int TTL
        {
            get { return m_TTL; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="recordType">Record type (A,MX, ...).</param>
        /// <param name="ttl">TTL (time to live) value in seconds.</param>
        public DNS_rr_base(QTYPE recordType, int ttl)
        {
            m_Type = recordType;
            m_TTL = ttl;
        }

        #endregion
    }
}