/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
    /// Implements SIP "Target-Dialog" value. Defined in RFC 4538.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4538 Syntax:
    ///     Target-Dialog = callid *(SEMI td-param)    ;callid from RFC 3261
    ///     td-param      = remote-param / local-param / generic-param
    ///     remote-param  = "remote-tag" EQUAL token
    ///     local-param   = "local-tag" EQUAL token
    /// </code>
    /// </remarks>
    public class SIP_t_TargetDialog : SIP_t_ValueWithParams
    {
        #region Members

        private string m_CallID = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets call ID.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid CallID value is passed.</exception>
        public string CallID
        {
            get { return m_CallID; }

            set
            {
                if (m_CallID == null)
                {
                    throw new ArgumentNullException("CallID");
                }
                if (m_CallID == "")
                {
                    throw new ArgumentException("Property 'CallID' may not be '' !");
                }

                m_CallID = value;
            }
        }

        /// <summary>
        /// Gets or sets 'remote-tag' parameter value. Value null means not specified.
        /// </summary>
        public string RemoteTag
        {
            get
            {
                SIP_Parameter parameter = Parameters["remote-tag"];
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
                    Parameters.Remove("remote-tag");
                }
                else
                {
                    Parameters.Set("remote-tag", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'local-tag' parameter value. Value null means not specified.
        /// </summary>
        public string LocalTag
        {
            get
            {
                SIP_Parameter parameter = Parameters["local-tag"];
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
                    Parameters.Remove("local-tag");
                }
                else
                {
                    Parameters.Set("local-tag", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP Target-Dialog value.</param>
        public SIP_t_TargetDialog(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Target-Dialog" from specified value.
        /// </summary>
        /// <param name="value">SIP "Target-Dialog" value.</param>
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
        /// Parses "Target-Dialog" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Target-Dialog = callid *(SEMI td-param)    ;callid from RFC 3261
                td-param      = remote-param / local-param / generic-param
                remote-param  = "remote-tag" EQUAL token
                local-param   = "local-tag" EQUAL token
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // callid
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("SIP Target-Dialog 'callid' value is missing !");
            }
            m_CallID = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Target-Dialog" value.
        /// </summary>
        /// <returns>Returns "Target-Dialog" value.</returns>
        public override string ToStringValue()
        {
            /*
                Target-Dialog = callid *(SEMI td-param)    ;callid from RFC 3261
                td-param      = remote-param / local-param / generic-param
                remote-param  = "remote-tag" EQUAL token
                local-param   = "local-tag" EQUAL token
            */

            StringBuilder retVal = new StringBuilder();

            // callid
            retVal.Append(m_CallID);

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}