﻿/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Linq;
using System.Net;
using System.Web;

using ASC.Core;
using ASC.Web.UserControls.Bookmarking.Util;

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
                || !context.Request.QueryString.AllKeys.Contains("url"))
            {

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
