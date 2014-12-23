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

namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "Session-Expires" value. Defined in RFC 4028.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4028 Syntax:
    ///     Session-Expires  = delta-seconds *(SEMI se-params)
    ///     se-params        = refresher-param / generic-param
    ///     refresher-param  = "refresher" EQUAL  ("uas" / "uac")
    /// </code>
    /// </remarks>
    public class SIP_t_SessionExpires : SIP_t_ValueWithParams
    {
        #region Members

        private int m_Expires = 90;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets after how many seconds session expires.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when value less than 90 is passed.</exception>
        public int Expires
        {
            get { return m_Expires; }

            set
            {
                if (m_Expires < 90)
                {
                    throw new ArgumentException("Property Expires value must be >= 90 !");
                }

                m_Expires = value;
            }
        }

        /// <summary>
        /// Gets or sets Session-Expires 'refresher' parameter. Normally this value is 'ua' or 'uas'.
        /// Value null means not specified.
        /// </summary>
        public string Refresher
        {
            get
            {
                SIP_Parameter parameter = Parameters["refresher"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    Parameters.Remove("refresher");
                }
                else
                {
                    Parameters.Set("refresher", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Session-Expires value.</param>
        public SIP_t_SessionExpires(string value)
        {
            Parse(value);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="expires">Specifies after many seconds session expires.</param>
        /// <param name="refresher">Specifies session refresher(uac/uas/null). Value null means not specified.</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public SIP_t_SessionExpires(int expires, string refresher)
        {
            if (m_Expires < 90)
            {
                throw new ArgumentException("Argument 'expires' value must be >= 90 !");
            }

            m_Expires = expires;
            Refresher = refresher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Session-Expires" from specified value.
        /// </summary>
        /// <param name="value">SIP "Session-Expires" value.</param>
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
        /// Parses "Session-Expires" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Session-Expires  = delta-seconds *(SEMI se-params)
                se-params        = refresher-param / generic-param
                refresher-param  = "refresher" EQUAL  ("uas" / "uac")      
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // delta-seconds
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Session-Expires delta-seconds value is missing !");
            }
            try
            {
                m_Expires = Convert.ToInt32(word);
            }
            catch
            {
                throw new SIP_ParseException("Invalid Session-Expires delta-seconds value !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Session-Expires" value.
        /// </summary>
        /// <returns>Returns "Session-Expires" value.</returns>
        public override string ToStringValue()
        {
            /*
                Session-Expires  = delta-seconds *(SEMI se-params)
                se-params        = refresher-param / generic-param
                refresher-param  = "refresher" EQUAL  ("uas" / "uac")      
            */

            StringBuilder retVal = new StringBuilder();

            // delta-seconds
            retVal.Append(m_Expires.ToString());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}