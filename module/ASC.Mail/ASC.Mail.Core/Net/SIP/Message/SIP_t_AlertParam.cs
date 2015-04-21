/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
    /// Implements SIP "alert-param" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     alert-param = LAQUOT absoluteURI RAQUOT *( SEMI generic-param )
    /// </code>
    /// </remarks>
    public class SIP_t_AlertParam : SIP_t_ValueWithParams
    {
        #region Members

        private string m_Uri = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets uri value.
        /// </summary>
        public string Uri
        {
            get { return m_Uri; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Uri value can't be null or empty !");
                }

                m_Uri = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "alert-param" from specified value.
        /// </summary>
        /// <param name="value">SIP "alert-param" value.</param>
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
        /// Parses "alert-param" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /* 
                alert-param = LAQUOT absoluteURI RAQUOT *( SEMI generic-param )
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse uri
            // Read to LAQUOT
            reader.QuotedReadToDelimiter('<');
            if (!reader.StartsWith("<"))
            {
                throw new SIP_ParseException("Invalid Alert-Info value, Uri not between <> !");
            }
            m_Uri = reader.ReadParenthesized();

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "alert-param" value.
        /// </summary>
        /// <returns>Returns "alert-param" value.</returns>
        public override string ToStringValue()
        {
            StringBuilder retVal = new StringBuilder();
            retVal.Append("<" + m_Uri + ">");
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}