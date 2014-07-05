/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Streams;
using log4net;
using Error = ASC.Xmpp.Core.protocol.Error;
using Stanza = ASC.Xmpp.Core.protocol.Base.Stanza;
using StanzaError = ASC.Xmpp.Core.protocol.client.Error;
using System.Configuration;

namespace ASC.Xmpp.Server.Handler
{
    public class XmppHandlerManager
    {
        private readonly XmppStreamManager streamManager;

        private readonly IXmppSender sender;

        private readonly XmppHandlerContext context;

        private readonly XmppStreamValidator validator;

        private readonly XmppXMLSchemaValidator schemaValidator;

        private static readonly ILog log = LogManager.GetLogger(typeof(XmppHandlerManager));

        private static readonly ILog logMessages = LogManager.GetLogger("ASC.Xmpp.Server.Messages");

        private const string RECIEVE_FORMAT = "Xmpp stream: connection {0}, namespace {1}\r\n\r\n(C) <<--------------------------------------\r\n{2}\r\n";

        // for migration from teamlab.com to onlyoffice.com
        private static string fromTeamlabToOnlyOffice = ConfigurationManager.AppSettings["jabber.from-teamlab-to-onlyoffice"] ?? "true";
        private static string fromServerInJid = ConfigurationManager.AppSettings["jabber.from-server-in-jid"] ?? "teamlab.com";
        private static string toServerInJid = ConfigurationManager.AppSettings["jabber.to-server-in-jid"] ?? "onlyoffice.com";

        public XmppHandlerStorage HandlerStorage
        {
            get;
            private set;
        }

        public XmppHandlerManager(XmppStreamManager streamManager, IXmppReceiver receiver, IXmppSender sender, IServiceProvider serviceProvider)
        {
            if (streamManager == null) throw new ArgumentNullException("streamManager");
            if (receiver == null) throw new ArgumentNullException("receiver");
            if (sender == null) throw new ArgumentNullException("sender");
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            this.streamManager = streamManager;
            this.sender = sender;
            this.HandlerStorage = new XmppHandlerStorage(serviceProvider);
            this.context = new XmppHandlerContext(serviceProvider);

            this.validator = new XmppStreamValidator();
            this.schemaValidator = new XmppXMLSchemaValidator();

            receiver.XmppStreamStart += XmppStreamStart;
            receiver.XmppStreamElement += XmppStreamElement;
            receiver.XmppStreamEnd += XmppStreamEnd;
        }

        public void AddXmppHandler(Jid address, IXmppHandler handler)
        {
            HandlerStorage.AddXmppHandler(address, handler);
        }

        public void RemoveXmppHandler(IXmppHandler handler)
        {
            HandlerStorage.RemoveXmppHandler(handler);
        }

        public void ProcessStreamStart(Node node, string ns, XmppStream xmppStream)
        {
            try
            {
                var stream = node as Stream;
                if (stream == null)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.BadFormat);
                    return;
                }
                if (!stream.HasTo)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.ImproperAddressing);//TODO: Return something more correct^)
                    return;
                }
                // for migration from teamlab.com to onlyoffice.com
                if (stream.To.Server.EndsWith(toServerInJid))
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.HostGone);
                    return;
                }
                if (!stream.To.IsServer)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.ImproperAddressing);
                    return;
                }
                var handlers = HandlerStorage.GetStreamStartHandlers(stream.To);
                if (handlers.Count == 0)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.HostUnknown);
                    return;
                }

                var handler = handlers.Find(h => h.Namespace == ns);
                if (handler == null)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.BadNamespacePrefix);
                    return;
                }

                xmppStream.Namespace = ns;
                xmppStream.Domain = stream.To.Server;
                xmppStream.Language = stream.Language;

                handler.StreamStartHandle(xmppStream, stream, context);
            }
            catch (Exception ex)
            {
                ProcessException(ex, node, xmppStream);
            }
        }

        public void ProcessStreamElement(Node node, XmppStream stream)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (stream == null) throw new ArgumentNullException("stream");

            try
            {
                schemaValidator.ValidateNode(node, stream, context);

                var address = new Jid(stream.Domain);

                foreach (var handler in HandlerStorage.GetStreamHandlers(address, node.GetType()))
                {
                    handler.ElementHandle(stream, (Element)node.Clone(), context);
                }

                if (node is Stanza)
                {
                    var stanza = (Stanza)node;

                    if (!validator.ValidateStanza(stanza, stream, context)) return;

                    if (stanza.HasTo) address = stanza.To;

                    var handlres = HandlerStorage.GetStanzaHandlers(address, GetStanzaType(stanza));
                    if (handlres.Count == 0)
                    {
                        if (stanza is IQ)
                        {
                            var iq = (IQ)stanza;
                            if ((iq.Type == IqType.error || iq.Type == IqType.result) && iq.HasTo && iq.To.HasUser)
                            {
                                //result and error retranslate to user
                                var session = context.SessionManager.GetSession(iq.To);
                                if (session != null)
                                {
                                    sender.SendTo(session, iq);
                                    return;
                                }
                            }
                            //result and error ignored by server
                        }
                        sender.SendTo(stream, XmppStanzaError.ToServiceUnavailable(stanza));
                        log.DebugFormat("Stanza handler not found for address '{0}'", address);
                        return;
                    }

                    bool iqHandled = true;
                    Stopwatch stopwatch = null;

                    foreach (var handler in handlres)
                    {
                        if (log.IsDebugEnabled)
                        {
                            stopwatch = Stopwatch.StartNew();
                        }

                        if (stanza is IQ)
                        {
                            var answer = handler.HandleIQ(stream, (IQ)stanza.Clone(), context);
                            if (answer != null)
                            {
                                sender.SendTo(stream, answer);
                                iqHandled = answer.Id == stanza.Id;
                            }
                        }
                        else if (stanza is Message)
                        {
                            handler.HandleMessage(stream, (Message)stanza.Clone(), context);
                        }
                        else if (stanza is Presence)
                        {
                            handler.HandlePresence(stream, (Presence)stanza.Clone(), context);
                        }
                        else
                        {
                            sender.SendTo(stream, XmppStanzaError.ToNotAcceptable(stanza));
                            return;
                        }

                        if (log.IsDebugEnabled)
                        {
                            stopwatch.Stop();
                            log.DebugFormat("Process stanza handler '{1}' on address '{0}', time: {2}ms", address, handler.GetType().FullName, stopwatch.Elapsed.TotalMilliseconds);
                        }
                    }
                    if (!iqHandled)
                    {
                        sender.SendTo(stream, XmppStanzaError.ToServiceUnavailable(stanza));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessException(ex, node, stream);
            }
        }

        public void ProcessStreamEnd(ICollection<Node> notSendedBuffer, XmppStream stream, string connectionId)
        {
            if (notSendedBuffer == null) throw new ArgumentNullException("notSendedBuffer");
            if (stream != null)
            {
                foreach (var session in context.SessionManager.GetStreamSessions(stream.Id))
                {
                    context.SessionManager.CloseSession(session.Jid);
                }

                foreach (var handler in HandlerStorage.GetStreamHandlers(stream.Domain))
                {
                    try
                    {
                        handler.StreamEndHandle(stream, notSendedBuffer, context);
                    }
                    catch (Exception) { }
                }
                streamManager.RemoveStream(stream.ConnectionId);
            }
            var connection = sender.GetXmppConnection(connectionId);
            if (connection != null) connection.Close();
        }


        private void XmppStreamStart(object sender, XmppStreamStartEventArgs e)
        {
            try
            {
                if (logMessages.IsDebugEnabled)
                {
                    logMessages.DebugFormat(RECIEVE_FORMAT, e.ConnectionId, e.Namespace, e.Node.ToString(Formatting.Indented));
                }

                var xmppStream = streamManager.GetOrCreateNewStream(e.ConnectionId);
                ProcessStreamStart(e.Node, e.Namespace, xmppStream);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error process stream start: {0}", ex);
            }
        }

        private void XmppStreamElement(object sender, XmppStreamEventArgs e)
        {
            try
            {
                if (logMessages.IsDebugEnabled)
                {
                    logMessages.DebugFormat(RECIEVE_FORMAT, e.ConnectionId, string.Empty, e.Node.ToString(Formatting.Indented));
                }

                var stream = streamManager.GetStream(e.ConnectionId);
                if (stream == null) return;
                ProcessStreamElement(e.Node, stream);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error process stream element: {0}", ex);
            }
        }

        private void XmppStreamEnd(object sender, XmppStreamEndEventArgs e)
        {
            try
            {
                log.DebugFormat("Xmpp stream end: connection {0}, not sended elements count {1}", e.ConnectionId, e.NotSendedBuffer.Count);

                var stream = streamManager.GetStream(e.ConnectionId);
                ProcessStreamEnd(e.NotSendedBuffer, stream, e.ConnectionId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error process stream end: {0}", ex);
            }
        }

        private Type GetStanzaType(Stanza stanza)
        {
            var iq = stanza as IQ;
            if (iq == null) return stanza.GetType();

            Element iqInfo = iq.Query ?? (iq.Bind ?? (iq.Vcard ?? ((Element)iq.Session ?? (Element)iq.Blocklist)));
            if (iqInfo != null) return iqInfo.GetType();

            foreach (var child in iq.ChildNodes)
            {
                if (child is Element) return child.GetType();
            }
            return stanza.GetType();
        }

        private void ProcessException(Exception ex, Node node, XmppStream stream)
        {
            if (ex is JabberException)
            {
                var je = (JabberException)ex;
                var error = je.ToElement();

                if (je.ErrorCode != ErrorCode.Forbidden)
                {
                    log.Warn("JabberError", ex);
                }
                if (je.StreamError)
                {
                    ((Error)error).Text = je.Message;
                    sender.SendTo(stream, error);
                }
                else
                {
                    if (node is Stanza && error is StanzaError)
                    {
                        sender.SendTo(stream, XmppStanzaError.ToErrorStanza((Stanza)node, (StanzaError)error));
                    }
                    else
                    {
                        var streamError = XmppStreamError.InternalServerError;
                        streamError.Text = "Stanza error in stream.";
                        sender.SendToAndClose(stream, streamError);
                    }
                }

                if (je.CloseStream) sender.CloseStream(stream);
            }
            else
            {
                log.Error("InternalServerError", ex);
                var error = XmppStreamError.InternalServerError;
                error.Text = ex.Message;
                sender.SendToAndClose(stream, error);
            }
        }
    }
}