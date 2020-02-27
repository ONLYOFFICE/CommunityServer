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
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Core.protocol.iq.vcard;
using ASC.Xmpp.Server.Handler;
using System;
using System.Collections.Generic;

namespace ASC.Xmpp.Server.Services
{
    public class XmppServiceBase : IXmppService
    {
        protected IList<IXmppHandler> Handlers
        {
            get;
            private set;
        }

        protected bool Registered
        {
            get;
            private set;
        }

        public Jid Jid
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IXmppService ParentService
        {
            get;
            set;
        }

        public DiscoInfo DiscoInfo
        {
            get;
            private set;
        }

        public virtual DiscoItem DiscoItem
        {
            get { return new DiscoItem() { Name = Name, Jid = Jid }; }
        }

        public virtual Vcard Vcard
        {
            get { return new Vcard() { Fullname = Name }; }
        }

        public XmppServiceBase()
        {
            Handlers = new List<IXmppHandler>();
            Registered = false;
            DiscoInfo = new DiscoInfo();
        }

        public XmppServiceBase(IXmppService parent)
            : this()
        {
            ParentService = parent;
        }

        public virtual void Configure(IDictionary<string, string> properties)
        {

        }

        public void OnRegister(IServiceProvider serviceProvider)
        {
            var handlerManager = (XmppHandlerManager)serviceProvider.GetService(typeof(XmppHandlerManager));
            var serviceManager = (XmppServiceManager)serviceProvider.GetService(typeof(XmppServiceManager));
            lock (Handlers)
            {
                foreach (var h in Handlers)
                {
                    handlerManager.AddXmppHandler(Jid, h);
                }
            }
            Registered = true;
            OnRegisterCore(handlerManager, serviceManager, serviceProvider);
            lock (Handlers)
            {
                DiscoveryFearures(Handlers);
            }
        }

        public void OnUnregister(IServiceProvider serviceProvider)
        {
            var handlerManager = (XmppHandlerManager)serviceProvider.GetService(typeof(XmppHandlerManager));
            var serviceManager = (XmppServiceManager)serviceProvider.GetService(typeof(XmppServiceManager));

            foreach (var h in Handlers) handlerManager.RemoveXmppHandler(h);
            Registered = false;
            OnUnregisterCore(handlerManager, serviceManager, serviceProvider);
        }

        protected virtual void OnRegisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider serviceProvider)
        {

        }

        protected virtual void OnUnregisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider serviceProvider)
        {

        }


        private void DiscoveryFearures(IList<IXmppHandler> handlers)
        {
            foreach (var handler in handlers)
            {
                foreach (XmppHandlerAttribute attribute in handler.GetType().GetCustomAttributes(typeof(XmppHandlerAttribute), true))
                {
                    var nameSpace = ElementFactory.GetElementNamespace(attribute.XmppElementType);
                    if (!string.IsNullOrEmpty(nameSpace) && !DiscoInfo.HasFeature(nameSpace))
                    {
                        DiscoInfo.AddFeature(new DiscoFeature(nameSpace));
                    }
                }
            }
        }
    }
}