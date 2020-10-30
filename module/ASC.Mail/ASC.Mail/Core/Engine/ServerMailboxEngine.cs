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
using System.Net.Mail;
using System.Security;
using ASC.Api.Exceptions;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Users;
using ASC.Mail.Authorization;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine.Operations;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Server.Core.Entities;
using ASC.Mail.Server.Utils;
using ASC.Mail.Utils;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using Mailbox = ASC.Mail.Core.Entities.Mailbox;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Mail.Core.Engine
{
    public class ServerMailboxEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public ServerMailboxEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.ServerMailboxEngine");
        }

        public List<ServerMailboxData> GetMailboxes()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            var list = new List<ServerMailboxData>();

            using (var daoFactory = new DaoFactory())
            {
                var mailboxDao = daoFactory.CreateMailboxDao();
                var mailboxes = mailboxDao.GetMailBoxes(new TenantServerMailboxesExp(Tenant));

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var addresses = serverAddressDao.GetList();

                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var domains = serverDomainDao.GetDomains();

                list.AddRange(from mailbox in mailboxes
                    let address =
                        addresses.FirstOrDefault(
                            a => a.MailboxId == mailbox.Id && a.IsAlias == false && a.IsMailGroup == false)
                    where address != null
                    let domain = domains.FirstOrDefault(d => d.Id == address.DomainId)
                    where domain != null
                    let serverAddressData = ToServerDomainAddressData(address, domain)
                    let aliases =
                        addresses.Where(a => a.MailboxId == mailbox.Id && a.IsAlias && !a.IsMailGroup)
                            .ToList()
                            .ConvertAll(a => ToServerDomainAddressData(a, domain))
                    select ToMailboxData(mailbox, serverAddressData, aliases));
            }

            return list;
        }

        public bool IsAddressAlreadyRegistered(string localPart, int domainId)
        {
            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var serverDomain = serverDomainDao.GetDomain(domainId);

                var isSharedDomain = serverDomain.Tenant == Defines.SHARED_TENANT_ID;

                if (!IsAdmin && !isSharedDomain)
                    throw new SecurityException("Need admin privileges.");

                var tenantQuota = CoreContext.TenantManager.GetTenantQuota(Tenant);

                if (isSharedDomain
                    && (tenantQuota.Trial
                        || tenantQuota.Free))
                {
                    throw new SecurityException("Not available in unpaid version");
                }

                if (string.IsNullOrEmpty(localPart))
                    throw new ArgumentException(@"Invalid local part.", "localPart");

                if (domainId < 0)
                    throw new ArgumentException(@"Invalid domain id.", "domainId");

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var state = serverAddressDao.IsAddressAlreadyRegistered(localPart, serverDomain.Name);

                return state;
            }
        }

        public bool IsAddressValid(string localPart, int domainId)
        {
            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var serverDomain = serverDomainDao.GetDomain(domainId);

                var isSharedDomain = serverDomain.Tenant == Defines.SHARED_TENANT_ID;

                if (!IsAdmin && !isSharedDomain)
                    throw new SecurityException("Need admin privileges.");

                var tenantQuota = CoreContext.TenantManager.GetTenantQuota(Tenant);

                if (isSharedDomain
                    && (tenantQuota.Trial
                        || tenantQuota.Free))
                {
                    throw new SecurityException("Not available in unpaid version");
                }

                if (string.IsNullOrEmpty(localPart))
                    return false;

                if (domainId < 0)
                    return false;

                if (localPart.Length > 64)
                    return false;

                if (!Parser.IsEmailLocalPartValid(localPart))
                    return false;

                return true;
            }
        }

        public ServerMailboxData CreateMailbox(string name, string localPart, int domainId, string userId)
        {
            ServerMailboxData mailboxData;

            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var serverDomain = serverDomainDao.GetDomain(domainId);

                var isSharedDomain = serverDomain.Tenant == Defines.SHARED_TENANT_ID;

                if (!IsAdmin && !isSharedDomain)
                    throw new SecurityException("Need admin privileges.");

                var tenantQuota = CoreContext.TenantManager.GetTenantQuota(Tenant);

                if (isSharedDomain
                    && (tenantQuota.Trial
                        || tenantQuota.Free))
                {
                    throw new SecurityException("Not available in unpaid version");
                }

                if (string.IsNullOrEmpty(localPart))
                    throw new ArgumentException(@"Invalid local part.", "localPart");

                if (domainId < 0)
                    throw new ArgumentException(@"Invalid domain id.", "domainId");

                if (name.Length > 255)
                    throw new ArgumentException(@"Sender name exceed limitation of 64 characters.", "name");

                Guid user;

                if (!Guid.TryParse(userId, out user))
                    throw new ArgumentException(@"Invalid user id.", "userId");

                if (isSharedDomain && !IsAdmin && user != SecurityContext.CurrentAccount.ID)
                    throw new SecurityException(
                        "Creation of a shared mailbox is allowed only for the current account if user is not admin.");

                var teamlabAccount = CoreContext.Authentication.GetAccountByID(user);

                if (teamlabAccount == null)
                    throw new InvalidDataException("Unknown user.");

                var userInfo = CoreContext.UserManager.GetUsers(user);

                if (userInfo.IsVisitor())
                    throw new InvalidDataException("User is visitor.");

                if (localPart.Length > 64)
                    throw new ArgumentException(@"Local part of mailbox exceed limitation of 64 characters.",
                        "localPart");

                if (!Parser.IsEmailLocalPartValid(localPart))
                    throw new ArgumentException("Incorrect local part of mailbox.");

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                if (serverAddressDao.IsAddressAlreadyRegistered(localPart, serverDomain.Name))
                {
                    throw new DuplicateNameException("You want to create a mailbox with already existing address.");
                }

                if (Defines.ServerDomainMailboxPerUserLimit > 0)
                {
                    var engineFactory = new EngineFactory(Tenant, userId);

                    var accounts = engineFactory.AccountEngine.GetAccountInfoList();

                    var countDomainMailboxes =
                        accounts.Count(a =>
                            a.IsTeamlabMailbox &&
                            Parser.ParseAddress(a.Email)
                                .Domain.Equals(serverDomain.Name, StringComparison.InvariantCultureIgnoreCase));

                    if (countDomainMailboxes >= Defines.ServerDomainMailboxPerUserLimit)
                    {
                        throw new ArgumentOutOfRangeException(
                            string.Format("Count of user's mailboxes must be less or equal {0}.",
                                Defines.ServerDomainMailboxPerUserLimit));
                    }
                }

                var serverDao = daoFactory.CreateServerDao();

                var server = serverDao.Get(Tenant);

                var mailboxLocalPart = localPart.ToLowerInvariant();

                var login = string.Format("{0}@{1}", mailboxLocalPart, serverDomain.Name);

                var mailboxDao = daoFactory.CreateMailboxDao();

                var existMailbox = mailboxDao.GetMailBox(new СoncreteUserMailboxExp(new MailAddress(login), Tenant, userId));

                if (existMailbox != null) {
                    throw new DuplicateNameException("You want to create a mailbox that is already connected.");
                }

                var password = PasswordGenerator.GenerateNewPassword(12);

                var utcNow = DateTime.UtcNow;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var mailbox = new Mailbox
                    {
                        Id = 0,
                        Tenant = Tenant,
                        User = userId,
                        Name = name,
                        Address = login,
                        OAuthToken = null,
                        OAuthType = (int) AuthorizationServiceType.None,
                        ServerId = server.ImapSettingsId,
                        Password = password,
                        SmtpServerId = server.SmtpSettingsId,
                        SmtpPassword = password,
                        SizeLast = 0,
                        MsgCountLast = 0,
                        BeginDate = Defines.MinBeginDate,
                        Imap = true,
                        Enabled = true,
                        IsTeamlabMailbox = true,
                        IsRemoved = false,
                        DateCreated = utcNow
                    };

                    mailbox.Id = mailboxDao.SaveMailBox(mailbox);

                    var address = new ServerAddress
                    {
                        Id = 0,
                        Tenant = Tenant,
                        MailboxId = mailbox.Id,
                        DomainId = serverDomain.Id,
                        AddressName = localPart,
                        IsAlias = false,
                        IsMailGroup = false,
                        DateCreated = utcNow
                    };

                    address.Id = serverAddressDao.Save(address);

                    var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                    var maildir = PostfixMaildirUtil.GenerateMaildirPath(serverDomain.Name, localPart, utcNow);

                    var serverMailbox = new Server.Core.Entities.Mailbox
                    {
                        Name = name,
                        Password = password,
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
                        Name = name,
                        Address = login,
                        GoTo = login,
                        Domain = serverDomain.Name,
                        IsActive = true,
                        IsGroup = false,
                        Modified = utcNow,
                        Created = utcNow
                    };

                    engine.SaveMailbox(serverMailbox, serverAddress);

                    tx.Commit();

                    CacheEngine.Clear(userId);

                    mailboxData = ToMailboxData(mailbox, ToServerDomainAddressData(address, login),
                        new List<ServerDomainAddressData>());
                }
            }

            return mailboxData;
        }

        public ServerMailboxData CreateMyCommonDomainMailbox(string name)
        {
            if (!SetupInfo.IsVisibleSettings("AdministrationPage") || !SetupInfo.IsVisibleSettings("MailCommonDomain") ||
                CoreContext.Configuration.Standalone)
            {
                throw new Exception("Common domain is not available");
            }

            var serverDomainEngine = new ServerDomainEngine(Tenant, User);

            var domain = serverDomainEngine.GetCommonDomain();

            if (domain == null)
                throw new SecurityException("Domain not found.");

            var userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            return CreateMailbox(userInfo.DisplayUserName(), name, domain.Id, userInfo.ID.ToString());
        }

        public ServerMailboxData UpdateMailboxDisplayName(int mailboxId, string name)
        {
            using (var daoFactory = new DaoFactory())
            {
                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var serverMailboxAddresses = serverAddressDao.GetList(mailboxId);

                if (!serverMailboxAddresses.Any())
                    throw new ArgumentException("Mailbox not found");

                var serverMailboxAddress = serverMailboxAddresses.FirstOrDefault(a => !a.IsAlias && !a.IsMailGroup);

                if (serverMailboxAddress == null)
                    throw new ArgumentException("Mailbox not found");

                var serverMailboxAliases = serverMailboxAddresses.Where(a => a.IsAlias).ToList();

                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var serverDomain = serverDomainDao.GetDomain(serverMailboxAddress.DomainId);

                var isSharedDomain = serverDomain.Tenant == Defines.SHARED_TENANT_ID;

                if (!IsAdmin && !isSharedDomain)
                    throw new SecurityException("Need admin privileges.");

                var tenantQuota = CoreContext.TenantManager.GetTenantQuota(Tenant);

                if (isSharedDomain
                    && (tenantQuota.Trial
                        || tenantQuota.Free))
                {
                    throw new SecurityException("Not available in unpaid version");
                }

                if (name.Length > 255)
                    throw new ArgumentException(@"Sender name exceed limitation of 64 characters.", "name");

                var mailboxDao = daoFactory.CreateMailboxDao();

                var serverMailbox =
                    mailboxDao.GetMailBox(new ConcreteTenantServerMailboxExp(mailboxId, Tenant, false));

                serverMailbox.Name = name;

                mailboxDao.SaveMailBox(serverMailbox);

                CacheEngine.Clear(serverMailbox.User);

                var address = ToServerDomainAddressData(serverMailboxAddress, serverDomain);

                var aliases = serverMailboxAliases.ConvertAll(a => ToServerDomainAddressData(a, serverDomain));

                var mailboxData = ToMailboxData(serverMailbox, address, aliases);

                return mailboxData;
            }
        }

        public ServerDomainAddressData AddAlias(int mailboxId, string aliasName)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(aliasName))
                throw new ArgumentException(@"Invalid alias name.", "aliasName");

            if (mailboxId < 0)
                throw new ArgumentException(@"Invalid mailbox id.", "mailboxId");

            if (aliasName.Length > 64)
                throw new ArgumentException(@"Local part of mailbox alias exceed limitation of 64 characters.", "aliasName");

            if (!Parser.IsEmailLocalPartValid(aliasName))
                throw new ArgumentException("Incorrect mailbox alias.");

            var mailboxAliasName = aliasName.ToLowerInvariant();

            using (var daoFactory = new DaoFactory())
            {
                var mailboxDao = daoFactory.CreateMailboxDao();

                var mailbox = mailboxDao.GetMailBox(new ConcreteTenantMailboxExp(mailboxId, Tenant));

                if (mailbox == null)
                    throw new ArgumentException("Mailbox not exists");

                if (!mailbox.IsTeamlabMailbox)
                    throw new ArgumentException("Invalid mailbox type");

                if (mailbox.Tenant == Defines.SHARED_TENANT_ID)
                    throw new InvalidOperationException("Adding mailbox alias is not allowed for shared domain.");

                var mailAddress = new MailAddress(mailbox.Address);

                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);
                var serverDomain = serverDomainDao.GetDomains().FirstOrDefault(d => d.Name == mailAddress.Host);

                if (serverDomain == null)
                    throw new ArgumentException("Domain not exists");

                var mailboxAddress = mailAddress.Address;

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                if (serverAddressDao.IsAddressAlreadyRegistered(mailboxAliasName, serverDomain.Name))
                {
                    throw new DuplicateNameException("You want to create a mailbox with already existing address.");
                }

                var utcNow = DateTime.UtcNow;

                var address = new ServerAddress
                {
                    Id = 0,
                    Tenant = Tenant,
                    MailboxId = mailbox.Id,
                    DomainId = serverDomain.Id,
                    AddressName = mailboxAliasName,
                    IsAlias = true,
                    IsMailGroup = false,
                    DateCreated = utcNow
                };

                var aliasEmail = string.Format("{0}@{1}", mailboxAliasName, serverDomain.Name);

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    address.Id = serverAddressDao.Save(address);

                    var serverAddress = new Alias
                    {
                        Name = mailbox.Name,
                        Address = aliasEmail,
                        GoTo = mailboxAddress,
                        Domain = serverDomain.Name,
                        IsActive = true,
                        IsGroup = false,
                        Modified = utcNow,
                        Created = utcNow
                    };

                    engine.SaveAlias(serverAddress);

                    tx.Commit();
                }

                CacheEngine.Clear(mailbox.User);

                return new ServerDomainAddressData
                {
                    Id = address.Id,
                    DomainId = address.DomainId,
                    Email = aliasEmail
                };
            }
        }

        public void RemoveAlias(int mailboxId, int addressId)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (mailboxId < 0)
                throw new ArgumentException(@"Invalid address id.", "mailboxId");

            using (var daoFactory = new DaoFactory())
            {
                var mailboxDao = daoFactory.CreateMailboxDao();

                var mailbox = mailboxDao.GetMailBox(new ConcreteTenantServerMailboxExp(mailboxId, Tenant, false));

                if (mailbox == null)
                    throw new ArgumentException("Mailbox not exists");

                if (!mailbox.IsTeamlabMailbox)
                    throw new ArgumentException("Invalid mailbox type");

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var alias = serverAddressDao.Get(addressId);

                if (!alias.IsAlias)
                    throw new ArgumentException("Address is not alias");

                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);
                var serverDomain = serverDomainDao.GetDomain(alias.DomainId);

                if (serverDomain == null)
                    throw new ArgumentException("Domain not exists");

                var aliasEmail = string.Format("{0}@{1}", alias.AddressName, serverDomain.Name);

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    serverAddressDao.Delete(addressId);
                    engine.RemoveAlias(aliasEmail);

                    tx.Commit();
                }

                CacheEngine.Clear(mailbox.User);
            }
        }

        public void RemoveMailbox(MailBoxData mailBox)
        {
            var engine = new EngineFactory(mailBox.TenantId);

            using (var daoFactory = new DaoFactory())
            {
                var serverAddressDao = daoFactory.CreateServerAddressDao(mailBox.TenantId);

                var serverMailboxAddresses = serverAddressDao.GetList(mailBox.MailBoxId);

                var serverMailboxAddress = serverMailboxAddresses.FirstOrDefault(a => !a.IsAlias && !a.IsMailGroup);

                if (serverMailboxAddress == null)
                    throw new InvalidDataException("Mailbox address not found");

                var serverDomainDao = daoFactory.CreateServerDomainDao(mailBox.TenantId);
                var serverDomain = serverDomainDao.GetDomain(serverMailboxAddress.DomainId);

                if (serverDomain == null)
                    throw new InvalidDataException("Domain not found");

                var serverGroupDao = daoFactory.CreateServerGroupDao(mailBox.TenantId);

                var serverGroups = serverGroupDao.GetList();

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(mailBox.TenantId);

                var serverEngine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                var utcNow = DateTime.UtcNow;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    foreach (var serverGroup in serverGroups)
                    {
                        var addresses = serverAddressDao.GetGroupAddresses(serverGroup.Id);

                        var index = addresses.FindIndex(a => a.Id == serverMailboxAddress.Id);

                        if (index < 0)
                            continue;

                        addresses.RemoveAt(index);

                        if (addresses.Count == 0)
                        {
                            serverGroupDao.Delete(serverGroup.Id);

                            serverAddressDao.DeleteAddressesFromMailGroup(serverGroup.Id);

                            serverEngine.RemoveAlias(serverGroup.Address);
                        }
                        else
                        {
                            serverAddressDao.DeleteAddressFromMailGroup(serverGroup.Id, serverMailboxAddress.Id);

                            var goTo = string.Join(",",
                                addresses.Select(m => string.Format("{0}@{1}", m.AddressName, serverDomain.Name)));

                            var serverAddress = new Alias
                            {
                                Name = "",
                                Address = serverGroup.Address,
                                GoTo = goTo,
                                Domain = serverDomain.Name,
                                IsActive = true,
                                IsGroup = true,
                                Modified = utcNow,
                                Created = serverGroup.DateCreated
                            };

                            serverEngine.SaveAlias(serverAddress);
                        }
                    }

                    serverAddressDao.Delete(serverMailboxAddresses.Select(a => a.Id).ToList());

                    foreach (var mailboxAddress in serverMailboxAddresses)
                    {
                        serverEngine.RemoveAlias(string.Format("{0}@{1}", mailboxAddress.AddressName, serverDomain.Name));
                    }

                    engine.MailboxEngine.RemoveMailBox(daoFactory, mailBox, false);

                    serverEngine.RemoveMailbox(string.Format("{0}@{1}", serverMailboxAddress.AddressName,
                        serverDomain.Name));

                    tx.Commit();
                }
            }
        }

        public MailOperationStatus RemoveMailbox(int id)
        {
            if (id < 0)
                throw new ArgumentException(@"Invalid server mailbox id.", "id");

            var engine = new EngineFactory(Tenant);

            var mailbox = engine.MailboxEngine.GetMailboxData(new ConcreteTenantServerMailboxExp(id, Tenant, false));

            if (mailbox == null)
                throw new ItemNotFoundException("Mailbox not found.");

            var isSharedDomain = mailbox.TenantId == Defines.SHARED_TENANT_ID;

            if (!IsAdmin && !isSharedDomain)
                throw new SecurityException("Need admin privileges.");

            if (isSharedDomain && !IsAdmin &&
                !mailbox.UserId.Equals(SecurityContext.CurrentAccount.ID.ToString(),
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityException(
                    "Removing of a shared mailbox is allowed only for the current account if user is not admin.");
            }

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
                           oType == MailOperationType.RemoveMailbox;
                })
                .ToList();

            var sameOperation = operations.FirstOrDefault(o =>
            {
                var oSource = o.GetProperty<string>(MailOperation.SOURCE);
                return oSource == mailbox.MailBoxId.ToString();
            });

            if (sameOperation != null)
            {
                return operationEngine.GetMailOperationStatus(sameOperation.Id);
            }

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                throw new MailOperationAlreadyRunningException("Remove mailbox operation already running.");

            var op = new MailRemoveMailserverMailboxOperation(tenant, user, mailbox);

            return operationEngine.QueueTask(op);
        }

        public void ChangePassword(int mailboxId, string password)
        {
            if (!CoreContext.Configuration.Standalone)
                throw new SecurityException("Not allowed in this version");

            if (mailboxId < 0)
                throw new ArgumentException(@"Invalid mailbox id.", "mailboxId");

            var trimPwd = Parser.GetValidPassword(password);

            using (var daoFactory = new DaoFactory())
            {
                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var serverMailboxAddresses = serverAddressDao.GetList(mailboxId);

                if (!serverMailboxAddresses.Any())
                    throw new ArgumentException("Mailbox not found");

                var serverMailboxAddress = serverMailboxAddresses.FirstOrDefault(a => !a.IsAlias && !a.IsMailGroup);

                if (serverMailboxAddress == null)
                    throw new ArgumentException("Mailbox not found");

                var mailboxDao = daoFactory.CreateMailboxDao();

                var exp = IsAdmin
                    ? (IMailboxExp) new ConcreteTenantMailboxExp(mailboxId, Tenant)
                    : new СoncreteUserMailboxExp(mailboxId, Tenant, User);

                var mailbox =
                        mailboxDao.GetMailBox(exp);

                if (mailbox == null) // Mailbox has been removed
                    throw new ArgumentException("Mailbox not found");

                var serverDao = daoFactory.CreateServerDao();

                var server = serverDao.Get(Tenant);

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                    engine.ChangePassword(mailbox.Address, trimPwd);

                    mailbox.Password = trimPwd;
                    mailbox.SmtpPassword = trimPwd;

                    mailboxDao.SaveMailBox(mailbox);

                    tx.Commit();
                }
            }
        }

        public static ServerMailboxData ToMailboxData(Mailbox mailbox, ServerDomainAddressData address,
            List<ServerDomainAddressData> aliases)
        {
            return new ServerMailboxData
            {
                Id = mailbox.Id,
                UserId = mailbox.User,
                Address = address,
                Name = mailbox.Name,
                Aliases = aliases
            };
        }

        public static ServerDomainAddressData ToServerDomainAddressData(ServerAddress address, ServerDomain domain)
        {
            var result = new ServerDomainAddressData
            {
                Id = address.Id,
                DomainId = address.DomainId,
                Email = string.Format("{0}@{1}", address.AddressName, domain.Name)
            };

            return result;
        }

        public static ServerDomainAddressData ToServerDomainAddressData(ServerAddress address, ServerDomainData domain)
        {
            var result = new ServerDomainAddressData
            {
                Id = address.Id,
                DomainId = address.DomainId,
                Email = string.Format("{0}@{1}", address.AddressName, domain.Name)
            };

            return result;
        }

        public static ServerDomainAddressData ToServerDomainAddressData(ServerAddress address, string email)
        {
            var result = new ServerDomainAddressData
            {
                Id = address.Id,
                DomainId = address.DomainId,
                Email = email
            };

            return result;
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
