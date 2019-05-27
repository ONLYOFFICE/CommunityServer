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

namespace ASC.Mail.Core.Dao
{
    public class ServerDomainDao : BaseDao, IServerDomainDao
    {
        protected static ITable table = new MailTableFactory().Create<ServerDomainTable>();

        public ServerDomainDao(IDbManager dbManager, int tenant)
            : base(table, dbManager, tenant)
        {
        }

        public int Save(ServerDomain domain)
        {
            var query = new SqlInsert(ServerDomainTable.TABLE_NAME)
                .InColumnValue(ServerDomainTable.Columns.Id, domain.Id)
                .InColumnValue(ServerDomainTable.Columns.DomainName, domain.Name)
                .InColumnValue(ServerDomainTable.Columns.Tenant, Tenant)
                .InColumnValue(ServerDomainTable.Columns.IsVerified, domain.IsVerified)
                .Identity(0, 0, true);

            if (domain.Id <= 0)
            {
                query.InColumnValue(ServerDomainTable.Columns.DateAdded, DateTime.UtcNow);
            }

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(ServerDomainTable.TABLE_NAME)
                .Where(ServerDomainTable.Columns.Tenant, Tenant)
                .Where(ServerDomainTable.Columns.Id, id);

            var result = Db.ExecuteNonQuery(query);

            query = new SqlDelete(ServerDnsTable.TABLE_NAME)
                .Where(ServerDnsTable.Columns.Tenant, Tenant)
                .Where(ServerDnsTable.Columns.DomainId, id);

            Db.ExecuteNonQuery(query);

            return result;
        }

        public List<ServerDomain> GetDomains()
        {
            var query = Query()
                .Where(Exp.In(ServerDomainTable.Columns.Tenant, new List<int> {Tenant, Defines.SHARED_TENANT_ID}));

            var list = Db.ExecuteList(query)
                .ConvertAll(ToServerDomain);

            return list;
        }

        public ServerDomain GetDomain(int id)
        {
            var query = Query()
                .Where(Exp.In(ServerDomainTable.Columns.Tenant, new List<int> { Tenant, Defines.SHARED_TENANT_ID }))
                .Where(ServerDomainTable.Columns.Id, id);

            var domain = Db.ExecuteList(query)
                .ConvertAll(ToServerDomain)
                .SingleOrDefault();

            return domain;
        }

        public bool IsDomainExists(string name)
        {
            var query = Query()
                .Where(ServerDomainTable.Columns.DomainName, name);

            var domain = Db.ExecuteList(query)
                .ConvertAll(ToServerDomain)
                .SingleOrDefault();

            return domain != null;
        }

        public int SetVerified(int id, bool isVerified)
        {
            var query = new SqlUpdate(ServerDomainTable.TABLE_NAME)
                .Set(ServerDomainTable.Columns.IsVerified, isVerified)
                .Set(ServerDomainTable.Columns.DateChecked, DateTime.UtcNow)
                .Where(ServerDomainTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        protected ServerDomain ToServerDomain(object[] r)
        {
            var d = new ServerDomain
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                Name = Convert.ToString(r[2]),
                IsVerified = Convert.ToBoolean(r[3]),
                DateAdded = Convert.ToDateTime(r[4]),
                DateChecked = Convert.ToDateTime(r[5])
            };

            return d;
        }
    }
}