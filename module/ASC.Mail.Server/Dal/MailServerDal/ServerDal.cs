/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Server.Dal
{
    public class ServerDal : DalBase
    {
        public ServerDal(int tenant)
            : base("mailserver", tenant)
        {
        }

        public ServerDal(string dbConnectionStringName, int tenant) : base(dbConnectionStringName, tenant)
        {
        }

        private static TenantServerDto GetFreeServer(IDbManager db)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            TenantServerDto serverDto;

            var serversQuery = new SqlQuery(ServerTable.name)
                .Select(ServerTable.Columns.id)
                .Select(ServerTable.Columns.connection_string)
                .Select(ServerTable.Columns.mx_record)
                .Select(ServerTable.Columns.server_type)
                .Select(ServerTable.Columns.smtp_settings_id)
                .Select(ServerTable.Columns.imap_settings_id)
                .Where(Exp.Gt(ServerTable.Columns.server_type, 0));

            var servers = db.ExecuteList(serversQuery)
                            .Select(r => r.ToTenantServerDto())
                            .ToList();

            if (servers.Count > 1)
            {
                const string mst_alias = "mst";
                const string mb_alias = "mb";

                var serverXmailboxes = new Dictionary<int, int>();

                foreach (var tenantServerDto in servers)
                {
                    var serverMailboxesQuery = new SqlQuery(TenantXServerTable.name.Alias(mst_alias))
                        .InnerJoin(MailboxTable.name.Alias(mb_alias),
                                   Exp.EqColumns(TenantXServerTable.Columns.id_tenant.Prefix(mst_alias),
                                                 MailboxTable.Columns.id_tenant.Prefix(mb_alias)))
                        .SelectCount()
                        .Where(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mb_alias), true)
                        .Where(MailboxTable.Columns.is_removed.Prefix(mb_alias), false)
                        .Where(TenantXServerTable.Columns.id_server.Prefix(mst_alias), tenantServerDto.id)
                        .GroupBy(TenantXServerTable.Columns.id_server.Prefix(mst_alias));

                    var count = db.ExecuteScalar<int>(serverMailboxesQuery);

                    serverXmailboxes.Add(tenantServerDto.id, count);

                }

                var lightestServerId = serverXmailboxes.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

                serverDto =
                    servers.First(s => s.id == lightestServerId);

            }
            else
            {
                serverDto = servers.FirstOrDefault();
            }

            return serverDto;
        }

        private TenantServerDto LinkServerWithTenant(DbManager db)
        {
            if(db == null)
                throw new ArgumentNullException("db");
            
            var server = GetFreeServer(db);

            if (server == null)
                throw new InvalidDataException("No mail servers registered.");

            var linkServerWithTenantQuery = new SqlInsert(TenantXServerTable.name)
                .InColumnValue(TenantXServerTable.Columns.id_server, server.id)
                .InColumnValue(TenantXServerTable.Columns.id_tenant, tenant);

            var insertedRows = db.ExecuteNonQuery(linkServerWithTenantQuery);
            if (insertedRows == 0)
                throw new InvalidOperationException(String.Format("Insert to {0} failed", TenantXServerTable.name));

            return server;
        }

        public TenantServerDto GetTenantServer()
        {
            TenantServerDto server;

            using (var db = GetDb())
            {
                server = GetTenantServer(db) ?? LinkServerWithTenant(db);
            }

            return server;
        }

    }
}
