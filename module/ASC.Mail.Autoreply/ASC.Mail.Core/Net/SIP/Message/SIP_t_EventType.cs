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

    #endregion

    /// <summary>
    /// Implements SIP "event-type" value. Defined in RFC 3265.
    /// </summary>
    public class SIP_t_EventType : SIP_t_Value
    {
        #region Members

        private string m_EventType = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets event type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value passed as value.</exception>
        public string EventType
        {
            get { return m_EventType; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("EventType");
                }

                m_EventType = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "event-type" from specified value.
        /// </summary>
        /// <param name="value">SIP "event-type" value.</param>
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
        /// Parses "event-type" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Get Method
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'event-type' value, event-type is missing !");
            }
            m_EventType = word;
        }

        /// <summary>
        /// Converts this to valid "event-type" value.
        /// </summary>
        /// <returns>Returns "event-type" value.</returns>
        public override string ToStringValue()
        {
            return m_EventType;
        }

        #endregion
    }
}