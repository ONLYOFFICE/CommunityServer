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
    #region usings

    using System;

    #endregion

    /// <summary>
    /// DNS SRV record. SRV record specifies the location of services. Defined in RFC 2782.
    /// </summary>
    [Serializable]
    public class DNS_rr_SRV : DNS_rr_base
    {
        #region Members

        private readonly int m_Port;
        private readonly int m_Priority = 1;
        private readonly string m_Target = "";
        private readonly int m_Weight = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets service priority. Lowest value means greater priority.
        /// </summary>
        public int Priority
        {
            get { return m_Priority; }
        }

        /// <summary>
        /// Gets weight. The weight field specifies a relative weight for entries with the same priority. 
        /// Larger weights SHOULD be given a proportionately higher probability of being selected.
        /// </summary>
        public int Weight
        {
            get { return m_Weight; }
        }

        /// <summary>
        /// Port where service runs.
        /// </summary>
        public int Port
        {
            get { return m_Port; }
        }

        /// <summary>
        /// Service provider host name or IP address.
        /// </summary>
        public string Target
        {
            get { return m_Target; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="priority">Service priority.</param>
        /// <param name="weight">Weight value.</param>
        /// <param name="port">Service port.</param>
        /// <param name="target">Service provider host name or IP address.</param>
        /// <param name="ttl">Time to live value in seconds.</param>
        public DNS_rr_SRV(int priority, int weight, int port, string target, int ttl) : base(QTYPE.SRV, ttl)
        {
            m_Priority = priority;
            m_Weight = weight;
            m_Port = port;
            m_Target = target;
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
        public static DNS_rr_SRV Parse(byte[] reply, ref int offset, int rdLength, int ttl)
        {
            // Priority Weight Port Target

            // Priority
            int priority = reply[offset++] << 8 | reply[offset++];

            // Weight
            int weight = reply[offset++] << 8 | reply[offset++];

            // Port
            int port = reply[offset++] << 8 | reply[offset++];

            // Target
            string target = "";
            Dns_Client.GetQName(reply, ref offset, ref target);

            return new DNS_rr_SRV(priority, weight, port, target, ttl);
        }

        #endregion
    }
}