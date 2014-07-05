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
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Server.Authorization;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Statistics;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Users;

namespace ASC.Xmpp.Server
{
	public class XmppServer : IServiceProvider, IDisposable
	{
		private UserManager userManager;

		private XmppGateway gateway;

		private XmppSender sender;

		private XmppServiceManager serviceManager;

		public StorageManager StorageManager
		{
			get;
			private set;
		}

		public AuthManager AuthManager
		{
			get;
			private set;
		}

		public XmppSessionManager SessionManager
		{
			get;
			private set;
		}

        public XmppStreamManager StreamManager
        {
            get;
            private set;
        }

        public XmppHandlerManager HandlerManager
        {
            get;
            private set;
        }

		public XmppServer()
		{
			StorageManager = new StorageManager();
			userManager = new UserManager(StorageManager);
			AuthManager = new AuthManager();

			StreamManager = new XmppStreamManager();
			SessionManager = new XmppSessionManager();

			gateway = new XmppGateway();
			sender = new XmppSender(gateway);

			serviceManager = new XmppServiceManager(this);
            HandlerManager = new XmppHandlerManager(StreamManager, gateway, sender, this);
		}

		public void AddXmppListener(IXmppListener listener)
		{
			gateway.AddXmppListener(listener);
		}

		public void RemoveXmppListener(string name)
		{
			gateway.RemoveXmppListener(name);
		}

		public void StartListen()
		{
			NetStatistics.Enabled = true;
			gateway.Start();
		}

		public void StopListen()
		{
			gateway.Stop();
		}

		public void RegisterXmppService(IXmppService service)
		{
			serviceManager.RegisterService(service);
		}

		public void UnregisterXmppService(Jid jid)
		{
			serviceManager.UnregisterService(jid);
		}

		public IXmppService GetXmppService(Jid jid)
		{
			return serviceManager.GetService(jid);
		}

		public void Dispose()
        {
            StorageManager.Dispose();
        }

		public object GetService(Type serviceType)
		{
			if (serviceType == typeof(IXmppReceiver))
			{
				return gateway;
			}
			if (serviceType == typeof(IXmppSender))
			{
				return sender;
			}
			if (serviceType == typeof(XmppSessionManager))
			{
				return SessionManager;
			}
			if (serviceType == typeof(XmppStreamManager))
			{
				return StreamManager;
			}
			if (serviceType == typeof(UserManager))
			{
				return userManager;
			}
			if (serviceType == typeof(StorageManager))
			{
				return StorageManager;
			}
			if (serviceType == typeof(XmppServiceManager))
			{
				return serviceManager;
			}
			if (serviceType == typeof(AuthManager))
			{
				return AuthManager;
			}
			if (serviceType == typeof(XmppHandlerManager))
			{
                return HandlerManager;
			}
			return null;
		}
	}
}