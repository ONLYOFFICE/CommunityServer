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
    /// Implements SIP "reason-value" value. Defined in rfc 3326.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3326 Syntax:
    ///     reason-value      =  protocol *(SEMI reason-params)
    ///     protocol          =  "SIP" / "Q.850" / token
    ///     reason-params     =  protocol-cause / reason-text / reason-extension
    ///     protocol-cause    =  "cause" EQUAL cause
    ///     cause             =  1*DIGIT
    ///     reason-text       =  "text" EQUAL quoted-string
    ///     reason-extension  =  generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_ReasonValue : SIP_t_ValueWithParams
    {
        #region Members

        private string m_Protocol = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets protocol.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        public string Protocol
        {
            get { return m_Protocol; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Protocol");
                }

                m_Protocol = value;
            }
        }

        /// <summary>
        /// Gets or sets 'cause' parameter value. The cause parameter contains a SIP status code. 
        /// Value -1 means not specified.
        /// </summary>
        public int Cause
        {
            get
            {
                if (Parameters["cause"] == null)
                {
                    return -1;
                }
                else
                {
                    return Convert.ToInt32(Parameters["cause"].Value);
                }
            }

            set
            {
                if (value < 0)
                {
                    Parameters.Remove("cause");
                }
                else
                {
                    Parameters.Set("cause", value.ToString());
                }
            }
        }

        /// <summary>
        /// Gets or sets 'text' parameter value. Value null means not specified.
        /// </summary>
        public string Text
        {
            get
            {
                SIP_Parameter parameter = Parameters["text"];
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
                    Parameters.Remove("text");
                }
                else
                {
                    Parameters.Set("text", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_ReasonValue() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP reason-value value.</param>
        public SIP_t_ReasonValue(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "reason-value" from specified value.
        /// </summary>
        /// <param name="value">SIP "reason-value" value.</param>
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
        /// Parses "reason-value" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                reason-value      =  protocol *(SEMI reason-params)
                protocol          =  "SIP" / "Q.850" / token
                reason-params     =  protocol-cause / reason-text / reason-extension
                protocol-cause    =  "cause" EQUAL cause
                cause             =  1*DIGIT
                reason-text       =  "text" EQUAL quoted-string
                reason-extension  =  generic-param
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // protocol
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("SIP reason-value 'protocol' value is missing !");
            }
            m_Protocol = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "reason-value" value.
        /// </summary>
        /// <returns>Returns "reason-value" value.</returns>
        public override string ToStringValue()
        {
            /*
                reason-value      =  protocol *(SEMI reason-params)
                protocol          =  "SIP" / "Q.850" / token
                reason-params     =  protocol-cause / reason-text / reason-extension
                protocol-cause    =  "cause" EQUAL cause
                cause             =  1*DIGIT
                reason-text       =  "text" EQUAL quoted-string
                reason-extension  =  generic-param
            */

            StringBuilder retVal = new StringBuilder();

            // Add protocol
            retVal.Append(m_Protocol);

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}