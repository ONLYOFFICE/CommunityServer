/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ASC.Data.Storage;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class MailModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("mail_attachment", "tenant", "id"),
                new TableInfo("mail_chain", "tenant") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_contacts", "tenant", "id") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_folder", "tenant") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_mail", "tenant", "id")
                    {
                        UserIDColumns = new[] {"id_user"},
                        DateColumns = new Dictionary<string, bool> {{"date_received", false}, {"date_sent", false}, {"chain_date", false}}
                    },
                new TableInfo("mail_mailbox", "tenant", "id")
                    {
                        UserIDColumns = new[] {"id_user"},
                        DateColumns = new Dictionary<string, bool> {{"begin_date", false}}
                    },
                new TableInfo("mail_tag", "tenant", "id") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_tag_addresses", "tenant"),
                new TableInfo("mail_tag_mail", "tenant") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_chain_x_crm_entity", "id_tenant"),
                new TableInfo("mail_mailbox_signature", "tenant"),
                new TableInfo("mail_mailbox_autoreply", "tenant"),
                new TableInfo("mail_mailbox_autoreply_history", "tenant"),
                new TableInfo("mail_contact_info", "tenant", "id") {UserIDColumns = new[] {"id_user"}}
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("mail_mail", "id", "mail_attachment", "id_mail"),
                new RelationInfo("mail_mailbox", "id", "mail_chain", "id_mailbox"),
                new RelationInfo("mail_tag", "id", "mail_chain", "tags"),
                new RelationInfo("crm_tag", "id", "mail_chain", "tags", typeof(CrmModuleSpecifics)),
                new RelationInfo("mail_mailbox", "id", "mail_mail", "id_mailbox"),
                new RelationInfo("crm_tag", "id", "mail_tag", "crm_id", typeof(CrmModuleSpecifics)),
                new RelationInfo("mail_tag", "id", "mail_tag_addresses", "id_tag", x => Convert.ToInt32(x["id_tag"]) > 0),
                new RelationInfo("crm_tag", "id", "mail_tag_addresses", "id_tag", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["id_tag"]) < 0),
                new RelationInfo("mail_mail", "id", "mail_tag_mail", "id_mail"),
                new RelationInfo("mail_tag", "id", "mail_tag_mail", "id_tag", x => Convert.ToInt32(x["id_tag"]) > 0),
                new RelationInfo("crm_tag", "id", "mail_tag_mail", "id_tag", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["id_tag"]) < 0),
                new RelationInfo("mail_mailbox", "id", "mail_chain_x_crm_entity", "id_mailbox"),
                new RelationInfo("crm_contact", "id", "mail_chain_x_crm_entity", "entity_id", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["entity_type"]) == 1),
                new RelationInfo("crm_case", "id", "mail_chain_x_crm_entity", "entity_id", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["entity_type"]) == 2),
                new RelationInfo("crm_deal", "id", "mail_chain_x_crm_entity", "entity_id", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["entity_type"]) == 3),
                new RelationInfo("mail_mailbox", "id", "mail_mailbox_signature", "id_mailbox"),
                new RelationInfo("files_folder", "id", "mail_mailbox", "email_in_folder", typeof(FilesModuleSpecifics)),
                new RelationInfo("mail_mailbox", "id", "mail_mailbox_autoreply", "id_mailbox"),
                new RelationInfo("mail_mailbox", "id", "mail_mailbox_autoreply_history", "id_mailbox"),
                new RelationInfo("mail_contacts", "id", "mail_contact_info", "id_contact")
            };

        public override ModuleName ModuleName
        {
            get { return ModuleName.Mail; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get { return _tables; }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get { return _tableRelations; }
        }

        protected override string GetSelectCommandConditionText(int tenantId, TableInfo table)
        {
            //optimization: 1) do not include "deleted" rows, 2) backup mail only for the last 30 days
            switch (table.Name)
            {
                case "mail_mailbox":
                    return string.Format("where t.is_removed = 0 and t.tenant = {0} and t.is_removed = 0", tenantId);

                //condition on chain_date because of Bug 18855 - transfer mail only for the last 30 days
                case "mail_mail":
                    return string.Format("inner join mail_mailbox t1 on t1.id = t.id_mailbox " +
                                         "where t.tenant = {0} and t.is_removed = 0 and t1.is_removed = 0 and t.chain_date > '{1}'",
                                         tenantId,
                                         DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-MM-dd HH:mm:ss"));

                case "mail_attachment":
                case "mail_tag_mail":
                    return string.Format("inner join mail_mail as t1 on t1.id = t.id_mail " +
                                         "inner join mail_mailbox as t2 on t2.id = t1.id_mailbox " +
                                         "where t1.tenant = {0} and t1.is_removed = 0 and t2.is_removed = 0 and t1.chain_date > '{1}'",
                                         tenantId,
                                         DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-MM-dd HH:mm:ss"));

                case "mail_chain":
                    return string.Format("inner join mail_mailbox t1 on t1.id = t.id_mailbox " +
                                         "where t.tenant = {0} and t1.is_removed = 0",
                                         tenantId);

                default:
                    return base.GetSelectCommandConditionText(tenantId, table);
            }
        }

        protected override string GetDeleteCommandConditionText(int tenantId, TableInfo table)
        {
            //delete all rows regardless of whether there is is_removed = 1 or not
            return base.GetSelectCommandConditionText(tenantId, table);
        }

        public override bool TryAdjustFilePath(ColumnMapper columnMapper, ref string filePath)
        {
            //todo: hack: will be changed later
            filePath = Regex.Replace(filePath, @"^[-\w]+(?=/)", match => columnMapper.GetUserMapping(match.Value));
            return !filePath.StartsWith("/");
        }

        protected override bool TryPrepareRow(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row, out Dictionary<string, object> preparedRow)
        {
            if (table.Name == "mail_mailbox")
            {
                var boxType = row["is_teamlab_mailbox"];
                if (boxType != null && Convert.ToBoolean(int.Parse(boxType.ToString())))
                {
                    preparedRow = null;
                    return false;
                }
            }
            return base.TryPrepareRow(connection, columnMapper, table, row, out preparedRow);
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, IEnumerable<RelationInfo> relations, ref object value)
        {
            relations = relations.ToList();
            if (relations.All(x => x.ChildTable == "mail_chain" && x.ChildColumn == "tags"))
            {
                var mappedTags = new List<string>();

                foreach (var tag in value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)))
                {
                    object tagId;
                    if (tag > 0)
                    {
                        tagId = columnMapper.GetMapping("mail_tag", "id", tag);
                    }
                    else
                    {
                        tagId = columnMapper.GetMapping("crm_tag", "id", -tag);
                        if (tagId != null)
                        {
                            tagId = -Convert.ToInt32(tagId);
                        }
                    }
                    if (tagId != null)
                    {
                        mappedTags.Add(tagId.ToString());
                    }
                }

                value = string.Join(",", mappedTags.ToArray());
                return true;
            }

            return base.TryPrepareValue(connection, columnMapper, table, columnName, relations, ref value);
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
        {
            if (relation.ParentTable == "crm_tag" && relation.ChildColumn == "id_tag"
                && (relation.ChildTable == "mail_tag_mail" || relation.ChildTable == "mail_tag_addresses"))
            {
                var crmTagId = columnMapper.GetMapping(relation.ParentTable, relation.ParentColumn, -Convert.ToInt32(value));
                if (crmTagId == null)
                    return false;

                value = -Convert.ToInt32(crmTagId);
                return true;
            }

            return base.TryPrepareValue(connection, columnMapper, relation, ref value);
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, ref object value)
        {
            if (table.Name == "mail_mailbox" && (columnName == "smtp_password" || columnName == "pop3_password") && value != null)
            {
                try
                {
                    value = Helpers.CreateHash(value as string); // save original hash
                }
                catch (Exception err)
                {
                    Logging.LogFactory.Create().Error("Can not prepare value {0}: {1}", value, err);
                    value = null;
                }
                return true;
            }
            return base.TryPrepareValue(connection, columnMapper, table, columnName, ref value);
        }

        public override void PrepareData(DataTable data)
        {
            if (data.TableName == "mail_mailbox")
            {
                var address = data.Columns.Cast<DataColumn>().Single(c => c.ColumnName == "address");
                var smtp = data.Columns.Cast<DataColumn>().Single(c => c.ColumnName == "smtp_password");
                var pop3 = data.Columns.Cast<DataColumn>().Single(c => c.ColumnName == "pop3_password");
                var token = data.Columns.Cast<DataColumn>().Single(c => c.ColumnName == "token");
                for (var i = 0; i < data.Rows.Count; i++)
                {
                    var row = data.Rows[i];
                    try
                    {
                        row[smtp] = Helpers.CreateHash2(row[smtp] as string);
                        row[pop3] = Helpers.CreateHash2(row[pop3] as string);
                        row[token] = Helpers.CreateHash2(row[token] as string);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogFactory.Create().Error("Can not prepare data {0}: {1}", row[address] as string, ex);
                        data.Rows.Remove(row);
                        i--;
                    }
                }
            }
        }

        public override Stream PrepareData(string key, Stream stream, ColumnMapper columnMapper)
        {
            if (!key.EndsWith("body.html")) return stream;

            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                var data = streamReader.ReadToEnd();
                data = Regex.Replace(data, @"(htmleditorfiles|aggregator)(\/0\/|\/[\d]+\/\d\d\/\d\d\/)([-\w]+(?=/))", 
                    match => "/" + TennantPath.CreatePath(columnMapper.GetTenantMapping().ToString()) + "/" + columnMapper.GetUserMapping(match.Groups[3].Value));

                var content = Encoding.UTF8.GetBytes(data);

                stream.Position = 0;
                stream.Write(content, 0, content.Length);
            }

            return stream;
        }
    }
}
