/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.Dal
{
    public class ServerDal : DalBase
    {
        public ServerDal(int tenant_id)
            : base("mailserver", tenant_id)
        {
        }

        public ServerDal(string db_connection_string_name, int tenant_id) : base(db_connection_string_name, tenant_id)
        {
        }

        private TenantServerDto GetFreeServer(DbManager db)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            const string ms_alias = "ms";
            const string mst_alias = "mst";
            const string mm_alias = "mm";

            var servers_query = new SqlQuery(ServerTable.name.Alias(ms_alias))
                .Select(ServerTable.Columns.id.Prefix(ms_alias))
                .Select(ServerTable.Columns.connection_string.Prefix(ms_alias))
                .Select(ServerTable.Columns.mx_record.Prefix(ms_alias))
                .Select(ServerTable.Columns.server_type.Prefix(ms_alias))
                .Select(ServerTable.Columns.smtp_settings_id.Prefix(ms_alias))
                .Select(ServerTable.Columns.imap_settings_id.Prefix(ms_alias))
                .LeftOuterJoin(TenantXServerTable.name.Alias(mst_alias),
                               Exp.EqColumns(ServerTable.Columns.id.Prefix(ms_alias),
                                             TenantXServerTable.Columns.id_server.Prefix(mst_alias)))
                .LeftOuterJoin(MailboxTable.name.Alias(mm_alias),
                               Exp.EqColumns(TenantXServerTable.Columns.id_tenant.Prefix(mst_alias),
                                             MailboxTable.Columns.id_tenant.Prefix(mm_alias)))
                .Where(Exp.Gt(ServerTable.Columns.server_type.Prefix(ms_alias), 0))
                .Where(Exp.Or(Exp.Eq(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mm_alias), true),
                              Exp.Eq(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mm_alias), Exp.Empty)))
                .Where(Exp.Or(Exp.Eq(MailboxTable.Columns.is_removed.Prefix(mm_alias), false),
                              Exp.Eq(MailboxTable.Columns.is_removed.Prefix(mm_alias), Exp.Empty)))
                .GroupBy(ServerTable.Columns.id.Prefix(ms_alias))
                .OrderBy("count(" + ServerTable.Columns.id.Prefix(ms_alias) + ")", true);

            return db.ExecuteList(servers_query)
                     .Select(r => r.ToTenantServerDto())
                     .FirstOrDefault();

        }

        private TenantServerDto LinkServerWithTenant(DbManager db)
        {
            if(db == null)
                throw new ArgumentNullException("db");
            
            var server = GetFreeServer(db);

            if (server == null)
                throw new InvalidDataException("No mail servers registered.");

            var link_server_with_tenant_query = new SqlInsert(TenantXServerTable.name)
                .InColumnValue(TenantXServerTable.Columns.id_server, server.id)
                .InColumnValue(TenantXServerTable.Columns.id_tenant, tenant_id);

            var inserted_rows = db.ExecuteNonQuery(link_server_with_tenant_query);
            if (inserted_rows == 0)
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
