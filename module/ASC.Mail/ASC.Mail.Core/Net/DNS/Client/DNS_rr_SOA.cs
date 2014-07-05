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
    #region usings

    using System;

    #endregion

    /// <summary>
    /// SOA record class.
    /// </summary>
    [Serializable]
    public class DNS_rr_SOA : DNS_rr_base
    {
        #region Members

        private readonly string m_AdminEmail = "";
        private readonly long m_Expire;
        private readonly long m_Minimum;
        private readonly string m_NameServer = "";
        private readonly long m_Refresh;
        private readonly long m_Retry;
        private readonly long m_Serial;

        #endregion

        #region Properties

        /// <summary>
        /// Gets name server.
        /// </summary>
        public string NameServer
        {
            get { return m_NameServer; }
        }

        /// <summary>
        /// Gets zone administrator email.
        /// </summary>
        public string AdminEmail
        {
            get { return m_AdminEmail; }
        }

        /// <summary>
        /// Gets version number of the original copy of the zone.
        /// </summary>
        public long Serial
        {
            get { return m_Serial; }
        }

        /// <summary>
        /// Gets time interval(in seconds) before the zone should be refreshed.
        /// </summary>
        public long Refresh
        {
            get { return m_Refresh; }
        }

        /// <summary>
        /// Gets time interval(in seconds) that should elapse before a failed refresh should be retried.
        /// </summary>
        public long Retry
        {
            get { return m_Retry; }
        }

        /// <summary>
        /// Gets time value(in seconds) that specifies the upper limit on the time interval that can elapse before the zone is no longer authoritative.
        /// </summary>
        public long Expire
        {
            get { return m_Expire; }
        }

        /// <summary>
        /// Gets minimum TTL(in seconds) field that should be exported with any RR from this zone. 
        /// </summary>
        public long Minimum
        {
            get { return m_Minimum; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="nameServer">Name server.</param>
        /// <param name="adminEmail">Zone administrator email.</param>
        /// <param name="serial">Version number of the original copy of the zone.</param>
        /// <param name="refresh">Time interval(in seconds) before the zone should be refreshed.</param>
        /// <param name="retry">Time interval(in seconds) that should elapse before a failed refresh should be retried.</param>
        /// <param name="expire">Time value(in seconds) that specifies the upper limit on the time interval that can elapse before the zone is no longer authoritative.</param>
        /// <param name="minimum">Minimum TTL(in seconds) field that should be exported with any RR from this zone.</param>
        /// <param name="ttl">TTL value.</param>
        public DNS_rr_SOA(string nameServer,
                          string adminEmail,
                          long serial,
                          long refresh,
                          long retry,
                          long expire,
                          long minimum,
                          int ttl) : base(QTYPE.SOA, ttl)
        {
            m_NameServer = nameServer;
            m_AdminEmail = adminEmail;
            m_Serial = serial;
            m_Refresh = refresh;
            m_Retry = retry;
            m_Expire = expire;
            m_Minimum = minimum;
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
        public static DNS_rr_SOA Parse(byte[] reply, ref int offset, int rdLength, int ttl)
        {
            /* RFC 1035 3.3.13. SOA RDATA format

				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				/                     MNAME                     /
				/                                               /
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				/                     RNAME                     /
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                    SERIAL                     |
				|                                               |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                    REFRESH                    |
				|                                               |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                     RETRY                     |
				|                                               |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                    EXPIRE                     |
				|                                               |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                    MINIMUM                    |
				|                                               |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

			where:

			MNAME           The <domain-name> of the name server that was the
							original or primary source of data for this zone.

			RNAME           A <domain-name> which specifies the mailbox of the
							person responsible for this zone.

			SERIAL          The unsigned 32 bit version number of the original copy
							of the zone.  Zone transfers preserve this value.  This
							value wraps and should be compared using sequence space
							arithmetic.

			REFRESH         A 32 bit time interval before the zone should be
							refreshed.

			RETRY           A 32 bit time interval that should elapse before a
							failed refresh should be retried.

			EXPIRE          A 32 bit time value that specifies the upper limit on
							the time interval that can elapse before the zone is no
							longer authoritative.
							
			MINIMUM         The unsigned 32 bit minimum TTL field that should be
							exported with any RR from this zone.
			*/

            //---- Parse record -------------------------------------------------------------//
            // MNAME
            string nameserver = "";
            Dns_Client.GetQName(reply, ref offset, ref nameserver);

            // RNAME
            string adminMailBox = "";
            Dns_Client.GetQName(reply, ref offset, ref adminMailBox);
            char[] adminMailBoxAr = adminMailBox.ToCharArray();
            for (int i = 0; i < adminMailBoxAr.Length; i++)
            {
                if (adminMailBoxAr[i] == '.')
                {
                    adminMailBoxAr[i] = '@';
                    break;
                }
            }
            adminMailBox = new string(adminMailBoxAr);

            // SERIAL
            long serial = reply[offset++] << 24 | reply[offset++] << 16 | reply[offset++] << 8 |
                          reply[offset++];

            // REFRESH
            long refresh = reply[offset++] << 24 | reply[offset++] << 16 | reply[offset++] << 8 |
                           reply[offset++];

            // RETRY
            long retry = reply[offset++] << 24 | reply[offset++] << 16 | reply[offset++] << 8 |
                         reply[offset++];

            // EXPIRE
            long expire = reply[offset++] << 24 | reply[offset++] << 16 | reply[offset++] << 8 |
                          reply[offset++];

            // MINIMUM
            long minimum = reply[offset++] << 24 | reply[offset++] << 16 | reply[offset++] << 8 |
                           reply[offset++];
            //--------------------------------------------------------------------------------//

            return new DNS_rr_SOA(nameserver, adminMailBox, serial, refresh, retry, expire, minimum, ttl);
        }

        #endregion
    }
}