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
using System.Globalization;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Data;
using ASC.CRM.Core;
using ASC.Core;
using System.Configuration;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Extension;
using ASC.Web.Core;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region db defines

        public const string MAIL_TAG_ADDRESSES = "mail_tag_addresses";
        public const string MAIL_TAG = "mail_tag";
        public const string MAIL_TAG_MAIL = "mail_tag_mail";

        public const string CRM_TAG = "crm_tag";

        public struct TagMailFields
        {
            public static string id_mail = "id_mail";
            public static string id_tag = "id_tag";
            public static string time_created = "time_created";
            public static string id_tenant = "tenant";
            public static string id_user = "id_user";
        };

        public struct TagAddressFields
        {
            public static string id_tag = "id_tag";
            public static string address = "address";
            public static string id_tenant = "tenant";
        };

        public struct TagFields
        {
            public static string id = "id";
            public static string id_user = "id_user";
            public static string id_tenant = "tenant";
            public static string name = "name";
            public static string style = "style";
            public static string addresses = "addresses";
            public static string count = "count";
            public static string crm_id = "crm_id";
        };

        public struct CrmTagFields
        {
            public static string id = "id";
            public static string title = "title";
            public static string tenant_id = "tenant_id";
            public static string entity_type = "entity_type";
        };

        #endregion

        #region public methods

        public void SetMessagesTag(int tenant, string user, int tag_id, IEnumerable<int> messages_ids)
        {
            var ids = messages_ids as IList<int> ?? messages_ids.ToList();

            if (!ids.Any()) return;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    List<int> valid_ids;
                    List<ChainInfo> chains;

                    GetValidForUserMessages(db, tenant, user, ids, out valid_ids, out chains);

                    SetMessagesTag(db, tenant, user, valid_ids, tag_id);

                    foreach (var chain in chains)
                        UpdateChainTags(db, chain.id, chain.folder, chain.mailbox, tenant, user);

                    tx.Commit();
                }
            }
        }

        public void SetConversationsTag(int tenant, string user, int tag_id, IEnumerable<int> messages_ids)
        {
            var ids = messages_ids as IList<int> ?? messages_ids.ToList();
            
            if (!ids.Any()) return;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    SetConversationsTag(db, tenant, user, tag_id, ids);

                    tx.Commit();
                }
            }
        }

        public void UnsetConversationsTag(int id_tenant, string id_user, int tag_id, IEnumerable<int> messages_ids)
        {
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                { 
                    var list_objects = GetChainedMessagesInfo(db, id_tenant, id_user, (List<int>) messages_ids,
                                                              new[]
                                                                  {
                                                                      MailTable.Columns.id, MailTable.Columns.chain_id,
                                                                      MailTable.Columns.folder,
                                                                      MailTable.Columns.id_mailbox
                                                                  });

                    var valid_ids = new List<int>();
                    var chains = new List<ChainInfo>();

                    list_objects.ForEach(
                        item =>
                            {
                                chains.Add(
                                    new ChainInfo
                                        {
                                            id = item[1].ToString(),
                                            folder = Convert.ToInt32(item[2]),
                                            mailbox = Convert.ToInt32(item[3])
                                        });

                                valid_ids.Add(Convert.ToInt32(item[0]));
                            });

                    UnsetMessagesTag(db, id_tenant, id_user, tag_id, valid_ids, chains);
                    
                    tx.Commit();
                }
            }
        }

        public void UnsetMessagesTag(int id_tenant, string id_user, int tag_id, IEnumerable<int> messages_ids)
        {
            using (var db = GetDb()) 
            {
                using (var tx = db.BeginTransaction())
                {
                    List<int> valid_ids;
                    List<ChainInfo> chains;

                    GetValidForUserMessages(db, id_tenant, id_user, messages_ids, out valid_ids, out chains);

                    UnsetMessagesTag(db, id_tenant, id_user, tag_id, valid_ids, chains);
                    
                    tx.Commit();
                }
            }
        }

        private void UnsetMessagesTag(DbManager db, int id_tenant, string id_user, int tag_id, 
            List<int> valid_ids, IEnumerable<ChainInfo> chains)
        {
            db.ExecuteNonQuery(
                new SqlDelete(MAIL_TAG_MAIL)
                    .Where(TagMailFields.id_tag, tag_id)
                    .Where(Exp.In(TagMailFields.id_mail, valid_ids)));

            UpdateTagCount(db, id_tenant, id_user, tag_id);

            foreach (var chain in chains)
                UpdateChainTags(db, chain.id, chain.folder, chain.mailbox, id_tenant, id_user);
        }

        private void GetValidForUserMessages(
            DbManager db,
            int id_tenant,
            string id_user,
            IEnumerable<int> messages_ids,
            out List<int> valid_ids,
            out List<ChainInfo> chains)
        {
            var valids = db.ExecuteList(
                    new SqlQuery(MailTable.name)
                        .Select(MailTable.Columns.id, MailTable.Columns.chain_id, MailTable.Columns.folder, MailTable.Columns.id_mailbox)
                        .Where(Exp.In(MailTable.Columns.id, messages_ids.ToArray()))
                        .Where(GetUserWhere(id_user, id_tenant)))
                    .ConvertAll(x => new { 
                        id = Convert.ToInt32(x[0]),
                        chain_id = x[1].ToString(),
                        folder = Convert.ToInt32(x[2]),
                        mailbox = Convert.ToInt32(x[3])});

            valid_ids = new List<int>();
            chains = new List<ChainInfo>();
            foreach (var item in valids)
            {
                valid_ids.Add(item.id);
                chains.Add(new ChainInfo { id = item.chain_id, folder = item.folder, mailbox = item.mailbox });
            }
        }

        public List<MailTag> GetTagsList(int id_tenant, string id_user, bool mail_only)
        {
            var tags = new Dictionary<int, MailTag>();

            using (var db = GetDb())
            {
                db.ExecuteList(new SqlQuery(MAIL_TAG)
                         .Select(TagFields.id, TagFields.name, TagFields.style, TagFields.addresses, TagFields.count, TagFields.crm_id)
                         .Where(GetUserWhere(id_user, id_tenant)))
                         .ForEach(r =>
                             tags.Add(0 < Convert.ToInt32(r[5]) ? -Convert.ToInt32(r[5]) : Convert.ToInt32(r[0]),
                             new MailTag( (0 < Convert.ToInt32(r[5]) && !mail_only) ? -Convert.ToInt32(r[5]) : Convert.ToInt32(r[0])
                                 , (string)r[1]
                                 , !string.IsNullOrEmpty(r[3].ToString()) ? r[3].ToString().Split(';').ToList() : new List<string>()
                                 , ConvertToString(r[2])
                                 , Convert.ToInt32(r[4])))
                         );
            }

            if (mail_only)
                return tags.Values.Where(p => p.Name != "").OrderByDescending(p => p.Id).ToList();

            #region Set up connection to CRM sequrity
            CoreContext.TenantManager.SetCurrentTenant(id_tenant);
            SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(new Guid(id_user)));

            if (!WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID.ToString(),
                                                    SecurityContext.CurrentAccount.ID))
                return tags.Values.Where(p => p.Name != "").OrderByDescending(p => p.Id).ToList();

            if (!DbRegistry.IsDatabaseRegistered("crm"))
            {
                DbRegistry.RegisterDatabase("crm", ConfigurationManager.ConnectionStrings["crm"]);
            }

            #endregion

            using (var db = new DbManager("crm"))
            {
                var q = new SqlQuery(CRM_TAG + " t")
                    .Select("t." + CrmTagFields.id, "t." + CrmTagFields.title)
                    .Where(Exp.Eq("t." + CrmTagFields.tenant_id, id_tenant))
                    .Where(Exp.Eq("t." + CrmTagFields.entity_type, CRM_CONTACT_ENTITY_TYPE));

                var crm_tags = db.ExecuteList(q)
                    .ConvertAll(r =>
                        new MailTag(-Convert.ToInt32(r[0])
                            ,(string)r[1]
                            ,new List<string>()
                            ,""
                            ,0));

                foreach (var tag in crm_tags)
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
                var result = db.ExecuteScalar<int>(new SqlQuery(MAIL_TAG)
                                                   .Select(TagFields.id)
                                                   .Where(GetUserWhere(user, tenant))
                                                   .Where(TagFields.name, name));

                return Convert.ToInt32(result) > 0;
            }
        }

        public MailTag GetMailTag(int tenant, string user, int tag_id)
        {
            var tags = GetMailTags(tenant, user, new EqExp(TagFields.id, tag_id));
            return tags.Any() ? tags[0] : null;
        }

        public MailTag GetMailTag(int tenant, string user, string name)
        {
            var tags = GetMailTags(tenant, user, new EqExp(TagFields.name, name));
            return tags.Any() ? tags[0] : null;
        }

        private List<MailTag> GetMailTags(int id_tenant, string id_user, Exp exp)
        {
            using (var db = GetDb())
            {
                return GetMailTags(db, id_tenant, id_user, exp);
            }
        }

        private List<MailTag> GetMailTags(IDbManager db, int id_tenant, string id_user, Exp exp)
        {
            return db.ExecuteList(
                new SqlQuery(MAIL_TAG)
                    .Select(TagFields.id, TagFields.name, TagFields.style, TagFields.addresses)
                    .Where(GetUserWhere(id_user, id_tenant))
                    .Where(exp))
                .ConvertAll(r =>
                    new MailTag(Convert.ToInt32(r[0]), (string)r[1],
                                    r[3] == null ? null : r[3].ToString().Split(';').ToList(), ConvertToString(r[2]), 0));
        }

        public int[] GetOrCreateTags(int tenant, string user, string[] names)
        {
            var exp = names.Aggregate(Exp.Empty, (current, name) => current | Exp.Eq(TagFields.name, name));
            var existing_tags = GetMailTags(tenant, user, exp);
            var existing_names = existing_tags.ConvertAll(t => t.Name.ToLower());
            var res = existing_tags.ConvertAll(t => t.Id);
            // ToDo: style from name
            res.AddRange(from name in names where !existing_names.Contains(name.ToLower())
                         select SaveMailTag(tenant,
                            user,
                            new MailTag(0,
                                name,
                                new List<string>(),
                                (Math.Abs(name.GetHashCode()%16)+1).ToString(CultureInfo.InvariantCulture), 0)
                            ) into new_tag
                         select new_tag.Id);
            return res.ToArray();
        }

        public MailTag SaveMailTag(int id_tenant, string id_user, MailTag tag)
        {
            using (var db = GetDb()) 
            {
                using (var tx = db.BeginTransaction())
                {
                    if (tag.Id == 0)
                    {
                        tag.Id = db.ExecuteScalar<int>(
                            new SqlInsert(MAIL_TAG)
                                .InColumnValue(TagFields.id, 0)
                                .InColumnValue(TagFields.id_tenant, id_tenant)
                                .InColumnValue(TagFields.id_user, id_user)
                                .InColumnValue(TagFields.name, tag.Name)
                                .InColumnValue(TagFields.style, tag.Style)
                                .InColumnValue(TagFields.addresses, string.Join(";", tag.Addresses.ToArray()))
                                .Identity(0, 0, true));
                    }
                    else
                    {
                        db.ExecuteNonQuery(
                            new SqlUpdate(MAIL_TAG)
                                .Set(TagFields.name, tag.Name)
                                .Set(TagFields.style, tag.Style)
                                .Set(TagFields.addresses, string.Join(";", tag.Addresses.ToArray()))
                                .Where(TagFields.id, tag.Id));

                        db.ExecuteNonQuery(
                                new SqlDelete(MAIL_TAG_ADDRESSES)
                                    .Where(TagAddressFields.id_tag, tag.Id));
                    }

                    if (tag.Addresses.Any())
                    {
                        var insert_query =
                            new SqlInsert(MAIL_TAG_ADDRESSES)
                                .InColumns(TagAddressFields.id_tag,
                                           TagAddressFields.address,
                                           TagAddressFields.id_tenant);

                        tag.Addresses
                           .ForEach(addr =>
                                    insert_query
                                        .Values(tag.Id, addr, id_tenant));

                        db.ExecuteNonQuery(insert_query);
                    }

                    tx.Commit();
                }
            }

            return tag;
        }

        public void DeleteTag(int id_tenant, string id_user, int tag_id)
        {
            using (var db = GetDb()) 
            {
                using (var tx = db.BeginTransaction())
                {
                    var affected = db.ExecuteNonQuery(
                        new SqlDelete(MAIL_TAG)
                            .Where(TagFields.id, tag_id)
                            .Where(GetUserWhere(id_user, id_tenant)));

                    if (0 < affected)
                    {
                        db.ExecuteNonQuery(
                            new SqlDelete(MAIL_TAG_MAIL)
                                .Where(TagMailFields.id_tag, tag_id)
                                .Where(GetUserWhere(id_user, id_tenant)));
                    }

                    tx.Commit();
                }
            }
        }

        #endregion

        #region private methods

        private void SetMessageTags(DbManager db, int tenant, string user, int message_id, IEnumerable<int> tag_ids)
        {
            var id_tags = tag_ids as IList<int> ?? tag_ids.ToList();
            if (!id_tags.Any()) return;

            CreateInsertDelegate create_insert_query = ()
                => new SqlInsert(MAIL_TAG_MAIL)
                        .IgnoreExists(true)
                        .InColumns(TagMailFields.id_mail,
                                   TagMailFields.id_tag,
                                   TagMailFields.id_tenant,
                                   TagMailFields.id_user);

            var insert_query = create_insert_query();

            int i, tags_len;
            for (i = 0, tags_len = id_tags.Count; i < tags_len; i++)
            {
                var tag_id = id_tags[i];

                insert_query
                    .Values(message_id, tag_id, tenant, user);

                if (i % 100 == 0 && i != 0 || i + 1 == tags_len)
                {
                    db.ExecuteNonQuery(insert_query);
                    insert_query = create_insert_query();
                }
            }

            UpdateTagsCount(db, tenant, user, id_tags);
        }

        private void SetMessagesTag(DbManager db, int tenant, string user, IEnumerable<int> message_ids, int tag_id)
        {
            var id_messages = message_ids as IList<int> ?? message_ids.ToList();
            if (!id_messages.Any()) return;

            CreateInsertDelegate create_insert_query = ()
                => new SqlInsert(MAIL_TAG_MAIL)
                        .IgnoreExists(true)
                        .InColumns(TagMailFields.id_mail,
                                   TagMailFields.id_tag,
                                   TagMailFields.id_tenant,
                                   TagMailFields.id_user);

            var insert_query = create_insert_query();

            int i, messagess_len;
            for (i = 0, messagess_len = id_messages.Count; i < messagess_len; i++)
            {
                var message_id = id_messages[i];

                insert_query
                    .Values(message_id, tag_id, tenant, user);

                if (i % 100 == 0 && i != 0 || i + 1 == messagess_len)
                {
                    db.ExecuteNonQuery(insert_query);
                    insert_query = create_insert_query();
                }
            }

            UpdateTagsCount(db, tenant, user, new[] { tag_id });
        }

        private void SetConversationsTag(DbManager db, int tenant, string user, int tag_id, IEnumerable<int> messages_ids)
        {
            var founded_chains = GetChainedMessagesInfo(db, tenant, user, (List<int>)messages_ids,
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

            if (!founded_chains.Any()) return;

            SetMessagesTag(db, tenant, user, founded_chains.Select(r => r.mail_id), tag_id);

            foreach (var chain in founded_chains.GroupBy(r => new { r.chain_id, r.folder, r.id_mailbox }))
                UpdateChainTags(db, chain.Key.chain_id, chain.Key.folder, chain.Key.id_mailbox, tenant, user);
        }

        // Add tags from Teamlab CRM to mail if needed.
        private void SetCrmTags(MailMessageItem mail, int tenant_id, string user_id)
        {
            try
            {
                if (mail.ParticipantsCrmContactsId == null)
                    mail.ParticipantsCrmContactsId = GetCrmContactsId(tenant_id, user_id, mail.FromEmail);

                var allowed_contact_ids = mail.ParticipantsCrmContactsId;

                using (var db = new DbManager("crm"))
                {
                    #region Extract CRM tags

                    if (allowed_contact_ids.Count == 0) return;
                    //Todo: Rewrite this to Api call
                    var tag_ids = db.ExecuteList(new SqlQuery("crm_entity_tag")
                                                           .Distinct()
                                                           .Select("tag_id")
                                                           .Where("entity_type", (int)EntityType.Contact)
                                                           .Where(Exp.In("entity_id", allowed_contact_ids)))
                                          .ConvertAll(r => -Convert.ToInt32(r[0]));

                    mail.TagIds = new ItemList<int>(tag_ids);

                    #endregion
                }
            }
            catch (Exception e)
            {
                _log.Error("SetCrmTags() Exception:\r\n{0}\r\n", e.ToString());
            }
        }

        private void UpdateTagsCount(DbManager db, int tenant, string user, IEnumerable<int> tag_ids)
        {
            foreach (var tag_id in tag_ids)
                UpdateTagCount(db, tenant, user, tag_id);
        }

        private void UpdateTagCount(DbManager db, int tenant, string user, int tad_id)
        {
            var count_query = new SqlQuery(MAIL_TAG_MAIL)
                .SelectCount()
                .Where(GetUserWhere(user, tenant))
                .Where(TagMailFields.id_tag, tad_id);

            var res = db.ExecuteNonQuery(
                new SqlUpdate(MAIL_TAG + " mt")
                    .Set("mt." + TagFields.count, count_query)
                    .Where(GetUserWhere(user, tenant, "mt"))
                    .Where(tad_id >= 0
                               ? Exp.Eq("mt." + TagFields.id, tad_id)
                               : Exp.Eq("mt." + TagFields.crm_id, -tad_id)));

            if (tad_id >= 0 || 0 != res) return;

            db.ExecuteNonQuery(
                new SqlInsert(MAIL_TAG, true)
                    .InColumnValue(TagFields.id_tenant, tenant)
                    .InColumnValue(TagFields.id_user, user)
                    .InColumnValue(TagFields.name, "")
                    .InColumnValue(TagFields.addresses, "")
                    .InColumnValue(TagFields.crm_id, -tad_id)
                    .InColumnValue(TagFields.count, db.ExecuteScalar<int>(count_query)));
        }

        #endregion
    }
}
