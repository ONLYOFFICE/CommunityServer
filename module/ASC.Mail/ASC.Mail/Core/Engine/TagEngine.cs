/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using ASC.Common.Logging;
using ASC.ElasticSearch;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Search;
using ASC.Web.Core;

namespace ASC.Mail.Core.Engine
{
    public class TagEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }
        public ILog Log { get; private set; }
        public EngineFactory Factory { get; private set; }

        public TagEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.TagEngine");
            Factory = new EngineFactory(Tenant, User);
        }

        public Tag GetTag(int id)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoTag = daoFactory.CreateTagDao(Tenant, User);

                var tag = daoTag.GetTag(id);

                return tag;
            }
        }

        public Tag GetTag(string name)
        {
            using (var daoFactory = new DaoFactory())
            {
                var dao = daoFactory.CreateTagDao(Tenant, User);

                var tag = dao.GetTag(name);

                return tag;
            }
        }

        public List<Tag> GetTags()
        {
            using (var daoFactory = new DaoFactory())
            {
                var dao = daoFactory.CreateTagDao(Tenant, User);

                var tagList = dao.GetTags();

                if (!WebItemSecurity.IsAvailableForMe(WebItemManager.CRMProductID))
                {
                    return tagList
                        .Where(p => p.TagName != "")
                        .OrderByDescending(p => p.Id)
                        .ToList();
                }

                var actualCrmTags = dao.GetCrmTags();

                var removedCrmTags =
                    tagList.Where(t => t.Id < 0 && !actualCrmTags.Exists(ct => ct.Id == t.Id))
                        .ToList();

                if (removedCrmTags.Any())
                {
                    dao.DeleteTags(removedCrmTags.Select(t => t.Id).ToList());
                    removedCrmTags.ForEach(t => tagList.Remove(t));
                }

                foreach (var crmTag in actualCrmTags)
                {
                    var tag = tagList.FirstOrDefault(t => t.Id == crmTag.Id);
                    if (tag != null)
                        tag.TagName = crmTag.TagName;
                    else
                        tagList.Add(crmTag);
                }

                return tagList
                    .Where(p => !string.IsNullOrEmpty(p.TagName))
                    .OrderByDescending(p => p.Id)
                    .ToList();
            }
        }

        public List<Tag> GetCrmTags(string email)
        {
            var tags = new List<Tag>();

            using (var daoFactory = new DaoFactory())
            {
                var daoCrmContacts = daoFactory.CreateCrmContactDao(Tenant, User);

                var allowedContactIds = daoCrmContacts.GetCrmContactIds(email);

                if (!allowedContactIds.Any())
                    return tags;

                var daoTag = daoFactory.CreateTagDao(Tenant, User);

                tags = daoTag.GetCrmTags(allowedContactIds);

                return tags
                    .Where(p => !string.IsNullOrEmpty(p.TagName))
                    .OrderByDescending(p => p.Id)
                    .ToList();
            }
        }

        public bool IsTagExists(string name)
        {
            using (var daoFactory = new DaoFactory())
            {
                var dao = daoFactory.CreateTagDao(Tenant, User);

                var tag = dao.GetTag(name);

                return tag != null;
            }
        }

        public Tag CreateTag(string name, string style, IEnumerable<string> addresses)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    var tagDao = daoFactory.CreateTagDao(Tenant, User);

                    var tagAddressDao = daoFactory.CreateTagAddressDao(Tenant, User);

                    var tag = tagDao.GetTag(name);

                    if (tag != null)
                        throw new ArgumentException("Tag name already exists");

                    var emails = addresses as IList<string> ?? addresses.ToList();

                    tag = new Tag
                    {
                        Id = 0,
                        TagName = name,
                        Tenant = Tenant,
                        User = User,
                        Addresses = string.Join(";", emails),
                        Style = style,
                        Count = 0,
                        CrmId = 0
                    };

                    var id = tagDao.SaveTag(tag);

                    if (id < 0)
                        throw new Exception("Save failed");

                    foreach (var email in emails)
                    {
                        tagAddressDao.Save(id, email);
                    }

                    tag.Id = id;

                    tx.Commit();

                    return tag;
                }
            }
        }

        public Tag UpdateTag(int id, string name, string style, IEnumerable<string> addresses)
        {
            using (var daoFactory = new DaoFactory())
            {
                var tagDao = daoFactory.CreateTagDao(Tenant, User);

                var tagAddressDao = daoFactory.CreateTagAddressDao(Tenant, User);

                var tag = tagDao.GetTag(id);

                if (tag == null)
                    throw new ArgumentException(@"Tag not found");

                if (!tag.TagName.Equals(name))
                {
                    var tagByName = tagDao.GetTag(name);

                    if (tagByName != null && tagByName.Id != id)
                        throw new ArgumentException(@"Tag name already exists");

                    tag.TagName = name;
                }

                if (!tag.Style.Equals(style))
                {
                    tag.Style = style;
                }

                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    var oldAddresses = tagAddressDao.GetTagAddresses(tag.Id);

                    var newAddresses = addresses as IList<string> ?? addresses.ToList();
                    tag.Addresses = string.Join(";", newAddresses);

                    tagDao.SaveTag(tag);

                    if (!newAddresses.Any())
                    {
                        if (oldAddresses.Any())
                            tagAddressDao.Delete(tag.Id);
                    }
                    else
                    {
                        foreach (var oldAddress in oldAddresses)
                        {
                            if (!newAddresses.Contains(oldAddress))
                            {
                                tagAddressDao.Delete(tag.Id, oldAddress);
                            }
                        }

                        foreach (var newAddress in newAddresses)
                        {
                            if (!oldAddresses.Contains(newAddress))
                            {
                                tagAddressDao.Save(tag.Id, newAddress);
                            }
                        }
                    }

                    tx.Commit();

                    return tag;
                }
            }
        }

        public bool DeleteTag(int id)
        {
            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    var tagDao = daoFactory.CreateTagDao(Tenant, User);

                    var tagAddressDao = daoFactory.CreateTagAddressDao(Tenant, User);

                    var tagMailDao = daoFactory.CreateTagMailDao(Tenant, User);

                    tagDao.DeleteTag(id);

                    tagAddressDao.Delete(id);

                    tagMailDao.DeleteByTagId(id);

                    tx.Commit();
                }
            }

            return true;
        }

        public List<int> GetOrCreateTags(int tenant, string user, string[] names)
        {
            var tagIds = new List<int>();

            if (!names.Any())
                return tagIds;

            using (var daoFactory = new DaoFactory())
            {
                var dao = daoFactory.CreateTagDao(Tenant, User);

                var tags = dao.GetTags();

                foreach (var name in names)
                {
                    var tag =
                        tags.FirstOrDefault(t => t.TagName.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                    if (tag != null)
                    {
                        tagIds.Add(tag.Id);
                        continue;
                    }

                    tag = new Tag
                    {
                        Id = 0,
                        TagName = name,
                        Addresses = "",
                        Count = 0,
                        CrmId = 0,
                        Style = (Math.Abs(name.GetHashCode() % 16) + 1).ToString(CultureInfo.InvariantCulture),
                        Tenant = tenant,
                        User = user
                    };

                    var id = dao.SaveTag(tag);

                    if (id > 0)
                    {
                        Log.InfoFormat("TagEngine->GetOrCreateTags(): new tag '{0}' with id = {1} has bee created",
                            name, id);

                        tagIds.Add(id);
                    }
                }
            }

            return tagIds;
        }

        public void SetMessagesTag(List<int> messageIds, int tagId)
        {
            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    if (!SetMessagesTag(daoFactory, messageIds, tagId))
                    {
                        tx.Rollback();
                        return;
                    }

                    tx.Commit();
                }
            }

            UpdateIndexerTags(messageIds, UpdateAction.Add, tagId);

            Log.InfoFormat("TagEngine->SetMessagesTag(): tag with id = {0} has bee added to messages [{1}]", tagId,
                string.Join(",", messageIds));
        }

        public bool SetMessagesTag(IDaoFactory daoFactory, List<int> messageIds, int tagId)
        {
            var daoTag = daoFactory.CreateTagDao(Tenant, User);

            var tag = daoTag.GetTag(tagId);

            if (tag == null)
            {
                return false;
            }

            List<int> validIds;
            List<ChainInfo> chains;

            GetValidForUserMessages(daoFactory, messageIds, out validIds, out chains);

            var daoTagMail = daoFactory.CreateTagMailDao(Tenant, User);

            daoTagMail.SetMessagesTag(validIds, tag.Id);

            UpdateTagsCount(daoTagMail, daoTag, tag);
            
            foreach (var chain in chains)
            {
                Factory.ChainEngine.UpdateChainTags(daoFactory, chain.Id, chain.Folder, chain.MailboxId, Tenant, User);
            }

            var daoMail = daoFactory.CreateMailDao(Tenant, User);

            // Change time_modified for index
            daoMail.SetMessagesChanged(validIds);

            return true;
        }

        public void UnsetMessagesTag(List<int> messageIds, int tagId)
        {
            List<int> validIds;

            using (var daoFactory = new DaoFactory())
            {
                var daoTagMail = daoFactory.CreateTagMailDao(Tenant, User);

                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    var daoTag = daoFactory.CreateTagDao(Tenant, User);

                    List<ChainInfo> chains;

                    GetValidForUserMessages(daoFactory, messageIds, out validIds, out chains);

                    daoTagMail.Delete(tagId, validIds);

                    var tag = daoTag.GetTag(tagId);

                    if(tag != null)
                        UpdateTagsCount(daoTagMail, daoTag, tag);

                    foreach (var chain in chains)
                    {
                        Factory.ChainEngine.UpdateChainTags(daoFactory, chain.Id, chain.Folder, chain.MailboxId, Tenant,
                            User);
                    }

                    var daoMail = daoFactory.CreateMailDao(Tenant, User);

                    // Change time_modified for index
                    daoMail.SetMessagesChanged(validIds);

                    tx.Commit();
                }
            }

            UpdateIndexerTags(validIds, UpdateAction.Remove, tagId);
        }

        public void SetConversationsTag(IEnumerable<int> messagesIds, int tagId)
        {
            var ids = messagesIds as IList<int> ?? messagesIds.ToList();

            if (!ids.Any()) return;

            List<int> validIds;

            using (var daoFactory = new DaoFactory())
            {
                var daoTagMail = daoFactory.CreateTagMailDao(Tenant, User);

                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    var daoTag = daoFactory.CreateTagDao(Tenant, User);

                    var tag = daoTag.GetTag(tagId);

                    if (tag == null)
                    {
                        tx.Rollback();
                        return;
                    }

                    var foundedChains = Factory.ChainEngine.GetChainedMessagesInfo(daoFactory, (List<int>)messagesIds);

                    if (!foundedChains.Any())
                    {
                        tx.Rollback();
                        return;
                    }

                    validIds = foundedChains.Select(r => r.Id).ToList();
                    var chains =
                        foundedChains.GroupBy(r => new {r.ChainId, r.Folder, r.MailboxId})
                            .Select(
                                r =>
                                    new ChainInfo
                                    {
                                        Id = r.Key.ChainId,
                                        Folder = r.Key.Folder,
                                        MailboxId = r.Key.MailboxId
                                    });

                    daoTagMail.SetMessagesTag(validIds, tag.Id);

                    UpdateTagsCount(daoTagMail, daoTag, tag);

                    foreach (var chain in chains)
                    {
                        Factory.ChainEngine.UpdateChainTags(daoFactory, chain.Id, chain.Folder, chain.MailboxId, Tenant,
                            User);
                    }

                    var daoMail = daoFactory.CreateMailDao(Tenant, User);

                    // Change time_modified for index
                    daoMail.SetMessagesChanged(validIds);

                    tx.Commit();
                }
            }

            UpdateIndexerTags(validIds, UpdateAction.Add, tagId);
        }

        public void UnsetConversationsTag(IEnumerable<int> messagesIds, int tagId)
        {
            var ids = messagesIds as IList<int> ?? messagesIds.ToList();

            if (!ids.Any()) return;

            List<int> validIds;

            using (var daoFactory = new DaoFactory())
            {
                var daoTagMail = daoFactory.CreateTagMailDao(Tenant, User);

                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    var daoTag = daoFactory.CreateTagDao(Tenant, User);

                    var foundedChains = Factory.ChainEngine.GetChainedMessagesInfo(daoFactory, (List<int>)messagesIds);

                    if (!foundedChains.Any())
                    {
                        tx.Rollback();
                        return;
                    }

                    validIds = foundedChains.Select(r => r.Id).ToList();

                    var chains =
                        foundedChains.GroupBy(r => new { r.ChainId, r.Folder, r.MailboxId })
                            .Select(
                                r =>
                                    new ChainInfo
                                    {
                                        Id = r.Key.ChainId,
                                        Folder = r.Key.Folder,
                                        MailboxId = r.Key.MailboxId
                                    });

                    daoTagMail.Delete(tagId, validIds);

                    var tag = daoTag.GetTag(tagId);

                    if (tag != null)
                        UpdateTagsCount(daoTagMail, daoTag, tag);

                    foreach (var chain in chains)
                    {
                        Factory.ChainEngine.UpdateChainTags(daoFactory, chain.Id, chain.Folder, chain.MailboxId, Tenant,
                            User);
                    }

                    var daoMail = daoFactory.CreateMailDao(Tenant, User);

                    // Change time_modified for index
                    daoMail.SetMessagesChanged(validIds);

                    tx.Commit();
                }
            }

            UpdateIndexerTags(validIds, UpdateAction.Remove, tagId);
        }

        private void UpdateIndexerTags(List<int> ids, UpdateAction action, int tagId)
        {
            if (!FactoryIndexer<MailWrapper>.Support || !FactoryIndexer.CheckState(false))
                return;

            if(ids == null || !ids.Any())
                return;

            var data = new MailWrapper
            {
                Tags = new List<TagWrapper>
                    {
                        new TagWrapper
                        {
                            Id = tagId
                        }
                    }
            };

            Expression<Func<Selector<MailWrapper>, Selector<MailWrapper>>> exp =
                s => s.In(m => m.Id, ids.ToArray());

            Factory.IndexEngine.Update(data, exp, action, s => s.Tags);
        }

        private static void UpdateTagsCount(ITagMailDao daoTagMail, ITagDao daoTag, Tag tag)
        {
            var count = daoTagMail.CalculateTagCount(tag.Id);

            tag.Count = count;

            daoTag.SaveTag(tag);
        }

        private void GetValidForUserMessages(IDaoFactory daoFactory, List<int> messagesIds, out List<int> validIds,
            out List<ChainInfo> chains)
        {
            var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

            var mailInfoList = daoMailInfo.GetMailInfoList(
                SimpleMessagesExp.CreateBuilder(Tenant, User)
                    .SetMessageIds(messagesIds)
                    .Build());

            validIds = new List<int>();
            chains = new List<ChainInfo>();

            foreach (var mailInfo in mailInfoList)
            {
                validIds.Add(mailInfo.Id);
                chains.Add(new ChainInfo
                {
                    Id = mailInfo.ChainId,
                    Folder = mailInfo.Folder,
                    MailboxId = mailInfo.MailboxId
                });
            }
        }
    }
}
