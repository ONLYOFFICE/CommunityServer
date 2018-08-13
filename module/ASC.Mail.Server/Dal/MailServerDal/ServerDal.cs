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
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.DbSchema;

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

            var serversQuery = new SqlQuery(ServerTable.Name)
                .Select(ServerTable.Columns.Id)
                .Select(ServerTable.Columns.ConnectionString)
                .Select(ServerTable.Columns.MxRecord)
                .Select(ServerTable.Columns.ServerType)
                .Select(ServerTable.Columns.SmtpSettingsId)
                .Select(ServerTable.Columns.ImapSettingsId)
                .Where(Exp.Gt(ServerTable.Columns.ServerType, 0));

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
                    var serverMailboxesQuery = new SqlQuery(TenantXServerTable.Name.Alias(mst_alias))
                        .InnerJoin(MailboxTable.Name.Alias(mb_alias),
                                   Exp.EqColumns(TenantXServerTable.Columns.Tenant.Prefix(mst_alias),
                                                 MailboxTable.Columns.Tenant.Prefix(mb_alias)))
                        .SelectCount()
                        .Where(MailboxTable.Columns.IsTeamlabMailbox.Prefix(mb_alias), true)
                        .Where(MailboxTable.Columns.IsRemoved.Prefix(mb_alias), false)
                        .Where(TenantXServerTable.Columns.ServerId.Prefix(mst_alias), tenantServerDto.id)
                        .GroupBy(TenantXServerTable.Columns.ServerId.Prefix(mst_alias));

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

            var linkServerWithTenantQuery = new SqlInsert(TenantXServerTable.Name)
                .InColumnValue(TenantXServerTable.Columns.ServerId, server.id)
                .InColumnValue(TenantXServerTable.Columns.Tenant, tenant);

            var insertedRows = db.ExecuteNonQuery(linkServerWithTenantQuery);
            if (insertedRows == 0)
                throw new InvalidOperationException(String.Format("Insert to {0} failed", TenantXServerTable.Name));

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

        public List<TenantServerSettingsDto> GetTenantServerSettings()
        {
            var server = GetTenantServer();

            List<TenantServerSettingsDto> settings;

            var query = new SqlQuery(MailboxServerTable.Name)
                .Select(MailboxServerTable.Columns.Id, 
                        MailboxServerTable.Columns.Type,
                        MailboxServerTable.Columns.Hostname, 
                        MailboxServerTable.Columns.Port,
                        MailboxServerTable.Columns.SocketType, 
                        MailboxServerTable.Columns.Username,
                        MailboxServerTable.Columns.Authentication)
                .Where(Exp.In(MailboxServerTable.Columns.Id, new[] {server.imap_settings_id, server.smtp_settings_id}));

            using (var db = GetDb())
            {
                settings = db.ExecuteList(query).ConvertAll(r => r.ToTenantServerSettingsDto()).ToList();
            }

            return settings;
        }

    }
}
