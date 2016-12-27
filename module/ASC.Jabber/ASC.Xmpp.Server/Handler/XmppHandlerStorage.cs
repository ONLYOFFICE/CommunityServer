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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ASC.Xmpp.Server.Handler
{
	public class XmppHandlerStorage
	{
        private IDictionary<Jid, List<IXmppStreamStartHandler>> streamStartHandlers = new Dictionary<Jid, List<IXmppStreamStartHandler>>();

        private IDictionary<string, List<IXmppStreamHandler>> streamHandlers = new Dictionary<string, List<IXmppStreamHandler>>();

        private IDictionary<string, List<IXmppStanzaHandler>> stanzaHandlers = new Dictionary<string, List<IXmppStanzaHandler>>();

		private ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		private IServiceProvider serviceProvider;


		public XmppHandlerStorage(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}


		public void AddXmppHandler(Jid address, IXmppHandler handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			try
			{
				locker.EnterWriteLock();

				if (handler is IXmppStreamStartHandler)
				{
					if (!streamStartHandlers.ContainsKey(address)) streamStartHandlers[address] = new List<IXmppStreamStartHandler>();
					streamStartHandlers[address].Add((IXmppStreamStartHandler)handler);
				}

				if (handler is IXmppStreamHandler)
				{
					foreach (var type in GetHandledTypes(handler))
					{
						var key = GetHandlerKey(address, type);
						if (!streamHandlers.ContainsKey(key)) streamHandlers[key] = new List<IXmppStreamHandler>();
						streamHandlers[key].Add((IXmppStreamHandler)handler);
					}
				}

				if (handler is IXmppStanzaHandler)
				{
					foreach (var type in GetHandledTypes(handler))
					{
						var key = GetHandlerKey(address, type);
						if (!stanzaHandlers.ContainsKey(key)) stanzaHandlers[key] = new List<IXmppStanzaHandler>();
						stanzaHandlers[key].Add((IXmppStanzaHandler)handler);
					}
				}
			}
			finally
			{
				locker.ExitWriteLock();
			}

			handler.OnRegister(serviceProvider);
		}

		public void RemoveXmppHandler(IXmppHandler handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			try
			{
				locker.EnterWriteLock();

				if (handler is IXmppStreamStartHandler)
				{
					foreach (var keyValuePair in new Dictionary<Jid, List<IXmppStreamStartHandler>>(streamStartHandlers))
					{
						foreach (var h in new List<IXmppStreamStartHandler>(keyValuePair.Value))
						{
							if (handler == h) streamStartHandlers[keyValuePair.Key].Remove(h);
						}
					}
				}

				if (handler is IXmppStreamHandler)
				{
					foreach (var keyValuePair in new Dictionary<string, List<IXmppStreamHandler>>(streamHandlers))
					{
						foreach (var h in new List<IXmppStreamHandler>(keyValuePair.Value))
						{
							if (handler == h) streamHandlers[keyValuePair.Key].Remove(h);
							if (streamHandlers[keyValuePair.Key].Count == 0)
							{
								streamHandlers.Remove(keyValuePair.Key);
							}

						}
					}
				}

				if (handler is IXmppStanzaHandler)
				{
					foreach (var keyValuePair in new Dictionary<string, List<IXmppStanzaHandler>>(stanzaHandlers))
					{
						foreach (var h in new List<IXmppStanzaHandler>(keyValuePair.Value))
						{
							if (handler == h) stanzaHandlers[keyValuePair.Key].Remove(h);
							if (stanzaHandlers[keyValuePair.Key].Count == 0)
							{
								stanzaHandlers.Remove(keyValuePair.Key);
							}

						}
					}
				}
			}
			finally
			{
				locker.ExitWriteLock();
			}

			handler.OnUnregister(serviceProvider);
		}

		public List<IXmppStreamStartHandler> GetStreamStartHandlers(Jid address)
		{
			try
			{
				locker.EnterReadLock();

				return streamStartHandlers.ContainsKey(address) ? streamStartHandlers[address] :
					streamStartHandlers.ContainsKey(Jid.Empty) ? streamStartHandlers[Jid.Empty] : new List<IXmppStreamStartHandler>();
			}
			finally
			{
				locker.ExitReadLock();
			}
		}

		public List<IXmppStreamHandler> GetStreamHandlers(string domain)
		{
			try
			{
				locker.EnterReadLock();

				var handlers = new List<IXmppStreamHandler>();
				foreach (var pair in streamHandlers)
				{
                    var jid = new Jid(pair.Key.Substring(0, pair.Key.IndexOf('|')));
                    if (jid.Server == domain)
                    {
                        foreach (var handler in pair.Value)
                        {
                            if (!handlers.Contains(handler)) handlers.Add(handler);
                        }
                    }
				}
				return handlers;
			}
			finally
			{
				locker.ExitReadLock();
			}
		}

		public List<IXmppStreamHandler> GetStreamHandlers(Jid address, Type streamElementType)
		{
			try
			{
				locker.EnterReadLock();

				var key = GetHandlerKey(address, streamElementType);
				return streamHandlers.ContainsKey(key) ? new List<IXmppStreamHandler>(streamHandlers[key]) : new List<IXmppStreamHandler>();
			}
			finally
			{
				locker.ExitReadLock();
			}
		}

		public List<IXmppStanzaHandler> GetStanzaHandlers(Jid to, Type stanzaType)
		{
			try
			{
				locker.EnterReadLock();

				var key = GetHandlerKey(to, stanzaType);
				if (stanzaHandlers.ContainsKey(key)) return new List<IXmppStanzaHandler>(stanzaHandlers[key]);

				if (to.Resource != null && to.Resource.Contains("/"))
				{
					var newTo = new Jid(to.ToString());
					newTo.Resource = newTo.Resource.Substring(0, newTo.Resource.IndexOf('/'));
					key = GetHandlerKey(newTo, stanzaType);
					if (stanzaHandlers.ContainsKey(key)) return new List<IXmppStanzaHandler>(stanzaHandlers[key]);
				}

				key = GetHandlerKey(to.Bare, stanzaType);
				if (stanzaHandlers.ContainsKey(key)) return new List<IXmppStanzaHandler>(stanzaHandlers[key]);

				key = GetHandlerKey(to.Server, stanzaType);
				if (stanzaHandlers.ContainsKey(key)) return new List<IXmppStanzaHandler>(stanzaHandlers[key]);

				if (stanzaType != typeof(Stanza)) return GetStanzaHandlers(to, typeof(Stanza));

				return new List<IXmppStanzaHandler>();
			}
			finally
			{
				locker.ExitReadLock();
			}
		}

		private Type[] GetHandledTypes(IXmppHandler handler)
		{
			var types = new List<object>(handler.GetType().GetCustomAttributes(typeof(XmppHandlerAttribute), true))
				.ConvertAll<Type>(o => ((XmppHandlerAttribute)o).XmppElementType);
			if (types.Count == 0) types.Add(null);
			return types.ToArray();
		}

		private string GetHandlerKey(object address, Type type)
		{
			return string.Format("{0}|{1}", address, type);
		}
	}
}
