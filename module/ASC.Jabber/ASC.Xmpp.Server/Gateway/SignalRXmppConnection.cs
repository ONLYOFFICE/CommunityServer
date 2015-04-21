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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x.muc;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASC.Xmpp.Server.Gateway
{
    public class SignalRXmppConnection : IXmppConnection
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(BoshXmppConnection));
        private static readonly SignalrServiceClient signalrServiceClient = new SignalrServiceClient();
        private static readonly TimeSpan _inactivityPeriod = TimeSpan.FromSeconds(310);
        private XmppServer _xmppServer;

        public SignalRXmppConnection(string connectionId, XmppServer xmppServer)
        {
            Id = connectionId;
            _xmppServer = xmppServer;
            IdleWatcher.StopWatch(Id);
            IdleWatcher.StartWatch(Id, _inactivityPeriod, IdleTimeout);
            _log.DebugFormat("Create new SignalR connection Id = {0}.", Id);
        }

        public string Id
        {
            get;
            private set;
        }

        public void Reset()
        {
            _log.DebugFormat("Reset connection {0}.", Id);
        }

        public void Close()
        {
            _log.DebugFormat("Close connection {0}.", Id);
            ((SignalRXmppListener)((XmppGateway)_xmppServer.GetService(typeof(IXmppReceiver))).GetXmppListener("SignalR Listener")).CloseXmppConnection(Id);
        }

        public void Send(Node node, Encoding encoding)
        {
            var elem = node as DirectionalElement;

            if (elem != null && elem.To != null)
            {
                var type = node.GetType();

                if (type == typeof(Message))
                {
                    try
                    {
                        string nameFrom = elem.From.User.ToLowerInvariant();
                        var message = (Message)node;

                        if (message.Body != null)
                        {
                            signalrServiceClient.SendMessage(nameFrom, message.To.User.ToLowerInvariant(),
                                message.Body, -1, elem.From.Server);
                        }
                        else if (message.FirstChild.HasTag(typeof(Invite)))
                        {
                            signalrServiceClient.SendInvite(nameFrom, message.To.User.ToLowerInvariant(), elem.To.Server);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorFormat("Unexpected error, connectionId = {0}, {1}, {2}, {3}", Id,
                            ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                    }
                }
            }
        }

        public void Send(string text, Encoding encoding)
        {
            _log.DebugFormat("Ignore send text connection {0}.", Id);
        }

        public void BeginReceive()
        {
        }

        public void UpdateTimeout()
        {
            _log.DebugFormat("Update timeout of connection {0}.", Id);
            IdleWatcher.UpdateTimeout(Id, _inactivityPeriod);
        }

        private void IdleTimeout(object sndr, TimeoutEventArgs e)
        {
            try
            {
                if (!Id.Equals(e.IdleObject)) return;

                _log.DebugFormat("Close jabber-signalr connection {0} by inactivity timeout.", Id);

                var xmppStream = _xmppServer.StreamManager.GetStream(Id);
                if (xmppStream == null)
                {
                    _log.DebugFormat("Stream already was closed", Id);
                    return;
                }
                var jid = new Jid(xmppStream.User, xmppStream.Domain, SignalRHelper.SIGNALR_RESOURCE);
                _xmppServer.SessionManager.CloseSession(jid);
                _xmppServer.StreamManager.RemoveStream(Id);
                Close();
                var jidSessions = _xmppServer.SessionManager.GetBareJidSessions(jid);
                var sender = (IXmppSender)_xmppServer.GetService(typeof(IXmppSender));
                var presence = new Presence
                {
                    Priority = SignalRHelper.PRIORITY,
                    From = jid,
                    Type = PresenceType.unavailable
                };
                sender.Broadcast(_xmppServer.SessionManager.GetSessions(), presence);
                if (jidSessions.Count > 0)
                {
                    var bestSessions = jidSessions.Where(s => !s.IsSignalRFake).OrderByDescending(s => s.Presence.Priority).ToArray();
                    if (bestSessions.Length > 0 && bestSessions[0].Presence != null)
                    {
                        var bestSession = bestSessions[0];
                        try
                        {
                            signalrServiceClient.SendState(bestSession.Jid.User.ToLowerInvariant(),
                                SignalRHelper.GetState(bestSession.Presence.Show, bestSession.Presence.Type), -1, bestSession.Jid.Server);
                        }
                        catch (Exception ex)
                        {
                            _log.ErrorFormat("Unexpected error, connectionId = {0}, {1}, {2}, {3}", Id,
                                ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                        }
                    }
                    else
                    {
                        _log.ErrorFormat("XMPP session Presence is null. Connection {0}", Id);
                    }
                }
                else
                {
                    try
                    {
                        signalrServiceClient.SendState(jid.User.ToLowerInvariant(), SignalRHelper.USER_OFFLINE, -1, jid.Server);
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorFormat("Unexpected error, connectionId = {0}, {1}, {2}, {3}", Id,
                            ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Unexpected error: {0}", ex);
            }
        }


        private void InvokeXmppStreamStart(Node node, string ns)
        {
            if (XmppStreamStart != null)
            {
                XmppStreamStart(this, new XmppStreamStartEventArgs(Id, node, ns));
            }
        }

        private void InvokeXmppStreamElement(Node node)
        {
            if (XmppStreamElement != null)
            {
                XmppStreamElement(this, new XmppStreamEventArgs(Id, node));
            }
        }

        private void InvokeXmppStreamEnd(IEnumerable<Node> buffer)
        {
            if (XmppStreamEnd != null)
            {
                XmppStreamEnd(this, new XmppStreamEndEventArgs(Id, buffer));
            }
        }

        private void InvokeClosed()
        {
            if (Closed != null)
            {
                Closed(this, new XmppConnectionCloseEventArgs());
            }
        }
        
        
        public event EventHandler<XmppStreamStartEventArgs> XmppStreamStart = delegate { };

        public event EventHandler<XmppStreamEventArgs> XmppStreamElement = delegate { };

        public event EventHandler<XmppStreamEndEventArgs> XmppStreamEnd = delegate { };

        public event EventHandler<XmppConnectionCloseEventArgs> Closed = delegate { };
    }
}
