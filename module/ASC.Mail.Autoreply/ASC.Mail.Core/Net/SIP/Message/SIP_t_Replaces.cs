/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "Replaces" value. Defined in RFC 3891.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3891 Syntax:
    ///     Replaces        = callid *(SEMI replaces-param)
    ///     replaces-param  = to-tag / from-tag / early-flag / generic-param
    ///     to-tag          = "to-tag" EQUAL token
    ///     from-tag        = "from-tag" EQUAL token
    ///     early-flag      = "early-only"
    /// </code>
    /// </remarks>
    public class SIP_t_Replaces : SIP_t_ValueWithParams
    {
        #region Members

        private string m_CallID = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets call id.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        public string CallID
        {
            get { return m_CallID; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("CallID");
                }

                m_CallID = value;
            }
        }

        /// <summary>
        /// Gets or sets Replaces 'to-tag' parameter. Value null means not specified.
        /// </summary>
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
                if (value == null)
                {
                    Parameters.Remove("to-tag");
                }
                else
                {
                    Parameters.Set("to-tag", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets Replaces 'from-tag' parameter. Value null means not specified.
        /// </summary>
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
                if (value == null)
                {
                    Parameters.Remove("from-tag");
                }
                else
                {
                    Parameters.Set("from-tag", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets Replaces 'early-flag' parameter.
        /// </summary>
        public bool EarlyFlag
        {
            get
            {
                if (Parameters.Contains("early-only"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (!value)
                {
                    Parameters.Remove("early-only");
                }
                else
                {
                    Parameters.Set("early-only", null);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Replaces" from specified value.
        /// </summary>
        /// <param name="value">SIP "Replaces" value.</param>
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
        /// Parses "Replaces" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Replaces        = callid *(SEMI replaces-param)
                replaces-param  = to-tag / from-tag / early-flag / generic-param
                to-tag          = "to-tag" EQUAL token
                from-tag        = "from-tag" EQUAL token
                early-flag      = "early-only"    
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // callid
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Replaces 'callid' value is missing !");
            }
            m_CallID = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Replaces" value.
        /// </summary>
        /// <returns>Returns "Replaces" value.</returns>
        public override string ToStringValue()
        {
            /*
                Replaces        = callid *(SEMI replaces-param)
                replaces-param  = to-tag / from-tag / early-flag / generic-param
                to-tag          = "to-tag" EQUAL token
                from-tag        = "from-tag" EQUAL token
                early-flag      = "early-only"    
            */

            StringBuilder retVal = new StringBuilder();

            // delta-seconds
            retVal.Append(m_CallID);

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}