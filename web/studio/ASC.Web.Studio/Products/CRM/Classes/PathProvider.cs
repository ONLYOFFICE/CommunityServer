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


#region Import

using System;
using System.Web;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;

#endregion

namespace ASC.Web.CRM
{
    public  static class PathProvider
    {

        public static readonly String BaseVirtualPath = "~/Products/CRM/";
        public static readonly String BaseAbsolutePath = CommonLinkUtility.ToAbsolute(BaseVirtualPath);

        public static String StartURL()
        {
            return BaseVirtualPath;
        }

        public static string BaseSiteUrl
        {
            get
            {
                HttpContext context = HttpContext.Current;
                string baseUrl = context.Request.GetUrlRewriter().Scheme + "://" + context.Request.GetUrlRewriter().Authority + context.Request.ApplicationPath.TrimEnd('/') + '/';
                return baseUrl;
            }
        }

        public static string GetVirtualPath(string physicalPath)
        {
            string rootpath = HttpContext.Current.Server.MapPath("~/");
            physicalPath = physicalPath.Replace(rootpath, "");
            physicalPath = physicalPath.Replace("\\", "/");

            return "~/" + physicalPath;
        }

        public static String GetFileStaticRelativePath(String fileName)
        {
            if (fileName.EndsWith(".js"))
            {
                //Attention: Only for ResourceBundleControl
                return VirtualPathUtility.ToAbsolute("~/Products/CRM/js/" + fileName);
            }
            if (fileName.EndsWith(".ascx"))
            {
                return VirtualPathUtility.ToAbsolute("~/Products/CRM/Controls/" + fileName);
            }
            if (fileName.EndsWith(".css") || fileName.EndsWith(".less"))
            {
                //Attention: Only for ResourceBundleControl
                return VirtualPathUtility.ToAbsolute("~/Products/CRM/App_Themes/default/css/" + fileName);
            }
            if (fileName.EndsWith(".png") || fileName.EndsWith(".gif") || fileName.EndsWith(".jpg"))
            {
                return WebPath.GetPath("/Products/CRM/App_Themes/default/images/" + fileName);
            }

            return fileName;
        }

    }
}