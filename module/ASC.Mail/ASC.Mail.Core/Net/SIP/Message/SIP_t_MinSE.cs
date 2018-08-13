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
    /// Implements SIP "Min-SE" value. Defined in RFC 4028.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4028 Syntax:
    ///     Min-SE = delta-seconds *(SEMI generic-param)
    /// </code>
    /// </remarks>
    public class SIP_t_MinSE : SIP_t_ValueWithParams
    {
        #region Members

        private int m_Time = 90;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets time in seconds when session expires.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when value is less than 1.</exception>
        public int Time
        {
            get { return m_Time; }

            set
            {
                if (m_Time < 1)
                {
                    throw new ArgumentException("Time value must be > 0 !");
                }

                m_Time = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Min-SE value.</param>
        public SIP_t_MinSE(string value)
        {
            Parse(value);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="minExpires">Minimum session expries value in seconds.</param>
        public SIP_t_MinSE(int minExpires)
        {
            m_Time = minExpires;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Min-SE" from specified value.
        /// </summary>
        /// <param name="value">SIP "Min-SE" value.</param>
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
        /// Parses "Min-SE" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Min-SE = delta-seconds *(SEMI generic-param)
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse address
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Min-SE delta-seconds value is missing !");
            }
            try
            {
                m_Time = Convert.ToInt32(word);
            }
            catch
            {
                throw new SIP_ParseException("Invalid Min-SE delta-seconds value !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Min-SE" value.
        /// </summary>
        /// <returns>Returns "Min-SE" value.</returns>
        public override string ToStringValue()
        {
            /*
                Min-SE = delta-seconds *(SEMI generic-param)
            */

            StringBuilder retVal = new StringBuilder();

            // Add address
            retVal.Append(m_Time.ToString());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}