/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Configuration;
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
        private static readonly string DesktopSkin = ConfigurationManager.AppSettings["web.desktop.skin"];


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
            var resolvedPath = path.ToLower().Replace(ThemeFolderTemplate, folderName);

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
                resolvedPath = path.ToLower().Replace(ThemeFolderTemplate, "default");

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
            if (HttpContext.Current != null && HttpContext.Current.Request.DesktopApp())
            {
                return DesktopSkin ?? "bright-blue";
            }

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