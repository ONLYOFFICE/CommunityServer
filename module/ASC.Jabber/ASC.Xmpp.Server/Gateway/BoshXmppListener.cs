/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using log4net;
using ASC.Xmpp.Server.Utils;

namespace ASC.Xmpp.Server.Gateway
{
    class BoshXmppListener : XmppListenerBase
    {
        private const string DEFAULT_BIND_URL = "http://*:5280/http-poll/";
        private const string DEFAULT_POLICY_FILE = "crossdomain.xml";

        private readonly HttpListener httpListener = new HttpListener();

        private System.Uri bindUri;
        private System.Uri domainUri;

        private string policyFile = DEFAULT_POLICY_FILE;
        private string policy = string.Empty;
        private bool policyLoaded = false;
        private long maxPacket = 524288;//512 kb

        private static readonly ILog log = LogManager.GetLogger(typeof(BoshXmppListener));


        public override void Configure(IDictionary<string, string> properties)
        {
            try
            {
                string hostname = Dns.GetHostName().ToLowerInvariant();

                string bindPrefix = properties.ContainsKey("bind") ? properties["bind"] : DEFAULT_BIND_URL;
                bindUri = new System.Uri(bindPrefix.Replace("*", hostname));

                string policyPrefix = properties.ContainsKey("policy") ? properties["policy"] : bindUri.Scheme + "://*:" + bindUri.Port;
                domainUri = new System.Uri(policyPrefix.Replace("*", hostname));

                if (policyPrefix.Contains(".")) policyPrefix = policyPrefix.Substring(0, policyPrefix.LastIndexOf("/"));
                if (!policyPrefix.EndsWith("/")) policyPrefix += "/";

                httpListener.Prefixes.Add(bindPrefix);
                httpListener.Prefixes.Add(policyPrefix);

                log.InfoFormat("Configure listener {0} on {1}", Name, bindPrefix);
                log.InfoFormat("Configure policy {0} on {1}", Name, policyPrefix);

                BoshXmppHelper.CompressResponse = properties.ContainsKey("compress") ? bool.Parse(properties["compress"]) : true;

                if (properties.ContainsKey("policyFile")) policyFile = properties["policyFile"];
                policyFile = PathUtils.GetAbsolutePath(policyFile);

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

                var url = ctx.Request.Url;
                log.DebugFormat("{0}: Begin process http request {1}", Name, url);

                if (url.AbsolutePath == bindUri.AbsolutePath)
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
                        connection = new BoshXmppConnection();
                        AddNewXmppConnection(connection);
                    }

                    connection.ProcessBody(body, ctx);
                }
                else if ((url.AbsolutePath == domainUri.AbsolutePath || url.AbsolutePath == "/crossdomain.xml") && ctx.Request.HttpMethod == "GET")
                {
                    SendPolicy(ctx);
                }
                else
                {
                    BoshXmppHelper.TerminateBoshSession(ctx, "bad-request");
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                if (ctx != null) BoshXmppHelper.TerminateBoshSession(ctx, "internal-server-error");
                if (Started) log.ErrorFormat("{0}: Error GetContextCallback: {1}", Name, e);
            }
        }

        private void SendPolicy(HttpListenerContext ctx)
        {
            log.DebugFormat("{0}: Send policy.", Name);

            if (!policyLoaded)
            {
                try
                {
                    policy = File.ReadAllText(policyFile);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Can not load policy file: {0}, error: {1}", policyFile, ex);
                }
                policyLoaded = true;
            }
            BoshXmppHelper.SendAndCloseResponse(ctx, policy);
        }
    }
}