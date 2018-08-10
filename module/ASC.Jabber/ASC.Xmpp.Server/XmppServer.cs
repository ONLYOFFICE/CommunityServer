/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Xmpp.Server.Authorization;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Users;
using System;

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