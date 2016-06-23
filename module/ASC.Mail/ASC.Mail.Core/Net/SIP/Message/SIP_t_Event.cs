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