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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.CRM.Core.Entities;
using Newtonsoft.Json;
using ASC.Web.CRM.Resources;

namespace ASC.CRM.Core.Dao
{
    public class TagDao : AbstractDao
    {
        #region Constructor

        public TagDao(int tenantID)
            : base(tenantID)
        {


        }

        #endregion

        #region Methods

        public bool IsExist(EntityType entityType, String tagName)
        {
            if (String.IsNullOrEmpty(tagName))
                throw new ArgumentNullException(tagName);

            return IsExistInDb(entityType, tagName);
        }

        private bool IsExistInDb(EntityType entityType, String tagName)
        {
            var q = new SqlQuery("crm_tag")
               .Select("1")
               .Where("tenant_id", TenantID)
               .Where("entity_type", (int)entityType)
               .Where("trim(lower(title))", tagName.Trim().ToLower())
               .SetMaxResults(1);

            return Db.ExecuteScalar<bool>(q);
        }

        private int GetTagId(EntityType entityType, String tagName)
        {
            var q = new SqlQuery("crm_tag")
               .Select("id")
               .Where("tenant_id", TenantID)
               .Where("entity_type", (int)entityType)
               .Where("trim(lower(title))", tagName.Trim().ToLower())
               .SetMaxResults(1);

            return Db.ExecuteScalar<int>(q);
        }

        public String[] GetAllTags(EntityType entityType)
        {
            return Db.ExecuteList(
                Query("crm_tag")
                .Select("title")
                .Where(Exp.Eq("entity_type", (int)entityType))
                .OrderBy("title", true)).ConvertAll(row => row[0].ToString()).ToArray();
        }

        public List<KeyValuePair<EntityType, string>> GetAllTags()
        {
            return Db.ExecuteList(
                Query("crm_tag")
                .Select("title", "entity_type")
                .OrderBy("title", true))
                .ConvertAll(row => new KeyValuePair<EntityType, string>(
                    (EntityType)Enum.Parse(typeof(EntityType), row[1].ToString(), true), 
                    row[0].ToString()));
        }

        public String GetTagsLinkCountJSON(EntityType entityType)
        {
            int[] tags = GetTagsLinkCount(entityType).ToArray();
            return JsonConvert.SerializeObject(tags);
        }

        public IEnumerable<int> GetTagsLinkCount(EntityType entityType)
        {
            var sqlQuery = new SqlQuery("crm_tag tbl_tag")
                .SelectCount("tag_id")
                .LeftOuterJoin("crm_entity_tag", Exp.EqColumns("id", "tag_id"))
                .Where(Exp.Eq("tbl_tag.entity_type", (int)entityType) & Exp.Eq("tbl_tag.tenant_id", TenantID))
                .OrderBy("title", true)
                .GroupBy("tbl_tag.id");

            var queryResult = Db.ExecuteList(sqlQuery);
            return queryResult.ConvertAll(row => Convert.ToInt32(row[0]));
        }


        public Dictionary<int, List<String>> GetEntitiesTags(EntityType entityType)
        {

            var result = new Dictionary<int, List<String>>();

            var sqlQuery =
                 new SqlQuery("crm_entity_tag")
                .Select("entity_id", "title")
                .LeftOuterJoin("crm_tag", Exp.EqColumns("id", "tag_id"))
                .Where(Exp.Eq("crm_tag.entity_type", (int)entityType) & Exp.Eq("crm_tag.tenant_id", TenantID));

            Db.ExecuteList(sqlQuery).ForEach(row =>
                                                        {
                                                            var entityID = Convert.ToInt32(row[0]);
                                                            var tagTitle = Convert.ToString(row[1]);

                                                            if (!result.ContainsKey(entityID))
                                                                result.Add(entityID, new List<String>
                                                                                        {
                                                                                        tagTitle
                                                                                        });
                                                            else
                                                                result[entityID].Add(tagTitle);

                                                        });
            return result;
        }

        public String[] GetEntityTags(EntityType entityType, int entityID)
        {

            SqlQuery sqlQuery = Query("crm_tag")
                .Select("title")
                .LeftOuterJoin("crm_entity_tag", Exp.EqColumns("id", "tag_id"))
                .Where(Exp.Eq("entity_id", entityID) & Exp.Eq("crm_tag.entity_type", (int)entityType));

            return Db.ExecuteList(sqlQuery).ConvertAll(row => Convert.ToString(row[0])).ToArray();
        }



        public String[] GetUnusedTags(EntityType entityType)
        {
            return Db.ExecuteList(Query("crm_tag")
                                    .Select("title")
                                    .LeftOuterJoin("crm_entity_tag", Exp.EqColumns("tag_id", "id"))
                                    .Where(Exp.Eq("tag_id", Exp.Empty) & Exp.Eq("crm_tag.entity_type", (int)entityType))).ConvertAll(row => Convert.ToString(row[0])).ToArray();
        }

        public bool CanDeleteTag(EntityType entityType, String tagName)
        {
            var tagID = Db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                    .Where(Exp.Eq("trim(lower(title))", tagName.Trim().ToLower()) & Exp.Eq("entity_type", (int)entityType)));

            return tagID != 0;
        }

        public void DeleteTag(EntityType entityType, String tagName)
        {
            if (!CanDeleteTag(entityType, tagName)) throw new ArgumentException();

            DeleteTagFromEntity(entityType, 0, tagName);
        }

        public void DeleteTagFromEntity(EntityType entityType, int entityID, String tagName)
        {
            var tagID = Db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                    .Where(Exp.Eq("trim(lower(title))", tagName.Trim().ToLower()) & Exp.Eq("entity_type", (int)entityType)));

            if (tagID == 0) return;

            var sqlQuery = new SqlDelete("crm_entity_tag")
                .Where(Exp.Eq("entity_type", (int)entityType) & Exp.Eq("tag_id", tagID));

            if (entityID > 0)
                sqlQuery.Where(Exp.Eq("entity_id", entityID));

            Db.ExecuteNonQuery(sqlQuery);

            if (entityID == 0)
                Db.ExecuteNonQuery(Delete("crm_tag")
                    .Where(Exp.Eq("id", tagID) & Exp.Eq("entity_type", (int)entityType)));
        }

        public void DeleteAllTagsFromEntity(EntityType entityType, int entityID)
        {
            if (entityID <= 0) return;

            var sqlQuery = new SqlDelete("crm_entity_tag")
                .Where(Exp.Eq("entity_type", (int)entityType) & Exp.Eq("entity_id", entityID));

            Db.ExecuteNonQuery(sqlQuery);
        }

        public void DeleteUnusedTags(EntityType entityType)
        {

            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            using (var tx = Db.BeginTransaction())
            {

                var sqlSubQuery = Query("crm_tag")
                    .Select("id")
                    .LeftOuterJoin("crm_entity_tag", Exp.EqColumns("tag_id", "id"))
                    .Where(Exp.Eq("crm_tag.entity_type", (int)entityType) & Exp.Eq("tag_id", Exp.Empty));

                var tagIDs = Db.ExecuteList(sqlSubQuery).ConvertAll(row => Convert.ToInt32(row[0]));

                if (tagIDs.Count > 0)
                    Db.ExecuteNonQuery(Delete("crm_tag").Where(Exp.In("id", tagIDs) & Exp.Eq("entity_type", (int)entityType)));


                tx.Commit();
            }

        }

        public int AddTag(EntityType entityType, String tagName, bool returnExisted = false)
        {
            tagName = CorrectTag(tagName);

            if (String.IsNullOrEmpty(tagName))
                throw new ArgumentNullException(CRMErrorsResource.TagNameNotSet);

            var existedTagId = GetTagId(entityType, tagName);

            if (existedTagId > 0)
            {
                if (returnExisted)
                    return existedTagId;

                throw new ArgumentException(CRMErrorsResource.TagNameBusy);
            }
            return AddTagInDb(entityType, tagName);
        }

        private int AddTagInDb(EntityType entityType, String tagName)
        {
            return Db.ExecuteScalar<int>(
                                Insert("crm_tag")
                                .InColumnValue("id", 0)
                                .InColumnValue("title", tagName)
                                .InColumnValue("entity_type", (int)entityType)
                                .Identity(1, 0, true));
        }


        public Dictionary<string, int> GetAndAddTags(EntityType entityType, String[] tags)
        {
            tags = tags.Select(CorrectTag).Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();

            var tagNamesAndIds = new Dictionary<string, int>();

            using (var tx = Db.BeginTransaction())
            {

                Db.ExecuteList(Query("crm_tag").Select("id", "title")
                        .Where(Exp.In("trim(lower(title))", tags.ToList().ConvertAll(t => t.ToLower())) & Exp.Eq("entity_type", (int)entityType)))
                        .ForEach(row =>
                        { tagNamesAndIds[row[1].ToString()] = (int)row[0]; });

                var tagsForCreate = tags.Where(t => !tagNamesAndIds.ContainsKey(t));

                foreach (var tagName in tagsForCreate)
                {
                    tagNamesAndIds.Add(tagName, AddTagInDb(entityType, tagName));
                }

                tx.Commit();
                return tagNamesAndIds;
            }
        }


        private int AddTagToEntityInDb(EntityType entityType, int entityID, String tagName)
        {
            tagName = CorrectTag(tagName);

            if (String.IsNullOrEmpty(tagName) || entityID == 0)
                throw new ArgumentException();

            var tagID = Db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                    .Where(Exp.Eq("trim(lower(title))", tagName.ToLower()) & Exp.Eq("entity_type", (int)entityType)));

            if (tagID == 0)
                tagID = AddTagInDb(entityType, tagName);

            Db.ExecuteNonQuery(new SqlInsert("crm_entity_tag", true)
                                        .InColumnValue("entity_id", entityID)
                                        .InColumnValue("entity_type", (int)entityType)
                                        .InColumnValue("tag_id", tagID));

            return tagID;
        }

        public int AddTagToEntity(EntityType entityType, int entityID, String tagName)
        {
            return AddTagToEntityInDb(entityType, entityID, tagName);
        }

        public int AddTagToContacts(int[] contactID, String tagName)
        {
            tagName = CorrectTag(tagName);

            if (String.IsNullOrEmpty(tagName) || contactID == null || contactID.Length == 0)
                throw new ArgumentException();

            using (var tx = Db.BeginTransaction())
            {
                var tagID = Db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                        .Where(Exp.Eq("trim(lower(title))", tagName.ToLower()) & Exp.Eq("entity_type", (int)EntityType.Contact)));

                if (tagID == 0)
                    tagID = AddTagInDb(EntityType.Contact, tagName);

                foreach (var id in contactID)
                {
                    Db.ExecuteNonQuery(new SqlInsert("crm_entity_tag", true)
                                            .InColumnValue("entity_id", id)
                                            .InColumnValue("entity_type", (int)EntityType.Contact)
                                            .InColumnValue("tag_id", tagID));
                }
                tx.Commit();
                return tagID;
            }
        }

        public int DeleteTagFromContacts(int[] contactID, String tagName)
        {
            if (String.IsNullOrEmpty(tagName) || contactID == null || contactID.Length == 0)
                throw new ArgumentException();

            using (var tx = Db.BeginTransaction())
            {
                var tagID = Db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                           .Where(Exp.Eq("trim(lower(title))", tagName.Trim().ToLower()) & Exp.Eq("entity_type", (int)EntityType.Contact)));

                if (tagID == 0)
                    throw new ArgumentException();


                foreach (var id in contactID)
                {
                    Db.ExecuteNonQuery(new SqlDelete("crm_entity_tag")
                                            .Where(Exp.Eq("entity_id", id) &
                                                   Exp.Eq("entity_type", (int)EntityType.Contact) &
                                                   Exp.Eq("tag_id", tagID)));

                }
                tx.Commit();
                return tagID;
            }
        }

        public void SetTagToEntity(EntityType entityType, int entityID, String[] tags)
        {
            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(new SqlDelete("crm_entity_tag")
                                                      .Where(Exp.Eq("entity_id", entityID) & Exp.Eq("entity_type", (int)entityType)));

                foreach (var tagName in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    AddTagToEntityInDb(entityType, entityID, tagName);
                }
                tx.Commit();
            }
        }

        private void AddTagToEntityInDb(EntityType entityType, int entityID, int tagID)
        {
            Db.ExecuteNonQuery(new SqlInsert("crm_entity_tag", true)
                                        .InColumnValue("entity_id", entityID)
                                        .InColumnValue("entity_type", (int)entityType)
                                        .InColumnValue("tag_id", tagID));
        }

        public void AddTagToEntity(EntityType entityType, int entityID, int[] tagIDs)
        {
            using (var tx = Db.BeginTransaction())
            {
                foreach (var tagID in tagIDs)
                {
                    AddTagToEntityInDb(entityType, entityID, tagID);
                }
                tx.Commit();
            }
        }

        private static string CorrectTag(string tag)
        {
            return tag == null
                       ? null
                       : tag.Trim()
                            .Replace("\r\n", string.Empty)
                            .Replace("\n", string.Empty)
                            .Replace("\r", string.Empty);
        }

        #endregion
    }
}
