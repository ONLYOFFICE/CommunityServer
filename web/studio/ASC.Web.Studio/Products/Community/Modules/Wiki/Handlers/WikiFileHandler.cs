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

            var file = new WikiEngine().GetFile(context.Request["file"]);
            if (file == null)
            {
                file = new WikiEngine().GetFile(context.Request["file"].Replace('_', ' '));
                if (file == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.End();
                    return;
                }
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
    }
}
