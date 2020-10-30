/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.Engine.Operations;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Extensions;
using ASC.Mail.Utils;
using ASC.Web.Core;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Mail.Core.Engine
{
    public class ServerDomainEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public ServerDomainEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.ServerDomainEngine");
        }

        public ServerDomainData GetDomain(int id)
        {
            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var serverDomain = serverDomainDao.GetDomain(id);

                if (serverDomain == null)
                    throw new Exception("Domain not found");

                var serverDnsDao = daoFactory.CreateServerDnsDao(Tenant, User);

                var dnsData = UpdateDnsStatus(serverDomain, serverDnsDao, serverDomainDao);

                return ToServerDomainData(serverDomain, dnsData);
            }
        }

        public List<ServerDomainData> GetDomains()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            using (var daoFactory = new DaoFactory())
            {
                return GetDomains(daoFactory);
            }
        }

        public List<ServerDomain> GetAllDomains()
        {
            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var domains = serverDomainDao.GetAllDomains();

                return domains;
            }
        }

        public List<ServerDomainData> GetDomains(IDaoFactory daoFactory)
        {
            var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

            var listDomains = serverDomainDao.GetDomains();

            if (!listDomains.Any())
                return new List<ServerDomainData>();

            var serverDnsDao = daoFactory.CreateServerDnsDao(Tenant, User);

            var list = new List<ServerDomainData>();

            foreach (var domain in listDomains)
            {
                var dnsData = UpdateDnsStatus(domain, serverDnsDao, serverDomainDao);

                var serverDomain = ToServerDomainData(domain, dnsData);

                list.Add(serverDomain);
            }

            return list;
        }

        public ServerDomainData GetCommonDomain()
        {
            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var domainCommon = serverDomainDao.GetDomains()
                    .SingleOrDefault(x => x.Tenant == Defines.SHARED_TENANT_ID);

                if (domainCommon == null)
                    return null;

                var serverDomain = ToServerDomainData(domainCommon, null);

                return serverDomain;
            }
        }

        public ServerDomainDnsData GetDnsData(int domainId)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (domainId < 0)
                throw new ArgumentException(@"Invalid domain id.", "domainId");

            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var domain = serverDomainDao.GetDomain(domainId);

                if (domain == null)
                    return null;

                var serverDnsDao = daoFactory.CreateServerDnsDao(Tenant, User);

                var dnsData = UpdateDnsStatus(domain, serverDnsDao, serverDomainDao, true);

                return dnsData;
            }
        }

        public bool IsDomainExists(string name)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"Invalid domain name.", "name");

            if (name.Length > 255)
                throw new ArgumentException(@"Domain name exceed limitation of 255 characters.", "name");

            if (!Parser.IsDomainValid(name))
                throw new ArgumentException(@"Incorrect domain name.", "name");

            var domainName = name.ToLowerInvariant();

            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);
                return serverDomainDao.IsDomainExists(domainName);
            }
        }

        public ServerDomainData AddDomain(string domain, int dnsId)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(domain))
                throw new ArgumentException(@"Invalid domain name.", "domain");

            if (domain.Length > 255)
                throw new ArgumentException(@"Domain name exceed limitation of 255 characters.", "domain");

            if (!Parser.IsDomainValid(domain))
                throw new ArgumentException(@"Incorrect domain name.", "domain");

            var domainName = domain.ToLowerInvariant();

            var engine = new EngineFactory(Tenant, User);

            var dnsLookup = new DnsLookup();

            using (var daoFactory = new DaoFactory())
            {
                var serverDao = daoFactory.CreateServerDao();

                var server = serverDao.Get(Tenant);

                var freeDns = engine.ServerEngine.GetOrCreateUnusedDnsData(daoFactory, server);

                if (freeDns.Id != dnsId)
                    throw new InvalidDataException("This dkim public key is already in use. Please reopen wizard again.");

                if (!CoreContext.Configuration.Standalone &&
                    !dnsLookup.IsDomainTxtRecordExists(domainName, freeDns.DomainCheckRecord.Value))
                {
                    throw new InvalidOperationException("txt record is not correct.");
                }

                var isVerified = freeDns.CheckDnsStatus(domainName);

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                    var utcNow = DateTime.UtcNow;

                    var mailServerEngine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                    var mailServerDomain = new Server.Core.Entities.Domain
                    {
                        Name = domainName,
                        Active = true,
                        Description = string.Format("Domain created in UtcTime: {0}, for tenant: {1}", utcNow, Tenant),
                        Created = utcNow,
                        Modified = utcNow
                    };

                    mailServerEngine.SaveDomain(mailServerDomain);

                    var serverDomain = new ServerDomain
                    {
                        Id = 0,
                        Tenant = Tenant,
                        Name = domainName,
                        IsVerified = isVerified,
                        DateAdded = utcNow,
                        DateChecked = utcNow
                    };

                    serverDomain.Id = serverDomainDao.Save(serverDomain);

                    var serverDnsDao = daoFactory.CreateServerDnsDao(Tenant, User);
                    var serverDns = serverDnsDao.GetById(freeDns.Id);

                    var mailServerDkim = new Server.Core.Entities.Dkim
                    {
                        DomainName = domainName,
                        Selector = serverDns.DkimSelector,
                        PrivateKey = serverDns.DkimPrivateKey,
                        PublicKey = serverDns.DkimPublicKey
                    };

                    mailServerEngine.SaveDkim(mailServerDkim);

                    serverDns.DomainId = serverDomain.Id;
                    serverDns.TimeModified = utcNow;
                    serverDnsDao.Save(serverDns);

                    tx.Commit();

                    return ToServerDomainData(serverDomain, freeDns);
                }
            }
        }

        public MailOperationStatus RemoveDomain(int id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (id < 0)
                throw new ArgumentException(@"Invalid domain id.", "id");

            var domain = GetDomain(id);

            if (domain.IsSharedDomain)
                throw new SecurityException("Can not remove shared domain.");

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operationEngine = new OperationEngine();

            var operations = operationEngine.MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.RemoveDomain;
                })
                .ToList();

            var sameOperation = operations.FirstOrDefault(o =>
            {
                var oSource = o.GetProperty<string>(MailOperation.SOURCE);
                return oSource == domain.Id.ToString();
            });

            if (sameOperation != null)
            {
                return operationEngine.GetMailOperationStatus(sameOperation.Id);
            }

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                throw new MailOperationAlreadyRunningException("Remove mailbox operation already running.");

            var op = new MailRemoveMailserverDomainOperation(tenant, user, domain);

            return operationEngine.QueueTask(op);
        }

        private ServerDomainDnsData UpdateDnsStatus(ServerDomain domain, 
            IServerDnsDao serverDnsDao, IServerDomainDao serverDomainDao, bool force = false)
        {
            var serverDns = serverDnsDao.Get(domain.Id);

            if (serverDns.UpdateRecords(domain.Name, force))
            {
                serverDnsDao.Save(serverDns);
            }

            var dnsData = ToServerDomainDnsData(serverDns);

            if (dnsData != null && domain.IsVerified != dnsData.IsVerified)
            {
                serverDomainDao.SetVerified(domain.Id, dnsData.IsVerified);
            }

            return dnsData;
        }

        protected ServerDomainData ToServerDomainData(ServerDomain domain, ServerDomainDnsData dns)
        {
            var serverDomain = new ServerDomainData
            {
                Id = domain.Id,
                Name = domain.Name,
                Dns = dns,
                IsSharedDomain = domain.Tenant == Defines.SHARED_TENANT_ID
            };

            return serverDomain;
        }

        protected ServerDomainDnsData ToServerDomainDnsData(ServerDns serverDns)
        {
            if (serverDns == null)
                return null;

            var dnsData = new ServerDomainDnsData
            {
                Id = serverDns.Id,
                MxRecord = new ServerDomainMxRecordData
                {
                    Host = serverDns.Mx,
                    IsVerified = serverDns.MxVerified,
                    Priority = Defines.ServerDnsMxRecordPriority
                },
                SpfRecord = new ServerDomainDnsRecordData
                {
                    Name = Defines.DNS_DEFAULT_ORIGIN,
                    IsVerified = serverDns.SpfVerified,
                    Value = serverDns.Spf
                },
                DkimRecord = new ServerDomainDkimRecordData
                {
                    Selector = serverDns.DkimSelector,
                    IsVerified = serverDns.DkimVerified,
                    PublicKey = serverDns.DkimPublicKey
                },
                DomainCheckRecord = new ServerDomainDnsRecordData
                {
                    Name = Defines.DNS_DEFAULT_ORIGIN,
                    IsVerified = true,
                    Value = serverDns.DomainCheck
                }
            };

            return dnsData;
        }

        private static bool IsAdmin
        {
            get
            {
                return WebItemSecurity.IsProductAdministrator(WebItemManager.MailProductID, SecurityContext.CurrentAccount.ID);
            }
        }
    }
}
