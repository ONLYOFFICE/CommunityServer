/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Extension;
using ASC.Mail.Aggregator.Filter;
using ASC.Core.Tenants;
using ASC.Core;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region public conversations methods

// ReSharper disable InconsistentNaming
        private const int chunk_size = 3;
// ReSharper restore InconsistentNaming

        public List<MailMessageItem> GetConversations(
            int id_tenant,
            string id_user,
            MailFilter filter,
            DateTime? utc_chain_from_date,
            int from_message,
            bool? prev_flag,
            out bool has_more)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            using (var db = GetDb())
            {
                var res = GetFilteredChains(
                    db,
                    id_tenant,
                    id_user,
                    filter,
                    utc_chain_from_date,
                    from_message,
                    prev_flag,
                    out has_more);

                if (prev_flag.GetValueOrDefault(false) && !has_more)
                {
                    if (res.Count == filter.PageSize)
                        res.Reverse();
                    else
                    {
                        res = GetFilteredChains(db, id_tenant, id_user, filter, null, 0, false, out has_more);
                        has_more = false;
                    }
                } else if(prev_flag.GetValueOrDefault(false))
                    res.Reverse();

                var chain_ids = new List<string>();
                res.ForEach(x => chain_ids.Add(x.ChainId));

                var query = new SqlQuery(ChainTable.name)
                        .Select(
                            ChainTable.Columns.id,
                            ChainTable.Columns.id_mailbox,
                            ChainTable.Columns.length,
                            ChainTable.Columns.unread,
                            ChainTable.Columns.importance,
                            ChainTable.Columns.has_attachments,
                            ChainTable.Columns.tags)
                        .Where(GetUserWhere(id_user, id_tenant))
                        .Where(Exp.In(ChainTable.Columns.id, chain_ids))
                        .Where(GetSearchFolders(ChainTable.Columns.folder, filter.PrimaryFolder));

                var extended_info = db.ExecuteList(query)
                    .ConvertAll(x => new {
                        id = x[0].ToString(),
                        id_mailbox = Convert.ToInt32(x[1]),
                        length = Convert.ToInt32(x[2]),
                        unread = Convert.ToBoolean(x[3]),
                        importance = Convert.ToBoolean(x[4]),
                        has_attachments = Convert.ToBoolean(x[5]),
                        tags = x[6].ToString()});

                foreach (var item in res) {
                    var founded_info = extended_info.FindAll(x => x.id_mailbox == item.MailboxId && x.id == item.ChainId);
                    if (!founded_info.Any()) continue;
                    item.IsNew = founded_info.Any(x => x.unread);
                    item.HasAttachments = founded_info.Any(x => x.has_attachments);
                    item.Important = founded_info.Any(x => x.importance);
                    item.ChainLength = founded_info.Sum(x => x.length);
                    var first_or_default = founded_info.FirstOrDefault(x => !string.IsNullOrEmpty(x.tags));
                    item.LabelsString = first_or_default != null ? first_or_default.tags : "";
                }

                return res;
            }
        }

        public long GetNextConversationId(int tenant, string user, int id, MailFilter filter)
        {
            using (var db = GetDb())
            {
                var chain_date = db.ExecuteScalar<DateTime>(new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.chain_date)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.id, id));

                filter.PageSize = 1;
                bool has_more;
                var messages = GetFilteredChains(db, tenant, user, filter, chain_date, id, false, out has_more);
                return messages.Any() ? messages.First().Id : 0;
            }
        }

        public void SetConversationsFolder(int id_tenant, string id_user, int folder, List<int> ids)
        {
            if (!MailFolder.IsIdOk(folder))
                throw new ArgumentException("can't set folder to none system folder");

            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var list_objects = GetChainedMessagesInfo(db, id_tenant, id_user, ids,
                                                          new[]
                                                              {
                                                                  MailTable.Columns.id, MailTable.Columns.unread, MailTable.Columns.folder,
                                                                  MailTable.Columns.chain_id, MailTable.Columns.id_mailbox
                                                              });

                if (!list_objects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesFolder(db, id_tenant, id_user, list_objects, folder);
                    RecalculateFolders(db, id_tenant, id_user, true);
                    tx.Commit();
                }
            }
        }

        public void RestoreConversations(int id_tenant, string id_user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var list_objects = GetChainedMessagesInfo(db, id_tenant, id_user, ids,
                                                        new[]
                                                            {
                                                                MailTable.Columns.id, MailTable.Columns.unread,
                                                                MailTable.Columns.folder, MailTable.Columns.folder_restore,
                                                                MailTable.Columns.chain_id, MailTable.Columns.id_mailbox
                                                            });
                if (!list_objects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    RestoreMessages(db, id_tenant, id_user, list_objects);
                    RecalculateFolders(db, id_tenant, id_user, true);
                    tx.Commit();
                }
            }
        }

        public void DeleteConversations(int id_tenant, string id_user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            long used_quota;

            using (var db = GetDb())
            {
                var list_objects = GetChainedMessagesInfo(db, id_tenant, id_user, ids,
                                                          new[]
                                                              {
                                                                  MailTable.Columns.id,
                                                                  MailTable.Columns.folder,
                                                                  MailTable.Columns.unread
                                                              });

                if (!list_objects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    used_quota = DeleteMessages(db, id_tenant, id_user, list_objects, false);
                    RecalculateFolders(db, id_tenant, id_user, true);
                    tx.Commit();
                }
            }

            QuotaUsedDelete(id_tenant, used_quota);
        }

        public void SetConversationsReadFlags(int id_tenant, string id_user, List<int> ids, bool is_read)
        {
            using (var db = GetDb())
            {
                var list_objects = GetChainedMessagesInfo(db, id_tenant, id_user, ids, MessageInfoToSetUnread.Fields)
                    .ConvertAll(x => new MessageInfoToSetUnread(x));

                if (!list_objects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesReadFlags(db, id_tenant, id_user, list_objects, is_read);
                    tx.Commit();
                }
            }
        }

        public void SetConversationsImportanceFlags(int tenant, string user, bool important, List<int> ids)
        {
            var chains_info_query = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.chain_id)
                .Select(MailTable.Columns.folder)
                .Select(MailTable.Columns.id_mailbox)
                .Where(GetUserWhere(user, tenant))
                .Where(new InExp(MailTable.Columns.id, ids.Select(x => (object)x).ToArray()));

            var update_mail_query = new SqlUpdate(MailTable.name)
                .Set(MailTable.Columns.importance, important)
                .Where(GetUserWhere(user, tenant));

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var chains_info = db.ExecuteList(chains_info_query)
                                        .ConvertAll(i => new ChainInfo{
                                            id = (string)i[0],
                                            folder = Convert.ToInt32(i[1]),
                                            mailbox = Convert.ToInt32(i[2])});

                    chains_info = chains_info.Distinct().ToList();

                    if(!chains_info.Any())
                        throw new Exception("no chain messages belong to current user");

                    var where_chain_builder = new StringBuilder("(");
                    for (int i = 0; i < chains_info.Count; ++i)
                    {
                        var chain = chains_info[i];

                        if(i > 0)
                        {
                            where_chain_builder.Append(" or ");
                        }

                        where_chain_builder.AppendFormat("({0} = '{1}' and {2} = {3}", MailTable.Columns.chain_id, chain.id,
                                                                                        MailTable.Columns.id_mailbox, chain.mailbox);

                        if (chain.folder == MailFolder.Ids.inbox || chain.folder == MailFolder.Ids.sent)
                        {
                            where_chain_builder.AppendFormat(" and ({0} = {1} or {0} = {2}))", MailTable.Columns.folder,
                                                                MailFolder.Ids.inbox, MailFolder.Ids.sent);
                        }
                        else
                        {
                            where_chain_builder.AppendFormat(" and {0} = {1})", MailTable.Columns.folder, chain.folder);
                        }
                    }
                    where_chain_builder.Append(")");

                    db.ExecuteNonQuery(update_mail_query.Where(new SqlExp(where_chain_builder.ToString())));

                    foreach (var message in ids)
                    {
                        UpdateMessageChainImportanceFlag(db, tenant, user, message);
                    }

                    tx.Commit();
                }
            }
        }

        public void UpdateChain(string chain_id, int folder, int id_mailbox, int tenant, string user_id)
        {
            using (var db = GetDb())
            {
                UpdateChain(db, chain_id, folder, id_mailbox, tenant, user_id);
            }
        }


        private void MarkChainAsCrmLinked(DbManager db, string id_chain, int id_mailbox, int id_tenant, IEnumerable<CrmContactEntity> contact_ids)
        {
            var add_new_link_query = new SqlInsert(ChainXCrmContactEntity.name)
                                        .InColumns(ChainXCrmContactEntity.Columns.id_chain,
                                        ChainXCrmContactEntity.Columns.id_mailbox,
                                        ChainXCrmContactEntity.Columns.id_tenant,
                                        ChainXCrmContactEntity.Columns.entity_id,
                                        ChainXCrmContactEntity.Columns.entity_type);
            foreach (var contact_entity in contact_ids)
            {
                add_new_link_query.Values(id_chain, id_mailbox, id_tenant, contact_entity.Id, contact_entity.Type);
            }
            db.ExecuteNonQuery(add_new_link_query);
        }


        private void UnmarkChainAsCrmLinked(DbManager db, string id_chain, int id_mailbox, int id_tenant, IEnumerable<CrmContactEntity> contact_ids)
        {
            foreach (var crm_contact_entity in contact_ids)
            {
                var remove_link_query = new SqlDelete(ChainXCrmContactEntity.name)
                    .Where(ChainXCrmContactEntity.Columns.id_chain, id_chain)
                    .Where(ChainXCrmContactEntity.Columns.id_mailbox, id_mailbox)
                    .Where(ChainXCrmContactEntity.Columns.id_tenant, id_tenant)
                    .Where(ChainXCrmContactEntity.Columns.entity_id, crm_contact_entity.Id)
                    .Where(ChainXCrmContactEntity.Columns.entity_type, crm_contact_entity.Type);
                db.ExecuteNonQuery(remove_link_query);
            }
        }

        public void UpdateCrmLinkedChainId(int id_mailbox, int id_tenant, string old_chain_id, string new_chain_id)
        {
            if (string.IsNullOrEmpty(old_chain_id))
                throw new ArgumentNullException("old_chain_id");

            if (string.IsNullOrEmpty(new_chain_id))
                throw new ArgumentNullException("new_chain_id");

            if (old_chain_id.Equals(new_chain_id)) return;
            
            using (var db = GetDb())
            {
                var update_old_chain_id_query = new SqlUpdate(ChainXCrmContactEntity.name)
                    .Set(ChainXCrmContactEntity.Columns.id_chain, new_chain_id)
                    .Where(ChainXCrmContactEntity.Columns.id_chain, old_chain_id)
                    .Where(ChainXCrmContactEntity.Columns.id_mailbox, id_mailbox)
                    .Where(ChainXCrmContactEntity.Columns.id_tenant, id_tenant);

                db.ExecuteNonQuery(update_old_chain_id_query);
            }
        }

        public void UpdateCrmLinkedMailboxId(string chain_id, int id_tenant, int old_mailbox_id, int new_mailbox_id)
        {
            if (old_mailbox_id < 1)
                throw new ArgumentException("old_mailbox_id must be > 0");

            if (new_mailbox_id < 1)
                throw new ArgumentException("new_mailbox_id must be > 0");

            if (old_mailbox_id == new_mailbox_id) return;

            using (var db = GetDb())
            {
                var update_old_chain_id_query = new SqlUpdate(ChainXCrmContactEntity.name)
                    .Set(ChainXCrmContactEntity.Columns.id_mailbox, new_mailbox_id)
                    .Where(ChainXCrmContactEntity.Columns.id_chain, chain_id)
                    .Where(ChainXCrmContactEntity.Columns.id_mailbox, old_mailbox_id)
                    .Where(ChainXCrmContactEntity.Columns.id_tenant, id_tenant);

                db.ExecuteNonQuery(update_old_chain_id_query);
            }
        }

        public void LinkChainToCrm(int id_message, int id_tenant, string id_user, List<CrmContactEntity> contact_ids)
        {
            var factory = new DaoFactory(CoreContext.TenantManager.GetCurrentTenant().TenantId, CRMConstants.DatabaseId);
            foreach (var crm_contact_entity in contact_ids)
            {

                switch (crm_contact_entity.Type)
                {
                    case ChainXCrmContactEntity.EntityTypes.Contact:
                        var crm_contact = factory.GetContactDao().GetByID(crm_contact_entity.Id);
                        CRMSecurity.DemandAccessTo(crm_contact);
                        break;
                    case ChainXCrmContactEntity.EntityTypes.Case:
                        var crm_case = factory.GetCasesDao().GetByID(crm_contact_entity.Id);
                        CRMSecurity.DemandAccessTo(crm_case);
                        break;
                    case ChainXCrmContactEntity.EntityTypes.Opportunity:
                        var crm_opportunity = factory.GetDealDao().GetByID(crm_contact_entity.Id);
                        CRMSecurity.DemandAccessTo(crm_opportunity);
                        break;
                }
            }
            
            using (var db = GetDb())
            {
                var chain_info = GetMessageChainInfo(db, id_tenant, id_user, id_message);
                MarkChainAsCrmLinked(db, chain_info.id, chain_info.mailbox, id_tenant, contact_ids);
                AddChainMailsToCrmHistory(db, chain_info, id_tenant, id_user, contact_ids);
            }
        }

        public void MarkChainAsCrmLinked(int id_message, int id_tenant, string id_user, List<CrmContactEntity> contact_ids)
        {
            using (var db = GetDb())
            {
                var chain_info = GetMessageChainInfo(db, id_tenant, id_user, id_message);
                MarkChainAsCrmLinked(db, chain_info.id, chain_info.mailbox, id_tenant, contact_ids);
            }
        }

        private void AddChainMailsToCrmHistory(DbManager db, ChainInfo chain_info, int id_tenant, string id_user, List<CrmContactEntity> contact_ids)
        {
            var search_folders = new List<int> {MailFolder.Ids.inbox, MailFolder.Ids.sent};
            var select_chained_mails = GetQueryForChainMessagesSelection(chain_info.mailbox, chain_info.id, search_folders);
            var crm_dal = new CrmHistoryDal(this, id_tenant, id_user);
            db.ExecuteList(select_chained_mails)
                .ConvertAll(record =>
                    {
                        var item = GetMailInfo(db, id_tenant, id_user, Convert.ToInt32(record[0]), true, true);
                        item.LinkedCrmEntityIds = contact_ids;
                        return item;
                    })
                .ForEach(crm_dal.AddRelationshipEvents);
        }


        public void UnmarkChainAsCrmLinked(int id_message, int id_tenant, string id_user, IEnumerable<CrmContactEntity> contact_ids)
        {
            using (var db = GetDb())
            {
                var chain_info = GetMessageChainInfo(db, id_tenant, id_user, id_message);
                UnmarkChainAsCrmLinked(db, chain_info.id, chain_info.mailbox, id_tenant, contact_ids);
            }
        }
        #endregion

        #region private conversations methods
        private List<MailMessageItem> GetFilteredChains(
            IDbManager db,
            int id_tenant,
            string id_user,
            MailFilter filter,
            DateTime? utc_chain_from_date,
            int from_message,
            bool? prev_flag,
            out bool has_more)
        {
            var res = new List<MailMessageItem>();
            var chains_to_skip = new List<ChainInfo>();
            var skip_flag = false;
            var chunck_index = 0;

            var sort_order = filter.SortOrder == "ascending";

            if (prev_flag.GetValueOrDefault(false))
                sort_order = !sort_order;

            var query_messages = new SqlQuery(MailTable.name)
                .Select(
                    MailTable.Columns.id,
                    MailTable.Columns.from,
                    MailTable.Columns.to,
                    MailTable.Columns.reply,
                    MailTable.Columns.subject,
                    MailTable.Columns.importance,
                    MailTable.Columns.date_sent,
                    MailTable.Columns.size,
                    MailTable.Columns.attach_count,
                    MailTable.Columns.unread,
                    MailTable.Columns.is_answered,
                    MailTable.Columns.is_forwarded,
                    MailTable.Columns.is_from_crm,
                    MailTable.Columns.is_from_tl,
                    MailTable.Columns.folder_restore,
                    MailTable.Columns.folder,
                    MailTable.Columns.chain_id,
                    MailTable.Columns.id_mailbox,
                    MailTable.Columns.chain_date)
                .Where(GetUserWhere(id_user, id_tenant))
                .Where(MailTable.Columns.is_removed, false)
                .ApplyFilter(filter)
                .OrderBy(MailTable.Columns.chain_date, sort_order);

            if (null != utc_chain_from_date)
            {
                query_messages = query_messages.Where(sort_order ?
                    Exp.Ge(MailTable.Columns.chain_date, utc_chain_from_date) :
                    Exp.Le(MailTable.Columns.chain_date, utc_chain_from_date));
                skip_flag = true;
            }

            // We are increasing the size of the page to check whether it is necessary to show the Next button.
            while (res.Count < filter.PageSize + 1)
            {
                query_messages.SetFirstResult(chunck_index * chunk_size * filter.PageSize).SetMaxResults(chunk_size * filter.PageSize);
                chunck_index++;

                var tenant_obj = CoreContext.TenantManager.GetTenant(id_tenant);
                var list = db
                    .ExecuteList(query_messages)
                    .ConvertAll(r =>
                        ConvertToConversation(r, tenant_obj));

                if (0 == list.Count)
                    break;

                foreach (var item in list)
                {
                    var chain_info = new ChainInfo {id = item.ChainId, mailbox = item.MailboxId};
                    
                    // Skip chains that was stored before and within from_message's chain.
                    if (skip_flag)
                    {
                        var tenant = CoreContext.TenantManager.GetTenant(id_tenant);
                        if (item.ChainDate != TenantUtil.DateTimeFromUtc(tenant, utc_chain_from_date.GetValueOrDefault()))
                            skip_flag = false;
                        else
                        {
                            if (item.Id == from_message)
                                skip_flag = false;
                            chains_to_skip.Add(chain_info);
                            continue;
                        }
                    }

                    if (chains_to_skip.Contains(chain_info))
                        continue;

                    var already_contains = false;
                    foreach(var chain in res){
                        if(chain.ChainId == item.ChainId && chain.MailboxId == item.MailboxId){
                            already_contains = true;
                            if(chain.Date < item.Date)
                                res[res.IndexOf(chain)] = item;
                            break;
                        }
                    }

                    if(!already_contains)
                        res.Add(item);

                    if (filter.PageSize + 1 == res.Count)
                        break;
                }

                var is_all_needed_conversation_gathered = filter.PageSize + 1 == res.Count;
                if (is_all_needed_conversation_gathered)
                    break;

                var is_enough_messages_for_page = chunk_size*filter.PageSize <= list.Count;
                if (!is_enough_messages_for_page)
                    break;
            }

            has_more = res.Count > filter.PageSize;

            if (has_more)
                res.RemoveAt(filter.PageSize);

            return res;
        }

        private static MailMessageItem ConvertToConversation(object[] r, Tenant tenant_obj)
        {
            var now = TenantUtil.DateTimeFromUtc(tenant_obj, DateTime.UtcNow);
            var date = TenantUtil.DateTimeFromUtc(tenant_obj, (DateTime)r[6]);
            var chain_date = TenantUtil.DateTimeFromUtc(tenant_obj, (DateTime)r[18]);

            var is_today = (now.Year == date.Year && now.Date == date.Date);
            var is_yesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            return new MailMessageItem
            {
                Id = Convert.ToInt64(r[0]),
                From = (string)r[1],
                To = (string)r[2],
                ReplyTo = (string)r[3],
                Subject = (string)r[4],
                Important = Convert.ToBoolean(r[5]),
                Date = date,
                Size = Convert.ToInt32(r[7]),
                HasAttachments = Convert.ToBoolean(r[8]),
                IsNew = Convert.ToBoolean(r[9]),
                IsAnswered = Convert.ToBoolean(r[10]),
                IsForwarded = Convert.ToBoolean(r[11]),
                IsFromCRM = Convert.ToBoolean(r[12]),
                IsFromTL = Convert.ToBoolean(r[13]),
                RestoreFolderId = r[14] != null ? Convert.ToInt32(r[14]) : -1,
                Folder = Convert.ToInt32(r[15]),
                ChainId = (string)(r[16] ?? ""),
                IsToday = is_today,
                IsYesterday = is_yesterday,
                MailboxId = Convert.ToInt32(r[17]),
                LabelsString = "",
                ChainDate = chain_date,
                ChainLength = 1
            };
        }

        private static Exp GetSearchFolders(string folder_name, int folder)
        {
            Exp condition;

            if (folder == MailFolder.Ids.inbox || folder == MailFolder.Ids.sent)
                condition = Exp.In(folder_name, new[] { MailFolder.Ids.inbox, MailFolder.Ids.sent });
            else
                condition = Exp.Eq(folder_name, folder);

            return condition;
        }

        private static List<int> GetSearchFolders(int folder) {
            var search_folders = new List<int>();

            if (folder == MailFolder.Ids.inbox || folder == MailFolder.Ids.sent)
                search_folders.AddRange(new[] { MailFolder.Ids.inbox, MailFolder.Ids.sent });
            else
                search_folders.Add(folder);

            return search_folders;
        }

       private static List<object[]> GetChainedMessagesInfo(IDbManager db, int tenant, string user, List<int> ids, string[] fields)
        {
            var selected_messages_query = string.Format(
                "SELECT {0}, {1}, {2} FROM {3} WHERE {4} IN ({5})",
                MailTable.Columns.folder,
                MailTable.Columns.chain_id,
                MailTable.Columns.id_mailbox,
                MailTable.name,
                MailTable.Columns.id,
                string.Join(",", ids.ConvertAll(x => x.ToString(CultureInfo.InvariantCulture)).ToArray()));

            var extended_fields = new List<string>
                {
                    MailTable.Columns.folder,
                    MailTable.Columns.chain_id,
                    MailTable.Columns.id_mailbox
                };
            extended_fields.AddRange(fields);

            var not_removed_user_messages_query = string.Format(
                "SELECT {0} FROM {1} WHERE {2}={3} AND {4}=\"{5}\" AND {6}=0",
                string.Join(", ", extended_fields.Distinct().ToArray()),
                MailTable.name,
                MailTable.Columns.id_tenant, tenant,
                MailTable.Columns.id_user, user,
                MailTable.Columns.is_removed);

            var query = string.Format(
                "SELECT {0} FROM ({1}) AS mm INNER JOIN ({2}) AS sm" +
                " ON mm.{3}=sm.{3} AND (mm.{4}=sm.{4} OR (mm.{4} IN (1,2) AND sm.{4} IN (1,2))) AND mm.{5}=sm.{5}",
                "mm." + string.Join(", mm.", fields),
                not_removed_user_messages_query,
                selected_messages_query,
                MailTable.Columns.chain_id,
                MailTable.Columns.folder,
                MailTable.Columns.id_mailbox);

            // Return collection of MAIL_MAIL.id by collection of MAIL_MAIL.chain_id
            return db.ExecuteList(query);
        }

        // Method for updating chain flags, date and length.
        private void UpdateChain(IDbManager db, string chain_id, int folder, int id_mailbox, int tenant, string user)
        {
            if (string.IsNullOrEmpty(chain_id)) return;

            var chain = db.ExecuteList(
                new SqlQuery(MailTable.name)
                    .SelectCount()
                    .SelectMax(MailTable.Columns.date_sent)
                    .SelectMax(MailTable.Columns.unread)
                    .SelectMax(MailTable.Columns.attach_count)
                    .SelectMax(MailTable.Columns.importance)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.is_removed, 0)
                    .Where(MailTable.Columns.chain_id, chain_id)
                    .Where(MailTable.Columns.id_mailbox, id_mailbox)
                    .Where(MailTable.Columns.folder, folder))
                .ConvertAll(x => new
                {
                    length = Convert.ToInt32(x[0]),
                    date = Convert.ToDateTime(x[1]),
                    unread = Convert.ToBoolean(x[2]),
                    attach_count = Convert.ToInt32(x[3]),
                    importance = Convert.ToBoolean(x[4])
                })
                .FirstOrDefault();

            if (chain == null) 
                throw new InvalidDataException("Conversation is absent in MAIL_MAIL");

            var stored_chain_info = db.ExecuteList(
                new SqlQuery(ChainTable.name)
                    .Select(ChainTable.Columns.unread)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.id, chain_id)
                    .Where(ChainTable.Columns.folder, folder)
                    .Where(MailTable.Columns.id_mailbox, id_mailbox));

            var chain_unread_flag = stored_chain_info.Any() && Convert.ToBoolean(stored_chain_info.First()[0]); 

            if (0 == chain.length)
            {
                db.ExecuteNonQuery(
                    new SqlDelete(ChainTable.name)
                        .Where(GetUserWhere(user, tenant))
                        .Where(ChainTable.Columns.id, chain_id)
                        .Where(ChainTable.Columns.id_mailbox, id_mailbox)
                        .Where(ChainTable.Columns.folder, folder));

                _log.Debug("UpdateChain() row deleted from chain table tenant='{0}', user_id='{1}', id_mailbox='{2}', folder='{3}', chain_id='{4}'",
                    tenant, user, id_mailbox, folder, chain_id);

                ChangeFolderCounters(db, tenant, user, folder, 0, 0, chain_unread_flag ? -1 : 0 , -1);
            }
            else
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.name)
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.is_removed, 0)
                        .Where(MailTable.Columns.chain_id, chain_id)
                        .Where(MailTable.Columns.id_mailbox, id_mailbox)
                        .Where(MailTable.Columns.folder, folder) // Folder condition important because chain has different dates in different folders(Ex: Sent and Inbox).
                        .Set(MailTable.Columns.chain_date, chain.date));

                var tags = GetChainTags(db, chain_id, folder, id_mailbox, tenant, user);

                var result = db.ExecuteNonQuery(
                    new SqlInsert(ChainTable.name, true)
                        .InColumnValue(ChainTable.Columns.id, chain_id)
                        .InColumnValue(ChainTable.Columns.id_mailbox, id_mailbox)
                        .InColumnValue(ChainTable.Columns.id_tenant, tenant)
                        .InColumnValue(ChainTable.Columns.id_user, user)
                        .InColumnValue(ChainTable.Columns.folder, folder)
                        .InColumnValue(ChainTable.Columns.length, chain.length)
                        .InColumnValue(ChainTable.Columns.unread, chain.unread)
                        .InColumnValue(ChainTable.Columns.has_attachments, chain.attach_count > 0)
                        .InColumnValue(ChainTable.Columns.importance, chain.importance)
                        .InColumnValue(ChainTable.Columns.tags, tags));

                if (result <= 0)
                    throw new InvalidOperationException(String.Format("Invalid insert to {0}", ChainTable.name));

                _log.Debug("UpdateChain() row inserted to chain table tenant='{0}', user_id='{1}', id_mailbox='{2}', folder='{3}', chain_id='{4}'",
                    tenant, user, id_mailbox, folder, chain_id);

                var unread_conv_diff = 0;
                var total_conv_diff = 0;

                if (!stored_chain_info.Any())
                {
                    total_conv_diff = 1;
                    unread_conv_diff = chain.unread ? 1 : 0;
                }
                else
                {
                    if (chain_unread_flag != chain.unread)
                        unread_conv_diff = chain.unread ? 1 : -1;
                }

                ChangeFolderCounters(db, tenant, user, folder, 0, 0, unread_conv_diff, total_conv_diff);
            }
        }

        private void UpdateChainTags(DbManager db, string chain_id, int folder, int id_mailbox, int tenant, string user)
        {
            var tags = GetChainTags(db, chain_id, folder, id_mailbox, tenant, user);

            db.ExecuteNonQuery(
                new SqlUpdate(ChainTable.name)
                    .Set(ChainTable.Columns.tags, tags)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.id, chain_id)
                    .Where(ChainTable.Columns.id_mailbox, id_mailbox)
                    .Where(ChainTable.Columns.folder, folder));
        }

        private void UpdateMessageChainAttachmentsFlag(IDbManager db, int tenant, string user, int message_id)
        {
            UpdateMessageChainFlag(db, tenant, user, message_id, MailTable.Columns.attach_count, ChainTable.Columns.has_attachments);
        }

        private void UpdateMessageChainUnreadFlag(IDbManager db, int tenant, string user, int message_id)
        {
            UpdateMessageChainFlag(db, tenant, user, message_id, MailTable.Columns.unread, ChainTable.Columns.unread);
        }

        private void UpdateMessageChainImportanceFlag(IDbManager db, int tenant, string user, int message_id)
        {
            UpdateMessageChainFlag(db, tenant, user, message_id, MailTable.Columns.importance, ChainTable.Columns.importance);
        }

        private void UpdateMessageChainFlag(IDbManager db, int tenant, string user, int message_id, string field_from, string field_to)
        {
            var chain = GetMessageChainInfo(db, tenant, user, message_id);

            if (string.IsNullOrEmpty(chain.id) || chain.folder < 1 || chain.mailbox < 1) return;

            var field_query = new SqlQuery(MailTable.name)
                .SelectMax(field_from)
                .Where(MailTable.Columns.is_removed, 0)
                .Where(MailTable.Columns.chain_id, chain.id)
                .Where(MailTable.Columns.id_mailbox, chain.mailbox)
                .Where(GetUserWhere(user, tenant))
                .Where(GetSearchFolders(MailTable.Columns.folder, chain.folder));

            var field_val = db.ExecuteScalar<bool>(field_query);

            var update_query = new SqlUpdate(ChainTable.name)
                    .Set(field_to, field_val)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.id, chain.id)
                    .Where(ChainTable.Columns.id_mailbox, chain.mailbox)
                    .Where(GetSearchFolders(ChainTable.Columns.folder, chain.folder));

            db.ExecuteNonQuery(update_query);
        }

        private ChainInfo GetMessageChainInfo(IDbManager db, int tenant, string user, int message_id)
        {
            var info = db.ExecuteList(new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.chain_id, MailTable.Columns.folder, MailTable.Columns.id_mailbox)
                .Where(MailTable.Columns.id, message_id)
                .Where(GetUserWhere(user, tenant)));

            return new ChainInfo
            {
                id = info[0][0] == null ? "" : info[0][0].ToString(),
                folder = Convert.ToInt32(info[0][1]),
                mailbox = Convert.ToInt32(info[0][2])
            };
        }

        private string GetChainTags(IDbManager db, string chain_id, int folder, int id_mailbox, int tenant,
                                    string user_id)
        {
            const string mm_alias = "ch";
            const string mtm_alias = "tm";

            var mail_query = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.id)
                .Where(MailTable.Columns.chain_id, chain_id)
                .Where(MailTable.Columns.is_removed, 0)
                .Where(GetUserWhere(user_id, tenant))
                .Where(MailTable.Columns.folder, folder)
                .Where(MailTable.Columns.id_mailbox, id_mailbox);

            var new_query = new SqlQuery(MAIL_TAG_MAIL + " " + mtm_alias)
                .Select(TagMailFields.id_tag.Prefix(mtm_alias))
                .Distinct()
                .InnerJoin(mail_query, mm_alias,
                           Exp.EqColumns(MailTable.Columns.id.Prefix(mm_alias), TagMailFields.id_mail.Prefix(mtm_alias)))
                .OrderBy(TagMailFields.time_created.Prefix(mtm_alias), true);


            var tags = db.ExecuteList(new_query)
                         .ConvertAll(x => x[0].ToString());

            return String.Join(",", tags.ToArray());
        }

        #endregion

        #region private declarations

        private struct ChainInfo {
            public string id;
            public int folder;
            public int mailbox;
        }

        #endregion
    }
}
