/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.Dns.Client
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// MX record class.
    /// </summary>
    [Serializable]
    public class DNS_rr_MX : DNS_rr_base, IComparable
    {
        #region Members

        private readonly string m_Host = "";
        private readonly int m_Preference;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="preference">MX record preference.</param>
        /// <param name="host">Mail host dns name.</param>
        /// <param name="ttl">TTL value.</param>
        public DNS_rr_MX(int preference, string host, int ttl) : base(QTYPE.MX, ttl)
        {
            m_Preference = preference;
            m_Host = host;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets mail host dns name.
        /// </summary>
        public string Host
        {
            get { return m_Host; }
        }

        /// <summary>
        /// Gets MX record preference. The lower number is the higher priority server.
        /// </summary>
        public int Preference
        {
            get { return m_Preference; }
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
        public static DNS_rr_MX Parse(byte[] reply, ref int offset, int rdLength, int ttl)
        {
            /* RFC 1035	3.3.9. MX RDATA format

			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                  PREFERENCE                   |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			/                   EXCHANGE                    /
			/                                               /
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

			where:

			PREFERENCE      
				A 16 bit integer which specifies the preference given to
				this RR among others at the same owner.  Lower values
                are preferred.

			EXCHANGE 
			    A <domain-name> which specifies a host willing to act as
                a mail exchange for the owner name. 
			*/

            int pref = reply[offset++] << 8 | reply[offset++];

            string name = "";
            if (Dns_Client.GetQName(reply, ref offset, ref name))
            {
                return new DNS_rr_MX(pref, name, ttl);
            }
            else
            {
                throw new ArgumentException("Invalid MX resource record data !");
            }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type. 
        /// </summary>
        /// <param name="obj">An object to compare with this instance. </param>
        /// <returns>Returns 0 if two objects are equal, returns negative value if this object is less,
        /// returns positive value if this object is grater.</returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (!(obj is DNS_rr_MX))
            {
                throw new ArgumentException("Argument obj is not MX_Record !");
            }

            DNS_rr_MX mx = (DNS_rr_MX) obj;
            if (Preference > mx.Preference)
            {
                return 1;
            }
            else if (Preference < mx.Preference)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        #endregion
    }
}