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

    #endregion

    /// <summary>
    /// Implements SIP "warning-value" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     warning-value = warn-code SP warn-agent SP warn-text
    ///     warn-code     = 3DIGIT
    ///     warn-agent    = hostport / pseudonym
    ///                      ;  the name or pseudonym of the server adding
    ///                      ;  the Warning header, for use in debugging
    ///     warn-text     = quoted-string
    ///     pseudonym     = token
    /// </code>
    /// </remarks>
    public class SIP_t_WarningValue : SIP_t_Value
    {
        #region Members

        private string m_Agent = "";
        private int m_Code;
        private string m_Text = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets warning code.
        /// </summary>
        public int Code
        {
            get { return m_Code; }

            set
            {
                if (value < 100 || value > 999)
                {
                    throw new ArgumentException("Property Code value must be 3 digit !");
                }

                m_Code = value;
            }
        }

        /// <summary>
        /// Gets or sets name or pseudonym of the server.
        /// </summary>
        public string Agent
        {
            get { return m_Agent; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Agent value may not be null or empty !");
                }

                m_Agent = value;
            }
        }

        /// <summary>
        /// Gets or sets warning text.
        /// </summary>
        public string Text
        {
            get { return m_Text; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Text value may not be null or empty !");
                }

                m_Text = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "warning-value" from specified value.
        /// </summary>
        /// <param name="value">SIP "warning-value" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("reader");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "warning-value" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*                
                warning-value  =  warn-code SP warn-agent SP warn-text
                warn-code      =  3DIGIT
                warn-agent     =  hostport / pseudonym
                                  ;  the name or pseudonym of the server adding
                                  ;  the Warning header, for use in debugging
                warn-text      =  quoted-string
                pseudonym      =  token
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'warning-value' value, warn-code is missing !");
            }
            try
            {
                Code = Convert.ToInt32(word);
            }
            catch
            {
                throw new SIP_ParseException("Invalid 'warning-value' warn-code value, warn-code is missing !");
            }

            word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'warning-value' value, warn-agent is missing !");
            }
            Agent = word;

            word = reader.ReadToEnd();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'warning-value' value, warn-text is missing !");
            }
            Agent = TextUtils.UnQuoteString(word);
        }

        /// <summary>
        /// Converts this to valid "warning-value" value.
        /// </summary>
        /// <returns>Returns "warning-value" value.</returns>
        public override string ToStringValue()
        {
            return m_Code + " " + m_Agent + " " + TextUtils.QuoteString(m_Text);
        }

        #endregion
    }
}