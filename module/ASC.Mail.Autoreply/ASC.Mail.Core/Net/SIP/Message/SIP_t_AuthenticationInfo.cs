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


namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "Authentication-Info" value. Defined in RFC 3261.
    /// According RFC 3261 authentication info can contain Digest authentication info only.
    /// </summary>
    public class SIP_t_AuthenticationInfo : SIP_t_Value
    {
        #region Members

        private string m_CNonce;
        private string m_NextNonce;
        private int m_NonceCount = -1;
        private string m_Qop;
        private string m_ResponseAuth;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets server next predicted nonce value. Value null means that value not specified.
        /// </summary>
        public string NextNonce
        {
            get { return m_NextNonce; }

            set { m_NextNonce = value; }
        }

        /// <summary>
        /// Gets or sets QOP value. Value null means that value not specified.
        /// </summary>
        public string Qop
        {
            get { return m_Qop; }

            set { m_Qop = value; }
        }

        /// <summary>
        /// Gets or sets rspauth value. Value null means that value not specified.
        /// This can be only HEX value.
        /// </summary>
        public string ResponseAuth
        {
            get { return m_ResponseAuth; }

            set
            {
                // TODO: Check that value is hex value

                m_ResponseAuth = value;
            }
        }

        /// <summary>
        /// Gets or sets cnonce value. Value null means that value not specified.
        /// </summary>
        public string CNonce
        {
            get { return m_CNonce; }

            set { m_CNonce = value; }
        }

        /// <summary>
        /// Gets or sets nonce count. Value -1 means that value not specified.
        /// </summary>
        public int NonceCount
        {
            get { return m_NonceCount; }

            set
            {
                if (value < 0)
                {
                    m_NonceCount = -1;
                }
                else
                {
                    m_NonceCount = value;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Authentication-Info valu value.</param>
        public SIP_t_AuthenticationInfo(string value)
        {
            Parse(new StringReader(value));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Authentication-Info" from specified value.
        /// </summary>
        /// <param name="value">SIP "Authentication-Info" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>value</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "Authentication-Info" from specified reader.
        /// </summary>
        /// <param name="reader">Reader what contains Authentication-Info value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Authentication-Info  =  "Authentication-Info" HCOLON ainfo *(COMMA ainfo)
                ainfo                =  nextnonce / message-qop / response-auth / cnonce / nonce-count
                nextnonce            =  "nextnonce" EQUAL nonce-value
                response-auth        =  "rspauth" EQUAL response-digest
                response-digest      =  LDQUOT *LHEX RDQUOT
                nc-value             =  8LHEX
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            while (reader.Available > 0)
            {
                string word = reader.QuotedReadToDelimiter(',');
                if (word != null && word.Length > 0)
                {
                    string[] name_value = word.Split(new[] {'='}, 2);
                    if (name_value[0].ToLower() == "nextnonce")
                    {
                        NextNonce = name_value[1];
                    }
                    else if (name_value[0].ToLower() == "qop")
                    {
                        Qop = name_value[1];
                    }
                    else if (name_value[0].ToLower() == "rspauth")
                    {
                        ResponseAuth = name_value[1];
                    }
                    else if (name_value[0].ToLower() == "cnonce")
                    {
                        CNonce = name_value[1];
                    }
                    else if (name_value[0].ToLower() == "nc")
                    {
                        NonceCount = Convert.ToInt32(name_value[1]);
                    }
                    else
                    {
                        throw new SIP_ParseException("Invalid Authentication-Info value !");
                    }
                }
            }
        }

        /// <summary>
        /// Converts SIP_t_AuthenticationInfo to valid Authentication-Info value.
        /// </summary>
        /// <returns></returns>
        public override string ToStringValue()
        {
            /*
                Authentication-Info  =  "Authentication-Info" HCOLON ainfo *(COMMA ainfo)
                ainfo                =  nextnonce / message-qop / response-auth / cnonce / nonce-count
                nextnonce            =  "nextnonce" EQUAL nonce-value
                response-auth        =  "rspauth" EQUAL response-digest
                response-digest      =  LDQUOT *LHEX RDQUOT
                nc-value             =  8LHEX
            */

            StringBuilder retVal = new StringBuilder();

            if (m_NextNonce != null)
            {
                retVal.Append("nextnonce=" + m_NextNonce);
            }

            if (m_Qop != null)
            {
                if (retVal.Length > 0)
                {
                    retVal.Append(',');
                }

                retVal.Append("qop=" + m_Qop);
            }

            if (m_ResponseAuth != null)
            {
                if (retVal.Length > 0)
                {
                    retVal.Append(',');
                }

                retVal.Append("rspauth=" + TextUtils.QuoteString(m_ResponseAuth));
            }

            if (m_CNonce != null)
            {
                if (retVal.Length > 0)
                {
                    retVal.Append(',');
                }

                retVal.Append("cnonce=" + m_CNonce);
            }

            if (m_NonceCount != -1)
            {
                if (retVal.Length > 0)
                {
                    retVal.Append(',');
                }

                retVal.Append("nc=" + m_NonceCount.ToString("X8"));
            }

            return retVal.ToString();
        }

        #endregion
    }
}