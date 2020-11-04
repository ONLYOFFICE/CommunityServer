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


namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.Globalization;

    #endregion

    /// <summary>
    /// Provides MIME related utility methods.
    /// </summary>
    public class MIME_Utils
    {
        #region Methods

        /// <summary>
        /// Converts date to RFC 2822 date time string.
        /// </summary>
        /// <param name="dateTime">Date time value to convert..</param>
        /// <returns>Returns RFC 2822 date time string.</returns>
        public static string DateTimeToRfc2822(DateTime dateTime)
        {
            return dateTime.ToString("ddd, dd MMM yyyy HH':'mm':'ss ", DateTimeFormatInfo.InvariantInfo) +
                   dateTime.ToString("zzz").Replace(":", "");
        }

        /// <summary>
        /// Parses RFC 2822 date-time from the specified value.
        /// </summary>
        /// <param name="value">RFC 2822 date-time string value.</param>
        /// <returns>Returns parsed datetime value.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static DateTime ParseRfc2822DateTime(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(value);
            }
            //Try parse dt
            DateTime parsedTime;
            if (DateTime.TryParse(value, out parsedTime))
            {
                return parsedTime;
            }
            /* RFC 2822 3.
             *      date-time       =       [ day-of-week "," ] date FWS time [CFWS]
             *      day-of-week     =       ([FWS] day-name) / obs-day-of-week
             *      day-name        =       "Mon" / "Tue" / "Wed" / "Thu" / "Fri" / "Sat" / "Sun"
             *      date            =       day month year 
             *      year            =       4*DIGIT / obs-year
             *      month           =       (FWS month-name FWS) / obs-month
             *      month-name      =       "Jan" / "Feb" / "Mar" / "Apr" / "May" / "Jun" / "Jul" / "Aug" / "Sep" / "Oct" / "Nov" / "Dec"
             *      day             =       ([FWS] 1*2DIGIT) / obs-day
             *      time            =       time-of-day FWS zone
             *      time-of-day     =       hour ":" minute [ ":" second ]
             *      hour            =       2DIGIT / obs-hour
             *      minute          =       2DIGIT / obs-minute
             *      second          =       2DIGIT / obs-second
             *      zone            =       (( "+" / "-" ) 4DIGIT) / obs-zone
             * 
             *      The date and time-of-day SHOULD express local time.
            */

            try
            {
                MIME_Reader r = new MIME_Reader(value);
                string v = r.Atom();
                // Skip optional [ day-of-week "," ] and read "day".
                if (v.Length == 3)
                {
                    r.Char(true);
                    v = r.Atom();
                }
                int day = Convert.ToInt32(v);
                v = r.Atom().ToLower();
                int month = 1;
                if (v == "jan")
                {
                    month = 1;
                }
                else if (v == "feb")
                {
                    month = 2;
                }
                else if (v == "mar")
                {
                    month = 3;
                }
                else if (v == "apr")
                {
                    month = 4;
                }
                else if (v == "may")
                {
                    month = 5;
                }
                else if (v == "jun")
                {
                    month = 6;
                }
                else if (v == "jul")
                {
                    month = 7;
                }
                else if (v == "aug")
                {
                    month = 8;
                }
                else if (v == "sep")
                {
                    month = 9;
                }
                else if (v == "oct")
                {
                    month = 10;
                }
                else if (v == "nov")
                {
                    month = 11;
                }
                else if (v == "dec")
                {
                    month = 12;
                }
                else
                {
                    throw new ArgumentException("Invalid month-name value '" + value + "'.");
                }
                int year = Convert.ToInt32(r.Atom());
                int hour = Convert.ToInt32(r.Atom());
                r.Char(true);
                int minute = Convert.ToInt32(r.Atom());
                int second = 0;
                // We have optional "second".
                if (r.Peek(true) == ':')
                {
                    r.Char(true);
                    second = Convert.ToInt32(r.Atom());
                }
                int timeZoneMinutes = 0;
                v = r.Atom();
                // We have RFC 2822 date. For example: +2000.
                if (v[0] == '+' || v[0] == '-')
                {
                    if (v[0] == '+')
                    {
                        timeZoneMinutes = (Convert.ToInt32(v.Substring(1, 2))*60 +
                                           Convert.ToInt32(v.Substring(3, 2)));
                    }
                    else
                    {
                        timeZoneMinutes =
                            -(Convert.ToInt32(v.Substring(1, 2))*60 + Convert.ToInt32(v.Substring(3, 2)));
                    }
                }
                    // We have RFC 822 date with abbrevated time zone name. For example: GMT.
                else
                {
                    v = v.ToUpper();

                    #region time zones

                    // Alpha Time Zone (military).
                    if (v == "A")
                    {
                        timeZoneMinutes = ((01*60) + 00);
                    }
                        // Australian Central Daylight Time.
                    else if (v == "ACDT")
                    {
                        timeZoneMinutes = ((10*60) + 30);
                    }
                        // Australian Central Standard Time.
                    else if (v == "ACST")
                    {
                        timeZoneMinutes = ((09*60) + 30);
                    }
                        // Atlantic Daylight Time.
                    else if (v == "ADT")
                    {
                        timeZoneMinutes = -((03*60) + 00);
                    }
                        // Australian Eastern Daylight Time.
                    else if (v == "AEDT")
                    {
                        timeZoneMinutes = ((11*60) + 00);
                    }
                        // Australian Eastern Standard Time.
                    else if (v == "AEST")
                    {
                        timeZoneMinutes = ((10*60) + 00);
                    }
                        // Alaska Daylight Time.
                    else if (v == "AKDT")
                    {
                        timeZoneMinutes = -((08*60) + 00);
                    }
                        // Alaska Standard Time.
                    else if (v == "AKST")
                    {
                        timeZoneMinutes = -((09*60) + 00);
                    }
                        // Atlantic Standard Time.
                    else if (v == "AST")
                    {
                        timeZoneMinutes = -((04*60) + 00);
                    }
                        // Australian Western Daylight Time.
                    else if (v == "AWDT")
                    {
                        timeZoneMinutes = ((09*60) + 00);
                    }
                        // Australian Western Standard Time.
                    else if (v == "AWST")
                    {
                        timeZoneMinutes = ((08*60) + 00);
                    }
                        // Bravo Time Zone (millitary).
                    else if (v == "B")
                    {
                        timeZoneMinutes = ((02*60) + 00);
                    }
                        // British Summer Time.
                    else if (v == "BST")
                    {
                        timeZoneMinutes = ((01*60) + 00);
                    }
                        // Charlie Time Zone (millitary).
                    else if (v == "C")
                    {
                        timeZoneMinutes = ((03*60) + 00);
                    }
                        // Central Daylight Time.
                    else if (v == "CDT")
                    {
                        timeZoneMinutes = -((05*60) + 00);
                    }
                        // Central European Daylight Time.
                    else if (v == "CEDT")
                    {
                        timeZoneMinutes = ((02*60) + 00);
                    }
                        // Central European Summer Time.
                    else if (v == "CEST")
                    {
                        timeZoneMinutes = ((02*60) + 00);
                    }
                        // Central European Time.
                    else if (v == "CET")
                    {
                        timeZoneMinutes = ((01*60) + 00);
                    }
                        // Central Standard Time.
                    else if (v == "CST")
                    {
                        timeZoneMinutes = -((06*60) + 00);
                    }
                        // Christmas Island Time.
                    else if (v == "CXT")
                    {
                        timeZoneMinutes = ((01*60) + 00);
                    }
                        // Delta Time Zone (military).
                    else if (v == "D")
                    {
                        timeZoneMinutes = ((04*60) + 00);
                    }
                        // Echo Time Zone (military).
                    else if (v == "E")
                    {
                        timeZoneMinutes = ((05*60) + 00);
                    }
                        // Eastern Daylight Time.
                    else if (v == "EDT")
                    {
                        timeZoneMinutes = -((04*60) + 00);
                    }
                        // Eastern European Daylight Time.
                    else if (v == "EEDT")
                    {
                        timeZoneMinutes = ((03*60) + 00);
                    }
                        // Eastern European Summer Time.
                    else if (v == "EEST")
                    {
                        timeZoneMinutes = ((03*60) + 00);
                    }
                        // Eastern European Time.
                    else if (v == "EET")
                    {
                        timeZoneMinutes = ((02*60) + 00);
                    }
                        // Eastern Standard Time.
                    else if (v == "EST")
                    {
                        timeZoneMinutes = -((05*60) + 00);
                    }
                        // Foxtrot Time Zone (military).
                    else if (v == "F")
                    {
                        timeZoneMinutes = (06*60 + 00);
                    }
                        // Golf Time Zone (military).
                    else if (v == "G")
                    {
                        timeZoneMinutes = ((07*60) + 00);
                    }
                        // Greenwich Mean Time.
                    else if (v == "GMT")
                    {
                        timeZoneMinutes = 0000;
                    }
                        // Hotel Time Zone (military).
                    else if (v == "H")
                    {
                        timeZoneMinutes = ((08*60) + 00);
                    }
                        // India Time Zone (military).
                    else if (v == "I")
                    {
                        timeZoneMinutes = ((09*60) + 00);
                    }
                        // Irish Summer Time.
                    else if (v == "IST")
                    {
                        timeZoneMinutes = ((01*60) + 00);
                    }
                        // Kilo Time Zone (millitary).
                    else if (v == "K")
                    {
                        timeZoneMinutes = ((10*60) + 00);
                    }
                        // Lima Time Zone (millitary).
                    else if (v == "L")
                    {
                        timeZoneMinutes = ((11*60) + 00);
                    }
                        // Mike Time Zone (millitary).
                    else if (v == "M")
                    {
                        timeZoneMinutes = ((12*60) + 00);
                    }
                        // Mountain Daylight Time.
                    else if (v == "MDT")
                    {
                        timeZoneMinutes = -((06*60) + 00);
                    }
                        // Mountain Standard Time.
                    else if (v == "MST")
                    {
                        timeZoneMinutes = -((07*60) + 00);
                    }
                        // November Time Zone (military).
                    else if (v == "N")
                    {
                        timeZoneMinutes = -((01*60) + 00);
                    }
                        // Newfoundland Daylight Time.
                    else if (v == "NDT")
                    {
                        timeZoneMinutes = -((02*60) + 30);
                    }
                        // Norfolk (Island) Time.
                    else if (v == "NFT")
                    {
                        timeZoneMinutes = ((11*60) + 30);
                    }
                        // Newfoundland Standard Time.
                    else if (v == "NST")
                    {
                        timeZoneMinutes = -((03*60) + 30);
                    }
                        // Oscar Time Zone (military).
                    else if (v == "O")
                    {
                        timeZoneMinutes = -((02*60) + 00);
                    }
                        // Papa Time Zone (military).
                    else if (v == "P")
                    {
                        timeZoneMinutes = -((03*60) + 00);
                    }
                        // Pacific Daylight Time.
                    else if (v == "PDT")
                    {
                        timeZoneMinutes = -((07*60) + 00);
                    }
                        // Pacific Standard Time.
                    else if (v == "PST")
                    {
                        timeZoneMinutes = -((08*60) + 00);
                    }
                        // Quebec Time Zone (military).
                    else if (v == "Q")
                    {
                        timeZoneMinutes = -((04*60) + 00);
                    }
                        // Romeo Time Zone (military).
                    else if (v == "R")
                    {
                        timeZoneMinutes = -((05*60) + 00);
                    }
                        // Sierra Time Zone (military).
                    else if (v == "S")
                    {
                        timeZoneMinutes = -((06*60) + 00);
                    } 
                        // Tango Time Zone (military).
                    else if (v == "T")
                    {
                        timeZoneMinutes = -((07*60) + 00);
                    }
                        // Uniform Time Zone (military).
                    else if (v == "")
                    {
                        timeZoneMinutes = -((08*60) + 00);
                    }
                        // Coordinated Universal Time.
                    else if (v == "UTC")
                    {
                        timeZoneMinutes = 0000;
                    }
                        // Victor Time Zone (militray).
                    else if (v == "V")
                    {
                        timeZoneMinutes = -((09*60) + 00);
                    }
                        // Whiskey Time Zone (military).
                    else if (v == "W")
                    {
                        timeZoneMinutes = -((10*60) + 00);
                    }
                        // Western European Daylight Time.
                    else if (v == "WEDT")
                    {
                        timeZoneMinutes = ((01*60) + 00);
                    }
                        // Western European Summer Time.
                    else if (v == "WEST")
                    {
                        timeZoneMinutes = ((01*60) + 00);
                    }
                        // Western European Time.
                    else if (v == "WET")
                    {
                        timeZoneMinutes = 0000;
                    }
                        // Western Standard Time.
                    else if (v == "WST")
                    {
                        timeZoneMinutes = ((08*60) + 00);
                    }
                        // X-ray Time Zone (military).
                    else if (v == "X")
                    {
                        timeZoneMinutes = -((11*60) + 00);
                    }
                        // Yankee Time Zone (military).
                    else if (v == "Y")
                    {
                        timeZoneMinutes = -((12*60) + 00);
                    }
                        // Zulu Time Zone (military).
                    else if (v == "Z")
                    {
                        timeZoneMinutes = 0000;
                    }

                    #endregion
                }

                // Convert time to UTC and then back to local.
                DateTime timeUTC =
                    new DateTime(year, month, day, hour, minute, second).AddMinutes(-(timeZoneMinutes));
                return
                    new DateTime(timeUTC.Year,
                                 timeUTC.Month,
                                 timeUTC.Day,
                                 timeUTC.Hour,
                                 timeUTC.Minute,
                                 timeUTC.Second,
                                 DateTimeKind.Utc).ToLocalTime();
            }
            catch (Exception x)
            {
                string dymmy = x.Message;
                throw new ArgumentException("Argumnet 'value' value '" + value +
                                            "' is not valid RFC 822/2822 date-time string.");
            }
        }

        /// <summary>
        /// Unfolds folded header field.
        /// </summary>
        /// <param name="value">Header field.</param>
        /// <returns>Returns unfolded header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public static string UnfoldHeader(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            /* RFC 2822 2.2.3 Long Header Fields.
				The process of moving from this folded multiple-line representation
				of a header field to its single line representation is called
				"unfolding". Unfolding is accomplished by simply removing any CRLF
				that is immediately followed by WSP.
            */

            return value.Replace("\r\n", "");
        }

        /// <summary>
        /// Creates Rfc 2822 3.6.4 message-id. Syntax: '&lt;' id-left '@' id-right '&gt;'.
        /// </summary>
        /// <returns></returns>
        public static string CreateMessageID()
        {
            return "<" + Guid.NewGuid().ToString().Replace("-", "").Substring(16) + "@" +
                   Guid.NewGuid().ToString().Replace("-", "").Substring(16) + ">";
        }

        #endregion
    }
}