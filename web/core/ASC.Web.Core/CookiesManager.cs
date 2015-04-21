/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

        public static void SetCookies(CookiesType type, string value, bool session = false)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Value = value;
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = session ? DateTime.MinValue : DateTime.Now.AddDays(365);
                if (type==CookiesType.AuthKey)
                {
                    HttpContext.Current.Response.Cookies[GetCookiesName(type)].HttpOnly = true;
                }
            }
        }

        public static void SetCookies(CookiesType type, string value, string domain, bool session = false)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Value = value;
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Domain = domain;
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = session ? DateTime.MinValue : DateTime.Now.AddDays(365);
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