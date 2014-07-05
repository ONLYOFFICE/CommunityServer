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

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="Date.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

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