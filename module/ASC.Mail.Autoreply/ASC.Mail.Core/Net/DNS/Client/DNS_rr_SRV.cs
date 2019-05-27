/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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