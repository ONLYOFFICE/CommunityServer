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
using ASC.Data.Storage;

namespace ASC.Web.Core.Utility.Skins
{
    public static class WebImageSupplier
    {
        public static string GetAbsoluteWebPath(string imgFileName)
        {
            return GetAbsoluteWebPath(imgFileName, Guid.Empty);
        }

        public static string GetAbsoluteWebPath(string imgFileName, Guid moduleID)
        {
            return GetImageAbsoluteWebPath(imgFileName, moduleID);
        }

        public static string GetImageFolderAbsoluteWebPath()
        {
            return GetImageFolderAbsoluteWebPath(Guid.Empty);
        }

        public static string GetImageFolderAbsoluteWebPath(Guid moduleID)
        {
            if (HttpContext.Current == null) return string.Empty;

            var currentThemePath = GetPartImageFolderRel(moduleID);
            return WebPath.GetPath(currentThemePath.ToLower());
        }

        private static string GetImageAbsoluteWebPath(string fileName, Guid partID)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            var filepath = GetPartImageFolderRel(partID) + "/" + fileName;
            return WebPath.GetPath(filepath.ToLower());
        }

        private static string GetPartImageFolderRel(Guid partID)
        {
            var folderName = "/skins/default/images";
            string itemFolder = null;
            if (!Guid.Empty.Equals(partID))
            {
                var product = WebItemManager.Instance[partID];
                if (product != null && product.Context != null)
                {
                    itemFolder = GetAppThemeVirtualPath(product) + "/default/images";
                }

                folderName = itemFolder ?? folderName;
            }
            return folderName.TrimStart('~').ToLowerInvariant();
        }

        private static string GetAppThemeVirtualPath(IWebItem webitem)
        {
            if (webitem == null || string.IsNullOrEmpty(webitem.StartURL))
            {
                return string.Empty;
            }

            var dir = webitem.StartURL.Contains(".") ?
                          webitem.StartURL.Substring(0, webitem.StartURL.LastIndexOf("/")) :
                          webitem.StartURL.TrimEnd('/');
            return dir + "/app_themes";
        }
    }
}