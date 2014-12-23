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
using ASC.Xmpp.Core.protocol.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ASC.Xmpp.Server.Session
{
    public class XmppSessionManager
    {
        private IDictionary<Jid, XmppSession> sessions = new Dictionary<Jid, XmppSession>();

        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public XmppSession GetSession(Jid jid)
        {
            if (jid == null) throw new ArgumentNullException("jid");
            try
            {
                locker.EnterReadLock();
                if (jid.HasResource)
                {
                    return sessions.ContainsKey(jid) ? sessions[jid] : null;
                }
                return (from s in sessions.Values where s.Jid.Bare == jid.Bare orderby s.Priority select s).LastOrDefault();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public XmppSession GetAvailableSession(Jid jid)
        {
            if (jid == null) throw new ArgumentNullException("jid");
            try
            {
                locker.EnterReadLock();
                if (jid.HasResource)
                {
                    return sessions.ContainsKey(jid) ? sessions[jid] : null;
                }
                return (from s in sessions.Values
                        where (s.Jid.Bare == jid.Bare && s.Presence != null && s.Presence.Type != PresenceType.unavailable)
                        orderby s.Priority select s).LastOrDefault();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public ICollection<XmppSession> GetSessions()
        {
            try
            {
                locker.EnterReadLock();
                return sessions.Values.ToList();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public IEnumerable<XmppSession> GetStreamSessions(string streamId)
        {
            if (string.IsNullOrEmpty(streamId)) return new List<XmppSession>();
            try
            {
                locker.EnterReadLock();
                return (from s in sessions.Values where s.Stream.Id == streamId select s).ToList();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public ICollection<XmppSession> GetBareJidSessions(string jid)
        {
            if (string.IsNullOrEmpty(jid)) return new List<XmppSession>();
            return GetBareJidSessions(new Jid(jid));
        }

        public ICollection<XmppSession> GetBareJidSessions(Jid jid)
        {
            return GetBareJidSessions(jid, GetSessionsType.All);
        }

        public ICollection<XmppSession> GetBareJidSessions(Jid jid, GetSessionsType getType)
        {
            if (jid == null) return new List<XmppSession>();
            try
            {
                locker.EnterReadLock();

                var bares = from s in sessions.Values where s.Jid.Bare == jid.Bare select s;
                if (getType == GetSessionsType.Available)
                {
                    bares = from s in bares where s.Available select s;
                }
                if (getType == GetSessionsType.RosterRequested)
                {
                    bares = from s in bares where s.RosterRequested select s;
                }
                return bares.ToList();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void AddSession(XmppSession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            try
            {
                locker.EnterWriteLock();
                sessions.Add(session.Jid, session);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void CloseSession(Jid jid)
        {
            var session = GetSession(jid);
            if (session != null && !session.IsSignalRFake && session.Available)
            {
                SoftInvokeEvent(SessionUnavailable, session);
            }

            try
            {
                locker.EnterWriteLock();
                if (sessions.ContainsKey(jid))
                {
                    sessions.Remove(jid);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void SetSessionPresence(XmppSession session, Presence presence)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (presence == null) throw new ArgumentNullException("presence");

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
            if (presence == null) return false;
            return presence.Type == PresenceType.available || presence.Type == PresenceType.invisible;
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
