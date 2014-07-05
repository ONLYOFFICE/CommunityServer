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
    /// Implements SIP "Content-Disposition" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     Content-Disposition  = disp-type *( SEMI disp-param )
    ///     disp-type            = "render" / "session" / "icon" / "alert" / disp-extension-token
    ///     disp-param           = handling-param / generic-param
    ///     handling-param       = "handling" EQUAL ( "optional" / "required" / other-handling )
    ///     other-handling       = token
    ///     disp-extension-token = token
    /// </code>
    /// </remarks>
    public class SIP_t_ContentDisposition : SIP_t_ValueWithParams
    {
        #region Members

        private string m_DispositionType = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets disposition type. Known values: "render","session","icon","alert".
        /// </summary>
        public string DispositionType
        {
            get { return m_DispositionType; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("DispositionType");
                }
                if (!TextUtils.IsToken(value))
                {
                    throw new ArgumentException("Invalid DispositionType value, value must be 'token' !");
                }

                m_DispositionType = value;
            }
        }

        /// <summary>
        /// Gets or sets 'handling' parameter value. Value null means not specified. 
        /// Known value: "optional","required".
        /// </summary>
        public string Handling
        {
            get
            {
                SIP_Parameter parameter = Parameters["handling"];
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
                    Parameters.Remove("handling");
                }
                else
                {
                    Parameters.Set("handling", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP SIP_t_ContentDisposition value.</param>
        public SIP_t_ContentDisposition(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Content-Disposition" from specified value.
        /// </summary>
        /// <param name="value">SIP "Content-Disposition" value.</param>
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
        /// Parses "Content-Disposition" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /* 
                Content-Disposition  = disp-type *( SEMI disp-param )
                disp-type            = "render" / "session" / "icon" / "alert" / disp-extension-token
                disp-param           = handling-param / generic-param
                handling-param       = "handling" EQUAL ( "optional" / "required" / other-handling )
                other-handling       = token
                disp-extension-token = token
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // disp-type
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("SIP Content-Disposition 'disp-type' value is missing !");
            }
            m_DispositionType = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Content-Disposition" value.
        /// </summary>
        /// <returns>Returns "Content-Disposition" value.</returns>
        public override string ToStringValue()
        {
            StringBuilder retVal = new StringBuilder();
            retVal.Append(m_DispositionType);
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}