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