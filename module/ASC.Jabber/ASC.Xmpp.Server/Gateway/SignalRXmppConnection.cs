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

using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x.muc;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Utils;
using log4net;
using System;
using System.Linq;
using System.Text;

namespace ASC.Xmpp.Server.Gateway
{
    public class SignalRXmppConnection : IXmppConnection
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(BoshXmppConnection));
        private static readonly ReverseJabberServiceClient _reverseJabberServiceClient = new ReverseJabberServiceClient();
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
                            _reverseJabberServiceClient.SendMessage(nameFrom, message.To.User.ToLowerInvariant(),
                                message.Body, -1, elem.From.Server);
                        }
                        else if (message.FirstChild.HasTag(typeof(Invite)))
                        {
                            _reverseJabberServiceClient.SendInvite(nameFrom, message.To.User.ToLowerInvariant(), elem.To.Server);
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
                            _reverseJabberServiceClient.SendState(bestSession.Jid.User.ToLowerInvariant(),
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
                        _reverseJabberServiceClient.SendState(jid.User.ToLowerInvariant(), SignalRHelper.USER_OFFLINE, -1, jid.Server);
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

        public event EventHandler<XmppStreamStartEventArgs> XmppStreamStart = delegate { };

        public event EventHandler<XmppStreamEventArgs> XmppStreamElement = delegate { };

        public event EventHandler<XmppStreamEndEventArgs> XmppStreamEnd = delegate { };

        public event EventHandler<XmppConnectionCloseEventArgs> Closed = delegate { };
    }
}
