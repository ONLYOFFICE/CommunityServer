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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Xmpp.Server.Session
{
    public class XmppSessionManager
    {
        private readonly ConcurrentDictionary<Jid, XmppSession> sessions = new ConcurrentDictionary<Jid, XmppSession>();


        public XmppSession GetSession(Jid jid)
        {
            if (jid == null)
            {
                throw new ArgumentNullException("jid");
            }

            if (jid.HasResource)
            {
                XmppSession s;
                sessions.TryGetValue(jid, out s);
                return s;
            }
            return sessions.Values
                .Where(s => s.Jid.Bare == jid.Bare)
                .OrderBy(s => s.Priority)
                .LastOrDefault();
        }

        public XmppSession GetAvailableSession(Jid jid)
        {
            if (jid == null)
            {
                throw new ArgumentNullException("jid");
            }

            if (jid.HasResource)
            {
                XmppSession s;
                sessions.TryGetValue(jid, out s);
                return s;
            }
            return sessions.Values
                .Where(s => s.Jid.Bare == jid.Bare && s.Presence != null && s.Presence.Type != PresenceType.unavailable)
                .OrderBy(s => s.Priority)
                .LastOrDefault();
        }

        public ICollection<XmppSession> GetSessions()
        {
            return sessions.Values;
        }

        public IEnumerable<XmppSession> GetStreamSessions(string streamId)
        {
            if (string.IsNullOrEmpty(streamId))
            {
                return new XmppSession[0];
            }

            return sessions.Values
                .Where(s => s.Stream.Id == streamId)
                .ToList();
        }

        public ICollection<XmppSession> GetBareJidSessions(Jid jid)
        {
            return GetBareJidSessions(jid, GetSessionsType.All);
        }

        public ICollection<XmppSession> GetBareJidSessions(Jid jid, GetSessionsType getType)
        {
            if (jid == null)
            {
                return new XmppSession[0];
            }

            var bares = sessions.Values.Where(s => s.Jid.Bare == jid.Bare);
            if (getType == GetSessionsType.Available)
            {
                bares = bares.Where(s => s.Available);
            }
            else if (getType == GetSessionsType.RosterRequested)
            {
                bares = bares.Where(s => s.RosterRequested);
            }
            return bares.ToList();
        }

        public void AddSession(XmppSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            sessions.TryAdd(session.Jid, session);
        }

        public void CloseSession(Jid jid)
        {
            var session = GetSession(jid);
            if (session != null && !session.IsSignalRFake && session.Available)
            {
                SoftInvokeEvent(SessionUnavailable, session);
            }

            sessions.TryRemove(jid, out session);
        }

        public void SetSessionPresence(XmppSession session, Presence presence)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            if (presence == null)
            {
                throw new ArgumentNullException("presence");
            }

            var oldPresence = session.Presence;
            session.Presence = presence;
            if (!IsAvailablePresence(oldPresence) && IsAvailablePresence(presence))
            {
                SoftInvokeEvent(SessionAvailable, session);
            }
            if (IsAvailablePresence(oldPresence) && !IsAvailablePresence(presence))
            {
                SoftInvokeEvent(SessionUnavailable, session);
            }
        }

        public event EventHandler<XmppSessionArgs> SessionAvailable;

        public event EventHandler<XmppSessionArgs> SessionUnavailable;

        private void SoftInvokeEvent(EventHandler<XmppSessionArgs> eventHandler, XmppSession session)
        {
            try
            {
                var handler = eventHandler;
                if (handler != null) handler(this, new XmppSessionArgs(session));
            }
            catch { }
        }

        private bool IsAvailablePresence(Presence presence)
        {
            return presence != null && (presence.Type == PresenceType.available || presence.Type == PresenceType.invisible);
        }
    }

    [Flags]
    public enum GetSessionsType
    {
        RosterRequested = 1,
        Available = 2,
        All = 3,
    }
}
