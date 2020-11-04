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
            return WebPath.GetPath(currentThemePath);
        }

        private static string GetImageAbsoluteWebPath(string fileName, Guid partID)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            var filepath = GetPartImageFolderRel(partID) + "/" + fileName;
            return WebPath.GetPath(filepath);
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
            return folderName.TrimStart('~');
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
            return dir + "/App_Themes";
        }
    }
}