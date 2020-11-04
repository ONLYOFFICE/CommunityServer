/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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