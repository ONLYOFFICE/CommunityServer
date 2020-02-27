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
using ASC.Mail.Server.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class ServerGroupDao : BaseDao, IServerGroupDao
    {
        protected static ITable table = new MailTableFactory().Create<ServerMailGroupTable>();

        public ServerGroupDao(IDbManager dbManager, int tenant) 
            : base(table, dbManager, tenant)
        {
        }

        public ServerGroup Get(int id)
        {
            var query = Query()
                .Where(ServerMailGroupTable.Columns.Tenant, Tenant)
                .Where(ServerMailGroupTable.Columns.Id, id);

            var group = Db.ExecuteList(query)
                .ConvertAll(ToServerGroup)
                .SingleOrDefault();

            return group;
        }

        public List<ServerGroup> GetList()
        {
            var query = Query()
                .Where(ServerMailGroupTable.Columns.Tenant, Tenant);

            var groups = Db.ExecuteList(query)
                .ConvertAll(ToServerGroup);

            return groups;
        }

        public List<ServerGroup> GetList(int domainId)
        {
            const string group_alias = "msg";
            const string address_alias = "msa";

            var query = Query(group_alias)
                .InnerJoin(ServerAddressTable.TABLE_NAME.Alias(address_alias),
                    Exp.EqColumns(
                        ServerMailGroupTable.Columns.AddressId.Prefix(group_alias),
                        ServerAddressTable.Columns.Id.Prefix(address_alias)
                        )
                )
                .Where(ServerMailGroupTable.Columns.Tenant.Prefix(group_alias), Tenant)
                .Where(ServerAddressTable.Columns.DomainId.Prefix(address_alias), domainId)
                .Where(ServerAddressTable.Columns.IsMailGroup.Prefix(address_alias), true);

            var groups = Db.ExecuteList(query)
                .ConvertAll(ToServerGroup);

            return groups;
        }

        public int Save(ServerGroup @group)
        {
            var query = new SqlInsert(ServerMailGroupTable.TABLE_NAME, true)
                .InColumnValue(ServerMailGroupTable.Columns.Id, group.Id)
                .InColumnValue(ServerMailGroupTable.Columns.Tenant, group.Tenant)
                .InColumnValue(ServerMailGroupTable.Columns.Address, group.Address)
                .InColumnValue(ServerMailGroupTable.Columns.AddressId, group.AddressId)
                .InColumnValue(ServerMailGroupTable.Columns.DateCreated, group.DateCreated)
                .Identity(0, 0, true);

            return Db.ExecuteScalar<int>(query);
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(ServerMailGroupTable.TABLE_NAME)
                .Where(ServerMailGroupTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        protected ServerGroup ToServerGroup(object[] r)
        {
            var group = new ServerGroup
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                AddressId = Convert.ToInt32(r[2]),
                Address = Convert.ToString(r[3]),
                DateCreated = Convert.ToDateTime(r[4])
            };

            return group;
        }
    }
}