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
using System.Linq;
using System.Threading;
using ASC.Collections;

namespace ASC.Xmpp.Server.Gateway
{
    public abstract class XmppListenerBase : IXmppListener
    {
        private IDictionary<string, IXmppConnection> connections = new SynchronizedDictionary<string, IXmppConnection>();

        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected bool Started
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            set;
        }

        public IEnumerable<string> ConnectionIds 
        {
            get { return connections.Values.Select(c => c.Id); }
        }

        public void Start()
        {
            lock (this)
            {
                if (Started) return;

                connections.Clear();

                Started = true;
                DoStart();
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (!Started) return;

                Started = false;
                DoStop();

                var keys = new string[connections.Keys.Count];
                connections.Keys.CopyTo(keys, 0);
                foreach (var key in keys)
                {
                    if (connections.ContainsKey(key)) connections[key].Close();
                }
                connections.Clear();
            }
        }

        public IXmppConnection GetXmppConnection(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return null;
            try
            {
                locker.EnterReadLock();
                return connections.ContainsKey(connectionId) ? connections[connectionId] : null;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void RemoveXmppXonnections()
        {
            connections.Clear();
        }

        public event EventHandler<XmppConnectionOpenEventArgs> OpenXmppConnection;

        protected void AddNewXmppConnection(IXmppConnection xmppConnection)
        {
            if (xmppConnection == null) throw new ArgumentNullException("xmppConnection");

            try
            {
                locker.EnterWriteLock();
                connections.Add(xmppConnection.Id, xmppConnection);
                xmppConnection.Closed += XmppConnectionClosed;
            }
            finally
            {
                locker.ExitWriteLock();
            }

            var handler = OpenXmppConnection;
            if (handler != null) handler(this, new XmppConnectionOpenEventArgs(xmppConnection));

            xmppConnection.BeginReceive();
        }

        protected void CloseXmppConnection(string connectionId)
        {
            try
            {
                locker.EnterWriteLock();

                var connection = GetXmppConnection(connectionId);
                if (connection != null)
                {
                    connection.Closed -= XmppConnectionClosed;
                    connections.Remove(connectionId);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        private void XmppConnectionClosed(object sender, XmppConnectionCloseEventArgs e)
        {
            try
            {
                locker.EnterWriteLock();

                var connection = (IXmppConnection)sender;
                connection.Closed -= XmppConnectionClosed;
                connections.Remove(connection.Id);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        #region IConfigurable Members

        public abstract void Configure(IDictionary<string, string> properties);

        #endregion

        protected abstract void DoStart();

        protected abstract void DoStop();
    }
}