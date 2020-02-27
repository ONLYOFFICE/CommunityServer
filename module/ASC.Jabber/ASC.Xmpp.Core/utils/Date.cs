/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


#region using

using System;

#endregion

namespace ASC.Xmpp.Core.utils
{

    #region usings

    #endregion

    /// <summary>
    ///   Class handles the XMPP time format
    /// </summary>
    public class Time
    {
        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="date"> </param>
        /// <returns> </returns>
        public static DateTime Date(string date)
        {
            // better put here a try catch in case a client sends a wrong formatted date
            try
            {
                var dt = new DateTime(int.Parse(date.Substring(0, 4)),
                                      int.Parse(date.Substring(4, 2)),
                                      int.Parse(date.Substring(6, 2)),
                                      int.Parse(date.Substring(9, 2)),
                                      int.Parse(date.Substring(12, 2)),
                                      int.Parse(date.Substring(15, 2)), DateTimeKind.Utc);

                return dt;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        ///   Get a XMPP string representation of a Date
        /// </summary>
        /// <param name="date"> DateTime </param>
        /// <returns> XMPP string representation of a DateTime value </returns>
        public static string Date(DateTime date)
        {
            return date.ToString("yyyyMMddTHH:mm:ss");
        }

        /// <summary>
        ///   The new standard used by XMPP in JEP-82 (ISO-8601)
        ///   <example>
        ///     1970-01-01T00:00Z
        ///   </example>
        /// </summary>
        /// <param name="date"> </param>
        /// <returns> </returns>
        public static DateTime ISO_8601Date(string date)
        {
            // .NET does a great Job parsing this Date profile
            try
            {
                return DateTime.Parse(date);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        ///   The new standard used by XMPP in JEP-82 (ISO-8601) converts a local DateTime to a ISO-8601 formatted date in UTC format.
        ///   <example>
        ///     1970-01-01T00:00Z
        ///   </example>
        /// </summary>
        /// <param name="date"> local Datetime </param>
        /// <returns> </returns>
        public static string ISO_8601Date(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            // return date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");	
            // return date.ToString("yyyy-MM-ddTHH:mm:ssZ");	

            // ("yyyy'-'MM'-'dd HH':'mm':'ss'Z'") 			
        }

        #endregion

        /*
            <x xmlns="jabber:x:delay" from="cachet@conference.cachet.myjabber.net/dan" stamp="20060303T15:43:08" />         
        */
    }
}