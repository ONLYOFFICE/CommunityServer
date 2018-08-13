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
using ASC.FullTextIndex;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.DbSchema;
using ASC.Mail.Aggregator.Extension;
using ASC.Mail.Aggregator.Filter;
using ASC.Web.CRM.Core;
using Autofac;
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
                var res = GetFilteredConversations(
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
                        res = GetFilteredConversations(db, tenant, user, filter, null, 0, false, out hasMore);
                        hasMore = false;

                        if (!res.Any())
                            return res;
                    }
                } else if(prevFlag.GetValueOrDefault(false))
                    res.Reverse();

                var chainIds = new List<string>();
                res.ForEach(x => chainIds.Add(x.ChainId));

                var query = new SqlQuery(ChainTable.Name)
                        .Select(
                            ChainTable.Columns.Id,
                            ChainTable.Columns.MailboxId,
                            ChainTable.Columns.Length,
                            ChainTable.Columns.Unread,
                            ChainTable.Columns.Importance,
                            ChainTable.Columns.HasAttachments,
                            ChainTable.Columns.Tags)
                        .Where(GetUserWhere(user, tenant))
                        .Where(Exp.In(ChainTable.Columns.Id, chainIds))
                        .Where(GetSearchFolders(ChainTable.Columns.Folder, filter.PrimaryFolder));

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
                var chainDate = db.ExecuteScalar<DateTime>(new SqlQuery(MailTable.Name)
                    .Select(MailTable.Columns.ChainDate)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.Id, id));

                filter.PageSize = 1;
                bool hasMore;
                var messages = GetFilteredConversations(db, tenant, user, filter, chainDate, id, false, out hasMore);
                return messages.Any() ? messages.First().Id : 0;
            }
        }

        public void SendConversationsToSpamTrainer(int tenant, string user, List<int> ids, bool isSpam, string httpContextScheme)
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
                    SendEmlUrlsToSpamTrainer(tenant, user, tlMails, isSpam, httpContextScheme);
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
                var tlMailboxesIdsQuery = new SqlQuery(MailboxTable.Name)
                    .Select(MailboxTable.Columns.Id)
                    .Where(MailboxTable.Columns.Tenant, tenant)
                    .Where(MailboxTable.Columns.User, user.ToLowerInvariant())
                    .Where(MailboxTable.Columns.IsTeamlabMailbox, true)
                    .Where(MailboxTable.Columns.IsRemoved, false);

                var tlMailboxesIds = db.ExecuteList(tlMailboxesIdsQuery)
                                         .ConvertAll(r => Convert.ToInt32(r[0]));

                if (!tlMailboxesIds.Any())
                    return streamList;

                streamList = GetChainedMessagesInfo(db, tenant, user, ids,
                                                     new[]
                                                         {
                                                             MailTable.Columns.Id,
                                                             MailTable.Columns.FolderRestore,
                                                             MailTable.Columns.MailboxId,
                                                             MailTable.Columns.Stream
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
                                              bool isSpam, string httpContextScheme)
        {
            if (!tlMails.Any())
                return;

            using (var db = GetDb())
            {
                var serverInformationQuery = new SqlQuery(ServerTable.Name)
                    .InnerJoin(TenantXServerTable.Name,
                               Exp.EqColumns(TenantXServerTable.Columns.ServerId, ServerTable.Columns.Id))
                    .Select(ServerTable.Columns.ConnectionString)
                    .Where(TenantXServerTable.Columns.Tenant, tenant);

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

                var apiHelper = new ApiHelper(httpContextScheme);

                foreach (var tlSpamMail in tlMails)
                {
                    try
                    {
                        var emlUrl = GetMailEmlUrl(tenant, user, tlSpamMail.Value);

                        apiHelper.SendEmlToSpamTrainer(serverInfo.server_ip, serverInfo.protocol, serverInfo.port,
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
                                                                  MailTable.Columns.Id, MailTable.Columns.Unread, MailTable.Columns.Folder,
                                                                  MailTable.Columns.ChainId, MailTable.Columns.MailboxId
                                                              });

                if (!listObjects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesFolder(db, tenant, user, listObjects, folder);
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
                                                                MailTable.Columns.Id, MailTable.Columns.Unread,
                                                                MailTable.Columns.Folder, MailTable.Columns.FolderRestore,
                                                                MailTable.Columns.ChainId, MailTable.Columns.MailboxId
                                                            });
                if (!listObjects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    RestoreMessages(db, tenant, user, listObjects);
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
                                                                  MailTable.Columns.Id,
                                                                  MailTable.Columns.Folder,
                                                                  MailTable.Columns.Unread
                                                              });

                if (!listObjects.Any())
                    return;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    usedQuota = DeleteMessages(db, tenant, user, listObjects);
                    tx.Commit();
                }
            }

            QuotaUsedDelete(tenant, usedQuota);
        }

        public void SetConversationsReadFlags(int tenant, string user, List<int> ids, bool isRead)
        {
            if (!ids.Any() || tenant < 0 || string.IsNullOrEmpty(user))
                return;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    SetMessagesReadFlags(db, tenant, user, ids, isRead, true);
                    tx.Commit();
                }
            }
        }

        public void SetConversationsImportanceFlags(int tenant, string user, bool important, List<int> ids)
        {
            var chainsInfoQuery = new SqlQuery(MailTable.Name)
                .Select(MailTable.Columns.ChainId)
                .Select(MailTable.Columns.Folder)
                .Select(MailTable.Columns.MailboxId)
                .Where(GetUserWhere(user, tenant))
                .Where(Exp.In(MailTable.Columns.Id, ids.Select(x => (object)x).ToArray()));

            var updateMailQuery = new SqlUpdate(MailTable.Name)
                .Set(MailTable.Columns.Importance, important)
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
                        var innerWhere = Exp.Eq(MailTable.Columns.ChainId, chain.id) & Exp.Eq(MailTable.Columns.MailboxId, chain.mailbox);

                        if (chain.folder == MailFolder.Ids.inbox || chain.folder == MailFolder.Ids.sent)
                        {
                            innerWhere &= (Exp.Eq(MailTable.Columns.Folder, MailFolder.Ids.inbox) | Exp.Eq(MailTable.Columns.Folder, MailFolder.Ids.sent));
                        }
                        else
                        {
                            innerWhere &= Exp.Eq(MailTable.Columns.Folder, chain.folder);
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
            var addNewLinkQuery = new SqlInsert(ChainXCrmContactEntity.Name)
                                        .InColumns(ChainXCrmContactEntity.Columns.ChainId,
                                        ChainXCrmContactEntity.Columns.MailboxId,
                                        ChainXCrmContactEntity.Columns.Tenant,
                                        ChainXCrmContactEntity.Columns.EntityId,
                                        ChainXCrmContactEntity.Columns.EntityType);
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
                var removeLinkQuery = new SqlDelete(ChainXCrmContactEntity.Name)
                    .Where(ChainXCrmContactEntity.Columns.ChainId, chainId)
                    .Where(ChainXCrmContactEntity.Columns.MailboxId, mailboxId)
                    .Where(ChainXCrmContactEntity.Columns.Tenant, tenant)
                    .Where(ChainXCrmContactEntity.Columns.EntityId, crmContactEntity.Id)
                    .Where(ChainXCrmContactEntity.Columns.EntityType, crmContactEntity.Type);
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

            var updateOldChainIdQuery = new SqlUpdate(ChainXCrmContactEntity.Name)
                .Set(ChainXCrmContactEntity.Columns.MailboxId, newMailboxId)
                .Where(ChainXCrmContactEntity.Columns.ChainId, chainId)
                .Where(ChainXCrmContactEntity.Columns.MailboxId, oldMailboxId)
                .Where(ChainXCrmContactEntity.Columns.Tenant, tenant);

            db.ExecuteNonQuery(updateOldChainIdQuery);

        }

        public void LinkChainToCrm(int messageId, int tenant, string user, List<CrmContactEntity> contactIds, string httpContextScheme)
        {
            using (var scope = DIHelper.Resolve())
            {
                var factory = scope.Resolve<DaoFactory>();
                foreach (var crmContactEntity in contactIds)
                {
                    switch (crmContactEntity.Type)
                    {
                        case CrmContactEntity.EntityTypes.Contact:
                            var crmContact = factory.ContactDao.GetByID(crmContactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmContact);
                            break;
                        case CrmContactEntity.EntityTypes.Case:
                            var crmCase = factory.CasesDao.GetByID(crmContactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmCase);
                            break;
                        case CrmContactEntity.EntityTypes.Opportunity:
                            var crmOpportunity = factory.DealDao.GetByID(crmContactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmOpportunity);
                            break;
                    }
                }
            }

            using (var db = GetDb())
            {
                var chainInfo = GetMessageChainInfo(db, tenant, user, messageId);
                MarkChainAsCrmLinked(db, chainInfo.id, chainInfo.mailbox, tenant, contactIds);
                AddChainMailsToCrmHistory(db, chainInfo, tenant, user, contactIds, httpContextScheme);
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
            List<CrmContactEntity> contactIds, string httpContextScheme)
        {
            var searchFolders = new List<int> {MailFolder.Ids.inbox, MailFolder.Ids.sent};
            var selectChainedMails = GetQueryForChainMessagesSelection(chainInfo.mailbox, chainInfo.id, searchFolders);
           
            var linkingMessages = db.ExecuteList(selectChainedMails)
                .ConvertAll(record =>
                {
                    var item = GetMailInfo(db, tenant, user, Convert.ToInt32(record[0]), new MailMessage.Options
                    {
                        LoadImages = true,
                        LoadBody = true,
                        NeedProxyHttp = false
                    });
                    item.LinkedCrmEntityIds = contactIds;
                    return item;
                });

            var crmDal = new CrmHistoryDal(tenant, user, httpContextScheme);
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

        private List<MailMessage> GetFilteredConversations(IDbManager db, int tenant, string user, MailFilter filter,
            DateTime? utcChainFromDate, int fromMessageId, bool? prevFlag, out bool hasMore)
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

            var queryMessages = new SqlQuery(MailTable.Name.Alias(mm_alias))
                .Select(GetMailMessagesColumns(mm_alias));

            if (filter.CustomLabels != null && filter.CustomLabels.Count > 0)
            {
                queryMessages = queryMessages
                    .InnerJoin(TagMailTable.Name.Alias(mtm_alias),
                        Exp.EqColumns(MailTable.Columns.Id.Prefix(mm_alias),
                            TagMailTable.Columns.MailId.Prefix(mtm_alias)))
                    .Where(Exp.In(TagMailTable.Columns.TagId.Prefix(mtm_alias), filter.CustomLabels));
            }

            queryMessages = queryMessages
                .ApplyFilter(filter, mm_alias);

            if (queryMessages == null)
            {
                hasMore = false;
                return res;
            }

            queryMessages
                .Where(TagMailTable.Columns.Tenant.Prefix(mm_alias), tenant)
                .Where(TagMailTable.Columns.User.Prefix(mm_alias), user)
                .Where(MailTable.Columns.IsRemoved.Prefix(mm_alias), false);

            if (filter.CustomLabels != null && filter.CustomLabels.Count > 0)
            {
                queryMessages = queryMessages
                    .GroupBy(1)
                    .Having(Exp.Eq(string.Format("count({0})", MailTable.Columns.Id.Prefix(mm_alias)),
                        filter.CustomLabels.Count()));
            }

            queryMessages = queryMessages.OrderBy(MailTable.Columns.ChainDate.Prefix(mm_alias), sortOrder);

            if (null != utcChainFromDate)
            {
                queryMessages = queryMessages.Where(sortOrder
                    ? Exp.Ge(MailTable.Columns.ChainDate.Prefix(mm_alias), utcChainFromDate)
                    : Exp.Le(MailTable.Columns.ChainDate.Prefix(mm_alias), utcChainFromDate));
                skipFlag = true;
            }

            List<int> ids;
            if (TryGetFullTextSearchIds(filter, out ids))
            {
                if (!ids.Any())
                {
                    hasMore = false;
                    return res;
                }
            }

            var tenantInfo = CoreContext.TenantManager.GetTenant(tenant);
            var utcNow = DateTime.UtcNow;
            var pageSize = filter.PageSize;

            // We are increasing the size of the page to check whether it is necessary to show the Next button.
            while (res.Count < pageSize + 1)
            {
                if (ids.Any())
                {
                    var partIds = ids.Skip(chunkIndex * CHUNK_SIZE * pageSize).Take(CHUNK_SIZE * pageSize).ToList();

                    if (!partIds.Any())
                        break;

                    queryMessages.Where(Exp.In(MailTable.Columns.Id.Prefix(mm_alias), partIds));
                }
                else
                {
                    queryMessages
                        .SetFirstResult(chunkIndex*CHUNK_SIZE*pageSize)
                        .SetMaxResults(CHUNK_SIZE*pageSize);
                }

                chunkIndex++;
                
                var listMessages = db
                    .ExecuteList(queryMessages)
                    .ConvertAll(r =>
                        ConvertToMailMessage(r, tenantInfo, utcNow));

                if (0 == listMessages.Count)
                    break;

                foreach (var item in listMessages)
                {
                    var chainInfo = new ChainInfo {id = item.ChainId, mailbox = item.MailboxId};

                    // Skip chains that was stored before and within from_message's chain.
                    if (skipFlag)
                    {
                        if (item.ChainDate !=
                            TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, utcChainFromDate.GetValueOrDefault()))
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
                    foreach (var chain in res)
                    {
                        if (chain.ChainId == item.ChainId && chain.MailboxId == item.MailboxId)
                        {
                            alreadyContains = true;
                            if (chain.Date < item.Date)
                                res[res.IndexOf(chain)] = item;
                            break;
                        }
                    }

                    if (!alreadyContains)
                        res.Add(item);

                    if (pageSize + 1 == res.Count)
                        break;
                }

                if (pageSize + 1 == res.Count)
                    break;
            }

            hasMore = res.Count > pageSize;

            if (hasMore)
                res.RemoveAt(pageSize);

            return res;
        }

        private static bool TryGetFullTextSearchIds(MailFilter filter, out List<int> ids)
        {
            ids = new List<int>();
            var actionSucceed = false;

            if (!string.IsNullOrEmpty(filter.SearchText) && FullTextSearch.SupportModule(FullTextSearch.MailModule))
            {
                var mailModule =
                    FullTextSearch.MailModule.Match(filter.SearchText)
                        .OrderBy(MailTable.Columns.DateSent, filter.SortOrder == "ascending");

                if (filter.PrimaryFolder != 1 && filter.PrimaryFolder != 2)
                {
                    mailModule.AddAttribute("folder", filter.PrimaryFolder);
                }
                else
                {
                    mailModule.AddAttribute("folder", new[] {1, 2});
                }

                ids = FullTextSearch.Search(mailModule);

                actionSucceed = true;
            }

            if (!string.IsNullOrEmpty(filter.FindAddress) && FullTextSearch.SupportModule(FullTextSearch.MailModule))
            {
                var tmpIds =
                    FullTextSearch.Search(FullTextSearch.MailModule.Match(filter.FindAddress,
                        filter.PrimaryFolder == MailFolder.Ids.sent || filter.PrimaryFolder == MailFolder.Ids.drafts
                            ? MailTable.Columns.To
                            : MailTable.Columns.From));

                actionSucceed = true;

                if (tmpIds.Any())
                {
                    ids = ids.Any() ? ids.Where(id => tmpIds.Exists(tid => tid == id)).ToList() : tmpIds;
                }
            }

            return actionSucceed;
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
            var selectedChainsQuery = new SqlQuery(MailTable.Name)
                .Select(MailTable.Columns.ChainId,
                    MailTable.Columns.MailboxId,
                    MailTable.Columns.Folder)
                .Where(Exp.In(MailTable.Columns.Id, ids))
                .Where(MailTable.Columns.Tenant, tenant)
                .Where(MailTable.Columns.User, user);

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
                MailTable.Columns.ChainId,
                MailTable.Columns.MailboxId,
                MailTable.Columns.Folder
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

                var selectedMessagesQuery = new SqlQuery(MailTable.Name)
                    .Select(extendedFields.ToArray())
                    .Where(Exp.In(MailTable.Columns.ChainId, partChains))
                    .Where(MailTable.Columns.Tenant, tenant)
                    .Where(MailTable.Columns.User, user)
                    .Where(MailTable.Columns.IsRemoved, false);

                var selectedMessages = db.ExecuteList(selectedMessagesQuery);

                unsortedMessages.AddRange(selectedMessages);

                i += max_query_count;

            } while (true);

            var result = unsortedMessages
                .Select(r => new
                {
                    id_chain = Convert.ToString(r[0]),
                    id_mailbox = Convert.ToInt32(r[1]),
                    folder = Convert.ToInt32(r[2]),
                    result = r.Skip(3).ToArray()
                })
                .Where(r => chainsInfo.FirstOrDefault(c =>
                    c.id_chain == r.id_chain &&
                    c.id_mailbox == r.id_mailbox &&
                    ((r.folder == 1 || r.folder == 2)
                        ? c.folder == 1 || c.folder == 2
                        : c.folder == r.folder)) != null)
                .Select(r => r.result)
                .ToList();

            return result;
        }

        // Method for updating chain flags, date and length.
        private void UpdateChain(IDbManager db, string chainId, int folder, int mailboxId, int tenant, string user)
        {
            if (string.IsNullOrEmpty(chainId)) return;

            var getChainQuery = new SqlQuery(MailTable.Name)
                .SelectCount()
                .SelectMax(MailTable.Columns.DateSent)
                .SelectMax(MailTable.Columns.Unread)
                .SelectMax(MailTable.Columns.AttachCount)
                .SelectMax(MailTable.Columns.Importance)
                .Where(GetUserWhere(user, tenant))
                .Where(MailTable.Columns.IsRemoved, 0)
                .Where(MailTable.Columns.ChainId, chainId)
                .Where(MailTable.Columns.MailboxId, mailboxId)
                .Where(MailTable.Columns.Folder, folder);

            var chain = db.ExecuteList(getChainQuery)
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

            var getStoredChainInfoQuery = new SqlQuery(ChainTable.Name)
                .Select(ChainTable.Columns.Unread)
                .Where(GetUserWhere(user, tenant))
                .Where(ChainTable.Columns.Id, chainId)
                .Where(ChainTable.Columns.Folder, folder)
                .Where(MailTable.Columns.MailboxId, mailboxId);

            var storedChainInfo = db.ExecuteList(getStoredChainInfoQuery);

            var chainUnreadFlag = storedChainInfo.Any() && Convert.ToBoolean(storedChainInfo.First()[0]); 

            if (0 == chain.length)
            {
                var deleteFromChainQuery = new SqlDelete(ChainTable.Name)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.Id, chainId)
                    .Where(ChainTable.Columns.MailboxId, mailboxId)
                    .Where(ChainTable.Columns.Folder, folder);

                var result = db.ExecuteNonQuery(deleteFromChainQuery);

                _log.Debug("UpdateChain() row deleted from chain table tenant='{0}', user_id='{1}', id_mailbox='{2}', folder='{3}', chain_id='{4}'",
                    tenant, user, mailboxId, folder, chainId);

                ChangeFolderCounters(db, tenant, user, folder, 0, 0, chainUnreadFlag ? -1 : 0 , -1);
            }
            else
            {
                var updateMailQuery = new SqlUpdate(MailTable.Name)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailTable.Columns.IsRemoved, 0)
                    .Where(MailTable.Columns.ChainId, chainId)
                    .Where(MailTable.Columns.MailboxId, mailboxId)
                    .Where(MailTable.Columns.Folder, folder)
                    // Folder condition important because chain has different dates in different folders(Ex: Sent and Inbox).
                    .Set(MailTable.Columns.ChainDate, chain.date);

                db.ExecuteNonQuery(updateMailQuery);

                var tags = GetChainTags(db, chainId, folder, mailboxId, tenant, user);

                var insertChainQuery =
                    new SqlInsert(ChainTable.Name, true)
                        .InColumnValue(ChainTable.Columns.Id, chainId)
                        .InColumnValue(ChainTable.Columns.MailboxId, mailboxId)
                        .InColumnValue(ChainTable.Columns.Tenant, tenant)
                        .InColumnValue(ChainTable.Columns.User, user)
                        .InColumnValue(ChainTable.Columns.Folder, folder)
                        .InColumnValue(ChainTable.Columns.Length, chain.length)
                        .InColumnValue(ChainTable.Columns.Unread, chain.unread)
                        .InColumnValue(ChainTable.Columns.HasAttachments, chain.attach_count > 0)
                        .InColumnValue(ChainTable.Columns.Importance, chain.importance)
                        .InColumnValue(ChainTable.Columns.Tags, tags);

                var result = db.ExecuteNonQuery(insertChainQuery);

                if (result <= 0)
                    throw new InvalidOperationException(string.Format("Invalid insert to {0}", ChainTable.Name));

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

                ChangeFolderCounters(db, tenant, user, folder, unreadConvDiff: unreadConvDiff, totalConvDiff: totalConvDiff);
            }
        }

        private void UpdateChainTags(DbManager db, string chainId, int folder, int mailboxId, int tenant, string user)
        {
            var tags = GetChainTags(db, chainId, folder, mailboxId, tenant, user);

            db.ExecuteNonQuery(
                new SqlUpdate(ChainTable.Name)
                    .Set(ChainTable.Columns.Tags, tags)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.Id, chainId)
                    .Where(ChainTable.Columns.MailboxId, mailboxId)
                    .Where(ChainTable.Columns.Folder, folder));
        }

        private void UpdateMessageChainAttachmentsFlag(IDbManager db, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(db, tenant, user, messageId, MailTable.Columns.AttachCount, ChainTable.Columns.HasAttachments);
        }

        private void UpdateMessageChainUnreadFlag(IDbManager db, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(db, tenant, user, messageId, MailTable.Columns.Unread, ChainTable.Columns.Unread);
        }

        private void UpdateMessageChainImportanceFlag(IDbManager db, int tenant, string user, int messageId)
        {
            UpdateMessageChainFlag(db, tenant, user, messageId, MailTable.Columns.Importance, ChainTable.Columns.Importance);
        }

        private void UpdateMessageChainFlag(IDbManager db, int tenant, string user, int messageId, string fieldFrom, string fieldTo)
        {
            var chain = GetMessageChainInfo(db, tenant, user, messageId);

            if (string.IsNullOrEmpty(chain.id) || chain.folder < 0 || chain.mailbox < 1) return;

            var fieldQuery = new SqlQuery(MailTable.Name)
                .SelectMax(fieldFrom)
                .Where(MailTable.Columns.IsRemoved, 0)
                .Where(MailTable.Columns.ChainId, chain.id)
                .Where(MailTable.Columns.MailboxId, chain.mailbox)
                .Where(GetUserWhere(user, tenant))
                .Where(MailTable.Columns.Folder, chain.folder);

            var fieldVal = db.ExecuteScalar<bool>(fieldQuery);

            var updateQuery = new SqlUpdate(ChainTable.Name)
                    .Set(fieldTo, fieldVal)
                    .Where(GetUserWhere(user, tenant))
                    .Where(ChainTable.Columns.Id, chain.id)
                    .Where(ChainTable.Columns.MailboxId, chain.mailbox)
                    .Where(ChainTable.Columns.Folder, chain.folder);

            db.ExecuteNonQuery(updateQuery);
        }

        private ChainInfo GetMessageChainInfo(IDbManager db, int tenant, string user, int messageId)
        {
            var info = db.ExecuteList(new SqlQuery(MailTable.Name)
                .Select(MailTable.Columns.ChainId, MailTable.Columns.Folder, MailTable.Columns.MailboxId)
                .Where(MailTable.Columns.Id, messageId)
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

            var newQuery = new SqlQuery(TagMailTable.Name.Alias(mtm_alias))
                .Select(TagMailTable.Columns.TagId.Prefix(mtm_alias))
                .InnerJoin(MailTable.Name.Alias(mm_alias),
                           Exp.EqColumns(MailTable.Columns.Id.Prefix(mm_alias), TagMailTable.Columns.MailId.Prefix(mtm_alias)))
                .Where(MailTable.Columns.ChainId.Prefix(mm_alias), chainId)
                .Where(MailTable.Columns.IsRemoved.Prefix(mm_alias), 0)
                .Where(TagMailTable.Columns.Tenant.Prefix(mtm_alias), tenant)
                .Where(TagMailTable.Columns.User.Prefix(mtm_alias), user)
                .Where(MailTable.Columns.Folder.Prefix(mm_alias), folder)
                .Where(MailTable.Columns.MailboxId.Prefix(mm_alias), mailboxId)
                .GroupBy(1)
                .OrderBy(TagMailTable.Columns.TimeCreated.Prefix(mtm_alias), true);

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
