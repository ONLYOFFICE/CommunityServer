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


using log4net;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Configuration;

namespace ASC.Web.Talk.HttpHandlers
{
    public class HttpPollHandler : HttpTaskAsyncHandler
    {
        private static readonly Uri boshUri = new Uri(VirtualPathUtility.AppendTrailingSlash(WebConfigurationManager.AppSettings["BoshPath"] ?? "http://localhost:5280/http-poll/"));
           
        private void CopyStream(Stream from, Stream to)
        {
            var buffer = new byte[1024];
            while (true)
            {
                var read = from.Read(buffer, 0, buffer.Length);
                if (read == 0) break;

                to.Write(buffer, 0, read);
            }
        }

        public override async System.Threading.Tasks.Task ProcessRequestAsync(HttpContext context)
        {
            var request = (HttpWebRequest)WebRequest.Create(boshUri);
            request.Method = context.Request.HttpMethod;

            request.UserAgent = context.Request.UserAgent;
            request.Accept = string.Join(",", context.Request.AcceptTypes);
            if (!string.IsNullOrEmpty(context.Request.Headers["Accept-Encoding"]))
            {
                request.Headers["Accept-Encoding"] = context.Request.Headers["Accept-Encoding"];
            }
            request.ContentType = context.Request.ContentType;
            request.ContentLength = context.Request.ContentLength;

            var stream = await request.GetRequestStreamAsync();

            using (var writer = new StreamWriter(stream))
            {
                CopyStream(context.Request.InputStream, stream);

                writer.Flush();
                writer.Dispose();
            }

            var response = await request.GetResponseAsync();

            context.Response.ContentType = response.ContentType;

            // copy headers & body
            foreach (string h in response.Headers)
            {
                context.Response.AppendHeader(h, response.Headers[h]);
            }

            using (var respStream = response.GetResponseStream())
            {
                CopyStream(respStream, context.Response.OutputStream);               
            }
        }
    }
       
}
