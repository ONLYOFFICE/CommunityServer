/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
    /// NAPRT(Naming Authority Pointer) resource record. Defined in RFC 3403.
    /// </summary>
    [Serializable]
    public class DNS_rr_NAPTR : DNS_rr_base
    {
        #region Members

        private readonly string m_Flags = "";
        private readonly int m_Order;
        private readonly int m_Preference;
        private readonly string m_Regexp = "";
        private readonly string m_Replacement = "";
        private readonly string m_Services = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets order in which the NAPTR records MUST be processed in order to accurately 
        /// represent the ordered list of Rules.
        /// </summary>
        public int Order
        {
            get { return m_Order; }
        }

        /// <summary>
        /// Gets the order in which NAPTR records with equal Order values SHOULD be processed, 
        /// low numbers being processed before high numbers.
        /// </summary>
        public int Preference
        {
            get { return m_Preference; }
        }

        /// <summary>
        /// Gets flags which control the rewriting and interpretation of the fields in the record.
        /// </summary>
        public string Flags
        {
            get { return m_Flags; }
        }

        /// <summary>
        /// Gets services related to this record. Known values can be get from: http://www.iana.org/assignments/enum-services.
        /// </summary>
        public string Services
        {
            get { return m_Services; }
        }

        /// <summary>
        /// Gets regular expression that is applied to the original string held by the client in order to 
        /// construct the next domain name to lookup.
        /// </summary>
        public string Regexp
        {
            get { return m_Regexp; }
        }

        /// <summary>
        /// Gets regular expressions replacement value.
        /// </summary>
        public string Replacement
        {
            get { return m_Replacement; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="order">Oorder in which the NAPTR records MUST be processed.</param>
        /// <param name="preference">Order in which NAPTR records with equal Order values SHOULD be processed.</param>
        /// <param name="flags">Flags which control the rewriting and interpretation of the fields in the record.</param>
        /// <param name="services">Services related to this record.</param>
        /// <param name="regexp">Regular expression that is applied to the original string.</param>
        /// <param name="replacement">Regular expressions replacement value.</param>
        /// <param name="ttl">Time to live value in seconds.</param>
        public DNS_rr_NAPTR(int order,
                            int preference,
                            string flags,
                            string services,
                            string regexp,
                            string replacement,
                            int ttl) : base(QTYPE.NAPTR, ttl)
        {
            m_Order = order;
            m_Preference = preference;
            m_Flags = flags;
            m_Services = services;
            m_Regexp = regexp;
            m_Replacement = replacement;
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
        public static DNS_rr_NAPTR Parse(byte[] reply, ref int offset, int rdLength, int ttl)
        {
            /* RFC 3403.
                The packet format for the NAPTR record is as follows
                                               1  1  1  1  1  1
                 0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                     ORDER                     |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                |                   PREFERENCE                  |
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                /                     FLAGS                     /
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                /                   SERVICES                    /
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                /                    REGEXP                     /
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                /                  REPLACEMENT                  /
                /                                               /
                +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
            */

            int order = reply[offset++] << 8 | reply[offset++];

            int preference = reply[offset++] << 8 | reply[offset++];

            string flags = Dns_Client.ReadCharacterString(reply, ref offset);

            string services = Dns_Client.ReadCharacterString(reply, ref offset);

            string regexp = Dns_Client.ReadCharacterString(reply, ref offset);

            string replacement = "";
            Dns_Client.GetQName(reply, ref offset, ref replacement);

            return new DNS_rr_NAPTR(order, preference, flags, services, regexp, replacement, ttl);
        }

        #endregion
    }
}