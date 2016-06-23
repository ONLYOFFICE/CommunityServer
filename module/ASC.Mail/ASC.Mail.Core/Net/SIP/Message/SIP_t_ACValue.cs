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
    /// Implements SIP "ac-value" value. Defined in RFC 3841.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3841 Syntax:
    ///     ac-value       = "*" *(SEMI ac-params)
    ///     ac-params      = feature-param / req-param / explicit-param / generic-param
    ///                      ;;feature param from RFC 3840
    ///                      ;;generic-param from RFC 3261
    ///     req-param      = "require"
    ///     explicit-param = "explicit"
    /// </code>
    /// </remarks>
    public class SIP_t_ACValue : SIP_t_ValueWithParams
    {
        #region Properties

        /// <summary>
        /// Gets or sets 'require' parameter value.
        /// </summary>
        public bool Require
        {
            get
            {
                SIP_Parameter parameter = Parameters["require"];
                if (parameter != null)
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
                    Parameters.Remove("require");
                }
                else
                {
                    Parameters.Set("require", null);
                }
            }
        }

        /// <summary>
        /// Gets or sets 'explicit' parameter value.
        /// </summary>
        public bool Explicit
        {
            get
            {
                SIP_Parameter parameter = Parameters["explicit"];
                if (parameter != null)
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
                    Parameters.Remove("explicit");
                }
                else
                {
                    Parameters.Set("explicit", null);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_ACValue() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP 'ac-value' value.</param>
        public SIP_t_ACValue(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "ac-value" from specified value.
        /// </summary>
        /// <param name="value">SIP "ac-value" value.</param>
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
        /// Parses "ac-value" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                ac-value       = "*" *(SEMI ac-params)
                ac-params      = feature-param / req-param / explicit-param / generic-param
                                 ;;feature param from RFC 3840
                                 ;;generic-param from RFC 3261
                req-param      = "require"
                explicit-param = "explicit"
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'ac-value', '*' is missing !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "ac-value" value.
        /// </summary>
        /// <returns>Returns "ac-value" value.</returns>
        public override string ToStringValue()
        {
            /*
                ac-value       = "*" *(SEMI ac-params)
                ac-params      = feature-param / req-param / explicit-param / generic-param
                                 ;;feature param from RFC 3840
                                 ;;generic-param from RFC 3261
                req-param      = "require"
                explicit-param = "explicit"
            */

            StringBuilder retVal = new StringBuilder();

            // *
            retVal.Append("*");

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}