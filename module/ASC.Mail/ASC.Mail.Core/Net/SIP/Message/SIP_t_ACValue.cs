/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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