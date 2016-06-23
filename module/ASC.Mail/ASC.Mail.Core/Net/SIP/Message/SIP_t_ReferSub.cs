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
    /// Implements SIP "refer-sub" value. Defined in RFC 4488.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4488 Syntax:
    ///     Refer-Sub       = refer-sub-value *(SEMI exten)
    ///     refer-sub-value = "true" / "false"
    ///     exten           = generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_ReferSub : SIP_t_ValueWithParams
    {
        #region Members

        private bool m_Value;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets refer-sub-value value.
        /// </summary>
        public bool Value
        {
            get { return m_Value; }

            set { m_Value = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_ReferSub() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Refer-Sub value.</param>
        public SIP_t_ReferSub(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Refer-Sub" from specified value.
        /// </summary>
        /// <param name="value">SIP "Refer-Sub" value.</param>
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
        /// Parses "Refer-Sub" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Refer-Sub       = refer-sub-value *(SEMI exten)
                refer-sub-value = "true" / "false"
                exten           = generic-param        
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // refer-sub-value
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Refer-Sub refer-sub-value value is missing !");
            }
            try
            {
                m_Value = Convert.ToBoolean(word);
            }
            catch
            {
                throw new SIP_ParseException("Invalid Refer-Sub refer-sub-value value !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "contact-param" value.
        /// </summary>
        /// <returns>Returns "contact-param" value.</returns>
        public override string ToStringValue()
        {
            /*
                Refer-Sub       = refer-sub-value *(SEMI exten)
                refer-sub-value = "true" / "false"
                exten           = generic-param        
            */

            StringBuilder retVal = new StringBuilder();

            // refer-sub-value
            retVal.Append(m_Value.ToString());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}