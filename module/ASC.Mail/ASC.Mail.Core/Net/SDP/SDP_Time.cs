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

namespace ASC.Mail.Net.SDP
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// A SDP_Time represents an <B>t=</B> SDP message field. Defined in RFC 4566 5.9. Timing.
    /// </summary>
    public class SDP_Time
    {
        #region Members

        private long m_StartTime;
        private long m_StopTime;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets start time when session must start. Network Time Protocol (NTP) time values in 
        /// seconds since 1900. 0 value means not specified, if StopTime is also 0, then means infinite session.
        /// </summary>
        public long StartTime
        {
            get { return m_StartTime; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Property StartTime value must be >= 0 !");
                }

                m_StopTime = value;
            }
        }

        /// <summary>
        /// Gets or sets stop time when session must end. Network Time Protocol (NTP) time values in 
        /// seconds since 1900. 0 value means not specified, if StopTime is also 0, then means infinite session.
        /// </summary>
        public long StopTime
        {
            get { return m_StopTime; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Property StopTime value must be >= 0 !");
                }

                m_StopTime = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses media from "t" SDP message field.
        /// </summary>
        /// <param name="tValue">"t" SDP message field.</param>
        /// <returns></returns>
        public static SDP_Time Parse(string tValue)
        {
            // t=<start-time> <stop-time>

            SDP_Time time = new SDP_Time();

            // Remove t=
            StringReader r = new StringReader(tValue);
            r.QuotedReadToDelimiter('=');

            //--- <start-time> ------------------------------------------------------------
            string word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("SDP message \"t\" field <start-time> value is missing !");
            }
            time.m_StartTime = Convert.ToInt64(word);

            //--- <stop-time> -------------------------------------------------------------
            word = r.ReadWord();
            if (word == null)
            {
                throw new Exception("SDP message \"t\" field <stop-time> value is missing !");
            }
            time.m_StopTime = Convert.ToInt64(word);

            return time;
        }

        /// <summary>
        /// Converts this to valid "t" string.
        /// </summary>
        /// <returns></returns>
        public string ToValue()
        {
            // t=<start-time> <stop-time>

            return "t=" + StartTime + " " + StopTime + "\r\n";
        }

        #endregion
    }
}