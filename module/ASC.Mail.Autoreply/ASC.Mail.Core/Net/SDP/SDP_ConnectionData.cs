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