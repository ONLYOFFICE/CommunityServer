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
using System.Linq;
using System.Text;

namespace ASC.Common.Utils
{
    public static class MimeHeaderUtils
    {
        public static string EncodeMime(string mimeHeaderValue)
        {
            return EncodeMime(mimeHeaderValue, Encoding.UTF8, false);
        }

        public static string EncodeMime(string mimeHeaderValue, Encoding charset, bool split)
        {
            if (MustEncode(mimeHeaderValue))
            {
                var result = new StringBuilder();
                var data = charset.GetBytes(mimeHeaderValue);
                var maxEncodedTextSize = split ? 75 - ("=?" + charset.WebName + "?" + "B"/*Base64 encode*/ + "?" + "?=").Length : int.MaxValue;

                result.Append("=?" + charset.WebName + "?B?");
                var stored = 0;
                var base64 = Convert.ToBase64String(data);
                for (var i = 0; i < base64.Length; i += 4)
                {
                    // Encoding buffer full, create new encoded-word.
                    if (stored + 4 > maxEncodedTextSize)
                    {
                        result.Append("?=\r\n =?" + charset.WebName + "?B?");
                        stored = 0;
                    }

                    result.Append(base64, i, 4);
                    stored += 4;
                }
                result.Append("?=");
                return result.ToString();
            }
            return mimeHeaderValue;
        }

        public static bool MustEncode(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Any(c => c > 127);
        }
    }
}