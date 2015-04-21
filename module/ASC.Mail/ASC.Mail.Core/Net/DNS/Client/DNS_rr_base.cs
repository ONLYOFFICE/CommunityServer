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