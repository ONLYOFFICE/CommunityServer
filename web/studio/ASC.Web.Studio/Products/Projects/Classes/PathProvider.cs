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
using ASC.Web.Core.Files;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Classes
{
    public class PathProvider
    {
        public static readonly string BaseVirtualPath;
        public static readonly string BaseAbsolutePath;

        static PathProvider()
        {
            BaseVirtualPath = "~/Products/Projects/";
            try
            {
                BaseAbsolutePath = CommonLinkUtility.ToAbsolute(BaseVirtualPath);
            }
            catch (Exception)
            {
                BaseAbsolutePath = BaseVirtualPath;
            }
        }

        public static string GetFileStaticRelativePath(String fileName)
        {
            var ext = FileUtility.GetFileExtension(fileName);
            switch (ext)
            {
                case ".js":
                    return VirtualPathUtility.ToAbsolute("~/Products/Projects/js/" + fileName);
                case ".png":
                    return WebPath.GetPath("/Products/Projects/App_Themes/Default/images/" + fileName);
                case ".ascx":
                    return CommonLinkUtility.ToAbsolute("~/Products/Projects/Controls/" + fileName);
                case ".css":
                case ".less":
                    return VirtualPathUtility.ToAbsolute("~/Products/Projects/App_Themes/default/css/" + fileName);
            }
            return fileName;
        }

        public static string GetVirtualPath(string physicalPath)
        {
            var rootpath = HttpContext.Current.Server.MapPath("~/");
            return "~/" + physicalPath.Replace(rootpath, string.Empty).Replace("\\", "/");
        }
    }
}