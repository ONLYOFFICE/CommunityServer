/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

namespace ASC.FederatedLogin.Profile
{
    public static class LoginProfileExtensions
    {
        public static Uri AddProfile(this Uri uri, LoginProfile profile)
        {
            return profile.AppendProfile(uri);
        }
        public static Uri AddProfileSession(this Uri uri, LoginProfile profile, HttpContext context)
        {
            return profile.AppendSessionProfile(uri, context);
        }

        public static Uri AddProfileCache(this Uri uri, LoginProfile profile)
        {
            return profile.AppendCacheProfile(uri);
        }

        public static LoginProfile GetProfile(this Uri uri)
        {
            var profile = new LoginProfile();
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(queryString[LoginProfile.QuerySessionParamName]) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                return (LoginProfile)HttpContext.Current.Session[queryString[LoginProfile.QuerySessionParamName]];
            }
            if (!string.IsNullOrEmpty(queryString[LoginProfile.QueryParamName]))
            {
                profile.ParseFromUrl(uri);
                return profile;
            }
            if (!string.IsNullOrEmpty(queryString[LoginProfile.QueryCacheParamName]))
            {
                return (LoginProfile)HttpRuntime.Cache.Get(queryString[LoginProfile.QuerySessionParamName]);
            }
            return null;
        }

        public static bool HasProfile(this Uri uri)
        {
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            return !string.IsNullOrEmpty(queryString[LoginProfile.QueryParamName]) || !string.IsNullOrEmpty(queryString[LoginProfile.QuerySessionParamName]) || !string.IsNullOrEmpty(queryString[LoginProfile.QueryCacheParamName]);
        }


    }
}