/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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