/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using DotNetOpenAuth.Messaging;

namespace OpenIdAuth.Utils
{
    public class Util
    {
        /// <summary>
        /// Enumerates through the individual set bits in a flag enum.
        /// </summary>
        /// <param name="flags">The flags enum value.</param>
        /// <returns>An enumeration of just the <i>set</i> bits in the flags enum.</returns>
        internal static IEnumerable<long> GetIndividualFlags(Enum flags)
        {
            var flagsLong = Convert.ToInt64(flags);
            for (var i = 0; i < sizeof(long) * 8; i++)
            { // long is the type behind the largest enum
                // Select an individual application from the scopes.
                var individualFlagPosition = (long)Math.Pow(2, i);
                var individualFlag = flagsLong & individualFlagPosition;
                if (individualFlag == individualFlagPosition)
                {
                    yield return individualFlag;
                }
            }
        }

        internal static Uri GetCallbackUrlFromContext()
        {
            Uri callback = MessagingUtilities.GetRequestUrlFromContext().StripQueryArgumentsWithPrefix("oauth_");
            return callback;
        }
    }
}