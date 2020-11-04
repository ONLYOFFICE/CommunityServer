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