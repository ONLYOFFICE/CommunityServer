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

using ASC.Core;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Notify.Jabber;
using ASC.Core.Users;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.extensions.commands;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Services.Jabber;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Streams;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ASC.Xmpp.Host
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, 
        InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
    public class JabberService : IJabberService
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(JabberService));
        private static string fromTeamlabToOnlyOffice = ConfigurationManager.AppSettings["jabber.from-teamlab-to-onlyoffice"] ?? "true";
        private static string fromServerInJid = ConfigurationManager.AppSettings["jabber.from-server-in-jid"] ?? "teamlab.com";
        private static string toServerInJid = ConfigurationManager.AppSettings["jabber.to-server-in-jid"] ?? "onlyoffice.com";
        private readonly XmppServer _xmppServer;
        private readonly ReverseJabberServiceClient _reverseJabberServiceClient;

        public JabberService(XmppServer xmppServer)
        {
            _xmppServer = xmppServer;
            _reverseJabberServiceClient = new ReverseJabberServiceClient();
        }

        public int GetNewMessagesCount(int tenantId, string userName)
        {
            var count = 0;
            try
            {
                count = _xmppServer.StorageManager.OfflineStorage.GetOfflineMessagesCount(GetJid(userName, tenantId));
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error, userName = {0}, {1}", userName, e);
            }
            return count;
        }

        public string GetUserToken(int tenantId, string userName)
        {
            string token = null;
            try
            {
                token = _xmppServer.AuthManager.GetUserToken(userName);
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error, userName = {0}, {1}, {2}, {3}", userName,
                    e.Message, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
            return token;
        }

        public void SendMessage(int tenantId, string from, string to, string text, string subject)
        {
            try
            {
                _log.DebugFormat("Send Message: tenantId={0}, from={1}, to={2}, text={3}", tenantId, from, to, text);
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                if (from == null)
                {
                    _reverseJabberServiceClient.SendMessage(string.Empty, to.ToLowerInvariant(), text, tenantId, string.Empty);
                }
                var jidFrom = GetJid(from, tenantId);
                var jidTo = to != string.Empty ? GetJid(to, tenantId) : new Jid(jidFrom.Server);
                var message = new Message(jidTo, jidFrom, MessageType.chat, text);

                var sessions = _xmppServer.SessionManager.GetBareJidSessions(jidTo, GetSessionsType.All);
                if (sessions.Count != 0)
                {
                    foreach (var session in sessions)
                    {
                        if (session != null && !session.IsSignalRFake)
                        {
                            ((IXmppSender)_xmppServer.GetService(typeof(IXmppSender))).SendTo(session, message);
                        }
                    }
                }
                else
                {
                    _xmppServer.StorageManager.OfflineStorage.SaveOfflineMessages(message);
                }

                var handlers = _xmppServer.HandlerManager.HandlerStorage.GetStanzaHandlers(jidFrom, typeof(Message));
                if (handlers.Count > 1)
                {
                    var messageArchiveHandler = handlers[1] as MessageArchiveHandler;
                    if (messageArchiveHandler != null)
                    {
                        messageArchiveHandler.HandleMessage(null, message, null);
                    }
                }
                else
                {
                    var messageArchiveHandler = new MessageArchiveHandler();
                    messageArchiveHandler.HandleMessage(null, message, null);
                    messageArchiveHandler.FlushMessageBuffer();
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error, from = {0}, to = {1}, {2}, {3}, {4}", from, to,
                    e.Message, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public void SendCommand(int tenantId, string from, string to, string command, bool fromTenant)
        {
            try
            {
                _log.DebugFormat("Send Command: tenantId={0}, from={1}, to={2}, text={3}", tenantId, from, to, command);

                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || string.IsNullOrEmpty(command)) return;
                var toJid = GetJid(to, tenantId, "TMTalk");
                var iq = new IQ(IqType.set, fromTenant ? new Jid(from) : GetJid(from, tenantId/*, "TMTalk"*/), toJid)
                {
                    Query = new Command(command)
                };
                var session = _xmppServer.SessionManager.GetSession(toJid);
                if (session != null)
                {
                    var sender = (IXmppSender)_xmppServer.GetService(typeof(IXmppSender));
                    sender.SendTo(session, iq);
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error, from = {0}, to = {1}, {2}, {3}, {4}", from, to,
                    e.Message, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public byte AddXmppConnection(string connectionId, string userName, byte state, int tenantId)
        {
            try
            {
                _log.DebugFormat("Add Xmpp Connection: connectionId={0}, userName={1}, state={2}, tenantId={3}", connectionId, userName, state, tenantId);
                var jid = GetJid(userName, tenantId, SignalRHelper.SIGNALR_RESOURCE);
                var listener = (SignalRXmppListener)((XmppGateway)_xmppServer.GetService(typeof(IXmppReceiver))).GetXmppListener("SignalR Listener");
                if (listener.GetXmppConnection(connectionId) != null)
                {
                    RemoveXmppConnection(connectionId, userName, tenantId);
                }
                listener.AddXmppConnection(connectionId, _xmppServer);
                var xmppStream = ((XmppStreamManager)_xmppServer.GetService(typeof(XmppStreamManager))).GetOrCreateNewStream(connectionId);
                xmppStream.Authenticate(userName);
                string domain = CoreContext.TenantManager.GetTenant(tenantId).TenantDomain;
                if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
                {
                    int place = domain.LastIndexOf(fromServerInJid);
                    if (place >= 0)
                    {
                        domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
                    }
                }
                xmppStream.Domain = domain;
                xmppStream.Connected = true;
                xmppStream.BindResource(SignalRHelper.SIGNALR_RESOURCE);

                var handler = _xmppServer.HandlerManager.HandlerStorage.GetStreamStartHandlers(jid)[0];
                var stream = new Stream
                {
                    To = new Jid(jid.Server),
                    Namespace = "http://etherx.jabber.org/streams",
                    Version = "1.6",
                    Language = string.Empty
                };

                handler.StreamStartHandle(xmppStream, stream, null);

                var session = new XmppSession(jid, xmppStream)
                {
                    RosterRequested = false,
                    Active = true,
                    IsSignalRFake = true
                };

                ((XmppSessionManager)_xmppServer.GetService(typeof(XmppSessionManager))).AddSession(session);

                var presence = new Presence(SignalRHelper.GetShowType(state), String.Empty, SignalRHelper.PRIORITY)
                {
                    From = jid,
                    Type = SignalRHelper.GetPresenceType(state)
                };

                _xmppServer.SessionManager.SetSessionPresence(session, presence);

                var sender = (IXmppSender)_xmppServer.GetService(typeof(IXmppSender));
                var sessions = _xmppServer.SessionManager.GetSessions().Where(s => s.Id != session.Id).ToArray();
                sender.Broadcast(sessions, session.Presence);

                var offlineMessages = _xmppServer.StorageManager.OfflineStorage.GetOfflineMessages(jid);
                if (offlineMessages.Count > 0)
                {
                    var users = new List<string>();
                    for (int i = 0; i < offlineMessages.Count; i++)
                    {
                        var from = offlineMessages[i].From;
                        var name = from.User != null ? from.User.ToLowerInvariant() : string.Empty;
                        if (!users.Contains(name))
                        {
                            users.Add(name);
                        }
                    }
                    _reverseJabberServiceClient.SendOfflineMessages(userName, users, tenantId);
                    _xmppServer.StorageManager.OfflineStorage.RemoveAllOfflineMessages(jid);
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error, userName = {0}, {1}, {2}, {3}", userName,
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
            return GetState(tenantId, userName);
        }

        public byte RemoveXmppConnection(string connectionId, string userName, int tenantId)
        {
            try
            {
                _log.DebugFormat("Remove Xmpp Connection: connectionId={0}, userName={1}, tenantId={2}", connectionId, userName, tenantId);

                var jid = GetJid(userName, tenantId, SignalRHelper.SIGNALR_RESOURCE);
                var listener = (SignalRXmppListener)((XmppGateway)_xmppServer.GetService(typeof(IXmppReceiver))).GetXmppListener("SignalR Listener");
                _xmppServer.SessionManager.CloseSession(jid);
                _xmppServer.StreamManager.RemoveStream(connectionId);
                listener.CloseXmppConnection(connectionId);
                var sender = (IXmppSender)_xmppServer.GetService(typeof(IXmppSender));
                Task.Run(() =>
                {
                    sender.Broadcast(_xmppServer.SessionManager.GetSessions(),
                        new Presence { Priority = SignalRHelper.PRIORITY, From = jid, Type = PresenceType.unavailable });
                });

                var userSession = _xmppServer.SessionManager.GetAvailableSession(jid.BareJid);
                if (userSession != null && userSession.Presence != null && userSession.Presence.Type != PresenceType.unavailable)
                {
                    return SignalRHelper.GetState(userSession.Presence.Show, userSession.Presence.Type);
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error, userName = {0}, {1}, {2}, {3}", userName,
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
            return SignalRHelper.USER_OFFLINE;
        }

        public byte SendState(int tenantId, string userName, byte state)
        {
            try
            {
                _log.DebugFormat("Send State: tenantId={0}, userName={1}, state={2}", tenantId, userName, state);
                var jid = GetJid(userName, tenantId, SignalRHelper.SIGNALR_RESOURCE);
                var userSession = _xmppServer.SessionManager.GetSession(jid);
                if (userSession != null)
                {
                    var sessions = _xmppServer.SessionManager.GetSessions().Where(s => s.Id != userSession.Id).ToArray();
                    var sender = (IXmppSender)_xmppServer.GetService(typeof(IXmppSender));
                    var presence = GetNewPresence(state, null, jid);

                    _xmppServer.SessionManager.SetSessionPresence(userSession, presence);
                    sender.Broadcast(sessions, presence);
                }
                return GetState(tenantId, userName);
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error, userName = {0}, {1}, {2}, {3}", userName,
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
            return SignalRHelper.USER_OFFLINE;
        }

        public MessageClass[] GetRecentMessages(int tenantId, string from, string to, int id)
        {
            MessageClass[] messageClasses = null;
            try
            {
                _log.DebugFormat("Get Recent Messages: tenantId={0}, from={1}, to={2}, id={3}", tenantId, from, to, id);
                var jidFrom = GetJid(from, tenantId);
                var jidTo = GetJid(to, tenantId);
                var archiveStore = ((StorageManager)_xmppServer.GetService(typeof(StorageManager))).GetStorage<DbMessageArchive>("archive");
                var handlers = _xmppServer.HandlerManager.HandlerStorage.GetStanzaHandlers(jidFrom, typeof(Message));
                if (handlers.Count > 1)
                {
                    var messageArchiveHandler = handlers[1] as MessageArchiveHandler;
                    if (messageArchiveHandler != null)
                    {
                        messageArchiveHandler.FlushMessageBuffer();
                    }
                }

                var messages = archiveStore.GetMessages(jidFrom, jidTo, id, SignalRHelper.NUMBER_OF_RECENT_MSGS);

                messageClasses = new MessageClass[messages.Length];
                for (int i = 0; i < messages.Length; i++)
                {
                    messageClasses[i] = new MessageClass();
                    messageClasses[i].DateTime = messages[i].XDelay != null ? messages[i].XDelay.Stamp : messages[i].DbStamp;
                    messageClasses[i].Id = messages[i].InternalId;
                    messageClasses[i].Text = messages[i].Body;
                    messageClasses[i].UserName = messages[i].From.User;
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error, from = {0}, to = {1}, {2}, {3}:{4}, {5}, {6}", from, to,
                    e, e.StackTrace, new StackTrace(e, true).GetFrame(0).GetFileLineNumber(),
                e.InnerException != null ? e.InnerException.Message : string.Empty, e.ToString());
            }
            return messageClasses;
        }

        public Dictionary<string, byte> GetAllStates(int tenantId, string from)
        {
            var states = new Dictionary<string, byte>();
            try
            {
                _log.Debug("Get All States");
                var userJid = GetJid(from, tenantId);
                ASCContext.SetCurrentTenant(userJid.Server);

                foreach (var user in ASCContext.UserManager.GetUsers().Where(u => !u.IsMe()))
			    {
                    userJid = GetJid(user.UserName, tenantId);
                    var session = _xmppServer.SessionManager.GetAvailableSession(userJid.BareJid);
                    if (session != null && session.Presence != null && session.Presence.Type != PresenceType.unavailable)
                    {
                        states.Add(userJid.User, SignalRHelper.GetState(session.Presence.Show, session.Presence.Type));
                    }
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error {0}", e);
            }
            return states;
        }

        public byte GetState(int tenantId, string from)
        {
            try
            {
                var session = _xmppServer.SessionManager.GetAvailableSession(GetJid(from, tenantId).BareJid);
                if (session != null && session.Presence != null && session.Presence.Type != PresenceType.unavailable)
                {
                    return SignalRHelper.GetState(session.Presence.Show, session.Presence.Type);
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error {0}", e);
            }
            return SignalRHelper.USER_OFFLINE;
        }

        public void Ping(string connectionId, int tenantId, string userName, byte state)
        {
            try
            {
                _log.DebugFormat("Ping, connectionId={0}, tenantId={1}, userName={2}, state={3}", connectionId, tenantId, userName, state);
                var listener = (SignalRXmppListener)((XmppGateway)_xmppServer.GetService(typeof(IXmppReceiver))).GetXmppListener("SignalR Listener");
                var connection = listener.GetXmppConnection(connectionId) as SignalRXmppConnection;
                if (connection != null)
                {
                    connection.UpdateTimeout();
                }
                else
                {
                    AddXmppConnection(connectionId, userName, state, tenantId);
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error {0}", e);
            }
        }

        public string HealthCheck(string userName, int tenantId)
        {
            try
            {
                Random rand = new Random();
                string connectionId = Guid.NewGuid().ToString();
                byte state = (byte)rand.Next(SignalRHelper.USER_ONLINE, SignalRHelper.USER_OFFLINE);
                var jid = GetJid(userName, tenantId);
                if (_xmppServer.SessionManager != null)
                {
                    state = AddXmppConnection(connectionId, userName, state, tenantId);
                    var session = _xmppServer.SessionManager.GetSession(jid);
                    if (session != null)
                    {
                        var realState = SignalRHelper.GetState(session.Presence.Show, session.Presence.Type);
                        if (realState != state)
                        {
                            throw new Exception("State is " + realState + " but should be " + state);
                        }

                        state = (byte)rand.Next(SignalRHelper.USER_ONLINE, SignalRHelper.USER_OFFLINE);
                        SendState(tenantId, userName, state);
                        session = _xmppServer.SessionManager.GetSession(jid);
                        realState = SignalRHelper.GetState(session.Presence.Show, session.Presence.Type);
                        if (realState != state)
                        {
                            throw new Exception("State is " + realState + " but should be " + state);
                        }

                        GetRecentMessages(tenantId, userName, "test.user", 0);
                    }
                    RemoveXmppConnection(connectionId, userName, tenantId);
                }
                else
                {
                    throw new Exception("SessionManager is null");
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Unexpected error {0}, {1}", e, e.StackTrace);
                return e.ToString();
            }
            return string.Empty;
        }

        private Presence GetNewPresence(byte state, Presence presence = null, Jid jid = null)
        {
            if (presence == null)
            {
                presence = new Presence(SignalRHelper.GetShowType(state), String.Empty) { From = jid, Priority = SignalRHelper.PRIORITY };
            }

            presence.Show = SignalRHelper.GetShowType(state);
            presence.Type = SignalRHelper.GetPresenceType(state);

            return presence;
        }

        private Jid GetJid(string userName, int tenant, string resource = null)
        {
            var t = CoreContext.TenantManager.GetTenant(tenant);
            if (t == null)
            {
                throw new Exception(string.Format("Tenant with id = {0} not found.", tenant));
            }
            string domain = t.TenantDomain;
            if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(toServerInJid))
            {
                int place = domain.LastIndexOf(toServerInJid);
                if (place >= 0)
                {
                    domain = domain.Remove(place, toServerInJid.Length).Insert(place, fromServerInJid);
                }
            }
            return new Jid(userName, domain, resource);
        }
    }
}