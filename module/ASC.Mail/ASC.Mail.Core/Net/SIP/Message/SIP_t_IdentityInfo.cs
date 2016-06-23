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
    /// Implements SIP "Identity-Info" value. Defined in RFC 4474.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4474 Syntax:
    ///     Identity-Info        = ident-info *( SEMI ident-info-params )
    ///     ident-info           = LAQUOT absoluteURI RAQUOT
    ///     ident-info-params    = ident-info-alg / ident-info-extension
    ///     ident-info-alg       = "alg" EQUAL token
    ///     ident-info-extension = generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_IdentityInfo : SIP_t_ValueWithParams
    {
        #region Members

        private string m_Uri = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets URI value.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid 'absoluteURI' value is passed.</exception>
        public string Uri
        {
            get { return m_Uri; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Uri");
                }
                if (value == "")
                {
                    throw new ArgumentException("Invalid Identity-Info 'absoluteURI' value !");
                }

                m_Uri = value;
            }
        }

        /// <summary>
        /// Gets or sets 'alg' parameter value. Value null means not specified.
        /// </summary>
        public string Alg
        {
            get
            {
                SIP_Parameter parameter = Parameters["alg"];
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
                    Parameters.Remove("alg");
                }
                else
                {
                    Parameters.Set("alg", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP 'Identity-Info' value.</param>
        public SIP_t_IdentityInfo(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Identity-Info" from specified value.
        /// </summary>
        /// <param name="value">SIP "Identity-Info" value.</param>
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
        /// Parses "Identity-Info" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Identity-Info        = ident-info *( SEMI ident-info-params )
                ident-info           = LAQUOT absoluteURI RAQUOT
                ident-info-params    = ident-info-alg / ident-info-extension
                ident-info-alg       = "alg" EQUAL token
                ident-info-extension = generic-param
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // absoluteURI
            try
            {
                string word = reader.ReadParenthesized();
                if (word == null)
                {
                    throw new SIP_ParseException("Invalid Identity-Info 'absoluteURI' value !");
                }
                m_Uri = word;
            }
            catch
            {
                throw new SIP_ParseException("Invalid Identity-Info 'absoluteURI' value !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Identity-Info" value.
        /// </summary>
        /// <returns>Returns "Identity-Info" value.</returns>
        public override string ToStringValue()
        {
            /*
                Identity-Info        = ident-info *( SEMI ident-info-params )
                ident-info           = LAQUOT absoluteURI RAQUOT
                ident-info-params    = ident-info-alg / ident-info-extension
                ident-info-alg       = "alg" EQUAL token
                ident-info-extension = generic-param
            */

            StringBuilder retVal = new StringBuilder();

            // absoluteURI
            retVal.Append("<" + m_Uri + ">");

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}