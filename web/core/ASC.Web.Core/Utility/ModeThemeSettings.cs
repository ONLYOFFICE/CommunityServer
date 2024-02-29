/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Runtime.Serialization;

using ASC.Core.Common.Settings;

namespace ASC.Web.Core.Utility
{
    public enum ModeTheme
    {
        light = 0,
        dark = 1
    }

    [Serializable]
    [DataContract]
    public class ModeThemeSettings : BaseSettings<ModeThemeSettings>
    {

        [DataMember(Name = "ModeThemeName")]
        public ModeTheme ModeThemeName { get; set; }

        [DataMember(Name = "AutoDetect")]
        public bool AutoDetect { get; set; }

        public override ISettings GetDefault()
        {
            return new ModeThemeSettings
            {
                ModeThemeName = ModeTheme.light,
                AutoDetect = true
            };
        }

        public override Guid ID
        {
            get { return new Guid("{0CA1BD54-081E-4F15-851B-4C86BDB69C3A}"); }
        }

        public static ModeThemeSettings GetModeThemesSettings()
        {
            var modeTheme = LoadForCurrentUser();

            if (modeTheme.AutoDetect)
            {
                var cookieValue = CookiesManager.GetCookies(CookiesType.ModeThemeKey);
                if (!string.IsNullOrEmpty(cookieValue) && Enum.TryParse<ModeTheme>(cookieValue, out var theme))
                {
                    return new ModeThemeSettings { AutoDetect = true, ModeThemeName = theme };
                }
            }

            return modeTheme;
        }

        public static void SaveModeTheme(ModeTheme theme, bool auto)
        {
            var settings = new ModeThemeSettings { ModeThemeName = theme, AutoDetect = auto};
            settings.SaveForCurrentUser();
        }
    }
}
