/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.extensions.bosh;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Utils;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using Uri = ASC.Xmpp.Core.protocol.Uri;

namespace ASC.Xmpp.Server.Gateway
{
    public class BoshXmppConnection : IXmppConnection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BoshXmppConnection));

        private TimeSpan waitPeriod = TimeSpan.FromSeconds(60);

        private TimeSpan inactivityPeriod = TimeSpan.FromSeconds(60);

        private long rid;

        private int window = 5;

        private ConcurrentQueue<Node> sendBuffer = new ConcurrentQueue<Node>();

        private readonly object wait = new object();

        private int closed;


        public BoshXmppConnection()
        {
            Id = UniqueId.CreateNewId();
            IdleWatcher.StartWatch(Id, inactivityPeriod, IdleTimeout);
            log.DebugFormat("Create new Bosh connection Id = {0}", Id);
        }

        #region IXmppConnection Members

        public string Id
        {
            get;
            private set;
        }

        public void Reset()
        {
            log.DebugFormat("Reset connection {0}", Id);
        }

        public void Close()
        {
            if (Interlocked.CompareExchange(ref closed, 1, 0) == 0)
            {
                lock (wait)
                {
                    Monitor.PulseAll(wait);
                }
                IdleWatcher.StopWatch(Id);

                try
                {
                    var handler = XmppStreamEnd;
                    if (handler != null) XmppStreamEnd(this, new XmppStreamEndEventArgs(Id, sendBuffer));
                }
                catch { }
                finally
                {
                    sendBuffer = new ConcurrentQueue<Node>();
                }

                try
                {
                    var handler = Closed;
                    if (handler != null) handler(this, new XmppConnectionCloseEventArgs());
                }
                catch { }

                log.DebugFormat("Close connection {0}", Id);
            }
        }

        public void Send(Node node, Encoding encoding)
        {
            var text = node.ToString();
            if (log.IsDebugEnabled) log.DebugFormat("Add node {0} to send buffer connection {1}", 200 < text.Length ? text.Substring(0, 195) + " ... " : text, Id);

            sendBuffer.Enqueue(node);
            lock (wait)
            {
                Monitor.PulseAll(wait);
            }
        }

        public void Send(string text, Encoding encoding)
        {
            log.DebugFormat("Ignore send text connection {0}", Id);
        }

        public void BeginReceive()
        {

        }

        public event EventHandler<XmppStreamStartEventArgs> XmppStreamStart;

        public event EventHandler<XmppStreamEndEventArgs> XmppStreamEnd;

        public event EventHandler<XmppStreamEventArgs> XmppStreamElement;

        public event EventHandler<XmppConnectionCloseEventArgs> Closed;

        #endregion

        public void ProcessBody(Body body, HttpListenerContext ctx)
        {
            try
            {
                IdleWatcher.UpdateTimeout(Id, TimeSpan.MaxValue);

                if (body == null) throw new ArgumentNullException("body");
                if (ctx == null) throw new ArgumentNullException("httpContext");

                log.DebugFormat("Start process body connection {0}\r\n{1}\r\n", Id, body);

                if (!ValidateBody(body))
                {
                    BoshXmppHelper.TerminateBoshSession(ctx, "bad-request");
                    return;
                }
                if (body.Type == BoshType.terminate)
                {
                    Close();
                    ctx.Response.Close();
                    return;
                }

                ReadBodyHeaders(body);

                if (string.IsNullOrEmpty(body.Sid) || body.XmppRestart)
                {
                    InvokeStreamStart(body);
                }
                foreach (var node in body.ChildNodes)
                {
                    if (node is Element) InvokeStreamElement((Element)node);
                }

                WriteBodyHeaders(body);

                log.DebugFormat("Connection {0} WAIT ...", Id);

                var waitResult = WaitAnswerOrDrop();

                if (waitResult == WaitResult.Success)
                {
                    log.DebugFormat("Connection {0} send answer", Id);
                    SendAnswer(body, ctx);
                }
                else if (waitResult == WaitResult.Timeout)
                {
                    log.DebugFormat("Connection {0} drop by timeout", Id);
                    BoshXmppHelper.SendAndCloseResponse(ctx, new Body().ToString());
                }
                else
                {
                    log.DebugFormat("Connection {0} terminate", Id);
                    BoshXmppHelper.TerminateBoshSession(ctx, body);
                }
            }
            finally
            {
                IdleWatcher.UpdateTimeout(Id, inactivityPeriod);
            }
        }

        private bool ValidateBody(Body body)
        {
            if (0 < body.Window) window = body.Window;
            if (0 < body.Polling) window = body.Polling;

            if (Volatile.Read(ref closed) == 1) return false;
            if (rid != 0 && window < Math.Abs(body.Rid - rid)) return false;
            return true;
        }

        private void ReadBodyHeaders(Body body)
        {
            rid = body.Rid;
            if (0 < body.Wait) waitPeriod = TimeSpan.FromSeconds(body.Wait);
            if (0 < body.Inactivity) inactivityPeriod = TimeSpan.FromSeconds(body.Inactivity);
        }

        private Body WriteBodyHeaders(Body body)
        {
            if (string.IsNullOrEmpty(body.Sid))
            {
                body.Sid = Id;
                body.Secure = false;
            }
            body.Ack = body.Rid != 0 ? body.Rid : rid;
            body.RemoveAttribute("rid");
            body.To = null;
            if (body.HasAttribute("xmpp:version") || body.HasAttribute("xmpp:restart"))
            {
                body.SetAttribute("xmlns:xmpp", "urn:xmpp:xbosh");
            }
            body.RemoveAllChildNodes();
            return body;
        }

        private WaitResult WaitAnswerOrDrop()
        {
            lock (wait)
            {
                Monitor.PulseAll(wait);
                if (!sendBuffer.IsEmpty)
                {
                    return WaitResult.Success;
                }
                Monitor.Wait(wait, waitPeriod);
            }

            if (Volatile.Read(ref closed) == 1)
            {
                // connection close
                return WaitResult.Terminate;
            }

            if (!sendBuffer.IsEmpty)
            {
                // connection not closed and need to send message
                return WaitResult.Success;
            }

            return WaitResult.Timeout; // ok, timeout, continue polling
        }

        private void SendAnswer(Body body, HttpListenerContext ctx)
        {
            try
            {
                var copy = Interlocked.Exchange(ref sendBuffer, new ConcurrentQueue<Node>());
                foreach (var node in copy)
                {
                    body.AddChild(node);
                    if (node.Namespace == Uri.STREAM) body.SetAttribute("xmlns:stream", Uri.STREAM);
                }

                if (Volatile.Read(ref closed) == 1) body.Type = BoshType.terminate;
                BoshXmppHelper.SendAndCloseResponse(ctx, body.ToString(), true, null);

                log.DebugFormat("Connection {0} Send buffer:\r\n{1}", Id, body);
            }
            catch (Exception e)
            {
                //BUG: Я думаю баг тут из за обрыва соединения при говенном кач-ве соединения.
                log.DebugFormat("Connection {0} Error send buffer: {1}", Id, e);
                Close();
            }
        }

        private void InvokeStreamStart(Body body)
        {
            var stream = new Stream();
            stream.Prefix = Uri.PREFIX;
            stream.Namespace = Uri.STREAM;
            stream.Version = body.Version;
            stream.Language = body.GetAttribute("lang");
            stream.To = body.To;

            var handler = XmppStreamStart;
            if (handler != null) handler(this, new XmppStreamStartEventArgs(Id, stream, Uri.CLIENT));
        }

        private void InvokeStreamElement(Element element)
        {
            var handler = XmppStreamElement;
            if (handler != null) handler(this, new XmppStreamEventArgs(Id, element));
        }

        private void IdleTimeout(object sender, TimeoutEventArgs e)
        {
            if (!Id.Equals(e.IdleObject)) return;

            log.DebugFormat("Close connection {0} by inactivity timeout.", Id);
            Close();
        }

        private enum WaitResult
        {
            Success,
            Terminate,
            Timeout
        }
    }
}