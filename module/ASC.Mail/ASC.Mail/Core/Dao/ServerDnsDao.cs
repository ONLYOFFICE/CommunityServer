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
    public class ServerDnsDao : BaseDao, IServerDnsDao
    {
        protected static ITable table = new MailTableFactory().Create<ServerDnsTable>();

        public string User { get; private set; }

        public ServerDnsDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            User = user;
        }

        public ServerDns Get(int domainId)
        {
            var query = Query()
                .Where(Exp.In(ServerDnsTable.Columns.Tenant, new List<int> {Tenant, Defines.SHARED_TENANT_ID}))
                .Where(ServerDnsTable.Columns.DomainId, domainId);

            var dns = Db.ExecuteList(query)
                .ConvertAll(ToServerDns)
                .SingleOrDefault();

            return dns;
        }

        public ServerDns GetById(int id)
        {
            var query = Query()
                .Where(ServerDnsTable.Columns.Tenant, Tenant)
                .Where(ServerDnsTable.Columns.Id, id);

            var dns = Db.ExecuteList(query)
                .ConvertAll(ToServerDns)
                .SingleOrDefault();

            return dns;
        }

        public ServerDns GetFree()
        {
            var query = Query()
                .Where(ServerDnsTable.Columns.Tenant, Tenant)
                .Where(ServerDnsTable.Columns.User, User)
                .Where(ServerDnsTable.Columns.DomainId, Defines.UNUSED_DNS_SETTING_DOMAIN_ID);

            var dns = Db.ExecuteList(query)
                .ConvertAll(ToServerDns)
                .SingleOrDefault();

            return dns;
        }

        public int Save(ServerDns dns)
        {
            var query = new SqlInsert(ServerDnsTable.TABLE_NAME, true)
                .InColumnValue(ServerDnsTable.Columns.Id, dns.Id)
                .InColumnValue(ServerDnsTable.Columns.Tenant, dns.Tenant)
                .InColumnValue(ServerDnsTable.Columns.User, dns.User)
                .InColumnValue(ServerDnsTable.Columns.DomainId, dns.DomainId)
                .InColumnValue(ServerDnsTable.Columns.DomainCheck, dns.DomainCheck)
                .InColumnValue(ServerDnsTable.Columns.DkimSelector, dns.DkimSelector)
                .InColumnValue(ServerDnsTable.Columns.DkimPrivateKey, dns.DkimPrivateKey)
                .InColumnValue(ServerDnsTable.Columns.DkimPublicKey, dns.DkimPublicKey)
                .InColumnValue(ServerDnsTable.Columns.DkimTtl, dns.DkimTtl)
                .InColumnValue(ServerDnsTable.Columns.DkimVerified, dns.DkimVerified)
                .InColumnValue(ServerDnsTable.Columns.DkimDateChecked, dns.DkimDateChecked)
                .InColumnValue(ServerDnsTable.Columns.Spf, dns.Spf)
                .InColumnValue(ServerDnsTable.Columns.SpfTtl, dns.SpfTtl)
                .InColumnValue(ServerDnsTable.Columns.SpfVerified, dns.SpfVerified)
                .InColumnValue(ServerDnsTable.Columns.SpfDateChecked, dns.SpfDateChecked)
                .InColumnValue(ServerDnsTable.Columns.Mx, dns.Mx)
                .InColumnValue(ServerDnsTable.Columns.MxTtl, dns.MxTtl)
                .InColumnValue(ServerDnsTable.Columns.MxVerified, dns.MxVerified)
                .InColumnValue(ServerDnsTable.Columns.MxDateChecked, dns.MxDateChecked)
                .InColumnValue(ServerDnsTable.Columns.TimeModified, dns.TimeModified)
                .Identity(0, 0, true);

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(ServerDnsTable.TABLE_NAME)
                .Where(ServerDnsTable.Columns.Tenant, Tenant)
                .Where(ServerDnsTable.Columns.User, User)
                .Where(ServerDnsTable.Columns.Id, id);

            var result = Db.ExecuteNonQuery(query);

            return result;
        }

        protected ServerDns ToServerDns(object[] r)
        {
            var s = new ServerDns
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                User = Convert.ToString(r[2]),
                DomainId = Convert.ToInt32(r[3]),
                DomainCheck = Convert.ToString(r[4]),
                DkimSelector = Convert.ToString(r[5]),
                DkimPrivateKey = Convert.ToString(r[6]),
                DkimPublicKey = Convert.ToString(r[7]),
                DkimTtl = Convert.ToInt32(r[8]),
                DkimVerified = Convert.ToBoolean(r[9]),
                DkimDateChecked = Convert.ToDateTime(r[10]),
                Spf = Convert.ToString(r[11]),
                SpfTtl = Convert.ToInt32(r[12]),
                SpfVerified = Convert.ToBoolean(r[13]),
                SpfDateChecked = Convert.ToDateTime(r[14]),
                Mx = Convert.ToString(r[15]),
                MxTtl = Convert.ToInt32(r[16]),
                MxVerified = Convert.ToBoolean(r[17]),
                MxDateChecked = Convert.ToDateTime(r[18]),
                TimeModified = Convert.ToDateTime(r[19])
            };

            return s;
        }
    }
}