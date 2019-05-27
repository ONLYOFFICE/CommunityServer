/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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