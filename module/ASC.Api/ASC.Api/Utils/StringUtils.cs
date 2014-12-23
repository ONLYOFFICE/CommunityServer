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

#region usings

using System;
using System.Globalization;
using System.Net.Mime;

#endregion

namespace ASC.Api.Utils
{
    public static class StringUtils
    {
        public static string TrimExtension(string path, int startIndex)
        {
            int index = path.LastIndexOf('.');
            if (index != -1 && index > startIndex)
            {
                path = path.Remove(index);
            }
            return path;
        }

        public static string GetExtension(string path)
        {
            int index = path.LastIndexOf('.');
            return index != -1 ? path.Substring(index) : string.Empty;
        }

        public static bool IsContentType(string mediaType, string contentType)
        {
            return !string.IsNullOrEmpty(contentType) && new ContentType(contentType).MediaType == mediaType;
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
                return s;
            string str = char.ToLower(s[0], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            if (s.Length > 1)
                str = str + s.Substring(1);
            return str;
        }

        public static string FirstPart(this string value, char separator)
        {
            int index;
            if (!string.IsNullOrEmpty(value) && (index=value.IndexOf(separator))!=-1)
            {
                return value.Substring(0, index);
            }
            return value;
        }
    }
}