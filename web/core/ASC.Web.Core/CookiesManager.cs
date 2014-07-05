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

using System;
using System.Web;
using System.Linq;

namespace ASC.Web.Core
{
    public enum CookiesType
    {
        AuthKey,
        NoMobile
    }

    public class CookiesManager
    {
        private const string AuthCookiesName = "asc_auth_key";
        private const string NoMobileCookiesName = "asc_nomobile";

        private static string GetCookiesName(CookiesType type)
        {
            switch (type)
            {
                case CookiesType.AuthKey: return AuthCookiesName;
                case CookiesType.NoMobile: return NoMobileCookiesName;
            }

            return string.Empty;
        }

        public static bool IsMobileBlocked()
        {
            return !string.IsNullOrEmpty(GetRequestVar(CookiesType.NoMobile));
        }

        public static string GetRequestVar(CookiesType type)
        {
            if (HttpContext.Current!=null)
            {
                var cookie = HttpContext.Current.Request.QueryString[GetCookiesName(type)] ?? HttpContext.Current.Request.Form[GetCookiesName(type)];
                return string.IsNullOrEmpty(cookie) ? GetCookies(type) : cookie;
            }
            return "";
        }

        public static void SetCookies(CookiesType type, string value)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Value = value;
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = DateTime.Now.AddDays(365);
                if (type==CookiesType.AuthKey)
                {
                    HttpContext.Current.Response.Cookies[GetCookiesName(type)].HttpOnly = true;
                }
            }
        }

        public static void SetCookies(CookiesType type, string value, string domain)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Value = value;
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Domain = domain;
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = DateTime.Now.AddDays(365);
                if (type == CookiesType.AuthKey)
                {
                    HttpContext.Current.Response.Cookies[GetCookiesName(type)].HttpOnly = true;
                }
            }
        }

        public static string GetCookies(CookiesType type)
        {
            if (HttpContext.Current != null)
            {
                var cookieName = GetCookiesName(type);
                if (HttpContext.Current.Response.Cookies.AllKeys.Contains(cookieName))
                    return HttpContext.Current.Response.Cookies[cookieName].Value ?? "";

                if (HttpContext.Current.Request.Cookies[cookieName] != null)
                    return HttpContext.Current.Request.Cookies[cookieName].Value ?? "";
            }
            return "";
        }

        public static void ClearCookies(CookiesType type)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Request.Cookies[GetCookiesName(type)] != null)
                    HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = DateTime.Now.AddDays(-3);
            }
        }
    }
}