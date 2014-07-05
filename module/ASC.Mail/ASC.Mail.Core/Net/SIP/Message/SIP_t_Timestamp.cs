/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements SIP "Timestamp" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     Timestamp = 1*(DIGIT) [ "." *(DIGIT) ] [ LWS delay ]
    ///         delay = *(DIGIT) [ "." *(DIGIT) ]
    /// </code>
    /// </remarks>
    public class SIP_t_Timestamp : SIP_t_Value
    {
        #region Members

        private decimal m_Delay;
        private decimal m_Time;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets time in seconds when request was sent.
        /// </summary>
        public decimal Time
        {
            get { return m_Time; }

            set { m_Time = value; }
        }

        /// <summary>
        /// Gets or sets delay time in seconds. Delay specifies the time between the UAS received 
        /// the request and generated response.
        /// </summary>
        public decimal Delay
        {
            get { return m_Delay; }

            set { m_Delay = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Timestamp: header field value.</param>
        public SIP_t_Timestamp(string value)
        {
            Parse(new StringReader(value));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="time">Time in seconds when request was sent.</param>
        /// <param name="delay">Delay time in seconds.</param>
        public SIP_t_Timestamp(decimal time, decimal delay)
        {
            m_Time = time;
            m_Delay = delay;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Timestamp" from specified value.
        /// </summary>
        /// <param name="value">SIP "Timestamp" value.</param>
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
        /// Parses "Timestamp" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /* RFC 3261.
                Timestamp =  "Timestamp" HCOLON 1*(DIGIT) [ "." *(DIGIT) ] [ LWS delay ]
                    delay =  *(DIGIT) [ "." *(DIGIT) ]
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Get time
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'Timestamp' value, time is missing !");
            }
            m_Time = Convert.ToDecimal(word);

            // Get optional delay
            word = reader.ReadWord();
            if (word != null)
            {
                m_Delay = Convert.ToDecimal(word);
            }
            else
            {
                m_Delay = 0;
            }
        }

        /// <summary>
        /// Converts this to valid "Timestamp" value.
        /// </summary>
        /// <returns>Returns "accept-range" value.</returns>
        public override string ToStringValue()
        {
            /* RFC 3261.
                Timestamp =  "Timestamp" HCOLON 1*(DIGIT) [ "." *(DIGIT) ] [ LWS delay ]
                    delay =  *(DIGIT) [ "." *(DIGIT) ]
            */

            if (m_Delay > 0)
            {
                return m_Time + " " + m_Delay;
            }
            else
            {
                return m_Time.ToString();
            }
        }

        #endregion
    }
}