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
    /// TXT record class.
    /// </summary>
    [Serializable]
    public class DNS_rr_TXT : DNS_rr_base
    {
        #region Members

        private readonly string m_Text = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets text.
        /// </summary>
        public string Text
        {
            get { return m_Text; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="ttl">TTL value.</param>
        public DNS_rr_TXT(string text, int ttl) : base(QTYPE.TXT, ttl)
        {
            m_Text = text;
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
        public static DNS_rr_TXT Parse(byte[] reply, ref int offset, int rdLength, int ttl)
        {
            // TXT RR

            string text = Dns_Client.ReadCharacterString(reply, ref offset);

            return new DNS_rr_TXT(text, ttl);
        }

        #endregion
    }
}