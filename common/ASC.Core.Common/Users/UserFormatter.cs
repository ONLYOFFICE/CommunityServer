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
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;

namespace ASC.Core.Users
{
    public class UserFormatter : IComparer<UserInfo>
    {
        private readonly DisplayUserNameFormat format;
        private static bool forceFormatChecked;
        private static string forceFormat;


        public UserFormatter()
            : this(DisplayUserNameFormat.Default)
        {
        }

        public UserFormatter(DisplayUserNameFormat format)
        {
            this.format = format;
        }


        public static string GetUserName(UserInfo userInfo, DisplayUserNameFormat format)
        {
            if (userInfo == null) throw new ArgumentNullException("userInfo");
            return string.Format(GetUserDisplayFormat(format), userInfo.FirstName, userInfo.LastName);
        }

        public static string GetUserName(String firstName, String lastName)
        {

            if (String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty("lastName")) throw new ArgumentException();

            return string.Format(GetUserDisplayFormat(DisplayUserNameFormat.Default), firstName, lastName);
        }

        public static string GetUserName(UserInfo userInfo)
        {
            return GetUserName(userInfo, DisplayUserNameFormat.Default);
        }

        int IComparer<UserInfo>.Compare(UserInfo x, UserInfo y)
        {
            return Compare(x, y, format);
        }

        public static int Compare(UserInfo x, UserInfo y)
        {
            return Compare(x, y, DisplayUserNameFormat.Default);
        }

        public static int Compare(UserInfo x, UserInfo y, DisplayUserNameFormat format)
        {
            if (x == null && y == null) return 0;
            if (x == null && y != null) return -1;
            if (x != null && y == null) return +1;

            var result = 0;
            if (format == DisplayUserNameFormat.Default) format = GetUserDisplayDefaultOrder();
            if (format == DisplayUserNameFormat.FirstLast)
            {
                result = String.Compare(x.FirstName, y.FirstName, true);
                if (result == 0) result = String.Compare(x.LastName, y.LastName, true);
            }
            else
            {
                result = String.Compare(x.LastName, y.LastName, true);
                if (result == 0) result = String.Compare(x.FirstName, y.FirstName, true);
            }
            return result;
        }

        private static readonly Dictionary<string, Dictionary<DisplayUserNameFormat, string>> DisplayFormats = new Dictionary<string, Dictionary<DisplayUserNameFormat, string>>
        {
            { "ru", new Dictionary<DisplayUserNameFormat, string>{ { DisplayUserNameFormat.Default, "{1} {0}" }, { DisplayUserNameFormat.FirstLast, "{0} {1}" }, { DisplayUserNameFormat.LastFirst, "{1} {0}" } } },
            { "default", new Dictionary<DisplayUserNameFormat, string>{ {DisplayUserNameFormat.Default, "{0} {1}" }, { DisplayUserNameFormat.FirstLast, "{0} {1}" }, { DisplayUserNameFormat.LastFirst, "{1}, {0}" } } },
        };

        private static string GetUserDisplayFormat()
        {
            return GetUserDisplayFormat(DisplayUserNameFormat.Default);
        }


        private static string GetUserDisplayFormat(DisplayUserNameFormat format)
        {
            if (!forceFormatChecked)
            {
                forceFormat = ConfigurationManagerExtension.AppSettings["core.user-display-format"];
                if (String.IsNullOrEmpty(forceFormat)) forceFormat = null;
                forceFormatChecked = true;
            }
            if (forceFormat != null) return forceFormat;
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            Dictionary<DisplayUserNameFormat, string> formats = null;
            if (!DisplayFormats.TryGetValue(culture, out formats))
            {
                var twoletter = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                if (!DisplayFormats.TryGetValue(twoletter, out formats)) formats = DisplayFormats["default"];
            }
            return formats[format];
        }

        public static DisplayUserNameFormat GetUserDisplayDefaultOrder()
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            Dictionary<DisplayUserNameFormat, string> formats = null;
            if (!DisplayFormats.TryGetValue(culture, out formats))
            {
                string twoletter = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                if (!DisplayFormats.TryGetValue(twoletter, out formats)) formats = DisplayFormats["default"];
            }
            var format = formats[DisplayUserNameFormat.Default];
            return format.IndexOf("{0}") < format.IndexOf("{1}") ? DisplayUserNameFormat.FirstLast : DisplayUserNameFormat.LastFirst;
        }

        public static Regex UserNameRegex = new Regex(ConfigurationManagerExtension.AppSettings["core.username.regex"] ?? "");

        public static bool IsValidUserName(string firstName, string lastName)
        {
            return UserNameRegex.IsMatch(firstName + lastName);
        }
    }
}
