/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Web.UserControls.Bookmarking.Util;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace ASC.Web.Community.HttpHandlers
{
    public class ThumbHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!SecurityContext.IsAuthenticated || !ThumbnailHelper.HasService 
                || !context.Request.QueryString.AllKeys.Contains("url")) {

                ProccessFail(context);
                return;
            }

            var url = context.Request.QueryString["url"];
            url = url.Replace("&amp;", "&");
            url = System.Net.WebUtility.UrlEncode(url);

            try
            {
                using (var wc = new WebClient())
                {
                    var bytes = wc.DownloadData(string.Format(ThumbnailHelper.ServiceUrl, url));
                    context.Response.ContentType = wc.ResponseHeaders["Content-Type"] ?? "image/png";
                    context.Response.BinaryWrite(bytes);
                    context.Response.Flush();
                }
            }
            catch
            {
                ProccessFail(context);
            }
        }

        private void ProccessFail(HttpContext context)
        {
            context.Response.Redirect("~/Products/Community/Modules/Bookmarking/App_Themes/default/images/noimageavailable.jpg");
        }
    }
}
