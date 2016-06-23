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