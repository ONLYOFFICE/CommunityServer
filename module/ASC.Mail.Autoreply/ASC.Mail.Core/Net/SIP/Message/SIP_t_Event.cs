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
    /// Implements SIP "Event" value. Defined in RFC 3265.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3265 Syntax:
    ///     Event       = event-type *( SEMI event-param )
    ///     event-param = generic-param / ( "id" EQUAL token )
    /// </code>
    /// </remarks>
    public class SIP_t_Event : SIP_t_ValueWithParams
    {
        #region Members

        private string m_EventType = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets event type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null vallue is passed.</exception>
        /// <exception cref="ArgumentException">Is raised when emptu string passed.</exception>
        public string EventType
        {
            get { return m_EventType; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("EventType");
                }
                if (value == "")
                {
                    throw new ArgumentException("Property EventType value can't be '' !");
                }

                m_EventType = value;
            }
        }

        /// <summary>
        /// Gets or sets 'id' parameter value. Value null means not specified.
        /// </summary>
        public string ID
        {
            get
            {
                SIP_Parameter parameter = Parameters["id"];
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
                    Parameters.Remove("id");
                }
                else
                {
                    Parameters.Set("id", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP 'Event' value.</param>
        public SIP_t_Event(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Event" from specified value.
        /// </summary>
        /// <param name="value">SIP "Event" value.</param>
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
        /// Parses "Event" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Event       = event-type *( SEMI event-param )
                event-param = generic-param / ( "id" EQUAL token )
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // event-type
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("SIP Event 'event-type' value is missing !");
            }
            m_EventType = word;

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Event" value.
        /// </summary>
        /// <returns>Returns "Event" value.</returns>
        public override string ToStringValue()
        {
            /*
                Event       = event-type *( SEMI event-param )
                event-param = generic-param / ( "id" EQUAL token )
            */

            StringBuilder retVal = new StringBuilder();

            // event-type
            retVal.Append(m_EventType);

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}