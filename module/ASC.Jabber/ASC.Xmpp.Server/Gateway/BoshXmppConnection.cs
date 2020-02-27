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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using ASC.Common.Logging;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.extensions.bosh;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Utils;
using ASC.Xmpp.Server.Storage;
using Uri = ASC.Xmpp.Core.protocol.Uri;

namespace ASC.Xmpp.Server.Gateway
{
    public class BoshXmppConnection : IXmppConnection
    {
        private const int defaultTimeout = 60; // seconds

        private static readonly ILog log = LogManager.GetLogger("ASC");

        private readonly object locker = new object();
        private readonly TimeSpan waitTimeout;
        private readonly TimeSpan inactivityTimeout;
        private int closed;
        private BoshXmppRequest currentRequest;
        private readonly List<Node> sendBuffer = new List<Node>();
        private DateTime lastRequestTime;


        public string Id { get; private set; }


        public event EventHandler<XmppStreamStartEventArgs> XmppStreamStart = delegate { };

        public event EventHandler<XmppStreamEndEventArgs> XmppStreamEnd = delegate { };

        public event EventHandler<XmppStreamEventArgs> XmppStreamElement = delegate { };

        public event EventHandler<XmppConnectionCloseEventArgs> Closed = delegate { };


        public BoshXmppConnection(Body body)
        {
            Id = UniqueId.CreateNewId();

            waitTimeout = TimeSpan.FromSeconds(0 < body.Wait ? body.Wait : defaultTimeout);
            inactivityTimeout = TimeSpan.FromSeconds(0 < body.Inactivity ? body.Inactivity : defaultTimeout);
        }


        public void BeginReceive()
        {
            IdleWatcher.StartWatch(Id, waitTimeout, OnWait);
        }

        public void Reset()
        {
            // nothing to do
        }

        public void Send(string text, Encoding encoding)
        {
            // ignore, send only node
        }

        public void Send(Node node, Encoding encoding)
        {
            lock (locker)
            {
                if (node != null)
                {
                    sendBuffer.Add(node);
                }
                if (currentRequest != null && sendBuffer.Any())
                {
                    try
                    {
                        var terminate = Volatile.Read(ref closed) == 1;
                        currentRequest.SendAndClose(sendBuffer, terminate);
                        sendBuffer.Clear();
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("Connection {0} Error send buffer: {1}", Id, e);
                    }
                    currentRequest = null;
                }
            }
        }

        public void Close()
        {
            if (Interlocked.CompareExchange(ref closed, 1, 0) == 0)
            {
                log.DebugFormat("Close connection {0}", Id);

                CloseRequest(true);

                try
                {
                    XmppStreamEnd(this, new XmppStreamEndEventArgs(Id, sendBuffer));
                }
                catch { }
                finally
                {
                    sendBuffer.Clear();
                }

                try
                {
                    Closed(this, new XmppConnectionCloseEventArgs());
                }
                catch { }
            }
        }

        public void ProcessBody(Body body, HttpListenerContext ctx)
        {
            log.DebugFormat("Start process body connection {0}", Id);
            lock (locker)
            {
                CloseRequest(false);

                lastRequestTime = DateTime.Now;
                currentRequest = new BoshXmppRequest(Id, body, ctx);
            }
            if (body.HasChildElements)
            {
                if (body.FirstChild.GetAttribute("type") == "notification")
                {
                    //сonfiguring store and write to data base
                    DbPushStore dbPushStore = new DbPushStore();
                    var properties = new Dictionary<string, string>(1);
                    properties.Add("connectionStringName", "default");
                    dbPushStore.Configure(properties);
                    dbPushStore.SaveUserEndpoint(
                        body.FirstChild.GetAttribute("username"), 
                        body.FirstChild.GetAttribute("endpoint"),
                        body.FirstChild.GetAttribute("browser"));
                }
            }
            
            if (body.Type == BoshType.terminate)
            {
                Close();
                return;
            }

            if (Volatile.Read(ref closed) == 1)
            {
                CloseRequest(true);
                return;
            }
           
            IdleWatcher.UpdateTimeout(Id, waitTimeout);

            if (string.IsNullOrEmpty(body.Sid) || body.XmppRestart)
            {
                var stream = new Stream
                {
                    Prefix = Uri.PREFIX,
                    Namespace = Uri.STREAM,
                    Version = body.Version,
                    Language = body.GetAttribute("lang"),
                    To = body.To,
                };
                XmppStreamStart(this, new XmppStreamStartEventArgs(Id, stream, Uri.CLIENT));
            }
            foreach (var element in body.ChildNodes.OfType<Element>())
            {
                XmppStreamElement(this, new XmppStreamEventArgs(Id, element));
            }

            Send((Node)null, null); // try to send a non-empty buffer
        }

        private void OnWait(object _, TimeoutEventArgs a)
        {
            try
            {
                lock (locker)
                {
                    if (currentRequest != null)
                    {
                        log.DebugFormat("Close request {0}", Id);
                        currentRequest.Close(false);
                        currentRequest = null;
                    }
                }
            }
            finally
            {
                if (lastRequestTime + waitTimeout + inactivityTimeout < DateTime.Now)
                {
                    Close();
                }
                else
                {
                    IdleWatcher.StartWatch(Id, waitTimeout, OnWait);
                }
            }
        }

        private void CloseRequest(bool terminate)
        {
            lock (locker)
            {
                if (currentRequest != null)
                {
                    currentRequest.Close(terminate);
                    currentRequest = null;
                }
            }
            log.DebugFormat("Close request {0}{1}", Id, terminate ? " with termination" : "");
        }
    }
}