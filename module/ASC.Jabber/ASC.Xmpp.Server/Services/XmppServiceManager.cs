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
using System.Threading;
using ASC.Collections;
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using log4net;

namespace ASC.Xmpp.Server.Services
{
    public class XmppServiceManager
    {
        private IServiceProvider serviceProvider;

        private IDictionary<Jid, IXmppService> services = new SynchronizedDictionary<Jid, IXmppService>();

        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static ILog log = LogManager.GetLogger(typeof(XmppServiceManager));


        public XmppServiceManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            this.serviceProvider = serviceProvider;
        }

        public void RegisterService(IXmppService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            try
            {
                locker.EnterWriteLock();
                services.Add(service.Jid, service);
            }
            finally
            {
                locker.ExitWriteLock();
            }

            log.DebugFormat("Register XMPP service '{0}' on '{1}'", service.Name, service.Jid);

            try
            {
                service.OnRegister(serviceProvider);
            }
            catch (Exception error)
            {
                log.ErrorFormat("Error on register service '{0}' and it has will unloaded. {1}", service.Name, error);
                UnregisterService(service.Jid);                
                throw;
            }
        }

        public void UnregisterService(Jid address)
        {
            if (address == null) throw new ArgumentNullException("address");
            IXmppService service = null;
            try
            {
                locker.EnterWriteLock();
                if (services.ContainsKey(address))
                {
                    service = services[address];
                    services.Remove(address);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }

            if (service != null)
            {
                log.DebugFormat("Unregister XMPP service '{0}' on '{1}'", service.Name, service.Jid);
                service.OnUnregister(serviceProvider);
            }
        }

        public ICollection<IXmppService> GetChildServices(IXmppService parentService)
        {
            return GetChildServices(parentService != null ? parentService.Jid : null);
        }

        public ICollection<IXmppService> GetChildServices(Jid parentAddress)
        {
            try
            {
                locker.EnterReadLock();

                var list = new List<IXmppService>();
                foreach (var s in services.Values)
                {
                    var parentJid = s.ParentService != null ? s.ParentService.Jid : null;
                    if (parentAddress == parentJid) list.Add(s);
                }
                return list;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public IXmppService GetService(Jid address)
        {
            if (address == null) return null;
            try
            {
                locker.EnterReadLock();
                return services.ContainsKey(address) ? services[address] : null;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
    }
}