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


#region usings

using System;
using System.Globalization;
using System.Net.Mime;
using ASC.Api.Impl.Routing;

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
            else
            {
                index = path.LastIndexOf(ApiRouteRegistrator.ExtensionBrace, StringComparison.Ordinal);
                if (index != -1 && index > startIndex)
                {
                    path = path.Remove(index);
                }
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