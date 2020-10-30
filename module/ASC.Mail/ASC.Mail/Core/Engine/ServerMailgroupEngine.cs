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
using System.Linq;
using System.Security;
using ASC.Common.Logging;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Server.Core.Entities;
using ASC.Mail.Utils;
using ASC.Web.Core;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Mail.Core.Engine
{
    public class ServerMailgroupEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public ServerMailgroupEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.ServerMailgroupEngine");
        }

        public List<ServerDomainGroupData> GetMailGroups()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            var list = new List<ServerDomainGroupData>();

            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var domains = serverDomainDao.GetDomains();

                var serverGroupDao = daoFactory.CreateServerGroupDao(Tenant);

                var groups = serverGroupDao.GetList();

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                list.AddRange(from serverGroup in groups
                    let address = serverAddressDao.Get(serverGroup.AddressId)
                    let domain = domains.FirstOrDefault(d => d.Id == address.DomainId)
                    where domain != null
                    let serverGroupAddress = ServerMailboxEngine.ToServerDomainAddressData(address, domain)
                    let serverGroupAddresses =
                        serverAddressDao.GetGroupAddresses(serverGroup.Id)
                            .ConvertAll(a => ServerMailboxEngine.ToServerDomainAddressData(a, domain))
                    select ToServerDomainGroupData(serverGroup.Id, serverGroupAddress, serverGroupAddresses));
            }

            return list;
        }

        public ServerDomainGroupData CreateMailGroup(string name, int domainId, List<int> addressIds)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"Invalid mailgroup name.", "name");

            if (domainId < 0)
                throw new ArgumentException(@"Invalid domain id.", "domainId");

            if (name.Length > 64)
                throw new ArgumentException(@"Local part of mailgroup exceed limitation of 64 characters.", "name");

            if (!Parser.IsEmailLocalPartValid(name))
                throw new ArgumentException(@"Incorrect group name.", "name");

            if (!addressIds.Any())
                throw new ArgumentException(@"Empty collection of address_ids.", "addressIds");

            var mailgroupName = name.ToLowerInvariant();

            using (var daoFactory = new DaoFactory())
            {
                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                var serverDomain = serverDomainDao.GetDomain(domainId);

                if (serverDomain.Tenant == Defines.SHARED_TENANT_ID)
                    throw new InvalidOperationException("Creating mail group is not allowed for shared domain.");

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                if (serverAddressDao.IsAddressAlreadyRegistered(mailgroupName, serverDomain.Name))
                {
                    throw new DuplicateNameException("You want to create a group with already existing address.");
                }

                var utcNow = DateTime.UtcNow;

                var address = new ServerAddress
                {
                    Id = 0,
                    Tenant = Tenant,
                    MailboxId = -1,
                    DomainId = serverDomain.Id,
                    AddressName = mailgroupName,
                    IsAlias = false,
                    IsMailGroup = true,
                    DateCreated = utcNow
                };

                var groupEmail = string.Format("{0}@{1}", mailgroupName, serverDomain.Name);

                var groupAddressData = ServerMailboxEngine.ToServerDomainAddressData(address, groupEmail);

                var newGroupMembers = serverAddressDao.GetList(addressIds);

                var newGroupMemberIds = newGroupMembers.ConvertAll(m => m.Id);

                var newGroupMemberDataList =
                    newGroupMembers.ConvertAll(m =>
                        ServerMailboxEngine.ToServerDomainAddressData(m,
                            string.Format("{0}@{1}", m.AddressName, serverDomain.Name)));

                var goTo = string.Join(",",
                    newGroupMembers.Select(m => string.Format("{0}@{1}", m.AddressName, serverDomain.Name)));

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                var group = new ServerGroup
                {
                    Id = 0,
                    Tenant = Tenant,
                    Address = groupEmail,
                    AddressId = 0,
                    DateCreated = utcNow
                };

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    address.Id = serverAddressDao.Save(address);

                    group.AddressId = address.Id;

                    var serverGroupDao = daoFactory.CreateServerGroupDao(Tenant);

                    group.Id = serverGroupDao.Save(group);

                    serverAddressDao.AddAddressesToMailGroup(group.Id, newGroupMemberIds);

                    var serverAddress = new Alias
                    {
                        Name = "",
                        Address = groupEmail,
                        GoTo = goTo,
                        Domain = serverDomain.Name,
                        IsActive = true,
                        IsGroup = true,
                        Modified = utcNow,
                        Created = utcNow
                    };

                    engine.SaveAlias(serverAddress);

                    tx.Commit();
                }

                CacheEngine.ClearAll();

                return ToServerDomainGroupData(group.Id, groupAddressData, newGroupMemberDataList);
            }
        }

        public ServerDomainGroupData AddMailGroupMember(int mailgroupId, int addressId)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (addressId < 0)
                throw new ArgumentException(@"Invalid address id.", "addressId");

            if (mailgroupId < 0)
                throw new ArgumentException(@"Invalid mailgroup id.", "mailgroupId");

            using (var daoFactory = new DaoFactory())
            {
                var serverGroupDao = daoFactory.CreateServerGroupDao(Tenant);

                var group = serverGroupDao.Get(mailgroupId);

                if (group == null)
                    throw new Exception("Group not found");

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var groupMembers = serverAddressDao.GetGroupAddresses(mailgroupId);

                if (groupMembers.Exists(a => a.Id == addressId))
                    throw new DuplicateNameException("Member already exists");

                var newMemberAddress = serverAddressDao.Get(addressId);

                if (newMemberAddress == null)
                    throw new Exception("Member not found");

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                var utcNow = DateTime.UtcNow;

                ServerAddress groupAddress;
                string groupEmail;
                List<ServerDomainAddressData> newGroupMemberDataList;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    serverAddressDao.AddAddressesToMailGroup(mailgroupId, new List<int> {addressId});

                    groupMembers.Add(newMemberAddress);

                    groupAddress = serverAddressDao.Get(group.AddressId);

                    var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                    var serverDomain = serverDomainDao.GetDomain(groupAddress.DomainId);

                    var goTo = string.Join(",",
                        groupMembers.Select(m => string.Format("{0}@{1}", m.AddressName, serverDomain.Name)));

                    groupEmail = string.Format("{0}@{1}", groupAddress.AddressName, serverDomain.Name);

                    newGroupMemberDataList =
                        groupMembers.ConvertAll(m =>
                            ServerMailboxEngine.ToServerDomainAddressData(m,
                                string.Format("{0}@{1}", m.AddressName, serverDomain.Name)));

                    var serverAddress = new Alias
                    {
                        Name = "",
                        Address = groupEmail,
                        GoTo = goTo,
                        Domain = serverDomain.Name,
                        IsActive = true,
                        IsGroup = true,
                        Modified = utcNow,
                        Created = utcNow
                    };

                    engine.SaveAlias(serverAddress);

                    tx.Commit();
                }

                var groupAddressData = ServerMailboxEngine.ToServerDomainAddressData(groupAddress, groupEmail);

                CacheEngine.ClearAll();

                return ToServerDomainGroupData(group.Id, groupAddressData, newGroupMemberDataList);
            }
        }

        public void RemoveMailGroupMember(int mailgroupId, int addressId)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (addressId < 0)
                throw new ArgumentException(@"Invalid address id.", "addressId");

            if (mailgroupId < 0)
                throw new ArgumentException(@"Invalid mailgroup id.", "mailgroupId");

            using (var daoFactory = new DaoFactory())
            {
                var serverGroupDao = daoFactory.CreateServerGroupDao(Tenant);

                var group = serverGroupDao.Get(mailgroupId);

                if (group == null)
                    throw new Exception("Group not found");

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var groupMembers = serverAddressDao.GetGroupAddresses(mailgroupId);

                var removeMember = groupMembers.FirstOrDefault(a => a.Id == addressId);

                if (removeMember == null)
                    throw new ArgumentException("Member not found");

                groupMembers.Remove(removeMember);

                if (groupMembers.Count == 0)
                    throw new Exception("Can't remove last member; Remove group.");

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                var groupAddress = serverAddressDao.Get(group.AddressId);

                var serverDomainDao = daoFactory.CreateServerDomainDao(Tenant);

                 var serverDomain = serverDomainDao.GetDomain(groupAddress.DomainId);

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                var utcNow = DateTime.UtcNow;

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    serverAddressDao.DeleteAddressFromMailGroup(mailgroupId, addressId);

                    var goTo = string.Join(",",
                        groupMembers.Select(m => string.Format("{0}@{1}", m.AddressName, serverDomain.Name)));

                    var groupEmail = string.Format("{0}@{1}", groupAddress.AddressName, serverDomain.Name);

                    var serverAddress = new Alias
                    {
                        Name = "",
                        Address = groupEmail,
                        GoTo = goTo,
                        Domain = serverDomain.Name,
                        IsActive = true,
                        IsGroup = true,
                        Modified = utcNow,
                        Created = group.DateCreated
                    };

                    engine.SaveAlias(serverAddress);

                    tx.Commit();
                }
            }

            CacheEngine.ClearAll();
        }

        public void RemoveMailGroup(int id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (id < 0)
                throw new ArgumentException(@"Invalid mailgroup id.", "id");

            using (var daoFactory = new DaoFactory())
            {
                var serverGroupDao = daoFactory.CreateServerGroupDao(Tenant);

                var group = serverGroupDao.Get(id);

                if (group == null)
                    throw new Exception("Group not found");

                var serverAddressDao = daoFactory.CreateServerAddressDao(Tenant);

                var serverDao = daoFactory.CreateServerDao();
                var server = serverDao.Get(Tenant);

                var engine = new Server.Core.ServerEngine(server.Id, server.ConnectionString);

                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    serverGroupDao.Delete(id);

                    serverAddressDao.DeleteAddressesFromMailGroup(id);

                    serverAddressDao.Delete(group.AddressId);

                    engine.RemoveAlias(group.Address);

                    tx.Commit();
                }
            }

            CacheEngine.ClearAll();
        }

        public static ServerDomainGroupData ToServerDomainGroupData(int groupId, ServerDomainAddressData address, List<ServerDomainAddressData> addresses)
        {
            var group = new ServerDomainGroupData
            {
                Id = groupId,
                Address = address,
                Addresses = addresses
            };

            return group;
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
