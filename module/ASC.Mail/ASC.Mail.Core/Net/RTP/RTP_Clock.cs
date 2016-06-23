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


namespace ASC.Mail.Net.RTP
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements RTP media clock.
    /// </summary>
    public class RTP_Clock
    {
        #region Members

        private readonly int m_BaseValue;
        private readonly DateTime m_CreateTime;
        private readonly int m_Rate = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets clock base value from where clock started.
        /// </summary>
        public int BaseValue
        {
            get { return m_BaseValue; }
        }

        /// <summary>
        /// Gets current clock rate in Hz.
        /// </summary>
        public int Rate
        {
            get { return m_Rate; }
        }

        /// <summary>
        /// Gets current RTP timestamp.
        /// </summary>
        public uint RtpTimestamp
        {
            get
            {
                /*
                    m_Rate  -> 1000ms
                    elapsed -> x
                */

                long elapsed = (long) ((DateTime.Now - m_CreateTime)).TotalMilliseconds;

                return (uint) (m_BaseValue + ((m_Rate*elapsed)/1000));
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="baseValue">Clock base value from where clock starts.</param>
        /// <param name="rate">Clock rate in Hz.</param>
        public RTP_Clock(int baseValue, int rate)
        {
            if (rate < 1)
            {
                throw new ArgumentException("Argument 'rate' value must be between 1 and 100 000.", "rate");
            }

            m_BaseValue = baseValue;
            m_Rate = rate;
            m_CreateTime = DateTime.Now;
        }

        #endregion
    }
}