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
        protected readonly int tenant;

        protected DalBase(string dbConnectionStringName, int tenant)
        {
            db_connection_string_name = dbConnectionStringName;
            this.tenant = tenant;
        }

        public MailDbContext CreateMailDbContext(bool needTransaction = false)
        {
            return new MailDbContext(db_connection_string_name, needTransaction);
        }

        //Todo: think about rewriting method GetDb for working with already open conncetions
        protected DbManager GetDb()
        {
            return new DbManager(db_connection_string_name);
        }

        protected MailBoxManager GetMailboxManager()
        {
            return new MailBoxManager();
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

            var serverInformationQuery = new SqlQuery(ServerTable.name)
                              .InnerJoin(TenantXServerTable.name, Exp.EqColumns(TenantXServerTable.Columns.id_server, ServerTable.Columns.id))
                              .Select(ServerTable.Columns.id)
                              .Select(ServerTable.Columns.connection_string)
                              .Select(ServerTable.Columns.mx_record)
                              .Select(ServerTable.Columns.server_type)
                              .Select(ServerTable.Columns.smtp_settings_id)
                              .Select(ServerTable.Columns.imap_settings_id)
                              .Where(TenantXServerTable.Columns.id_tenant, tenant);

            var serverInformation = db.ExecuteList(serverInformationQuery);
            return serverInformation.Select(record => record.ToTenantServerDto()).FirstOrDefault();
        }
    }
}
