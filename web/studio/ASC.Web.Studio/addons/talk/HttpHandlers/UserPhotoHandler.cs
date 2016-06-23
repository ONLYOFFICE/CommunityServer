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
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Web.Talk.Addon;

namespace ASC.Web.Talk.HttpHandlers
{
    public class UserPhotoHandler : IHttpHandler
    {
        public bool IsReusable
        {
            // To enable pooling, return true here.
            // This keeps the handler in memory.
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string imagepath = UserPhotoManager.GetMediumPhotoURL(new Guid());

            if (!String.IsNullOrEmpty(context.Request["id"]))
            {
                imagepath = UserPhotoManager.GetMediumPhotoURL(CoreContext.UserManager.GetUserByUserName(context.Request["id"].Split('@')[0]).ID);
            }

            var host = new Uri(CommonLinkUtility.ServerRootPath).Host;
            if (String.Equals(context.Request["id"], host, StringComparison.InvariantCultureIgnoreCase))
            {
                imagepath = WebImageSupplier.GetAbsoluteWebPath("teamlab.png", TalkAddon.AddonID);
            }

            context.Response.Redirect(imagepath, false);
            context.Response.StatusCode = 301;

            //try
            //{
            //  HttpWebRequest HWRequest = (HttpWebRequest)HttpWebRequest.Create(CommonLinkUtility.GetFullAbsolutePath(imagepath));
            //  HttpWebResponse HWResponse = (HttpWebResponse)HWRequest.GetResponse();
            //  HWResponse.GetResponseStream().StreamCopyTo(context.Response.OutputStream);
            //  context.Response.Cache.SetCacheability(HttpCacheability.Public);
            //  context.Response.ContentType = HWResponse.ContentType;
            //  context.Response.BufferOutput = false;
            //}
            //catch (Exception)
            //{
            //  context.Response.ContentType = "text/html";
            //  context.Response.BufferOutput = false;
            //  context.Response.StatusCode = 404;
            //  context.Response.End();
            //}
        }
    }
}
