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