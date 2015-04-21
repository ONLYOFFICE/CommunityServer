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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Data;
using ASC.CRM.Core;
using ASC.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Web.Core;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region public methods

        public void SetMessagesTag(int tenant, string user, int tagId, IEnumerable<int> messagesIds)
        {
            var ids = messagesIds as IList<int> ?? messagesIds.ToList();

            if (!ids.Any()) return;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    List<int> validIds;
                    List<ChainInfo> chains;

                    GetValidForUserMessages(db, tenant, user, ids, out validIds, out chains);

                    SetMessagesTag(db, tenant, user, validIds, tagId);

                    foreach (var chain in chains)
                        UpdateChainTags(db, chain.id, chain.folder, chain.mailbox, tenant, user);

                    tx.Commit();
                }
            }
        }

        public void SetConversationsTag(int tenant, string user, int tagId, IEnumerable<int> messagesIds)
        {
            var ids = messagesIds as IList<int> ?? messagesIds.ToList();
            
            if (!ids.Any()) return;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    SetConversationsTag(db, tenant, user, tagId, ids);

                    tx.Commit();
                }
            }
        }

        public void UnsetConversationsTag(int tenant, string user, int tagId, IEnumerable<int> messagesIds)
        {
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                { 
                    var listObjects = GetChainedMessagesInfo(db, tenant, user, (List<int>) messagesIds,
                                                              new[]
                                                                  {
                                                                      MailTable.Columns.id, MailTable.Columns.chain_id,
                                                                      MailTable.Columns.folder,
                                                                      MailTable.Columns.id_mailbox
                                                                  });

                    var validIds = new List<int>();
                    var chains = new List<ChainInfo>();

                    listObjects.ForEach(
                        item =>
                            {
                                chains.Add(
                                    new ChainInfo
                                        {
                                            id = item[1].ToString(),
                                            folder = Convert.ToInt32(item[2]),
                                            mailbox = Convert.ToInt32(item[3])
                                        });

                                validIds.Add(Convert.ToInt32(item[0]));
                            });

                    UnsetMessagesTag(db, tenant, user, tagId, validIds, chains);
                    
                    tx.Commit();
                }
            }
        }

        public void UnsetMessagesTag(int tenant, string user, int tagId, IEnumerable<int> messagesIds)
        {
            using (var db = GetDb()) 
            {
                using (var tx = db.BeginTransaction())
                {
                    List<int> validIds;
                    List<ChainInfo> chains;

                    GetValidForUserMessages(db, tenant, user, messagesIds, out validIds, out chains);

                    UnsetMessagesTag(db, tenant, user, tagId, validIds, chains);
                    
                    tx.Commit();
                }
            }
        }

        private void UnsetMessagesTag(DbManager db, int tenant, string user, int tagId, 
            ICollection validIds, IEnumerable<ChainInfo> chains)
        {
            db.ExecuteNonQuery(
                new SqlDelete(TagMailTable.name)
                    .Where(TagMailTable.Columns.id_tag, tagId)
                    .Where(Exp.In(TagMailTable.Columns.id_mail, validIds)));

            UpdateTagCount(db, tenant, user, tagId);

            foreach (var chain in chains)
                UpdateChainTags(db, chain.id, chain.folder, chain.mailbox, tenant, user);
        }

        private void GetValidForUserMessages(
            IDbManager db,
            int tenant,
            string user,
            IEnumerable<int> messagesIds,
            out List<int> validIds,
            out List<ChainInfo> chains)
        {
            var valids = db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.id, MailTable.Columns.chain_id, MailTable.Columns.folder, MailTable.Columns.id_mailbox)
                        .Where(Exp.In(MailTable.Columns.id, messagesIds.ToArray()))
                        .Where(GetUserWhere(user, tenant)))
                    .ConvertAll(x => new { 
                        id = Convert.ToInt32(x[0]),
                        chain_id = x[1].ToString(),
                        folder = Convert.ToInt32(x[2]),
                        mailbox = Convert.ToInt32(x[3])});

            validIds = new List<int>();
            chains = new List<ChainInfo>();
            foreach (var item in valids)
            {
                validIds.Add(item.id);
                chains.Add(new ChainInfo { id = item.chain_id, folder = item.folder, mailbox = item.mailbox });
            }
        }

        public List<MailTag> GetTagsList(int tenant, string user, bool mailOnly)
        {
            var tags = new Dictionary<int, MailTag>();

            using (var db = GetDb())
            {
                db.ExecuteList(new SqlQuery(TagTable.name)
                                   .Select(TagTable.Columns.id, TagTable.Columns.name, TagTable.Columns.style,
                                           TagTable.Columns.addresses, TagTable.Columns.count, TagTable.Columns.crm_id)
                                   .Where(GetUserWhere(user, tenant)))
                  .ForEach(r =>
                           tags.Add(0 < Convert.ToInt32(r[5]) ? -Convert.ToInt32(r[5]) : Convert.ToInt32(r[0]),
                                    new MailTag(
                                        (0 < Convert.ToInt32(r[5]) && !mailOnly)
                                            ? -Convert.ToInt32(r[5])
                                            : Convert.ToInt32(r[0])
                                        , (string) r[1]
                                        ,
                                        !string.IsNullOrEmpty(r[3].ToString())
                                            ? r[3].ToString().Split(';').ToList()
                                            : new List<string>()
                                        , ConvertToString(r[2])
                                        , Convert.ToInt32(r[4])))
                    );
            }

            if (mailOnly)
                return tags.Values.Where(p => p.Name != "").OrderByDescending(p => p.Id).ToList();

            //TODO: Move to crm api

            #region Set up connection to CRM sequrity
            CoreContext.TenantManager.SetCurrentTenant(tenant);
            SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(new Guid(user)));

            if (!WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID.ToString(),
                                                    SecurityContext.CurrentAccount.ID))
                return tags.Values.Where(p => p.Name != "").OrderByDescending(p => p.Id).ToList();

            #endregion

            const string crm_tag_alias = "t";

            using (var db = new DbManager("crm"))
            {
                var q = new SqlQuery(CrmTagTable.name.Alias(crm_tag_alias))
                    .Select(CrmTagTable.Columns.id.Prefix(crm_tag_alias), CrmTagTable.Columns.title.Prefix(crm_tag_alias))
                    .Where(CrmTagTable.Columns.tenant_id.Prefix(crm_tag_alias), tenant)
                    .Where(CrmTagTable.Columns.entity_type.Prefix(crm_tag_alias), CRM_CONTACT_ENTITY_TYPE);

                var crmTags = db.ExecuteList(q)
                    .ConvertAll(r =>
                        new MailTag(-Convert.ToInt32(r[0])
                            ,(string)r[1]
                            ,new List<string>()
                            ,""
                            ,0));

                foreach (var tag in crmTags)
                {
                    if (tags.ContainsKey(tag.Id))
                        tags[tag.Id].Name = tag.Name;
                    else
                        tags.Add(tag.Id, tag);
                }
            }

            return tags.Values.Where(t => t.Name != "").OrderByDescending(p => p.Id).ToList();
        }

        public bool TagExists(int tenant, string user, string name)
        {
            using (var db = GetDb())
            {
                var result = db.ExecuteScalar<int>(new SqlQuery(TagTable.name)
                                                   .Select(TagTable.Columns.id)
                                                   .Where(GetUserWhere(user, tenant))
                                                   .Where(TagTable.Columns.name, name));

                return Convert.ToInt32(result) > 0;
            }
        }

        public MailTag GetMailTag(int tenant, string user, int tagId)
        {
            var tags = GetMailTags(tenant, user, new EqExp(TagTable.Columns.id, tagId));
            return tags.Any() ? tags[0] : null;
        }

        public MailTag GetMailTag(int tenant, string user, string name)
        {
            var tags = GetMailTags(tenant, user, new EqExp(TagTable.Columns.name, name));
            return tags.Any() ? tags[0] : null;
        }

        private List<MailTag> GetMailTags(int tenant, string user, Exp exp)
        {
            using (var db = GetDb())
            {
                return GetMailTags(db, tenant, user, exp);
            }
        }

        private List<MailTag> GetMailTags(IDbManager db, int tenant, string user, Exp exp)
        {
            return db.ExecuteList(
                new SqlQuery(TagTable.name)
                    .Select(TagTable.Columns.id, TagTable.Columns.name, TagTable.Columns.style, TagTable.Columns.addresses)
                    .Where(GetUserWhere(user, tenant))
                    .Where(exp))
                .ConvertAll(r =>
                    new MailTag(Convert.ToInt32(r[0]), (string)r[1],
                                    r[3] == null ? null : r[3].ToString().Split(';').ToList(), ConvertToString(r[2]), 0));
        }

        public int[] GetOrCreateTags(int tenant, string user, string[] names)
        {
            var exp = names.Aggregate(Exp.Empty, (current, name) => current | Exp.Eq(TagTable.Columns.name, name));
            var existingTags = GetMailTags(tenant, user, exp);
            var existingNames = existingTags.ConvertAll(t => t.Name.ToLower());
            var res = existingTags.ConvertAll(t => t.Id);
            // ToDo: style from name
            res.AddRange(from name in names
                         where !existingNames.Contains(name.ToLower())
                         select SaveMailTag(tenant,
                                            user,
                                            new MailTag(0,
                                                        name,
                                                        new List<string>(),
                                                        (Math.Abs(name.GetHashCode()%16) + 1).ToString(
                                                            CultureInfo.InvariantCulture), 0)
                             )
                         into newTag
                         select newTag.Id);
            return res.ToArray();
        }

        public MailTag SaveMailTag(int tenant, string user, MailTag tag)
        {
            using (var db = GetDb()) 
            {
                using (var tx = db.BeginTransaction())
                {
                    if (tag.Id == 0)
                    {
                        tag.Id = db.ExecuteScalar<int>(
                            new SqlInsert(TagTable.name)
                                .InColumnValue(TagTable.Columns.id, 0)
                                .InColumnValue(TagTable.Columns.id_tenant, tenant)
                                .InColumnValue(TagTable.Columns.id_user, user)
                                .InColumnValue(TagTable.Columns.name, tag.Name)
                                .InColumnValue(TagTable.Columns.style, tag.Style)
                                .InColumnValue(TagTable.Columns.addresses, string.Join(";", tag.Addresses.ToArray()))
                                .Identity(0, 0, true));
                    }
                    else
                    {
                        db.ExecuteNonQuery(
                            new SqlUpdate(TagTable.name)
                                .Set(TagTable.Columns.name, tag.Name)
                                .Set(TagTable.Columns.style, tag.Style)
                                .Set(TagTable.Columns.addresses, string.Join(";", tag.Addresses.ToArray()))
                                .Where(TagTable.Columns.id, tag.Id));

                        db.ExecuteNonQuery(
                                new SqlDelete(TagAddressTable.name)
                                    .Where(TagAddressTable.Columns.id_tag, tag.Id));
                    }

                    if (tag.Addresses.Any())
                    {
                        var insertQuery =
                            new SqlInsert(TagAddressTable.name)
                                .InColumns(TagAddressTable.Columns.id_tag,
                                           TagAddressTable.Columns.address,
                                           TagAddressTable.Columns.id_tenant);

                        tag.Addresses
                           .ForEach(addr =>
                                    insertQuery
                                        .Values(tag.Id, addr, tenant));

                        db.ExecuteNonQuery(insertQuery);
                    }

                    tx.Commit();
                }
            }

            return tag;
        }

        public void DeleteTag(int tenant, string user, int tagId)
        {
            using (var db = GetDb()) 
            {
                using (var tx = db.BeginTransaction())
                {
                    var affected = db.ExecuteNonQuery(
                        new SqlDelete(TagTable.name)
                            .Where(TagTable.Columns.id, tagId)
                            .Where(GetUserWhere(user, tenant)));

                    if (0 < affected)
                    {
                        db.ExecuteNonQuery(
                            new SqlDelete(TagMailTable.name)
                                .Where(TagMailTable.Columns.id_tag, tagId)
                                .Where(GetUserWhere(user, tenant)));
                    }

                    tx.Commit();
                }
            }
        }

        #endregion

        #region private methods

        private void SetMessageTags(DbManager db, int tenant, string user, int messageId, IEnumerable<int> tagIds)
        {
            var idTags = tagIds as IList<int> ?? tagIds.ToList();
            if (!idTags.Any()) return;

            CreateInsertDelegate createInsertQuery = ()
                => new SqlInsert(TagMailTable.name)
                        .IgnoreExists(true)
                        .InColumns(TagMailTable.Columns.id_mail,
                                   TagMailTable.Columns.id_tag,
                                   TagMailTable.Columns.id_tenant,
                                   TagMailTable.Columns.id_user);

            var insertQuery = createInsertQuery();

            int i, tagsLen;
            for (i = 0, tagsLen = idTags.Count; i < tagsLen; i++)
            {
                var tagId = idTags[i];

                insertQuery
                    .Values(messageId, tagId, tenant, user);

                if ((i%100 != 0 || i == 0) && i + 1 != tagsLen) continue;
                db.ExecuteNonQuery(insertQuery);
                insertQuery = createInsertQuery();
            }

            UpdateTagsCount(db, tenant, user, idTags);
        }

        private void SetMessagesTag(DbManager db, int tenant, string user, IEnumerable<int> messageIds, int tagId)
        {
            var idMessages = messageIds as IList<int> ?? messageIds.ToList();
            if (!idMessages.Any()) return;

            CreateInsertDelegate createInsertQuery = ()
                => new SqlInsert(TagMailTable.name)
                        .IgnoreExists(true)
                        .InColumns(TagMailTable.Columns.id_mail,
                                   TagMailTable.Columns.id_tag,
                                   TagMailTable.Columns.id_tenant,
                                   TagMailTable.Columns.id_user);

            var insertQuery = createInsertQuery();

            int i, messagessLen;
            for (i = 0, messagessLen = idMessages.Count; i < messagessLen; i++)
            {
                var messageId = idMessages[i];

                insertQuery
                    .Values(messageId, tagId, tenant, user);

                if ((i%100 != 0 || i == 0) && i + 1 != messagessLen) continue;
                db.ExecuteNonQuery(insertQuery);
                insertQuery = createInsertQuery();
            }

            UpdateTagsCount(db, tenant, user, new[] { tagId });
        }

        private void SetConversationsTag(DbManager db, int tenant, string user, int tagId, IEnumerable<int> messagesIds)
        {
            var foundedChains = GetChainedMessagesInfo(db, tenant, user, (List<int>)messagesIds,
                       new[]
                            {
                                MailTable.Columns.id, 
                                MailTable.Columns.chain_id,
                                MailTable.Columns.folder,
                                MailTable.Columns.id_mailbox
                            })
                       .ConvertAll(r => new
                       {
                           mail_id = Convert.ToInt32(r[0]),
                           chain_id = r[1].ToString(),
                           folder = Convert.ToInt32(r[2]),
                           id_mailbox = Convert.ToInt32(r[3])
                       });

            if (!foundedChains.Any()) return;

            SetMessagesTag(db, tenant, user, foundedChains.Select(r => r.mail_id), tagId);

            foreach (var chain in foundedChains.GroupBy(r => new { r.chain_id, r.folder, r.id_mailbox }))
                UpdateChainTags(db, chain.Key.chain_id, chain.Key.folder, chain.Key.id_mailbox, tenant, user);
        }

        // Add tags from Teamlab CRM to mail if needed.
        private void SetCrmTags(MailMessageItem mail, int tenant, string user)
        {
            try
            {
                if (mail.ParticipantsCrmContactsId == null)
                    mail.ParticipantsCrmContactsId = GetCrmContactsId(tenant, user, mail.FromEmail);

                var allowedContactIds = mail.ParticipantsCrmContactsId;

                //Todo: Rewrite this to Api call

                using (var db = new DbManager("crm"))
                {
                    #region Extract CRM tags

                    if (allowedContactIds.Count == 0) return;

                    var tagIds = db.ExecuteList(new SqlQuery(CrmEntityTagTable.name)
                                                           .Distinct()
                                                           .Select(CrmEntityTagTable.Columns.tag_id)
                                                           .Where(CrmEntityTagTable.Columns.entity_type, (int)EntityType.Contact)
                                                           .Where(Exp.In(CrmEntityTagTable.Columns.entity_id, allowedContactIds)))
                                          .ConvertAll(r => -Convert.ToInt32(r[0]));

                    mail.TagIds = new ItemList<int>(tagIds);

                    #endregion
                }
            }
            catch (Exception e)
            {
                _log.Error("SetCrmTags() Exception:\r\n{0}\r\n", e.ToString());
            }
        }

        private void UpdateTagsCount(DbManager db, int tenant, string user, IEnumerable<int> tagIds)
        {
            foreach (var tagId in tagIds)
                UpdateTagCount(db, tenant, user, tagId);
        }

        private void UpdateTagCount(DbManager db, int tenant, string user, int tadId)
        {
            var countQuery = new SqlQuery(TagMailTable.name)
                .SelectCount()
                .Where(GetUserWhere(user, tenant))
                .Where(TagMailTable.Columns.id_tag, tadId);

            const string tag_alias = "mt";

            var res = db.ExecuteNonQuery(
                new SqlUpdate(TagTable.name.Alias(tag_alias))
                    .Set(TagTable.Columns.count.Prefix(tag_alias), countQuery)
                    .Where(GetUserWhere(user, tenant, tag_alias))
                    .Where(tadId >= 0
                               ? Exp.Eq(TagTable.Columns.id.Prefix(tag_alias), tadId)
                               : Exp.Eq(TagTable.Columns.crm_id.Prefix(tag_alias), -tadId)));

            if (tadId >= 0 || 0 != res) return;

            db.ExecuteNonQuery(
                new SqlInsert(TagTable.name, true)
                    .InColumnValue(TagTable.Columns.id_tenant, tenant)
                    .InColumnValue(TagTable.Columns.id_user, user)
                    .InColumnValue(TagTable.Columns.name, "")
                    .InColumnValue(TagTable.Columns.addresses, "")
                    .InColumnValue(TagTable.Columns.crm_id, -tadId)
                    .InColumnValue(TagTable.Columns.count, db.ExecuteScalar<int>(countQuery)));
        }

        #endregion
    }
}
