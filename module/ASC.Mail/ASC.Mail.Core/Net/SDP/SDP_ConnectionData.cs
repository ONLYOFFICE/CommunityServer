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

namespace ASC.Mail.Net.SDP
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// A SDP_ConnectionData represents an <B>c=</B> SDP message field. Defined in RFC 4566 5.7. Connection Data.
    /// </summary>
    public class SDP_ConnectionData
    {
        #region Members

        private string m_Address = "";
        private string m_AddressType = "";
        private string m_NetType = "IN";

        #endregion

        #region Properties

        /// <summary>
        /// Gets net type. Currently it's always IN(Internet).
        /// </summary>
        public string NetType
        {
            get { return m_NetType; }
        }

        /// <summary>
        /// Gets or sets address type. Currently defined values IP4 or IP6.
        /// </summary>
        public string AddressType
        {
            get { return m_AddressType; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property AddressType can't be null or empty !");
                }

                m_AddressType = value;
            }
        }

        /// <summary>
        /// Gets or sets connection address.
        /// </summary>
        public string Address
        {
            get { return m_Address; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Address can't be null or empty !");
                }

                m_Address = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses media from "c" SDP message field.
        /// </summary>
        /// <param name="cValue">"m" SDP message field.</param>
        /// <returns></returns>
        public static SDP_ConnectionData Parse(string cValue)
        {
            // c=<nettype> <addrtype> <connection-address>

            SDP_ConnectionData connectionInfo = new SDP_ConnectionData();

            // Remove c=
            StringReader r = new StringReader(cValue);
            r.QuotedReadToDelimiter('=');

            //--- <nettype> ------------------------------------------------------------
            string word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("SDP message \"c\" field <nettype> value is missing !");
            }
            connectionInfo.m_NetType = word;

            //--- <addrtype> -----------------------------------------------------------
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("SDP message \"c\" field <addrtype> value is missing !");
            }
            connectionInfo.m_AddressType = word;

            //--- <connection-address> -------------------------------------------------
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("SDP message \"c\" field <connection-address> value is missing !");
            }
            connectionInfo.m_Address = word;

            return connectionInfo;
        }

        /// <summary>
        /// Converts this to valid connection data stirng. 
        /// </summary>
        /// <returns></returns>
        public string ToValue()
        {
            // c=<nettype> <addrtype> <connection-address>

            return "c=" + NetType + " " + AddressType + " " + Address + "\r\n";
        }

        #endregion
    }
}