/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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