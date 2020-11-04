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