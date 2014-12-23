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