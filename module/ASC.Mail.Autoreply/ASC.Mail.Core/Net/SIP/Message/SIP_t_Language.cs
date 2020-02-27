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
    /// Implements SIP "language" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     language       = language-range *(SEMI accept-param)
    ///     language-range = ( ( 1*8ALPHA *( "-" 1*8ALPHA ) ) / "*" )
    ///     accept-param   = ("q" EQUAL qvalue) / generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_Language : SIP_t_ValueWithParams
    {
        #region Members

        private string m_LanguageRange = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets language range. Value *(STAR) means all languages.
        /// </summary>
        public string LanguageRange
        {
            get { return m_LanguageRange; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property LanguageRange value can't be null or empty !");
                }

                m_LanguageRange = value;
            }
        }

        /// <summary>
        /// Gets or sets qvalue parameter. Targets are processed from highest qvalue to lowest. 
        /// This value must be between 0.0 and 1.0. Value -1 means that value not specified.
        /// </summary>
        public double QValue
        {
            get
            {
                SIP_Parameter parameter = Parameters["qvalue"];
                if (parameter != null)
                {
                    return Convert.ToDouble(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentException("Property QValue value must be between 0.0 and 1.0 !");
                }

                if (value < 0)
                {
                    Parameters.Remove("qvalue");
                }
                else
                {
                    Parameters.Set("qvalue", value.ToString());
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "language" from specified value.
        /// </summary>
        /// <param name="value">SIP "language" value.</param>
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
        /// Parses "language" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /* 
                language       = language-range *(SEMI accept-param)
                language-range = ( ( 1*8ALPHA *( "-" 1*8ALPHA ) ) / "*" )
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse content-coding
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException(
                    "Invalid Accept-Language value, language-range value is missing !");
            }
            m_LanguageRange = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "language" value.
        /// </summary>
        /// <returns>Restuns "language" value.</returns>
        public override string ToStringValue()
        {
            /* 
                Accept-Language  =  "Accept-Language" HCOLON [ language *(COMMA language) ]
                language         =  language-range *(SEMI accept-param)
                language-range   =  ( ( 1*8ALPHA *( "-" 1*8ALPHA ) ) / "*" )
            */

            StringBuilder retVal = new StringBuilder();
            retVal.Append(m_LanguageRange);
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}