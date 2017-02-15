/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Xmpp.Server.Gateway
{
    public abstract class XmppListenerBase : IXmppListener
    {
        private readonly object locker = new object();
        private readonly ConcurrentDictionary<string, IXmppConnection> connections = new ConcurrentDictionary<string, IXmppConnection>();

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

        public void Start()
        {
            lock (locker)
            {
                if (!Started)
                {
                    Started = true;
                    DoStart();
                }
            }
        }

        public void Stop()
        {
            lock (locker)
            {
                connections.Values
                    .ToList()
                    .ForEach(c => c.Close());
                connections.Clear();

                if (Started)
                {
                    Started = false;
                    DoStop();
                }
            }
        }

        public IXmppConnection GetXmppConnection(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                return null;
            }
            IXmppConnection conn;
            connections.TryGetValue(connectionId, out conn);
            return conn;
        }

        public event EventHandler<XmppConnectionOpenEventArgs> OpenXmppConnection = delegate { };

        protected void AddNewXmppConnection(IXmppConnection xmppConnection)
        {
            if (xmppConnection == null)
            {
                throw new ArgumentNullException("xmppConnection");
            }

            connections.TryAdd(xmppConnection.Id, xmppConnection);
            xmppConnection.Closed += XmppConnectionClosed;

            OpenXmppConnection(this, new XmppConnectionOpenEventArgs(xmppConnection));
            xmppConnection.BeginReceive();
        }

        protected void CloseXmppConnection(string connectionId)
        {
            IXmppConnection conn;
            if (connections.TryRemove(connectionId, out conn))
            {
                conn.Closed -= XmppConnectionClosed;
            }
        }

        private void XmppConnectionClosed(object sender, XmppConnectionCloseEventArgs e)
        {
            var connection = (IXmppConnection)sender;
            if (connection != null)
            {
                connection.Closed -= XmppConnectionClosed;
                connections.TryRemove(connection.Id, out connection);
            }
        }

        public abstract void Configure(IDictionary<string, string> properties);

        protected abstract void DoStart();

        protected abstract void DoStop();
    }
}