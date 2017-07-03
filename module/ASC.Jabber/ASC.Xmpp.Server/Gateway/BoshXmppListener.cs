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
using System.Collections.Generic;
using System.Net;

namespace ASC.Xmpp.Server.Gateway
{
    class BoshXmppListener : XmppListenerBase
    {
        private const string DEFAULT_BIND_URL = "http://*:5280/http-poll/";

        private readonly HttpListener httpListener = new HttpListener();

        private Uri bindUri;
        private long maxPacket = 1048576; //1024 kb

        private static readonly ILog log = LogManager.GetLogger(typeof(BoshXmppListener));


        public override void Configure(IDictionary<string, string> properties)
        {
            try
            {
                string hostname = Dns.GetHostName().ToLowerInvariant();

                string bindPrefix = properties.ContainsKey("bind") ? properties["bind"] : DEFAULT_BIND_URL;
                bindUri = new Uri(bindPrefix.Replace("*", hostname));

                httpListener.Prefixes.Add(bindPrefix);

                log.InfoFormat("Configure listener {0} on {1}", Name, bindPrefix);

                if (properties.ContainsKey("maxpacket"))
                {
                    var value = 0L;
                    if (long.TryParse(properties["maxpacket"], out value)) maxPacket = value;
                }
            }
            catch (Exception e)
            {
                log.DebugFormat("Error configure listener {0}: {1}", Name, e);
                throw;
            }
        }

        protected override void DoStart()
        {
            httpListener.Start();
            BeginGetContext();
        }

        protected override void DoStop()
        {
            httpListener.Stop();
        }

        private void BeginGetContext()
        {
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.BeginGetContext(GetContextCallback, null);
            }
        }

        private void GetContextCallback(IAsyncResult asyncResult)
        {
            HttpListenerContext ctx = null;
            try
            {
                try
                {
                    ctx = httpListener.EndGetContext(asyncResult);
                }
                finally
                {
                    BeginGetContext();
                }

                if (maxPacket < ctx.Request.ContentLength64)
                {
                   
                    BoshXmppHelper.TerminateBoshSession(ctx, "request-too-large");
                    return;
                }

                if (ctx.Request.Url.AbsolutePath == bindUri.AbsolutePath)
                {
                    var body = BoshXmppHelper.ReadBodyFromRequest(ctx);
                    if (body == null)
                    {
                        BoshXmppHelper.TerminateBoshSession(ctx, "bad-request");
                        return;
                    }

                    var connection = GetXmppConnection(body.Sid) as BoshXmppConnection;

                    if (!string.IsNullOrEmpty(body.Sid) && connection == null)
                    {
                        BoshXmppHelper.TerminateBoshSession(ctx, "item-not-found");
                        return;
                    }

                    if (connection == null)
                    {
                        connection = new BoshXmppConnection(body);
                        log.DebugFormat("Create new Bosh connection Id = {0}", connection.Id);

                        AddNewXmppConnection(connection);
                    }
                    connection.ProcessBody(body, ctx);
                }
                else
                {
                    BoshXmppHelper.TerminateBoshSession(ctx, "bad-request");
                    log.DebugFormat("{0}: Unknown uri request {1}", Name, ctx.Request.Url);
                }
            }
            catch (Exception e)
            {
                BoshXmppHelper.TerminateBoshSession(ctx, "internal-server-error");
                if (Started && !(e is ObjectDisposedException))
                {
                    log.ErrorFormat("{0}: Error GetContextCallback: {1}", Name, e);
                }
            }
        }
    }
}