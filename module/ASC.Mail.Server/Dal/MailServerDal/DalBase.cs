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
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Server.Dal
{
    public abstract class DalBase
    {
        protected readonly string db_connection_string_name;
        protected readonly int tenant_id;

        protected DalBase(string db_connection_string_name, int tenant_id)
        {
            this.db_connection_string_name = db_connection_string_name;
            this.tenant_id = tenant_id;
        }

        public MailDbContext CreateMailDbContext(bool need_transaction = false)
        {
            return new MailDbContext(db_connection_string_name, need_transaction);
        }

        //Todo: think about rewriting method GetDb for working with already open conncetions
        protected DbManager GetDb()
        {
            return new DbManager(db_connection_string_name);
        }

        protected MailBoxManager GetMailboxManager()
        {
            return new MailBoxManager(25);
        }

        protected T NullSafeExecuteScalar<T>(DbManager manager, SqlInsert query)
        {
            if (manager == null)
            {
                using (var db = GetDb())
                {
                    return db.ExecuteScalar<T>(query);
                }
            }

            return manager.ExecuteScalar<T>(query);
        }

        protected List<object[]> NullSafeExecuteList(DbManager manager, SqlQuery query)
        {
            if (manager == null)
            {
                using (var db = GetDb())
                {
                    return db.ExecuteList(query);
                }
            }

            return manager.ExecuteList(query);
        }

        protected int NullSafeExecuteNonQuery(DbManager manager, ISqlInstruction query)
        {
            if (manager == null)
            {
                using (var db = GetDb())
                {
                    return db.ExecuteNonQuery(query);
                }
            }

            return manager.ExecuteNonQuery(query);
        }

        protected TenantServerDto GetTenantServer(DbManager db)
        {
            if (db == null) throw new ArgumentNullException("db");

            var server_information_query = new SqlQuery(ServerTable.name)
                              .InnerJoin(TenantXServerTable.name, Exp.EqColumns(TenantXServerTable.Columns.id_server, ServerTable.Columns.id))
                              .Select(ServerTable.Columns.id)
                              .Select(ServerTable.Columns.connection_string)
                              .Select(ServerTable.Columns.mx_record)
                              .Select(ServerTable.Columns.server_type)
                              .Select(ServerTable.Columns.smtp_settings_id)
                              .Select(ServerTable.Columns.imap_settings_id)
                              .Where(TenantXServerTable.Columns.id_tenant, tenant_id);

            var server_information = db.ExecuteList(server_information_query);
            return server_information.Select(record => record.ToTenantServerDto()).FirstOrDefault();
        }
    }
}
