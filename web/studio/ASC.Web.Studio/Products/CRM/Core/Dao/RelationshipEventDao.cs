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
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.Files.Core;

using ASC.Web.CRM;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Search;
using ASC.Web.Files.Api;
using ASC.Web.Studio.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OrderBy = ASC.CRM.Core.Entities.OrderBy;
using File = ASC.Files.Core.File;

namespace ASC.CRM.Core.Dao
{

    public class CachedRelationshipEventDao : RelationshipEventDao
    {
        #region Import

        private readonly HttpRequestDictionary<RelationshipEvent> _contactCache = new HttpRequestDictionary<RelationshipEvent>("crm_relationshipEvent");

        #endregion

        #region Constructor

        public CachedRelationshipEventDao(int tenantID)
            : base(tenantID)
        {

        }


        #endregion

        #region Methods

        public override RelationshipEvent GetByID(int eventID)
        {
            return _contactCache.Get(eventID.ToString(), () => GetByIDBase(eventID));

        }

        private RelationshipEvent GetByIDBase(int eventID)
        {
            return base.GetByID(eventID);
        }

        private void ResetCache(int dealID)
        {
            _contactCache.Reset(dealID.ToString());
        }

        #endregion

    }

    public class RelationshipEventDao : AbstractDao
    {
        #region Members


        #endregion

        #region Constructor

        public RelationshipEventDao(int tenantID)
            : base(tenantID)
        {

        }

        #endregion

        #region Methods

        public RelationshipEvent AttachFiles(int contactID, EntityType entityType, int entityID, int[] fileIDs)
        {
            if (entityID > 0 && !_supportedEntityType.Contains(entityType))
                throw new ArgumentException();


            var relationshipEvent = new RelationshipEvent
            {
                CategoryID = (int)HistoryCategorySystem.FilesUpload,
                ContactID = contactID,
                EntityType = entityType,
                EntityID = entityID,
                Content = HistoryCategorySystem.FilesUpload.ToLocalizedString()
            };

            relationshipEvent = CreateItem(relationshipEvent);

            AttachFiles(relationshipEvent.ID, fileIDs);

            return relationshipEvent;
        }

        public void AttachFiles(int eventID, int[] fileIDs)
        {
            if (fileIDs.Length == 0) return;

            using (var dao = FilesIntegration.GetTagDao())
            {
                var tags = fileIDs.ToList().ConvertAll(fileID => new Tag("RelationshipEvent_" + eventID, TagType.System, Guid.Empty) { EntryType = FileEntryType.File, EntryId = fileID });

                dao.SaveTags(tags);
            }

            if (fileIDs.Length > 0)
                Db.ExecuteNonQuery(Update("crm_relationship_event").Set("have_files", true).Where("id", eventID));
        }

        public int GetFilesCount(int[] contactID, EntityType entityType, int entityID)
        {
            return GetFilesIDs(contactID, entityType, entityID).Length;
        }

        private object[] GetFilesIDs(int[] contactID, EntityType entityType, int entityID)
        {
            if (entityID > 0 && entityType != EntityType.Opportunity && entityType != EntityType.Case)
                throw new ArgumentException();


            var sqlQuery = Query("crm_relationship_event").Select("id");

            if (contactID != null && contactID.Length > 0)
                sqlQuery.Where(Exp.In("contact_id", contactID));

            if (entityID > 0)
                sqlQuery.Where(Exp.Eq("entity_id", entityID) & Exp.Eq("entity_type", (int)entityType));

            sqlQuery.Where(Exp.Eq("have_files", true));

            var tagNames = Db.ExecuteList(sqlQuery).ConvertAll(row => String.Format("RelationshipEvent_{0}", row[0]));
            using (var tagdao = FilesIntegration.GetTagDao())
            {
                return tagdao.GetTags(tagNames.ToArray(), TagType.System).Where(t => t.EntryType == FileEntryType.File).Select(t => t.EntryId).ToArray();
            }
        }

        public List<File> GetAllFiles(int[] contactID, EntityType entityType, int entityID)
        {
            using (var filedao = FilesIntegration.GetFileDao())
            {
                var ids = GetFilesIDs(contactID, entityType, entityID);
                var files = 0 < ids.Length ? filedao.GetFiles(ids) : new List<File>();

                files.ForEach(CRMSecurity.SetAccessTo);

                return files.ToList();
            }
        }

        public Dictionary<int, List<File>> GetFiles(int[] eventID)
        {

            if (eventID == null || eventID.Length == 0)
                throw new ArgumentException("eventID");

            using (var tagdao = FilesIntegration.GetTagDao())
            using (var filedao = FilesIntegration.GetFileDao())
            {


                var findedTags = tagdao.GetTags(eventID.Select(item => String.Concat("RelationshipEvent_", item)).ToArray(),
                    TagType.System).Where(t => t.EntryType == FileEntryType.File);

                var filesID = findedTags.Select(t => t.EntryId).ToArray();

                var files = 0 < filesID.Length ? filedao.GetFiles(filesID) : new List<File>();

                var filesTemp = new Dictionary<object, File>();

                files.ForEach(item =>
                                  {
                                      if (!filesTemp.ContainsKey(item.ID))
                                          filesTemp.Add(item.ID, item);
                                  });
                                
                return findedTags.Where(x => filesTemp.ContainsKey(x.EntryId)).GroupBy(x => x.TagName).ToDictionary(x => Convert.ToInt32(x.Key.Split(new[] { '_' })[1]),
                                                                  x => x.Select(item => filesTemp[item.EntryId]).ToList());



            }

        }

        public List<File> GetFiles(int eventID)
        {
            if (eventID == 0)
                throw new ArgumentException("eventID");

            using (var tagdao = FilesIntegration.GetTagDao())
            using (var filedao = FilesIntegration.GetFileDao())
            {
                var ids = tagdao.GetTags(String.Concat("RelationshipEvent_", eventID), TagType.System).Where(t => t.EntryType == FileEntryType.File).Select(t => t.EntryId).ToArray();
                var files = 0 < ids.Length ? filedao.GetFiles(ids) : new List<File>();

                files.ForEach(CRMSecurity.SetAccessTo);

                return files.ToList();
            }
        }

        private void RemoveAllFiles(int[] contactID, EntityType entityType, int entityID)
        {

            if (entityID > 0 && entityType != EntityType.Opportunity && entityType != EntityType.Case)
            {
                throw new ArgumentException();
            }
            
            var files = GetAllFiles(contactID, entityType, entityID);
            using (var dao = FilesIntegration.GetFileDao())
            {
                foreach (var file in files)
                {
                    dao.DeleteFile(file.ID);
                }
            }
        }

        public List<int> RemoveFile(File file)
        {
            CRMSecurity.DemandDelete(file);

            List<int> eventIDs;

            using (var tagdao = FilesIntegration.GetTagDao())
            {
                var tags = tagdao.GetTags(file.ID, FileEntryType.File, TagType.System).ToList().FindAll(tag => tag.TagName.StartsWith("RelationshipEvent_"));
                eventIDs = tags.Select(item => Convert.ToInt32(item.TagName.Split(new[] { '_' })[1])).ToList();
            }
            using (var dao = FilesIntegration.GetFileDao())
            {
                dao.DeleteFile(file.ID);
            }

            foreach (var eventID in eventIDs)
            {
                if (GetFiles(eventID).Count == 0)
                {
                    Db.ExecuteNonQuery(Update("crm_relationship_event").Set("have_files", false).Where("id", eventID));
                }
            }

            Db.ExecuteNonQuery(Update("crm_invoice").Set("file_id", 0).Where("file_id", file.ID));

            return eventIDs;
        }


        public int GetCount(int[] contactID, EntityType entityType, int entityID)
        {
            if (entityID > 0 && entityType != EntityType.Opportunity && entityType != EntityType.Case)
                throw new ArgumentException();

            var sqlQuery = Query("crm_relationship_event").SelectCount();

            contactID = contactID.Where(item => item != 0).ToArray();

            if (contactID.Length > 0)
                sqlQuery.Where(Exp.In("contact_id", contactID));

            if (entityID > 0)
                sqlQuery.Where(Exp.Eq("entity_id", entityID) & Exp.Eq("entity_type", (int)entityType));

            return Db.ExecuteScalar<int>(sqlQuery);
        }

        public RelationshipEvent CreateItem(RelationshipEvent item)
        {
            CRMSecurity.DemandCreateOrUpdate(item);

            var htmlBody = String.Empty;

            if (item.CreateOn == DateTime.MinValue)
                item.CreateOn = TenantUtil.DateTimeNow();
            item.CreateBy = ASC.Core.SecurityContext.CurrentAccount.ID;
            item.LastModifedBy = ASC.Core.SecurityContext.CurrentAccount.ID;

            if (item.CategoryID == (int)HistoryCategorySystem.MailMessage)
            {
                var jsonObj = JObject.Parse(item.Content);
                var messageId = jsonObj.Value<Int32>("message_id");

                var apiServer = new Api.ApiServer();
                var msg = apiServer.GetApiResponse(
                    String.Format("{0}mail/messages/{1}.json?id={1}&loadImages=true&needSanitize=true", SetupInfo.WebApiBaseUrl, messageId), "GET");

                if (msg == null)
                    throw new ArgumentException("Mail message cannot be found");

                var msgResponseWrapper = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(msg)));
                var msgRequestObj = msgResponseWrapper.Value<JObject>("response");
                string messageUrl;

                htmlBody = msgRequestObj.Value<String>("htmlBody");

                using (var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlBody)))
                {
                    var filePath = String.Format("folder_{0}/message_{1}.html", (messageId/1000 + 1)*1000, messageId);

                    Global.GetStore().Save("mail_messages",filePath,fileStream);

                    messageUrl = String.Format("{0}HttpHandlers/filehandler.ashx?action=mailmessage&message_id={1}", PathProvider.BaseAbsolutePath, messageId);

                }

                var msg_date_created = msgRequestObj.Value<String>("date");
                var message_id = msgRequestObj.Value<Int32>("id");
                item.Content = JsonConvert.SerializeObject(new
                     {
                         @from = msgRequestObj.Value<String>("from"),
                         to = msgRequestObj.Value<String>("to"),
                         cc = msgRequestObj.Value<String>("cc"),
                         bcc = msgRequestObj.Value<String>("bcc"),
                         subject = msgRequestObj.Value<String>("subject"),
                         important = msgRequestObj.Value<Boolean>("important"),
                         chain_id = msgRequestObj.Value<String>("chainId"),
                         is_sended = msgRequestObj.Value<Int32>("folder") != 1,
                         date_created = msg_date_created,
                         introduction = msgRequestObj.Value<String>("introduction"),
                         message_id = message_id,
                         message_url = messageUrl
                     });

                item.CreateOn = DateTime.Parse(msg_date_created, CultureInfo.InvariantCulture);

                var sqlQueryFindMailsAlready = Query("crm_relationship_event")
                                                .SelectCount()
                                                .Where("contact_id", item.ContactID)
                                                .Where(Exp.Like("content", string.Format("\"message_id\":{0},", message_id)))
                                                .Where("entity_type", (int)item.EntityType)
                                                .Where("entity_id", item.EntityID)
                                                .Where("category_id", item.CategoryID);
                if (Db.ExecuteScalar<int>(sqlQueryFindMailsAlready) > 0)
                    throw new Exception("Already exists");
            }


            item.ID = Db.ExecuteScalar<int>(
                            Insert("crm_relationship_event")
                            .InColumnValue("id", 0)
                            .InColumnValue("contact_id", item.ContactID)
                            .InColumnValue("content", item.Content)
                            .InColumnValue("create_on", TenantUtil.DateTimeToUtc(item.CreateOn))
                            .InColumnValue("create_by", item.CreateBy)
                            .InColumnValue("entity_type", (int)item.EntityType)
                            .InColumnValue("entity_id", item.EntityID)
                            .InColumnValue("category_id", item.CategoryID)
                            .InColumnValue("last_modifed_on", DateTime.UtcNow)
                            .InColumnValue("last_modifed_by", item.LastModifedBy)
                            .InColumnValue("have_files", false)
                            .Identity(1, 0, true));

            if (item.CreateOn.Kind == DateTimeKind.Utc)
                item.CreateOn = TenantUtil.DateTimeFromUtc(item.CreateOn);

            FactoryIndexer<EventsWrapper>.IndexAsync(item);
            return item;
        }

        public virtual RelationshipEvent GetByID(int eventID)
        {
              return Db.ExecuteList(GetRelationshipEventQuery(Exp.Eq("id", eventID)))
                 .ConvertAll(ToRelationshipEvent).FirstOrDefault();
        }

        public int GetAllItemsCount()
        {
            return Db.ExecuteScalar<int>(Query("crm_relationship_event").SelectCount());
        }

        public List<RelationshipEvent> GetAllItems()
        {
            return GetItems(String.Empty,
                EntityType.Any,
                0,
                Guid.Empty,
                0,
                DateTime.MinValue,
                DateTime.MinValue,
                0,
                0, null);
        }

        public List<RelationshipEvent> GetItems(
            String searchText,
            EntityType entityType,
            int entityID,
            Guid createBy,
            int categoryID,
            DateTime fromDate,
            DateTime toDate,
            int from,
            int count,
            OrderBy orderBy)
        {
            var sqlQuery = GetRelationshipEventQuery(null);

            if (entityID > 0)
                switch (entityType)
                {
                    case EntityType.Contact:
                        var isCompany = false;
                        isCompany = Db.ExecuteScalar<bool>(Query("crm_contact").Select("is_company").Where(Exp.Eq("id", entityID)));

                        if (isCompany)
                            return GetItems(searchText, EntityType.Company, entityID, createBy, categoryID, fromDate, toDate, from, count, orderBy);
                        else
                            return GetItems(searchText, EntityType.Person, entityID, createBy, categoryID, fromDate, toDate, from, count, orderBy);
                    case EntityType.Person:
                        sqlQuery.Where(Exp.Eq("contact_id", entityID));
                        break;
                    case EntityType.Company:

                        var personIDs = GetRelativeToEntity(entityID, EntityType.Person, null).ToList();

                        if (personIDs.Count == 0)
                            sqlQuery.Where(Exp.Eq("contact_id", entityID));
                        else
                        {
                            personIDs.Add(entityID);
                            sqlQuery.Where(Exp.In("contact_id", personIDs));
                        }

                        break;
                    case EntityType.Case:
                    case EntityType.Opportunity:
                        sqlQuery.Where(Exp.Eq("entity_id", entityID) &
                                       Exp.Eq("entity_type", (int)entityType));
                        break;
                }

            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Between("create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate.AddDays(1).AddMinutes(-1))));
            else if (fromDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Ge("create_on", TenantUtil.DateTimeToUtc(fromDate)));
            else if (toDate != DateTime.MinValue)
                sqlQuery.Where(Exp.Le("create_on", TenantUtil.DateTimeToUtc(toDate).AddDays(1).AddMinutes(-1)));

            if (createBy != Guid.Empty)
                sqlQuery.Where(Exp.Eq("create_by", createBy));

            if (categoryID != 0)
                sqlQuery.Where(Exp.Eq("category_id", categoryID));

            if (!String.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim();

                var keywords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                   .ToArray();

                List<int> eventsIds;
                if (!FactoryIndexer<EventsWrapper>.TrySelectIds(s => s.MatchAll(searchText), out eventsIds))
                {
                    if (keywords.Length > 0)
                        sqlQuery.Where(BuildLike(new[] { "content" }, keywords));
                }
                else
                {
                    if (eventsIds.Count == 0) return new List<RelationshipEvent>();
                    sqlQuery.Where(Exp.In("id", eventsIds));
                }
            }

            if (0 < from && from < int.MaxValue)
                sqlQuery.SetFirstResult(from);

            if (0 < count && count < int.MaxValue)
                sqlQuery.SetMaxResults(count);

            if (orderBy != null && Enum.IsDefined(typeof(RelationshipEventByType), orderBy.SortedBy))
                switch ((RelationshipEventByType)orderBy.SortedBy)
                {

                    case RelationshipEventByType.Category:
                        sqlQuery.OrderBy("category_id", orderBy.IsAsc);
                        break;
                    case RelationshipEventByType.Content:
                        sqlQuery.OrderBy("content", orderBy.IsAsc);
                        break;
                    case RelationshipEventByType.CreateBy:
                        sqlQuery.OrderBy("create_by", orderBy.IsAsc);
                        break;
                    case RelationshipEventByType.Created:
                        sqlQuery.OrderBy("create_on", orderBy.IsAsc);
                        break;
                }
            else
                sqlQuery.OrderBy("create_on", false);

            return Db.ExecuteList(sqlQuery)
                .ConvertAll(row => ToRelationshipEvent(row));
        }


        private static RelationshipEvent ToRelationshipEvent(object[] row)
        {
            return new RelationshipEvent
                       {

                           ID = Convert.ToInt32(row[0]),
                           ContactID = Convert.ToInt32(row[1]),
                           Content = Convert.ToString(row[2]),
                           EntityID = Convert.ToInt32(row[3]),
                           EntityType = (EntityType)Convert.ToInt32(row[4]),
                           CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[5])),
                           CreateBy = ToGuid(row[6]),
                           CategoryID = Convert.ToInt32(row[7])
                       };
        }

        private SqlQuery GetRelationshipEventQuery(Exp where)
        {
            SqlQuery sqlQuery = Query("crm_relationship_event")
               .Select("id",
                       "contact_id",
                       "content",
                       "entity_id",
                       "entity_type",
                       "create_on",
                       "create_by",
                       "category_id"
                  );

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        public void DeleteItem(int id)
        {
            var item = GetByID(id);
            if (item == null) throw new ArgumentException();
            
            DeleteItem(item);
        }

        public void DeleteItem(RelationshipEvent item)
        {
            CRMSecurity.DemandDelete(item);

            var relativeFiles = GetFiles(item.ID);

            var nowFilesEditing = relativeFiles.Where(file => (file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing);

            if (nowFilesEditing.Count() != 0)
                throw new ArgumentException();

            relativeFiles.ForEach(f => RemoveFile(f));

            Db.ExecuteNonQuery(Delete("crm_relationship_event").Where(Exp.Eq("id", item.ID)));

            FactoryIndexer<EventsWrapper>.DeleteAsync(item);
        }

        #endregion

        [DataContract]
        internal class CrmHistoryContent
        {
            [DataMember]
            public string to;
            [DataMember]
            public string from;
            [DataMember]
            public string cc;
            [DataMember]
            public string bcc;
            [DataMember]
            public string subject;
            [DataMember]
            public bool important;
            [DataMember]
            public string chain_id;
            [DataMember]
            public bool is_sended;
            [DataMember]
            public string date_created;
            [DataMember]
            public string introduction;
            [DataMember]
            public long message_id;
        }

        private static string GetHistoryContentJson(JObject apiResponse)
        {
            var content_struct = new CrmHistoryContent
                {
                    @from = apiResponse.Value<String>("from"),
                    to = apiResponse.Value<String>("to"),
                    cc = apiResponse.Value<String>("cc"),
                    bcc = apiResponse.Value<String>("bcc"),
                    subject = apiResponse.Value<String>("subject"),
                    important = apiResponse.Value<Boolean>("important"),
                    chain_id = apiResponse.Value<String>("chainId"),
                    is_sended = apiResponse.Value<Int32>("folder") == 1,
                    date_created = apiResponse.Value<String>("date"),
                    introduction = apiResponse.Value<String>("introduction"),
                    message_id = apiResponse.Value<Int32>("id")
                };

            var serializer = new DataContractJsonSerializer(typeof(CrmHistoryContent));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, content_struct);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
