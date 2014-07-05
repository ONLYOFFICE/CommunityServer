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
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "Retry-After" value. Defined in rfc 3261.
    /// Retry after specifies how many seconds the service is expected to be unavailable to the requesting client.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     Retry-After = delta-seconds [ comment ] *( SEMI retry-param )
    ///     retry-param = ("duration" EQUAL delta-seconds) / generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_RetryAfter : SIP_t_ValueWithParams
    {
        #region Members

        private int m_Time;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets how many seconds the service is expected to be unavailable to the requesting client.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when when value less than 1 is passed.</exception>
        public int Time
        {
            get { return m_Time; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("Property Time value must be >= 1 !");
                }

                m_Time = value;
            }
        }

        /// <summary>
        /// Gets or sets 'duration' parameter value. The 'duration' parameter indicates how long the 
        /// called party will be reachable starting at the initial time of availability. If no duration 
        /// parameter is given, the service is assumed to be available indefinitely. Value -1 means not specified.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when when value less than 1 is passed.</exception>
        public int Duration
        {
            get
            {
                SIP_Parameter parameter = Parameters["duration"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("duration");
                }
                else
                {
                    if (value < 1)
                    {
                        throw new ArgumentException("Property Duration value must be >= 1 !");
                    }

                    Parameters.Set("duration", value.ToString());
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP Retry-After value.</param>
        public SIP_t_RetryAfter(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Retry-After" from specified value.
        /// </summary>
        /// <param name="value">SIP "Retry-After" value.</param>
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
        /// Parses "Retry-After" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Retry-After = delta-seconds [ comment ] *( SEMI retry-param )
                retry-param = ("duration" EQUAL delta-seconds) / generic-param
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // delta-seconds
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("SIP Retry-After 'delta-seconds' value is missing !");
            }
            try
            {
                m_Time = Convert.ToInt32(word);
            }
            catch
            {
                throw new SIP_ParseException("Invalid SIP Retry-After 'delta-seconds' value !");
            }

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Retry-After" value.
        /// </summary>
        /// <returns>Returns "Retry-After" value.</returns>
        public override string ToStringValue()
        {
            /*
                Retry-After = delta-seconds [ comment ] *( SEMI retry-param )
                retry-param = ("duration" EQUAL delta-seconds) / generic-param
            */

            StringBuilder retVal = new StringBuilder();

            // delta-seconds
            retVal.Append(m_Time);

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}