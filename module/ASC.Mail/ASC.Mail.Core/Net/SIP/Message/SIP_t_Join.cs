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
    /// Implements SIP "Join" value. Defined in RFC 3911.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3911 Syntax:
    ///     Join       = callid *(SEMI join-param)
    ///     join-param = to-tag / from-tag / generic-param
    ///     to-tag     = "to-tag" EQUAL token
    ///     from-tag   = "from-tag" EQUAL token
    /// </code>
    /// </remarks>
    public class SIP_t_Join : SIP_t_ValueWithParams
    {
        #region Members

        private SIP_t_CallID m_pCallID;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets call ID value.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised �when null value passed.</exception>
        public SIP_t_CallID CallID
        {
            get { return m_pCallID; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("CallID");
                }

                m_pCallID = value;
            }
        }

        /// <summary>
        /// Gets or sets to-tag parameter value. This value is mandatory.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid ToTag value is passed.</exception>
        public string ToTag
        {
            get
            {
                SIP_Parameter parameter = Parameters["to-tag"];
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
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("ToTag is mandatory and cant be null or empty !");
                }
                else
                {
                    Parameters.Set("to-tag", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets from-tag parameter value. This value is mandatory.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when invalid FromTag value is passed.</exception>
        public string FromTag
        {
            get
            {
                SIP_Parameter parameter = Parameters["from-tag"];
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
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("FromTag is mandatory and cant be null or empty !");
                }
                else
                {
                    Parameters.Set("from-tag", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Join value.</param>
        public SIP_t_Join(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Join" from specified value.
        /// </summary>
        /// <param name="value">SIP "Join" value.</param>
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
        /// Parses "Join" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Join       = callid *(SEMI join-param)
                join-param = to-tag / from-tag / generic-param
                to-tag     = "to-tag" EQUAL token
                from-tag   = "from-tag" EQUAL token
              
                A Join header MUST contain exactly one to-tag and exactly one from-
                tag, as they are required for unique dialog matching.
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse address
            SIP_t_CallID callID = new SIP_t_CallID();
            callID.Parse(reader);
            m_pCallID = callID;

            // Parse parameters
            ParseParameters(reader);

            // Check that to and from tags exist.
            if (Parameters["to-tag"] == null)
            {
                throw new SIP_ParseException("Join value mandatory to-tag value is missing !");
            }
            if (Parameters["from-tag"] == null)
            {
                throw new SIP_ParseException("Join value mandatory from-tag value is missing !");
            }
        }

        /// <summary>
        /// Converts this to valid "Join" value.
        /// </summary>
        /// <returns>Returns "Join" value.</returns>
        public override string ToStringValue()
        {
            /*
                Join       = callid *(SEMI join-param)
                join-param = to-tag / from-tag / generic-param
                to-tag     = "to-tag" EQUAL token
                from-tag   = "from-tag" EQUAL token 
            */

            StringBuilder retVal = new StringBuilder();

            // Add address
            retVal.Append(m_pCallID.ToStringValue());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}