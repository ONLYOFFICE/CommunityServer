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