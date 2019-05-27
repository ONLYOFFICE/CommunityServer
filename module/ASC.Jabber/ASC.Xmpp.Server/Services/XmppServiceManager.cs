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


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ASC.Common.Logging;
using ASC.Xmpp.Core.protocol;

namespace ASC.Xmpp.Server.Services
{
    public class XmppServiceManager
    {
        private readonly IServiceProvider serviceProvider;

        private readonly ConcurrentDictionary<Jid, IXmppService> services = new ConcurrentDictionary<Jid, IXmppService>();

        private readonly static ILog log = LogManager.GetLogger("ASC");


        public XmppServiceManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            this.serviceProvider = serviceProvider;
        }

        public void RegisterService(IXmppService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            services.TryAdd(service.Jid, service);

            log.DebugFormat("Register XMPP service '{0}' on '{1}'", service.Name, service.Jid);

            try
            {
                service.OnRegister(serviceProvider);
            }
            catch (Exception error)
            {
                log.ErrorFormat("Error on register service '{0}' and it will be unloaded. {1}", service.Name, error);
                UnregisterService(service.Jid);
                throw;
            }
        }

        public void UnregisterService(Jid address)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            IXmppService service;
            if (services.TryRemove(address, out service))
            {
                log.DebugFormat("Unregister XMPP service '{0}' on '{1}'", service.Name, service.Jid);
                service.OnUnregister(serviceProvider);
            }
        }

        public ICollection<IXmppService> GetChildServices(Jid parentAddress)
        {
            var list = new List<IXmppService>();
            foreach (var s in services.Values)
            {
                var parentJid = s.ParentService != null ? s.ParentService.Jid : null;
                if (parentAddress == parentJid)
                    list.Add(s);
            }
            return list;
        }

        public IXmppService GetService(Jid address)
        {
            if (address == null)
                return null;

            IXmppService service;
            services.TryGetValue(address, out service);
            return service;
        }
    }
}