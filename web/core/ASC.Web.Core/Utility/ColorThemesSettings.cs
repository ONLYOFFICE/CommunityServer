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
using System.IO;
using System.Runtime.Serialization;
using System.Web;
using ASC.Core.Common.Settings;

namespace ASC.Web.Core.Utility
{
    [Serializable]
    [DataContract]
    public class ColorThemesSettings : BaseSettings<ColorThemesSettings>
    {
        public const string ThemeFolderTemplate = "<theme_folder>";
        private const string DefaultName = "pure-orange";


        [DataMember(Name = "ColorThemeName")]
        public string ColorThemeName { get; set; }

        [DataMember(Name = "FirstRequest")]
        public bool FirstRequest { get; set; }

        public override ISettings GetDefault()
        {
            return new ColorThemesSettings
                {
                    ColorThemeName = DefaultName,
                    FirstRequest = true
                };
        }

        public override Guid ID
        {
            get { return new Guid("{AB5B3C97-A972-475C-BB13-71936186C4E6}"); }
        }

        public static string GetThemeFolderName(string path)
        {
            var folderName = GetColorThemesSettings();
            var resolvedPath = path.Replace(ThemeFolderTemplate, folderName);

            if (!VirtualPathUtility.IsAbsolute(resolvedPath))
                resolvedPath = VirtualPathUtility.ToAbsolute(resolvedPath);

            try
            {
                var filePath = HttpContext.Current.Server.MapPath(resolvedPath);
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("", path);
            }
            catch (Exception)
            {
                resolvedPath = path.Replace(ThemeFolderTemplate, "default");

                if (!VirtualPathUtility.IsAbsolute(resolvedPath))
                    resolvedPath = VirtualPathUtility.ToAbsolute(resolvedPath);

                var filePath = HttpContext.Current.Server.MapPath(resolvedPath);

                if (!File.Exists(filePath))
                    throw new FileNotFoundException("", path);
            }

            return resolvedPath;
        }

        public static string GetColorThemesSettings()
        {
            var colorTheme = Load();
            var colorThemeName = colorTheme.ColorThemeName;

            if (colorTheme.FirstRequest)
            {
                colorTheme.FirstRequest = false;
                colorTheme.Save();
            }

            return colorThemeName;
        }

        public static void SaveColorTheme(string theme)
        {
            var settings = new ColorThemesSettings { ColorThemeName = theme, FirstRequest = false };
            var path = "/skins/" + ThemeFolderTemplate;
            var resolvedPath = path.ToLower().Replace(ThemeFolderTemplate, theme);

            try
            {
                var filePath = HttpContext.Current.Server.MapPath(resolvedPath);
                if (Directory.Exists(filePath))
                {
                    settings.Save();
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}