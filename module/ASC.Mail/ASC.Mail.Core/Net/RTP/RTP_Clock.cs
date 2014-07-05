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