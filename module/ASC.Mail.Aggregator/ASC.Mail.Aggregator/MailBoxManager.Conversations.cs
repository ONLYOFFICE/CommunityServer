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
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Extension;
using ASC.Mail.Aggregator.Filter;
using Newtonsoft.Json.Linq;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region public conversations methods

        private const int CHUNK_SIZE = 3;

        public List<MailMessage> GetConversations(
            int tenant,
            string user,
            MailFilter filter,
            DateTime? utcChainFromDate,
            int fromMessageId,
            bool? prevFlag,
            out bool hasMore)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            using (var db = GetDb())
            {
                var res = GetFilteredChains(
                    db,
                    tenant,
                    user,
                    filter,
                    utcChainFromDate,
                    fromMessageId,
                    prevFlag,
                    out hasMore);

                if (!res.Any())
                    return res;

                if (prevFlag.GetValueOrDefault(false) && !hasMore)
                {
                    if (res.Count == filter.PageSize)
                        res.Reverse();
                    else
                    {
                        res = GetFilteredChains(db, tenant, user, filter, null, 0, false, out hasMore);
                        hasMore = false;

                        if (!res.Any())
                            return res;
                    }
                } else if(prevFlag.GetValueOrDefault(false))
                    res.Reverse();

                var chainIds = new List<string>();
                res.ForEach(x => chainIds.Add(x.ChainId));

                var query = new SqlQuery(ChainTable.name)
                        .Select(
                            ChainTable.Columns.id,
                            ChainTable.Columns.id_mailbox,
                            ChainTable.Columns.length,
                            ChainTable.Columns.unread,
                            ChainTable.Columns.importance,
                            ChainTable.Columns.has_attachments,
                            ChainTable.Columns.tags)
                        .Where(GetUserWhere(user, tenant))
                        .Where(Exp.In(ChainTable.Columns.id, chainIds))
                        .Where(GetSearchFolders(ChainTable.Columns.folder, filter.PrimaryFolder));

                var extendedInfo = db.ExecuteList(query)
                    .ConvertAll(x => new {
                        id = x[0].ToString(),
                        id_mailbox = Convert.ToInt32(x[1]),
                        length = Convert.ToInt32(x[2]),
                        unread = Convert.ToBoolean(x[3]),
                        importance = Convert.ToBoolean(x[4]),
                        has_attachments = Convert.ToBoolean(x[5]),
                        tags = x[6].ToString()});

                foreach (var item in res) {
                    var foundedInfo = extendedInfo.FindAll(x => x.id_mailbox == item.MailboxId && x.id == item.ChainId);
                    if (!foundedInfo.Any()) continue;
                    item.IsNew = foundedInfo.Any(x => x.unread);
                    item.HasAttachments = foundedInfo.Any(x => x.has_attachments);
                    item.Important = foundedInfo.Any(x => x.importance);
                    item.ChainLength = foundedInfo.Sum(x => x.length);
                    var firstOrDefault = foundedInfo.FirstOrDefault(x => !string.IsNullOrEmpty(x.tags));
                    item.LabelsString = firstOrDefault != null ? firstOrDefault.tags : "";
                }

                return res;
            }
        }

        public long GetNextConversationId(int tenant, string user, int id, MailFilter filter)
        {
            using (var db = GetDb())
            {
                var chainDate = db.ExecuteScalar<DateTime>(new SqlQuery(MailTable.name)
                    .Select(MailTable.Columns.chain_date)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.id, id));

                filter.PageSize = 1;
                bool hasMore;
                var messages = GetFilteredChains(db, tenant, user, filter, chainDate, id, false, out hasMore);
                return messages.Any() ? messages.First().Id : 0;
            }
        }

        public void SendConversationsToSpamTrainer(int tenant, string user, List<int> ids, bool isSpam)
        {
            var userCulture = Thread.CurrentThread.CurrentCulture;
            var userUiCulture = Thread.CurrentThread.CurrentUICulture;
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture = userCulture;
                    Thread.CurrentThread.CurrentUICulture = userUiCulture;

                    CoreContext.TenantManager.SetCurrentTenant(tenant);

                    var tlMails = GetTlMailStreamList(tenant, user, ids);
                    SendEmlUrlsToSpamTrainer(tenant, user, tlMails, isSpam);
                }
                catch (Exception ex)
                {
                    _log.Error("SendConversationsToSpamTrainer() failed with exception:\r\n{0}", ex.ToString());
                }
            });

        }

        private Dictionary<int, string> GetTlMailStreamList(int tenant, string user, List<int> ids)
        {
            var streamList = new Dictionary<int, string>();

            using (var db = GetDb())
            {
                var tlMailboxesIdsQuery = new SqlQuery(MailboxTable.name)
                    .Select(MailboxTable.Columns.id)
                    .Where(MailboxTable.Columns.id_tenant, tenant)
                    .Where(MailboxTable.Columns.id_user, user.ToLowerInvariant())
                    .Where(MailboxTable.Columns.is_teamlab_mailbox, true)
                    .Where(MailboxTable.Columns.is_removed, false);

                var tlMailboxesIds = db.ExecuteList(tlMailboxesIdsQuery)
                                         .ConvertAll(r => Convert.ToInt32(r[0]));

                if (!tlMailboxesIds.Any())
                    return streamList;

                streamList = GetChainedMessagesInfo(db, tenant, user, ids,
                                                     new[]
                                                         {
                                                             MailTable.Columns.id,
                                                             MailTable.Columns.folder_restore,
                                                             MailTable.Columns.id_mailbox,
                                                             MailTable.Columns.stream
                                                         })
                    .ConvertAll(r => new
                        {
                            id_mail = Convert.ToInt32(r[0]),
                            folder_restore = Convert.ToInt32(r[1]),
                            id_mailbox = Convert.ToInt32(r[2]),
                            stream = Convert.ToString(r[3])
                        })
                    .Where(r => r.folder_restore != MailFolder.Ids.sent)
                    .Where(r => tlMailboxesIds.Contains(r.id_mailbox))
                    .ToDictionary(r => r.id_mail, r => r.stream);
            }

            return streamList;
        }

        private void SendEmlUrlsToSpamTrainer(int tenant, string user, Dictionary<int, string> tlMails,
                                              bool isSpam)
        {
            if (!tlMails.Any())
                return;

            using (var db = GetDb())
            {
                var serverInformationQuery = new SqlQuery(ServerTable.name)
                    .InnerJoin(TenantXServerTable.name,
                               Exp.EqColumns(TenantXServerTable.Columns.id_server, ServerTable.Columns.id))
                    .Select(ServerTable.Columns.connection_string)
                    .Where(TenantXServerTable.Columns.id_tenant, tenant);

                var serverInfo = db.ExecuteList(serverInformationQuery)
                                    .ConvertAll(r =>
                                        {
                                            var connectionString = Convert.ToString(r[0]);
                                            var json = JObject.Parse(connectionString);

                                            if (json["Api"] != null)
                                            {
                                                return new
                                                    {
                                                        server_ip = json["Api"]["Server"].ToString(),
                                                        port = Convert.ToInt32(json["Api"]["Port"].ToString()),
                                                        protocol = json["Api"]["Protocol"].ToString(),
                                                        version = json["Api"]["Version"].ToString(),
                                                        token = json["Api"]["Token"].ToString()
                                                    };
                                            }

                                            return null;
                                        }
                    ).SingleOrDefault(info => info != null);

                if (serverInfo == null)
                {
                    _log.Error(
                        "SendEmlUrlsToSpamTrainer: Can't sent task to spam trainer. Empty server api info.");
                    return;
                }


                foreach (var tlSpamMail in tlMails)
                {
                    try
                    {
                        var emlUrl = GetMailEmlUrl(tenant, user, tlSpamMail.Value);

                        ApiHelper.SendEmlToSpamTrainer(serverInfo.server_ip, serverInfo.protocol, serverInfo.port,
                                                        serverInfo.version, serverInfo.token, emlUrl, isSpam);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("SendEmlUrlsToSpamTrainer() Exception: \r\n {0}", ex.ToString());
                    }
                }
            }
        }

        public void SetConversationsFolder(int tenant, string user, int folder, List<int> ids)
        {
            if (!MailFolder.IsIdOk(folder))
                throw new ArgumentException("can't set folder to none system folder");

            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var listObjects = GetChainedMessagesInfo(db, tenant, user, ids,
                                                          new[]
                                                              {
                                                                  MailTable.Columns.id, MailTable.Columns.unread, MailTable.Columns.folder,
                                                                  MailTable.Columns.chain_id, MailTable.Columns.id_mailbox
                                                              });

                if (!listObjects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesFolder(db, tenant, user, listObjects, folder);
                    RecalculateFolders(db, tenant, user, true);
                    tx.Commit();
                }
            }
        }

        public void RestoreConversations(int tenant, string user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            using (var db = GetDb())
            {
                var listObjects = GetChainedMessagesInfo(db, tenant, user, ids,
                                                        new[]
                                                            {
                                                                MailTable.Columns.id, MailTable.Columns.unread,
                                                                MailTable.Columns.folder, MailTable.Columns.folder_restore,
                                                                MailTable.Columns.chain_id, MailTable.Columns.id_mailbox
                                                            });
                if (!listObjects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    RestoreMessages(db, tenant, user, listObjects);
                    RecalculateFolders(db, tenant, user, true);
                    tx.Commit();
                }
            }
        }

        public void DeleteConversations(int tenant, string user, List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentNullException("ids");

            long usedQuota;

            using (var db = GetDb())
            {
                var listObjects = GetChainedMessagesInfo(db, tenant, user, ids,
                                                          new[]
                                                              {
                                                                  MailTable.Columns.id,
                                                                  MailTable.Columns.folder,
                                                                  MailTable.Columns.unread
                                                              });

                if (!listObjects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    usedQuota = DeleteMessages(db, tenant, user, listObjects, false);
                    RecalculateFolders(db, tenant, user, true);
                    tx.Commit();
                }
            }

            QuotaUsedDelete(tenant, usedQuota);
        }

        public void SetConversationsReadFlags(int tenant, string user, List<int> ids, bool isRead)
        {
            using (var db = GetDb())
            {
                var listObjects = GetChainedMessagesInfo(db, tenant, user, ids, MessageInfoToSetUnread.Fields)
                    .ConvertAll(x => new MessageInfoToSetUnread(x));

                if (!listObjects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesReadFlags(db, tenant, user, listObjects, isRead);
                    tx.Commit();
                }
            }
        }

        public void SetConversationsImportanceFlags(int tenant, string user, bool important, List<int> ids)
        {
            var chainsInfoQuery = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.chain_id)
                .Select(MailTable.Columns.folder)
                .Select(MailTable.Columns.id_mailbox)
                .Where(GetUserWhere(user, tenant))
                .Where(Exp.In(MailTable.Columns.id, ids.Select(x => (object)x).ToArray()));

            var updateMailQuery = new SqlUpdate(MailTable.name)
                .Set(MailTable.Columns.importance, important)
                .Where(GetUserWhere(user, tenant));

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var chainsInfo = db.ExecuteList(chainsInfoQuery)
                                        .ConvertAll(i => new ChainInfo{
                                            id = (string)i[0],
                                            folder = Convert.ToInt32(i[1]),
                                            mailbox = Convert.ToInt32(i[2])});

                    chainsInfo = chainsInfo.Distinct().ToList();

                    if(!chainsInfo.Any())
                        throw new Exception("no chain messages belong to current user");

                    var where = Exp.Empty;
                    foreach (var chain in chainsInfo)
                    {
                        var innerWhere = Exp.Eq(MailTable.Columns.chain_id, chain.id) & Exp.Eq(MailTable.Columns.id_mailbox, chain.mailbox);

                        if (chain.folder == MailFolder.Ids.inbox || chain.folder == MailFolder.Ids.sent)
                        {
                            innerWhere &= (Exp.Eq(MailTable.Columns.folder, MailFolder.Ids.inbox) | Exp.Eq(MailTable.Columns.folder, MailFolder.Ids.sent));
                        }
                        else
                        {
                            innerWhere &= Exp.Eq(MailTable.Columns.folder, chain.folder);
                        }

                        @where |= innerWhere;
                    }

                    db.ExecuteNonQuery(updateMailQuery.Where(where));

                    foreach (var message in ids)
                    {
                        UpdateMessageChainImportanceFlag(db, tenant, user, message);
                    }

                    tx.Commit();
                }
            }
        }

        public void UpdateChain(string chainId, int folder, int mailboxId, int tenant, string user)
        {
            using (var db = GetDb())
            {
                UpdateChain(db, chainId, folder, mailboxId, tenant, user);
            }
        }


        private void MarkChainAsCrmLinked(DbManager db, string chainId, int mailboxId, int tenant, IEnumerable<CrmContactEntity> contactIds)
        {
            var addNewLinkQuery = new SqlInsert(ChainXCrmContactEntity.name)
                                        .InColumns(ChainXCrmContactEntity.Columns.id_chain,
                                        ChainXCrmContactEntity.Columns.id_mailbox,
                                        ChainXCrmContactEntity.Columns.id_tenant,
                                        ChainXCrmContactEntity.Columns.entity_id,
                                        ChainXCrmContactEntity.Columns.entity_type);
            foreach (var contactEntity in contactIds)
            {
                addNewLinkQuery.Values(chainId, mailboxId, tenant, contactEntity.Id, contactEntity.Type);
            }
            db.ExecuteNonQuery(addNewLinkQuery);
        }


        private void UnmarkChainAsCrmLinked(DbManager db, string chainId, int mailboxId, int tenant, IEnumerable<CrmContactEntity> contactIds)
        {
            foreach (var crmContactEntity in contactIds)
            {
                var removeLinkQuery = new SqlDelete(ChainXCrmContactEntity.name)
                    .Where(ChainXCrmContactEntity.Columns.id_chain, chainId)
                    .Where(ChainXCrmContactEntity.Columns.id_mailbox, mailboxId)
                    .Where(ChainXCrmContactEntity.Columns.id_tenant, tenant)
                    .Where(ChainXCrmContactEntity.Columns.entity_id, crmContactEntity.Id)
                    .Where(ChainXCrmContactEntity.Columns.entity_type, crmContactEntity.Type);
                db.ExecuteNonQuery(removeLinkQuery);
            }
        }

        private void UpdateCrmLinkedMailboxId(DbManager db, string chainId, int tenant, int oldMailboxId,
            int newMailboxId)
        {
            if (oldMailboxId < 1)
                throw new ArgumentException("old_mailbox_id must be > 0");

            if (newMailboxId < 1)
                throw new ArgumentException("new_mailbox_id must be > 0");

            if (oldMailboxId == newMailboxId) return;

            var updateOldChainIdQuery = new SqlUpdate(ChainXCrmContactEntity.name)
                .Set(ChainXCrmContactEntity.Columns.id_mailbox, newMailboxId)
                .Where(ChainXCrmContactEntity.Columns.id_chain, chainId)
                .Where(ChainXCrmContactEntity.Columns.id_mailbox, oldMailboxId)
                .Where(ChainXCrmContactEntity.Columns.id_tenant, tenant);

            db.ExecuteNonQuery(updateOldChainIdQuery);

        }

        public void LinkChainToCrm(int messageId, int tenant, string user, List<CrmContactEntity> contactIds)
        {
            var factory = new DaoFactory(CoreContext.TenantManager.GetCurrentTenant().TenantId, CRMConstants.DatabaseId);
            foreach (var crmContactEntity in contactIds)
            {
                switch (crmContactEntity.Type)
                {
                    case ChainXCrmContactEntity.EntityTypes.Contact:
                        var crmContact = factory.GetContactDao().GetByID(crmContactEntity.Id);
                        CRMSecurity.DemandAccessTo(crmContact);
                        break;
                    case ChainXCrmContactEntity.EntityTypes.Case:
                        var crmCase = factory.GetCasesDao().GetByID(crmContactEntity.Id);
                        CRMSecurity.DemandAccessTo(crmCase);
                        break;
                    case ChainXCrmContactEntity.EntityTypes.Opportunity:
                        var crmOpportunity = factory.GetDealDao().GetByID(crmContactEntity.Id);
                        CRMSecurity.DemandAccessTo(crmOpportunity);
                        break;
                }
            }
            
            using (var db = GetDb())
            {
                var chainInfo = GetMessageChainInfo(db, tenant, user, messageId);
                MarkChainAsCrmLinked(db, chainInfo.id, chainInfo.mailbox, tenant, contactIds);
                AddChainMailsToCrmHistory(db, chainInfo, tenant, user, contactIds);
            }
        }

        public void MarkChainAsCrmLinked(int messageId, int tenant, string user, List<CrmContactEntity> contactIds)
        {
            using (var db = GetDb())
            {
                var chainInfo = GetMessageChainInfo(db, tenant, user, messageId);
                MarkChainAsCrmLinked(db, chainInfo.id, chainInfo.mailbox, tenant, contactIds);
            }
        }

        private void AddChainMailsToCrmHistory(DbManager db, ChainInfo chainInfo, int tenant, string user,
            List<CrmContactEntity> contactIds)
        {
            var searchFolders = new List<int> {MailFolder.Ids.inbox, MailFolder.Ids.sent};
            var selectChainedMails = GetQueryForChainMessagesSelection(chainInfo.mailbox, chainInfo.id, searchFolders);
            var crmDal = new CrmHistoryDal(tenant, user);
            var linkingMessages = db.ExecuteList(selectChainedMails)
                .ConvertAll(record =>
                {
                    var item = GetMailInfo(db, tenant, user, Convert.ToInt32(record[0]), true, true);
                    item.LinkedCrmEntityIds = contactIds;
                    return item;
                });

            foreach (var message in linkingMessages)
            {
                try
                {
                    crmDal.AddRelationshipEvents(message);
                }
                catch (ApiHelperException ex)
                {
                    if (!ex.Message.Equals("Already exists"))
                        throw;
                }
            }
        }

        public void UnmarkChainAsCrmLinked(int messageId, int tenant, string user, IEnumerable<CrmContactEntity> contactIds)
        {
            using (var db = GetDb())
            {
                var chainInfo = GetMessageChainInfo(db, tenant, user, messageId);
                UnmarkChainAsCrmLinked(db, chainInfo.id, chainInfo.mailbox, tenant, contactIds);
            }
        }
        #endregion

        #region private conversations methods
        private List<MailMessage> GetFilteredChains(IDbManager db, int tenant, string user, MailFilter filter, DateTime? utcChainFromDate, int fromMessageId, bool? prevFlag, out bool hasMore)
        {
            var res = new List<MailMessage>();
            var chainsToSkip = new List<ChainInfo>();
            var skipFlag = false;
            var chunkIndex = 0;

            var sortOrder = filter.SortOrder == "ascending";

            if (prevFlag.GetValueOrDefault(false))
                sortOrder = !sortOrder;

            const string mm_alias = "ch";
            const string mtm_alias = "tm";

            var queryMessages = new SqlQuery(MailTable.name.Alias(mm_alias))
                .Select(
                    MailTable.Columns.id.Prefix(mm_alias),
                    MailTable.Columns.from.Prefix(mm_alias),
                    MailTable.Columns.to.Prefix(mm_alias),
                    MailTable.Columns.reply.Prefix(mm_alias),
                    MailTable.Columns.subject.Prefix(mm_alias),
                    MailTable.Columns.importance.Prefix(mm_alias),
                    MailTable.Columns.date_sent.Prefix(mm_alias),
                    MailTable.Columns.size.Prefix(mm_alias),
                    MailTable.Columns.attach_count.Prefix(mm_alias),
                    MailTable.Columns.unread.Prefix(mm_alias),
                    MailTable.Columns.is_answered.Prefix(mm_alias),
                    MailTable.Columns.is_forwarded.Prefix(mm_alias),
                    MailTable.Columns.is_from_crm.Prefix(mm_alias),
                    MailTable.Columns.is_from_tl.Prefix(mm_alias),
                    MailTable.Columns.folder_restore.Prefix(mm_alias),
                    MailTable.Columns.folder.Prefix(mm_alias),
                    MailTable.Columns.chain_id.Prefix(mm_alias),
                    MailTable.Columns.id_mailbox.Prefix(mm_alias),
                    MailTable.Columns.chain_date.Prefix(mm_alias));

            if (filter.CustomLabels != null && filter.CustomLabels.Count > 0)
            {
                queryMessages = queryMessages
                    .InnerJoin(TagMailTable.name + " " + mtm_alias,
                                Exp.EqColumns(MailTable.Columns.id.Prefix(mm_alias), TagMailTable.Columns.id_mail.Prefix(mtm_alias)))
                    .Where(Exp.In(TagMailTable.Columns.id_tag.Prefix(mtm_alias), filter.CustomLabels));
            }

            queryMessages = queryMessages
                .ApplyFilter(filter, mm_alias)
                .Where(TagMailTable.Columns.id_tenant.Prefix(mm_alias), tenant)
                .Where(TagMailTable.Columns.id_user.Prefix(mm_alias), user)
                .Where(MailTable.Columns.is_removed.Prefix(mm_alias), false);

            if (filter.CustomLabels != null && filter.CustomLabels.Count > 0)
            {
                queryMessages = queryMessages
                    .GroupBy(1)
                    .Having(Exp.Eq(string.Format("count({0})", MailTable.Columns.id.Prefix(mm_alias)), filter.CustomLabels.Count()));
            }

            queryMessages = queryMessages.OrderBy(MailTable.Columns.chain_date, sortOrder);

            if (null != utcChainFromDate)
            {
                queryMessages = queryMessages.Where(sortOrder ?
                    Exp.Ge(MailTable.Columns.chain_date, utcChainFromDate) :
                    Exp.Le(MailTable.Columns.chain_date, utcChainFromDate));
                skipFlag = true;
            }

            // We are increasing the size of the page to check whether it is necessary to show the Next button.
            while (res.Count < filter.PageSize + 1)
            {
                queryMessages.SetFirstResult(chunkIndex * CHUNK_SIZE * filter.PageSize).SetMaxResults(CHUNK_SIZE * filter.PageSize);
                chunkIndex++;

                var tenantInfo = CoreContext.TenantManager.GetTenant(tenant);
                var list = db
                    .ExecuteList(queryMessages)
                    .ConvertAll(r =>
                        ConvertToConversation(r, tenantInfo));

                if (0 == list.Count)
                    break;

                foreach (var item in list)
                {
                    var chainInfo = new ChainInfo {id = item.ChainId, mailbox = item.MailboxId};
                    
                    // Skip chains that was stored before and within from_message's chain.
                    if (skipFlag)
                    {
                        if (item.ChainDate != TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, utcChainFromDate.GetValueOrDefault()))
                            skipFlag = false;
                        else
                        {
                            if (item.Id == fromMessageId)
                                skipFlag = false;
                            chainsToSkip.Add(chainInfo);
                            continue;
                        }
                    }

                    if (chainsToSkip.Contains(chainInfo))
                        continue;

                    var alreadyContains = false;
                    foreach(var chain in res){
                        if(chain.ChainId == item.ChainId && chain.MailboxId == item.MailboxId){
                            alreadyContains = true;
                            if(chain.Date < item.Date)
                                res[res.IndexOf(chain)] = item;
                            break;
                        }
                    }

                    if(!alreadyContains)
                        res.Add(item);

                    if (filter.PageSize + 1 == res.Count)
                        break;
                }

                var isAllNeededConversationGathered = filter.PageSize + 1 == res.Count;
                if (isAllNeededConversationGathered)
                    break;

                var isEnoughMessagesForPage = CHUNK_SIZE*filter.PageSize <= list.Count;
                if (!isEnoughMessagesForPage)
                    break;
            }

            hasMore = res.Count > filter.PageSize;

            if (hasMore)
                res.RemoveAt(filter.PageSize);

            return res;
        }

        private static MailMessage ConvertToConversation(object[] r, Tenant tenantInfo)
        {
            var now = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, DateTime.UtcNow);
            var date = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, (DateTime)r[6]);
            var chainDate = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, (DateTime)r[18]);

            var isToday = (now.Year == date.Year && now.Date == date.Date);
            var isYesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            return new MailMessage
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
                IsToday = isToday,
                IsYesterday = isYesterday,
                MailboxId = Convert.ToInt32(r[17]),
                LabelsString = "",
                ChainDate = chainDate,
                ChainLength = 1
            };
        }

        private static Exp GetSearchFolders(string folderName, int folder)
        {
            Exp condition;

            if (folder == MailFolder.Ids.inbox || folder == MailFolder.Ids.sent)
                condition = Exp.In(folderName, new[] { MailFolder.Ids.inbox, MailFolder.Ids.sent });
            else
                condition = Exp.Eq(folderName, folder);

            return condition;
        }

        private static List<object[]> GetChainedMessagesInfo(IDbManager db, int tenant, string user, List<int> ids,
                                                            IEnumerable<string> fields)
        {
            var selectedChainsQuery = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.chain_id,
                        MailTable.Columns.id_mailbox,
                        MailTable.Columns.folder)
                .Where(Exp.In(MailTable.Columns.id, ids))
                .Where(MailTable.Columns.id_tenant, tenant)
                .Where(MailTable.Columns.id_user, user);

            var chainsInfo = db.ExecuteList(selectedChainsQuery)
                                .ConvertAll(r => new
                                    {
                                        id_chain = Convert.ToString(r[0]),
                                        id_mailbox = Convert.ToInt32(r[1]),
                                        folder = Convert.ToInt32(r[2])
                                    })
                                .ToList();

            var extendedFields = new List<string>
                {
                    MailTable.Columns.chain_id,
                    MailTable.Columns.id_mailbox,
                    MailTable.Columns.folder
                };

            extendedFields.AddRange(fields);

            var chainArray = chainsInfo.Select(r => (object) r.id_chain).Distinct().ToArray();

            const int max_query_count = 25;
            var i = 0;
            var unsortedMessages = new List<object[]>();

            do
            {
                var partChains = chainArray.Skip(i).Take(max_query_count).ToArray();

                if (partChains.Length == 0)
                    break;

                var selectedMessagesQuery = new SqlQuery(MailTable.name)
                    .Select(extendedFields.ToArray())
                    .Where(Exp.In(MailTable.Columns.chain_id, partChains))
                    .Where(MailTable.Columns.id_tenant, tenant)
                    .Where(MailTable.Columns.id_user, user)
                    .Where(MailTable.Columns.is_removed, false);

                var selectedMessages = db.ExecuteList(selectedMessagesQuery);

                unsortedMessages.AddRange(selectedMessages);

                i += max_query_count;

            } while (true);

            var result =
                unsortedMessages.Where(r =>
                    {
                        var curIdChain = Convert.ToString(r[0]);
                        var curIdMailbox = Convert.ToInt32(r[1]);
                        var curFolder = Convert.ToInt32(r[2]);

                        return chainsInfo.Exists(c =>
                                                  c.id_chain == curIdChain &&
                                                  c.id_mailbox == curIdMailbox &&
                                                  (curFolder == 1 || curFolder == 2)
                                                      ? c.folder == 1 || c.folder == 2
                                                      : c.folder == curFolder);
                    })
                                 .Select(r => r.Skip(3).ToArray())
                                 .ToList();

            return result;
        }

        // Method for updating chain flags, date and length.
        private void UpdateChain(IDbManager db, string chainId, int folder, int mailboxId, int tenant, string user)
        {
            if (string.IsNullOrEmpty(chainId)) return;

            var chain = db.ExecuteList(
                new SqlQuery(MailTable.name)
                    .SelectCount()
                    .SelectMax(MailTable.Columns.date_sent)
                    .SelectMax(MailTable.Columns.unread)
                    .SelectMax(MailTable.Columns.attach_count)
                    .SelectMax(MailTable.Columns.importance)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.is_removed, 0)
                    .Where(MailTable.Columns.chain_id, chainId)
                    .Where(MailTable.Columns.id_mailbox, mailboxId)
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

            var storedChainInfo = db.ExecuteList(
                new SqlQuery(ChainTable.name)
                    .Select(ChainTable.Columns.unread)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.id, chainId)
                    .Where(ChainTable.Columns.folder, folder)
                    .Where(MailTable.Columns.id_mailbox, mailboxId));

            var chainUnreadFlag = storedChainInfo.Any() && Convert.ToBoolean(storedChainInfo.First()[0]); 

            if (0 == chain.length)
            {
                db.ExecuteNonQuery(
                    new SqlDelete(ChainTable.name)
                        .Where(GetUserWhere(user, tenant))
                        .Where(ChainTable.Columns.id, chainId)
                        .Where(ChainTable.Columns.id_mailbox, mailboxId)
                        .Where(ChainTable.Columns.folder, folder));

                _log.Debug("UpdateChain() row deleted from chain table tenant='{0}', user_id='{1}', id_mailbox='{2}', folder='{3}', chain_id='{4}'",
                    tenant, user, mailboxId, folder, chainId);

                ChangeFolderCounters(db, tenant, user, folder, 0, 0, chainUnreadFlag ? -1 : 0 , -1);
            }
            else
            {
                db.ExecuteNonQuery(
                    new SqlUpdate(MailTable.name)
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailTable.Columns.is_removed, 0)
                        .Where(MailTable.Columns.chain_id, chainId)
                        .Where(MailTable.Columns.id_mailbox, mailboxId)
                        .Where(MailTable.Columns.folder, folder) // Folder condition important because chain has different dates in different folders(Ex: Sent and Inbox).
                        .Set(MailTable.Columns.chain_date, chain.date));

                var tags = GetChainTags(db, chainId, folder, mailboxId, tenant, user);

                var result = db.ExecuteNonQuery(
                    new SqlInsert(ChainTable.name, true)
                        .InColumnValue(ChainTable.Columns.id, chainId)
                        .InColumnValue(ChainTable.Columns.id_mailbox, mailboxId)
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
                    tenant, user, mailboxId, folder, chainId);

                var unreadConvDiff = 0;
                var totalConvDiff = 0;

                if (!storedChainInfo.Any())
                {
                    totalConvDiff = 1;
                    unreadConvDiff = chain.unread ? 1 : 0;
                }
                else
                {
                    if (chainUnreadFlag != chain.unread)
                        unreadConvDiff = chain.unread ? 1 : -1;
                }

                ChangeFolderCounters(db, tenant, user, folder, 0, 0, unreadConvDiff, totalConvDiff);
            }
        }

        private void UpdateChainTags(DbManager db, string chainId, int folder, int mailboxId, int tenant, string user)
        {
            var tags = GetChainTags(db, chainId, folder, mailboxId, tenant, user);

            db.ExecuteNonQuery(
                new SqlUpdate(ChainTable.name)
                    .Set(ChainTable.Columns.tags, tags)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.id, chainId)
                    .Where(ChainTable.Columns.id_mailbox, mailboxId)
                    .Where(ChainTable.Columns.folder, folder));
        }

        private void UpdateMessageChainAttachmentsFlag(IDbManager db, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(db, tenant, user, messageId, MailTable.Columns.attach_count, ChainTable.Columns.has_attachments);
        }

        private void UpdateMessageChainUnreadFlag(IDbManager db, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(db, tenant, user, messageId, MailTable.Columns.unread, ChainTable.Columns.unread);
        }

        private void UpdateMessageChainImportanceFlag(IDbManager db, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(db, tenant, user, messageId, MailTable.Columns.importance, ChainTable.Columns.importance);
        }

        private void UpdateMessageChainFlag(IDbManager db, int tenant, string user, int messageId, string fieldFrom, string fieldTo)
        {
            var chain = GetMessageChainInfo(db, tenant, user, messageId);

            if (string.IsNullOrEmpty(chain.id) || chain.folder < 1 || chain.mailbox < 1) return;

            var fieldQuery = new SqlQuery(MailTable.name)
                .SelectMax(fieldFrom)
                .Where(MailTable.Columns.is_removed, 0)
                .Where(MailTable.Columns.chain_id, chain.id)
                .Where(MailTable.Columns.id_mailbox, chain.mailbox)
                .Where(GetUserWhere(user, tenant))
                .Where(GetSearchFolders(MailTable.Columns.folder, chain.folder));

            var fieldVal = db.ExecuteScalar<bool>(fieldQuery);

            var updateQuery = new SqlUpdate(ChainTable.name)
                    .Set(fieldTo, fieldVal)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.id, chain.id)
                    .Where(ChainTable.Columns.id_mailbox, chain.mailbox)
                    .Where(GetSearchFolders(ChainTable.Columns.folder, chain.folder));

            db.ExecuteNonQuery(updateQuery);
        }

        private ChainInfo GetMessageChainInfo(IDbManager db, int tenant, string user, int messageId)
        {
            var info = db.ExecuteList(new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.chain_id, MailTable.Columns.folder, MailTable.Columns.id_mailbox)
                .Where(MailTable.Columns.id, messageId)
                .Where(GetUserWhere(user, tenant)));

            return new ChainInfo
            {
                id = info[0][0] == null ? "" : info[0][0].ToString(),
                folder = Convert.ToInt32(info[0][1]),
                mailbox = Convert.ToInt32(info[0][2])
            };
        }

        private string GetChainTags(IDbManager db, string chainId, int folder, int mailboxId, int tenant, string user)
        {
            const string mm_alias = "ch";
            const string mtm_alias = "tm";

            var newQuery = new SqlQuery(TagMailTable.name.Alias(mtm_alias))
                .Select(TagMailTable.Columns.id_tag.Prefix(mtm_alias))
                .InnerJoin(MailTable.name + " " + mm_alias,
                           Exp.EqColumns(MailTable.Columns.id.Prefix(mm_alias), TagMailTable.Columns.id_mail.Prefix(mtm_alias)))
                .Where(MailTable.Columns.chain_id.Prefix(mm_alias), chainId)
                .Where(MailTable.Columns.is_removed.Prefix(mm_alias), 0)
                .Where(TagMailTable.Columns.id_tenant.Prefix(mtm_alias), tenant)
                .Where(TagMailTable.Columns.id_user.Prefix(mtm_alias), user)
                .Where(MailTable.Columns.folder.Prefix(mm_alias), folder)
                .Where(MailTable.Columns.id_mailbox.Prefix(mm_alias), mailboxId)
                .GroupBy(1)
                .OrderBy(TagMailTable.Columns.time_created.Prefix(mtm_alias), true);

            var tags = db.ExecuteList(newQuery)
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
