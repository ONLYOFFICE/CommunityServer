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
    using System.Net;

    #endregion

    /// <summary>
    /// This class provides utility methods for RTP.
    /// </summary>
    public class RTP_Utils
    {
        #region Methods

        /// <summary>
        /// Generates random SSRC value.
        /// </summary>
        /// <returns>Returns random SSRC value.</returns>
        public static uint GenerateSSRC()
        {
            return (uint) new Random().Next(100000, int.MaxValue);
        }

        /// <summary>
        /// Generates random CNAME value.
        /// </summary>
        /// <returns></returns>
        public static string GenerateCNAME()
        {
            // user@host.randomTag

            return Environment.UserName + "@" + Dns.GetHostName() + "." +
                   Guid.NewGuid().ToString().Substring(0, 8);
        }

        /// <summary>
        /// Converts specified DateTime value to short NTP time. Note: NTP time is in UTC.
        /// </summary>
        /// <param name="value">DateTime value to convert. This value must be in local time.</param>
        /// <returns>Returns NTP value.</returns>
        public static uint DateTimeToNTP32(DateTime value)
        {
            /*
                In some fields where a more compact representation is
                appropriate, only the middle 32 bits are used; that is, the low 16
                bits of the integer part and the high 16 bits of the fractional part.
                The high 16 bits of the integer part must be determined
                independently.
            */

            return (uint) ((DateTimeToNTP64(value) >> 16) & 0xFFFFFFFF);
        }

        /// <summary>
        /// Converts specified DateTime value to long NTP time. Note: NTP time is in UTC.
        /// </summary>
        /// <param name="value">DateTime value to convert. This value must be in local time.</param>
        /// <returns>Returns NTP value.</returns>
        public static ulong DateTimeToNTP64(DateTime value)
        {
            /*
                Wallclock time (absolute date and time) is represented using the
                timestamp format of the Network Time Protocol (NTP), which is in
                seconds relative to 0h UTC on 1 January 1900 [4].  The full
                resolution NTP timestamp is a 64-bit unsigned fixed-point number with
                the integer part in the first 32 bits and the fractional part in the
                last 32 bits. In some fields where a more compact representation is
                appropriate, only the middle 32 bits are used; that is, the low 16
                bits of the integer part and the high 16 bits of the fractional part.
                The high 16 bits of the integer part must be determined
                independently.
            */

            TimeSpan ts = ((value.ToUniversalTime() - new DateTime(1900, 1, 1, 0, 0, 0)));

            return ((ulong) (ts.TotalMilliseconds%1000) << 32) | (uint) (ts.Milliseconds << 22);
        }

        #endregion
    }
}