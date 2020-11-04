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
using System.Linq;
using System.Net.Mail;
using System.Security;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Extensions;
using ASC.Mail.Server.Core.Entities;
using ASC.Mail.Server.Utils;
using ASC.Mail.Utils;
using ASC.Web.Core;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Mail.Core.Engine
{
    public class ServerEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public ServerEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.ServerEngine");
        }

        public List<MailAddressInfo> GetAliases(int mailboxId)
        {
            const string server_address = "sa";
            const string server_domain = "sd";

            var queryForExecution = new SqlQuery(ServerAddressTable.TABLE_NAME.Alias(server_address))
                .Select(ServerAddressTable.Columns.Id.Prefix(server_address))
                .Select(ServerAddressTable.Columns.AddressName.Prefix(server_address))
                .Select(ServerDomainTable.Columns.DomainName.Prefix(server_domain))
                .Select(ServerDomainTable.Columns.Id.Prefix(server_domain))
                .InnerJoin(ServerDomainTable.TABLE_NAME.Alias(server_domain),
                    Exp.EqColumns(ServerAddressTable.Columns.DomainId.Prefix(server_address),
                        ServerDomainTable.Columns.Id.Prefix(server_domain)))
                .Where(ServerAddressTable.Columns.MailboxId.Prefix(server_address), mailboxId)
                .Where(ServerAddressTable.Columns.IsAlias.Prefix(server_address), true);

            using (var daoFactory = new DaoFactory())
            {
                var res = daoFactory.DbManager.ExecuteList(queryForExecution);
                if (res == null)
                {
                    return new List<MailAddressInfo>();
                }
                return res.ConvertAll(r => new MailAddressInfo(Convert.ToInt32(r[0]),
                      string.Format("{0}@{1}", r[1], r[2]), Convert.ToInt32(r[3])));
            }
        }

        public List<MailAddressInfo> GetGroups(int mailboxId)
        {
            const string groups = "sg";
            const string group_x_address = "ga";
            const string server_address = "sa";
            const string server_domain = "sd";

            var queryForExecution = new SqlQuery(ServerAddressTable.TABLE_NAME.Alias(server_address))
                .Select(ServerMailGroupTable.Columns.Id.Prefix(groups))
                .Select(ServerMailGroupTable.Columns.Address.Prefix(groups))
                .Select(ServerDomainTable.Columns.Id.Prefix(server_domain))
                .InnerJoin(ServerDomainTable.TABLE_NAME.Alias(server_domain),
                           Exp.EqColumns(ServerAddressTable.Columns.DomainId.Prefix(server_address),
                                         ServerDomainTable.Columns.Id.Prefix(server_domain)))
                .InnerJoin(ServerMailGroupXAddressesTable.TABLE_NAME.Alias(group_x_address),
                           Exp.EqColumns(ServerAddressTable.Columns.Id.Prefix(server_address),
                                         ServerMailGroupXAddressesTable.Columns.AddressId.Prefix(group_x_address)))
                .InnerJoin(ServerMailGroupTable.TABLE_NAME.Alias(groups),
                           Exp.EqColumns(ServerMailGroupXAddressesTable.Columns.MailGroupId.Prefix(group_x_address),
                                         ServerMailGroupTable.Columns.Id.Prefix(groups)))
                .Where(ServerAddressTable.Columns.MailboxId.Prefix(server_address), mailboxId);

            using (var daoFactory = new DaoFactory())
            {
                var res = daoFactory.DbManager.ExecuteList(queryForExecution);
                if (res == null)
                {
                    return new List<MailAddressInfo>();
                }
                return res.ConvertAll(r =>
                    new MailAddressInfo(Convert.ToInt32(r[0]), Convert.ToString(r[1]), Convert.ToInt32(r[2])));
            }
        }

        public Entities.Server GetLinkedServer()
        {
            using (var daoFactory = new DaoFactory())
            {
                var serverDao = daoFactory.CreateServerDao();

                var linkedServer = serverDao.Get(Tenant);

                return linkedServer;
            }
        }

        public List<Entities.Server> GetAllServers()
        {
            using (var daoFactory = new DaoFactory())
            {
                return GetAllServers(daoFactory);
            }
        }

        private static List<Entities.Server> GetAllServers(IDaoFactory daoFactory)
        {
            var serverDao = daoFactory.CreateServerDao();

            var servers = serverDao.GetList();

            return servers;
        }

        public void Link(Entities.Server server, int tenant)
        {
            if(server == null)
                return;

            using (var daoFactory = new DaoFactory())
            {
                Link(daoFactory, server, tenant);
            }
        }

        public void Link(IDaoFactory daoFactory, Entities.Server server, int tenant)
        {
            if (server == null)
                return;

            var serverDao = daoFactory.CreateServerDao();

            var result = serverDao.Link(server, Tenant);

            if (result <= 0)
                throw new Exception("Invalid insert operation");
        }

        public void UnLink(Entities.Server server, int tenant)
        {
            if (server == null)
                return;

            using (var daoFactory = new DaoFactory())
            {
                var serverDao = daoFactory.CreateServerDao();

                serverDao.UnLink(server, Tenant);
            }
        }

        public int Save(Entities.Server server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            using (var daoFactory = new DaoFactory())
            {
                var serverDao = daoFactory.CreateServerDao();

                var id = serverDao.Save(server);

                return id;
            }
        }

        public void Delete(int serverId)
        {
            if (serverId <= 0)
                throw new ArgumentOutOfRangeException("serverId");

            using (var daoFactory = new DaoFactory())
            {
                var serverDao = daoFactory.CreateServerDao();

                serverDao.Delete(serverId);
            }
        }

        public Entities.Server GetOrCreate(IDaoFactory daoFactory)
        {
            var serverDao = daoFactory.CreateServerDao();

            var linkedServer = serverDao.Get(Tenant);

            if (linkedServer != null) 
                return linkedServer;

            var servers = GetAllServers(daoFactory);

            if (!servers.Any())
                throw new Exception("Mail Server not configured");

            var server = servers.First();

            Link(daoFactory, server, Tenant);

            linkedServer = server;

            return linkedServer;
        }

        public ServerData GetMailServer()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            using (var daoFactory = new DaoFactory())
            {
                return GetMailServer(daoFactory);
            }
        }

        public string GetMailServerMxDomain()
        {
            using (var daoFactory = new DaoFactory())
            {
                var server = GetMailServer(daoFactory);

                return server.Dns.MxRecord.Host;
            }
        }

        private ServerData GetMailServer(IDaoFactory daoFactory)
        {
            var linkedServer = GetOrCreate(daoFactory);

            var dns = GetOrCreateUnusedDnsData(daoFactory, linkedServer);

            var mailboxServerDao = daoFactory.CreateMailboxServerDao();

            var inServer = mailboxServerDao.GetServer(linkedServer.ImapSettingsId);
            var outServer = mailboxServerDao.GetServer(linkedServer.SmtpSettingsId);

            return new ServerData
            {
                Id = linkedServer.Id,
                Dns = dns,
                ServerLimits = new ServerLimitData
                {
                    MailboxMaxCountPerUser = Defines.ServerDomainMailboxPerUserLimit
                },
                InServer = inServer,
                OutServer = outServer
            };
        }

        public ServerDomainDnsData GetOrCreateUnusedDnsData()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            using (var daoFactory = new DaoFactory())
            {
                var server = GetOrCreate(daoFactory);
                return GetOrCreateUnusedDnsData(daoFactory, server);
            }
        }

        public ServerDomainDnsData GetOrCreateUnusedDnsData(IDaoFactory daoFactory, Entities.Server server)
        {
            var serverDnsDao = daoFactory.CreateServerDnsDao(Tenant, User);

            var dnsSettings = serverDnsDao.GetFree();

            if (dnsSettings == null)
            {
                string privateKey, publicKey;
                CryptoUtil.GenerateDkimKeys(out privateKey, out publicKey);

                var domainCheckValue = PasswordGenerator.GenerateNewPassword(16);
                var domainCheck = Defines.ServerDnsDomainCheckPrefix + ": " + domainCheckValue;

                var serverDns = new ServerDns
                {
                    Id = 0,
                    Tenant = Tenant,
                    User = User,
                    DomainId = Defines.UNUSED_DNS_SETTING_DOMAIN_ID,
                    DomainCheck = domainCheck,
                    DkimSelector = Defines.ServerDnsDkimSelector,
                    DkimPrivateKey = privateKey,
                    DkimPublicKey = publicKey,
                    DkimTtl = Defines.ServerDnsDefaultTtl,
                    DkimVerified = false,
                    DkimDateChecked = null,
                    Spf = Defines.ServerDnsSpfRecordValue,
                    SpfTtl = Defines.ServerDnsDefaultTtl,
                    SpfVerified = false,
                    SpfDateChecked = null,
                    Mx = server.MxRecord,
                    MxTtl = Defines.ServerDnsDefaultTtl,
                    MxVerified = false,
                    MxDateChecked = null,
                    TimeModified = DateTime.UtcNow
                };

                serverDns.Id = serverDnsDao.Save(serverDns);

                dnsSettings = serverDns;
            }

            var dnsData = new ServerDomainDnsData
            {
                Id = dnsSettings.Id,
                MxRecord = new ServerDomainMxRecordData
                {
                    Host = dnsSettings.Mx,
                    IsVerified = false,
                    Priority = Defines.ServerDnsMxRecordPriority
                },
                DkimRecord = new ServerDomainDkimRecordData
                {
                    Selector = dnsSettings.DkimSelector,
                    IsVerified = false,
                    PublicKey = dnsSettings.DkimPublicKey
                },
                DomainCheckRecord = new ServerDomainDnsRecordData
                {
                    Name = Defines.DNS_DEFAULT_ORIGIN,
                    IsVerified = false,
                    Value = dnsSettings.DomainCheck
                },
                SpfRecord = new ServerDomainDnsRecordData
                {
                    Name = Defines.DNS_DEFAULT_ORIGIN,
                    IsVerified = false,
                    Value = dnsSettings.Spf
                }
            };

            return dnsData;
        }

        public bool CheckDomainOwnership(string domain)
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

            var dns = GetOrCreateUnusedDnsData();

            var dnsLookup = new DnsLookup();

            return dnsLookup.IsDomainTxtRecordExists(domainName, dns.DomainCheckRecord.Value);
        }

        public ServerNotificationAddressData CreateNotificationAddress(string localPart, string password, int domainId)
        {
            if (!CoreContext.Configuration.Standalone)
                throw new SecurityException("Only for standalone");

            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(localPart))
                throw new ArgumentNullException("localPart", @"Invalid address username.");

            localPart = localPart.ToLowerInvariant().Trim();

            if (localPart.Length > 64)
                throw new ArgumentException(@"Local part of address exceed limitation of 64 characters.", "localPart");

            if (!Parser.IsEmailLocalPartValid(localPart))
                throw new ArgumentException(@"Incorrect address username.", "localPart");

            var trimPwd = Parser.GetValidPassword(password);

            if (domainId < 0)
                throw new ArgumentException(@"Invalid domain id.", "domainId");

            var notificationAddressSettings = ServerNotificationAddressSettings.LoadForTenant(Tenant);

            if (!string.IsNullOrEmpty(notificationAddressSettings.NotificationAddress))
            {
                RemoveNotificationAddress(notificationAddressSettings.NotificationAddress);
            }

            var utcNow = DateTime.UtcNow;

            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);
                var serverDomain = serverDomainDao.GetDomain(domainId);

                if (localPart.Length + serverDomain.Name.Length > 318) // 318 because of @ sign
                    throw new ArgumentException(@"Address of mailbox exceed limitation of 319 characters.", "localPart");

                var login = string.Format("{0}@{1}", localPart, serverDomain.Name);

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                if (server == null)
                    throw new ArgumentException("Server not configured");

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                var maildir = PostfixMaildirUtil.GenerateMaildirPath(serverDomain.Name, localPart, utcNow);

                var serverMailbox = new Server.Core.Entities.Mailbox
                {
                    Name = localPart,
                    Password = trimPwd,
                    Login = login,
                    LocalPart = localPart,
                    Domain = serverDomain.Name,
                    Active = true,
                    Quota = 0,
                    Maldir = maildir,
                    Modified = utcNow,
                    Created = utcNow,
                };

                var serverAddress = new Alias
                {
                    Name = localPart,
                    Address = login,
                    GoTo = login,
                    Domain = serverDomain.Name,
                    IsActive = true,
                    IsGroup = false,
                    Modified = utcNow,
                    Created = utcNow
                };

                engine.SaveMailbox(serverMailbox, serverAddress, false);

                notificationAddressSettings = new ServerNotificationAddressSettings {NotificationAddress = login};

                notificationAddressSettings.SaveForTenant(Tenant);

                var mailboxServerDao = daoFactory.CreateMailboxServerDao();

                var smtpSettings = mailboxServerDao.GetServer(server.SmtpSettingsId);

                var address = new MailAddress(login);

                var notifyAddress = new ServerNotificationAddressData
                {
                    Email = address.ToString(),
                    SmtpPort = smtpSettings.Port,
                    SmtpServer = smtpSettings.Hostname,
                    SmtpAccount = address.ToLogin(smtpSettings.Username),
                    SmptEncryptionType = smtpSettings.SocketType,
                    SmtpAuth = true,
                    SmtpAuthenticationType = smtpSettings.Authentication
                };

                return notifyAddress;
            }
        }

        public void RemoveNotificationAddress(string address)
        {
            if (!CoreContext.Configuration.Standalone)
                throw new SecurityException("Only for standalone");

            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");

            var deleteAddress = address.ToLowerInvariant().Trim();
            var notificationAddressSettings = ServerNotificationAddressSettings.LoadForTenant(Tenant);

            if (notificationAddressSettings.NotificationAddress != deleteAddress)
                throw new ArgumentException("Mailbox not exists");

            var mailAddress = new MailAddress(deleteAddress);

            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);
                var serverDomain = serverDomainDao.GetDomains().FirstOrDefault(d => d.Name == mailAddress.Host);

                if(serverDomain == null)
                    throw new ArgumentException("Domain not exists");

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                if (server == null)
                    throw new ArgumentException("Server not configured");

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                engine.RemoveMailbox(deleteAddress);
            }

            var addressSettings = notificationAddressSettings.GetDefault() as ServerNotificationAddressSettings;
            if (addressSettings != null && !addressSettings.SaveForTenant(Tenant))
            {
                throw new Exception("Could not delete notification address setting.");
            }
        }

        public ServerFullData GetMailServerFullInfo()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            var fullServerInfo = new ServerFullData();
            var mailboxDataList = new List<ServerMailboxData>();
            var mailgroupDataList = new List<ServerDomainGroupData>();
            
            var domainEngine = new ServerDomainEngine(Tenant, User);

            using (var daoFactory = new DaoFactory())
            {
                var server = GetMailServer(daoFactory);

                var domains = domainEngine.GetDomains(daoFactory);

                var mailboxDao = daoFactory.CreateMailboxDao();

                var mailboxes = mailboxDao.GetMailBoxes(new TenantServerMailboxesExp(Tenant));

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var addresses = serverAddressDao.GetList();

                foreach (var mailbox in mailboxes)
                {
                    var address =
                        addresses.FirstOrDefault(
                            a => a.MailboxId == mailbox.Id && a.IsAlias == false && a.IsMailGroup == false);

                    if (address == null)
                        continue;

                    var domain = domains.FirstOrDefault(d => d.Id == address.DomainId);

                    if (domain == null)
                        continue;

                    var serverAddressData = ServerMailboxEngine.ToServerDomainAddressData(address, domain);

                    var aliases =
                        addresses.Where(a => a.MailboxId == mailbox.Id && a.IsAlias && !a.IsMailGroup)
                            .ToList()
                            .ConvertAll(a => ServerMailboxEngine.ToServerDomainAddressData(a, domain));

                    mailboxDataList.Add(ServerMailboxEngine.ToMailboxData(mailbox, serverAddressData, aliases));
                }

                var serverGroupDao = daoFactory.CreateServerGroupDao(Tenant);

                var groups = serverGroupDao.GetList();

                foreach (var serverGroup in groups)
                {
                    var address =
                        addresses.FirstOrDefault(
                            a => a.Id == serverGroup.AddressId && !a.IsAlias && a.IsMailGroup);

                    if (address == null)
                        continue;

                    var domain = domains.FirstOrDefault(d => d.Id == address.DomainId);

                    if (domain == null)
                        continue;

                    var email = string.Format("{0}@{1}", address.AddressName, domain.Name);

                    var serverGroupAddress = ServerMailboxEngine.ToServerDomainAddressData(address, email);

                    var serverGroupAddresses =
                        serverAddressDao.GetGroupAddresses(serverGroup.Id)
                            .ConvertAll(a => ServerMailboxEngine.ToServerDomainAddressData(a,
                                string.Format("{0}@{1}", a.AddressName, domain.Name)));

                    mailgroupDataList.Add(ServerMailgroupEngine.ToServerDomainGroupData(serverGroup.Id, serverGroupAddress, serverGroupAddresses));
                }

                fullServerInfo.Server = server;
                fullServerInfo.Domains = domains;
                fullServerInfo.Mailboxes = mailboxDataList;
                fullServerInfo.Mailgroups = mailgroupDataList;
            }

            return fullServerInfo;
        }

        public string GetServerVersion()
        {
            using (var daoFactory = new DaoFactory())
            {
                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                if (server == null)
                    return null;

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);
                var version = engine.GetVersion();
                return version;
            }
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
