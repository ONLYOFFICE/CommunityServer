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

using ASC.Xmpp.Server.Statistics;
using log4net;
using System;
using System.Collections.Generic;

namespace ASC.Xmpp.Server.Gateway
{
	public class XmppGateway : IXmppReceiver
	{
		private readonly object syncRoot = new object();

		private bool started = false;

        private readonly IDictionary<string, IXmppListener> listeners = new Dictionary<string, IXmppListener>();

        private readonly IDictionary<string, string> connectionListenerMap = new Dictionary<string, string>();

		private readonly static ILog log = LogManager.GetLogger(typeof(XmppGateway));


		public void AddXmppListener(IXmppListener listener)
		{
			lock (syncRoot)
			{
				try
				{
					if (started) throw new InvalidOperationException();
					if (listener == null) throw new ArgumentNullException("listener");

					listeners.Add(listener.Name, listener);
					listener.OpenXmppConnection += OpenXmppConnection;

					log.DebugFormat("Add listener '{0}'", listener.Name);
				}
				catch (Exception e)
				{
					log.ErrorFormat("Error add listener '{0}': {1}", listener.Name, e);
					throw;
				}
			}
		}

		public void RemoveXmppListener(string name)
		{
			lock (syncRoot)
			{
				try
				{
					if (started) throw new InvalidOperationException();
					if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

					if (listeners.ContainsKey(name))
					{
						var listener = listeners[name];
						listener.OpenXmppConnection -= OpenXmppConnection;
						listeners.Remove(name);

						log.DebugFormat("Remove listener '{0}'", listener.Name);
					}
				}
				catch (Exception e)
				{
					log.ErrorFormat("Error remove listener '{0}': {1}", name, e);
					throw;
				}
			}
		}

		public void Start()
		{
			lock (syncRoot)
			{
				foreach (var listener in listeners.Values)
				{
					try
					{
						listener.Start();
						log.DebugFormat("Started listener '{0}'", listener.Name);
					}
					catch (Exception e)
					{
						log.ErrorFormat("Error start listener '{0}': {1}", listener.Name, e);
					}
				}
				started = true;
			}
		}

		public void Stop()
		{
			lock (syncRoot)
			{
				foreach (var listener in listeners.Values)
				{
					try
					{
						listener.Stop();
						log.DebugFormat("Stopped listener '{0}'", listener.Name);
					}
					catch (Exception e)
					{
						log.ErrorFormat("Error stop listener '{0}': {1}", listener.Name, e);
					}
				}
				started = false;
			}

			log.DebugFormat("Net statistics: read bytes {0}, write bytes {1}", NetStatistics.GetReadBytes(), NetStatistics.GetWriteBytes());
		}

        public IXmppListener GetXmppListener(string listenerName)
        {
            lock (syncRoot)
            {
                return listeners[listenerName];
            }
        }

		public IXmppConnection GetXmppConnection(string connectionId)
		{
			if (string.IsNullOrEmpty(connectionId)) return null;

			string listenerName = null;
            IXmppListener listener = null;
            lock (syncRoot)
            {
			    if (!connectionListenerMap.TryGetValue(connectionId, out listenerName) || listenerName == null) return null;
                if (!listeners.TryGetValue(listenerName, out listener) || listener == null) return null;
            }
			return listener.GetXmppConnection(connectionId);
		}


		public event EventHandler<XmppStreamStartEventArgs> XmppStreamStart;

		public event EventHandler<XmppStreamEndEventArgs> XmppStreamEnd;

		public event EventHandler<XmppStreamEventArgs> XmppStreamElement;


		private void OpenXmppConnection(object sender, XmppConnectionOpenEventArgs e)
		{
            lock (syncRoot)
            {
                connectionListenerMap[e.XmppConnection.Id] = ((IXmppListener)sender).Name;
            }
			e.XmppConnection.Closed += XmppConnectionClose;
			e.XmppConnection.XmppStreamEnd += XmppConnectionXmppStreamEnd;
			e.XmppConnection.XmppStreamElement += XmppConnectionXmppStreamElement;
			e.XmppConnection.XmppStreamStart += XmppConnectionXmppStreamStart;
		}

		private void XmppConnectionClose(object sender, XmppConnectionCloseEventArgs e)
		{
			var connection = (IXmppConnection)sender;

			connection.XmppStreamStart -= XmppConnectionXmppStreamStart;
			connection.XmppStreamElement -= XmppConnectionXmppStreamElement;
			connection.XmppStreamEnd -= XmppConnectionXmppStreamEnd;
			connection.Closed -= XmppConnectionClose;
            lock (syncRoot)
            {
                connectionListenerMap.Remove(connection.Id);
            }
		}


		private void XmppConnectionXmppStreamStart(object sender, XmppStreamStartEventArgs e)
		{
			var handler = XmppStreamStart;
			if (handler != null) handler(this, e);
		}

		private void XmppConnectionXmppStreamElement(object sender, XmppStreamEventArgs e)
		{
			var handler = XmppStreamElement;
			if (handler != null) handler(this, e);
		}

		private void XmppConnectionXmppStreamEnd(object sender, XmppStreamEndEventArgs e)
		{
			var handler = XmppStreamEnd;
			if (handler != null) handler(this, e);
		}
	}
}
