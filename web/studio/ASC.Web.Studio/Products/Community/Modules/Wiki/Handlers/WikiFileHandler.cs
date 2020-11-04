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


using System.Text.RegularExpressions;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core;
using System.Net;
using System.Web;

namespace ASC.Web.UserControls.Wiki.Handlers
{
    public class WikiFileHandler : IHttpHandler
    {
        public static string ImageExtentions = ".png.jpg.bmp.gif";


        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                if (!SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey)))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.End();
                    return;
                }
            }

            context.Response.Clear();
            if (string.IsNullOrEmpty(context.Request["file"]))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.End();
                return;
            }

            var file = GetFile(context.Request["file"]);
            if (file == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.End();
                return;
            }
            if (string.IsNullOrEmpty(file.FileLocation))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.End();
                return;
            }

            var storage = StorageFactory.GetStorage(CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString(), WikiSection.Section.DataStorage.ModuleName);
            context.Response.Redirect(storage.GetUri(WikiSection.Section.DataStorage.DefaultDomain, file.FileLocation).ToString());
        }

        private Data.File GetFile(string fileName)
        {
            var file = new WikiEngine().GetFile(fileName);
            if (file != null) return file;
            var regex = new Regex(Regex.Escape("_"));
            var newFileName = regex.Replace(fileName, " ", 1);
            return fileName != newFileName ? GetFile(newFileName) : null;
        }
    }
}
