/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Xmpp.Core.protocol.extensions.bosh;
using ASC.Xmpp.Core.utils;
using log4net;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;

namespace ASC.Xmpp.Server.Gateway
{
    static class BoshXmppHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BoshXmppHelper));

        public static Body ReadBodyFromRequest(HttpListenerContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");

            try
            {
                if (!ctx.Request.HasEntityBody) return null;

                string body;
                using (var streamReader = new StreamReader(ctx.Request.InputStream))
                {
                    body = streamReader.ReadToEnd();
                }

                return ElementSerializer.DeSerializeElement<Body>(body);
            }
            catch (Exception e)
            {
                if (e is HttpListenerException || e is ObjectDisposedException)
                {
                    // ignore
                }
                else
                {
                    log.ErrorFormat("Error read body from request: {0}", e);
                }
            }
            return null;
        }

        public static void TerminateBoshSession(HttpListenerContext ctx, string condition)
        {
            TerminateBoshSession(ctx, null, condition);
        }

        public static void TerminateBoshSession(HttpListenerContext ctx, Body body)
        {
            TerminateBoshSession(ctx, body, null);
        }

        private static void TerminateBoshSession(HttpListenerContext ctx, Body body, string condition)
        {
            if (ctx == null || ctx.Response == null) return;

            if (body == null) body = new Body();
            try
            {
                body.Type = BoshType.terminate;
                if (!string.IsNullOrEmpty(condition)) body.SetAttribute("condition", condition);

                SendAndCloseResponse(ctx, body);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error TerminateBoshSession body: {0}\r\n{1}", body, e);
            }
        }

        public static void SendAndCloseResponse(HttpListenerContext ctx, Body body)
        {
            var response = ctx.Response;
            try
            {
                var text = body.ToString();

                response.ContentType = "text/xml; charset=utf-8";
                var buffer = Encoding.UTF8.GetBytes(text);

                var headerValues = ctx.Request.Headers.GetValues("Accept-Encoding");
                if (headerValues != null && headerValues.Contains("gzip", StringComparer.InvariantCultureIgnoreCase))
                {
                    response.AddHeader("Content-Encoding", "gzip");
                    using (var ms = new MemoryStream())
                    {
                        using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                        {
                            gzip.Write(buffer, 0, buffer.Length);
                        }
                        buffer = ms.ToArray();
                    }
                }

                response.ContentLength64 = buffer.Length;
                response.Headers.Add(HttpResponseHeader.CacheControl, "no-cache");
                response.Headers.Add(HttpResponseHeader.Vary, "*");
                response.Headers.Add(HttpResponseHeader.Pragma, "no-cache");
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Flush();
            }
            finally
            {
                try
                {
                    response.Close();
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}