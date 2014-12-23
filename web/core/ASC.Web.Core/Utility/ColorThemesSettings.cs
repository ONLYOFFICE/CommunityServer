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

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Web;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.Utility
{
    [Serializable]
    [DataContract]
    public class ColorThemesSettings : ISettings
    {
        private const string DefaultName = "default";

        [DataMember(Name = "ColorThemeName")]
        public string ColorThemeName { get; set; }

        [DataMember(Name = "FirstRequest")]
        public bool FirstRequest { get; set; }

        public ISettings GetDefault()
        {
            return new ColorThemesSettings
                {
                    ColorThemeName = DefaultName,
                    FirstRequest = true
                };
        }

        public Guid ID
        {
            get { return new Guid("{AB5B3C97-A972-475C-BB13-71936186C4E6}"); }
        }

        public static string GetThemeFolderName(string path)
        {
            var folderName = GetColorThemesSettings();
            var resolvedPath = path.ToLower().Replace("<theme_folder>", folderName);

            if (!VirtualPathUtility.IsAbsolute(resolvedPath))
                resolvedPath = VirtualPathUtility.ToAbsolute(resolvedPath);

            var filePath = HttpContext.Current.Server.MapPath(resolvedPath);

            if (!File.Exists(filePath))
            {
                resolvedPath = path.ToLower().Replace("<theme_folder>", "default");

                if (!VirtualPathUtility.IsAbsolute(resolvedPath))
                    resolvedPath = VirtualPathUtility.ToAbsolute(resolvedPath);

                filePath = HttpContext.Current.Server.MapPath(resolvedPath);

                if (!File.Exists(filePath))
                    throw new FileNotFoundException("", path);
            }

            return resolvedPath;
        }

        public static string GetColorThemesSettings()
        {
            var colorTheme = SettingsManager.Instance.LoadSettings<ColorThemesSettings>(TenantProvider.CurrentTenantID);
            var colorThemeName = colorTheme.ColorThemeName;

            if (colorTheme.FirstRequest)
            {
                if (colorTheme.ColorThemeName == DefaultName)
                {
                    var partnerId = CoreContext.TenantManager.GetCurrentTenant().PartnerId;
                    if (!string.IsNullOrEmpty(partnerId))
                    {
                        var partner = CoreContext.PaymentManager.GetPartner(partnerId);
                        if (partner != null && partner.Status == PartnerStatus.Approved && !partner.Removed)
                        {
                            colorThemeName = partner.Theme;
                        }
                    }
                }

                SaveColorTheme(colorThemeName);
            }

            return colorThemeName;
        }

        public static void SaveColorTheme(string theme)
        {
            var settings = new ColorThemesSettings { ColorThemeName = theme, FirstRequest = false };
            SettingsManager.Instance.SaveSettings(settings, TenantProvider.CurrentTenantID);
        }
    }
}