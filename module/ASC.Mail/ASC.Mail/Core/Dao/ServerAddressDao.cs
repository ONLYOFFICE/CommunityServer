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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class ServerAddressDao : BaseDao, IServerAddressDao
    {
        protected static ITable table = new MailTableFactory().Create<ServerAddressTable>();

        public ServerAddressDao(IDbManager dbManager, int tenant) 
            : base(table, dbManager, tenant)
        {
        }

        public ServerAddress Get(int id)
        {
            var query = Query()
                .Where(ServerAddressTable.Columns.Tenant, Tenant)
                .Where(ServerAddressTable.Columns.Id, id);

            var address = Db.ExecuteList(query)
                .ConvertAll(ToServerAddress)
                .SingleOrDefault();

            return address;
        }

        public List<ServerAddress> GetList(List<int> ids = null)
        {
            var query = Query()
                .Where(ServerAddressTable.Columns.Tenant, Tenant);

            if (ids != null && ids.Any())
            {
                query.Where(Exp.In(ServerAddressTable.Columns.Id, ids));
            }

            var list = Db.ExecuteList(query)
                .ConvertAll(ToServerAddress);

            return list;
        }

        public List<ServerAddress> GetList(int mailboxId)
        {
            var query = Query()
                .Where(ServerAddressTable.Columns.Tenant, Tenant)
                .Where(ServerAddressTable.Columns.MailboxId, mailboxId);

            var list = Db.ExecuteList(query)
                .ConvertAll(ToServerAddress);

            return list;
        }

        public List<ServerAddress> GetGroupAddresses(int groupId)
        {
            const string m_x_a_alias = "mxa";
            const string address_alias = "msa";

            var query = Query(address_alias)
                .InnerJoin(ServerMailGroupXAddressesTable.TABLE_NAME.Alias(m_x_a_alias),
                    Exp.EqColumns(ServerMailGroupXAddressesTable.Columns.AddressId.Prefix(m_x_a_alias),
                        ServerAddressTable.Columns.Id.Prefix(address_alias)))
                .Where(ServerMailGroupXAddressesTable.Columns.MailGroupId.Prefix(m_x_a_alias), groupId)
                .Where(ServerAddressTable.Columns.Tenant.Prefix(address_alias), Tenant);

            return Db.ExecuteList(query)
                .ConvertAll(ToServerAddress);
        }

        public List<ServerAddress> GetDomainAddresses(int domainId)
        {
            var query = Query()
                .Where(ServerAddressTable.Columns.Tenant, Tenant)
                .Where(ServerAddressTable.Columns.DomainId, domainId);

            var list = Db.ExecuteList(query)
                .ConvertAll(ToServerAddress);

            return list;
        }

        public void AddAddressesToMailGroup(int groupId, List<int> addressIds)
        {
            var query = new SqlInsert(ServerMailGroupXAddressesTable.TABLE_NAME)
                .InColumns(ServerMailGroupXAddressesTable.Columns.AddressId,
                    ServerMailGroupXAddressesTable.Columns.MailGroupId);

            addressIds.ForEach(addressId => query.Values(addressId, groupId));

            Db.ExecuteNonQuery(query);
        }

        public void DeleteAddressFromMailGroup(int groupId, int addressId)
        {
            var query = new SqlDelete(ServerMailGroupXAddressesTable.TABLE_NAME)
                .Where(ServerMailGroupXAddressesTable.Columns.AddressId, addressId)
                .Where(ServerMailGroupXAddressesTable.Columns.MailGroupId, groupId);

            Db.ExecuteNonQuery(query);
        }

        public void DeleteAddressesFromMailGroup(int groupId)
        {
            var query = new SqlDelete(ServerMailGroupXAddressesTable.TABLE_NAME)
                .Where(ServerMailGroupXAddressesTable.Columns.MailGroupId, groupId);

            Db.ExecuteNonQuery(query);
        }

        public void DeleteAddressesFromAnyMailGroup(List<int> addressIds)
        {
            var query = new SqlDelete(ServerMailGroupXAddressesTable.TABLE_NAME)
                .Where(Exp.In(ServerMailGroupXAddressesTable.Columns.AddressId, addressIds));

            Db.ExecuteNonQuery(query);
        }

        public int Save(ServerAddress address)
        {
            var query = new SqlInsert(ServerAddressTable.TABLE_NAME, true)
                .InColumnValue(ServerAddressTable.Columns.Id, address.Id)
                .InColumnValue(ServerAddressTable.Columns.AddressName, address.AddressName)
                .InColumnValue(ServerAddressTable.Columns.Tenant, address.Tenant)
                .InColumnValue(ServerAddressTable.Columns.DomainId, address.DomainId)
                .InColumnValue(ServerAddressTable.Columns.MailboxId, address.MailboxId)
                .InColumnValue(ServerAddressTable.Columns.IsMailGroup, address.IsMailGroup)
                .InColumnValue(ServerAddressTable.Columns.IsAlias, address.IsAlias)
                .Identity(0, 0, true);

            if (address.Id <= 0)
            {
                query
                    .InColumnValue(ServerAddressTable.Columns.DateCreated, DateTime.UtcNow);
            }

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(ServerAddressTable.TABLE_NAME)
                .Where(ServerAddressTable.Columns.Tenant, Tenant)
                .Where(ServerAddressTable.Columns.Id, id);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        public int Delete(List<int> ids)
        {
            var query = new SqlDelete(ServerAddressTable.TABLE_NAME)
                .Where(ServerAddressTable.Columns.Tenant, Tenant)
                .Where(Exp.In(ServerAddressTable.Columns.Id, ids));

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        private const string DOMAIN_ALIAS = "msd";
        private const string ADDRESS_ALIAS = "msa";

        public bool IsAddressAlreadyRegistered(string addressName, string domainName)
        {
            if (string.IsNullOrEmpty(addressName))
                throw new ArgumentNullException("addressName");

            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var addressQuery = new SqlQuery(ServerAddressTable.TABLE_NAME.Alias(ADDRESS_ALIAS))
                .InnerJoin(ServerDomainTable.TABLE_NAME.Alias(DOMAIN_ALIAS),
                    Exp.EqColumns(
                        ServerAddressTable.Columns.DomainId.Prefix(ADDRESS_ALIAS),
                        ServerDomainTable.Columns.Id.Prefix(DOMAIN_ALIAS))
                )
                .Select(ServerAddressTable.Columns.Id.Prefix(ADDRESS_ALIAS))
                .Where(ServerAddressTable.Columns.AddressName.Prefix(ADDRESS_ALIAS), addressName)
                .Where(Exp.In(ServerAddressTable.Columns.Tenant.Prefix(ADDRESS_ALIAS),
                    new List<int> {Tenant, Defines.SHARED_TENANT_ID}))
                .Where(ServerDomainTable.Columns.DomainName.Prefix(DOMAIN_ALIAS), domainName);

            return Db.ExecuteList(addressQuery).Any();
        }

        protected ServerAddress ToServerAddress(object[] r)
        {
            var s = new ServerAddress
            {
                Id = Convert.ToInt32(r[0]),
                AddressName = Convert.ToString(r[1]),
                Tenant = Convert.ToInt32(r[2]),
                DomainId = Convert.ToInt32(r[3]),
                MailboxId = Convert.ToInt32(r[4]),
                IsMailGroup = Convert.ToBoolean(r[5]),
                IsAlias = Convert.ToBoolean(r[6]),
                DateCreated = Convert.ToDateTime(r[7])
            };

            return s;
        }
    }
}