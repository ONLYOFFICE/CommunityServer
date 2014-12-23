/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

namespace ASC.CRM.Core.Dao
{
    public class TagDao : AbstractDao
    {
        #region Constructor

        public TagDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {


        }

        #endregion

        #region Methods

        public bool IsExist(EntityType entityType, String tagName)
        {
            if (String.IsNullOrEmpty(tagName))
                throw new ArgumentNullException(tagName);

            using (var db = GetDb())
            {
                return IsExist(entityType, tagName, db);
            }
        }

        public bool IsExist(EntityType entityType, String tagName, DbManager db)
        {
            var q = new SqlQuery("crm_tag")
               .Select("1")
               .Where("tenant_id", TenantID)
               .Where("entity_type", (int)entityType)
               .Where("lower(title)", tagName.ToLower())
               .SetMaxResults(1);

            return db.ExecuteScalar<bool>(q);
        }

        public String[] GetAllTags(EntityType entityType)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(
                   Query("crm_tag")
                   .Select("title")
                   .Where(Exp.Eq("entity_type", (int)entityType))
                   .OrderBy("title", true)).ConvertAll(row => row[0].ToString()).ToArray();
            }
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

            using (var db = GetDb())
            {
                var queryResult = db.ExecuteList(sqlQuery);
                return queryResult.ConvertAll(row => Convert.ToInt32(row[0]));
            }
        }


        public Dictionary<int, List<String>> GetEntitiesTags(EntityType entityType)
        {

            var result = new Dictionary<int, List<String>>();

            var sqlQuery =
                 new SqlQuery("crm_entity_tag")
                .Select("entity_id", "title")
                .LeftOuterJoin("crm_tag", Exp.EqColumns("id", "tag_id"))
                .Where(Exp.Eq("crm_tag.entity_type", (int)entityType) & Exp.Eq("crm_tag.tenant_id", TenantID))
                .OrderBy("entity_id", true)
                .OrderBy("title", true);

            using (var db = GetDb())
            {
                db.ExecuteList(sqlQuery).ForEach(row =>
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
            }
            return result;
        }

        public String[] GetEntityTags(EntityType entityType, int entityID)
        {

            SqlQuery sqlQuery = Query("crm_tag")
                .Select("title")
                .LeftOuterJoin("crm_entity_tag", Exp.EqColumns("id", "tag_id"))
                .Where(Exp.Eq("entity_id", entityID) & Exp.Eq("crm_tag.entity_type", (int)entityType));

            using (var db = GetDb())
            {
                return db.ExecuteList(sqlQuery).ConvertAll(row => Convert.ToString(row[0])).ToArray();
            }
        }



        public String[] GetUnusedTags(EntityType entityType)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(Query("crm_tag")
                                      .Select("title")
                                      .LeftOuterJoin("crm_entity_tag", Exp.EqColumns("tag_id", "id"))
                                      .Where(Exp.Eq("tag_id", Exp.Empty) & Exp.Eq("crm_tag.entity_type", (int)entityType))).ConvertAll(row => Convert.ToString(row[0])).ToArray();
            }
        }

        public bool CanDeleteTag(EntityType entityType, String tagName)
        {
            using (var db = GetDb())
            {
                var tagID = db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                     .Where(Exp.Eq("lower(title)", tagName.ToLower()) & Exp.Eq("entity_type", (int)entityType)));

                if (tagID == 0) return false;

                var count = db.ExecuteScalar<int>(new SqlQuery("crm_entity_tag").SelectCount()
                                        .Where(Exp.Eq("tag_id", tagID)));

                return count == 0;
            }
        }

        public void DeleteTag(EntityType entityType, String tagName)
        {
            if (!CanDeleteTag(entityType, tagName)) throw new ArgumentException();

            DeleteTagFromEntity(entityType, 0, tagName);
        }

        public void DeleteTagFromEntity(EntityType entityType, int entityID, String tagName)
        {
            using (var db = GetDb())
            {
                var tagID = db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                     .Where(Exp.Eq("lower(title)", tagName.ToLower()) & Exp.Eq("entity_type", (int)entityType)));

                if (tagID == 0) return;

                var sqlQuery = new SqlDelete("crm_entity_tag")
                    .Where(Exp.Eq("entity_type", (int)entityType) & Exp.Eq("tag_id", tagID));

                if (entityID > 0)
                    sqlQuery.Where(Exp.Eq("entity_id", entityID));

                db.ExecuteNonQuery(sqlQuery);

                if (entityID == 0)
                    db.ExecuteNonQuery(Delete("crm_tag")
                        .Where(Exp.Eq("id", tagID) & Exp.Eq("entity_type", (int)entityType)));
            }
        }

        public void DeleteAllTagsFromEntity(EntityType entityType, int entityID)
        {
            if (entityID <= 0) return;
            using (var db = GetDb())
            {
                var sqlQuery = new SqlDelete("crm_entity_tag")
                    .Where(Exp.Eq("entity_type", (int)entityType) & Exp.Eq("entity_id", entityID));

                db.ExecuteNonQuery(sqlQuery);
            }
        }

        public void DeleteUnusedTags(EntityType entityType)
        {

            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {

                var sqlSubQuery = Query("crm_tag")
                    .Select("id")
                    .LeftOuterJoin("crm_entity_tag", Exp.EqColumns("tag_id", "id"))
                    .Where(Exp.Eq("crm_tag.entity_type", (int)entityType) & Exp.Eq("tag_id", Exp.Empty));

                var tagIDs = db.ExecuteList(sqlSubQuery).ConvertAll(row => Convert.ToInt32(row[0]));

                if (tagIDs.Count > 0)
                    db.ExecuteNonQuery(Delete("crm_tag").Where(Exp.In("id", tagIDs) & Exp.Eq("entity_type", (int)entityType)));


                tx.Commit();
            }

        }

        public int AddTag(EntityType entityType, String tagName)
        {
            if (String.IsNullOrEmpty(tagName))
                throw new ArgumentNullException("Tag name is empty");

            using (var db = GetDb())
            {
                if (IsExist(entityType, tagName, db)) throw new ArgumentException("Tag with this name already exists");
                return AddTag(entityType, tagName, db);
            }
        }

        public int AddTag(EntityType entityType, String tagName, DbManager db)
        {
            return db.ExecuteScalar<int>(
                                Insert("crm_tag")
                                .InColumnValue("id", 0)
                                .InColumnValue("title", tagName)
                                .InColumnValue("entity_type", (int)entityType)
                                .Identity(1, 0, true));
        }

        public int AddTagToEntity(EntityType entityType, int entityID, String tagName, DbManager db)
        {
            if (String.IsNullOrEmpty(tagName) || entityID == 0)
                throw new ArgumentException();
            tagName = tagName.Trim();

            var tagID = db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                    .Where(Exp.Eq("lower(title)", tagName.ToLower()) & Exp.Eq("entity_type", (int)entityType)));

            if (tagID == 0)
                tagID = AddTag(entityType, tagName, db);

            db.ExecuteNonQuery(new SqlInsert("crm_entity_tag", true)
                                        .InColumnValue("entity_id", entityID)
                                        .InColumnValue("entity_type", (int)entityType)
                                        .InColumnValue("tag_id", tagID));

            return tagID;
        }

        public int AddTagToEntity(EntityType entityType, int entityID, String tagName)
        {
            if (String.IsNullOrEmpty(tagName) || entityID == 0)
                throw new ArgumentException();
            tagName = tagName.Trim();

            using (var db = GetDb())
            {
                return AddTagToEntity(entityType, entityID, tagName, db);
            }
        }

        public int AddTagToContacts(int[] contactID, String tagName)
        {
            if (String.IsNullOrEmpty(tagName) || contactID == null || contactID.Length == 0)
                throw new ArgumentException();

            tagName = tagName.Trim();

            var entityType = EntityType.Contact;

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var tagID = db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                        .Where(Exp.Eq("lower(title)", tagName.ToLower()) & Exp.Eq("entity_type", (int)entityType)));

                if (tagID == 0)
                    tagID = AddTag(entityType, tagName, db);

                foreach (var id in contactID)
                {
                    db.ExecuteNonQuery(new SqlInsert("crm_entity_tag", true)
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

            tagName = tagName.Trim();

            var entityType = EntityType.Contact;

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var tagID = db.ExecuteScalar<int>(Query("crm_tag").Select("id")
                                           .Where(Exp.Eq("lower(title)", tagName.ToLower()) & Exp.Eq("entity_type", (int)entityType)));

                if (tagID == 0)
                    throw new ArgumentException();


                foreach (var id in contactID)
                {
                    db.ExecuteNonQuery(new SqlDelete("crm_entity_tag")
                                            .Where(Exp.Eq("entity_id", id) &
                                                   Exp.Eq("entity_type", (int)entityType) &
                                                   Exp.Eq("tag_id", tagID)));

                }
                tx.Commit();
                return tagID;
            }
        }

        public void SetTagToEntity(EntityType entityType, int entityID, String[] tags)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                db.ExecuteNonQuery(new SqlDelete("crm_entity_tag")
                                                      .Where(Exp.Eq("entity_id", entityID) & Exp.Eq("entity_type", (int)entityType)));

                foreach (var tagName in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    AddTagToEntity(entityType, entityID, tagName, db);
                }
                tx.Commit();
            }
        }

        #endregion
    }
}
