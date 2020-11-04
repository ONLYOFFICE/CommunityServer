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