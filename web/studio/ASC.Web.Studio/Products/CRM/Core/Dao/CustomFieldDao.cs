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


#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using Newtonsoft.Json;
using ASC.Common.Data;
using ASC.Core;
using ASC.ElasticSearch;
using ASC.Web.CRM.Core.Search;
using Newtonsoft.Json.Linq;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CustomFieldDao : AbstractDao
    {
        public CustomFieldDao(int tenantID)
            : base(tenantID)
        {

        }

        public void SaveList(List<CustomField> items)
        {
            if (items == null || items.Count == 0) return;

            using (var tx = Db.BeginTransaction(true))
            {
                foreach (var customField in items)
                {
                    SetFieldValueInDb(customField.EntityType, customField.EntityID, customField.ID, customField.Value);
                }

                tx.Commit();
            }

        }

        public void SetFieldValue(EntityType entityType, int entityID, int fieldID, String fieldValue)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            fieldValue = fieldValue.Trim();

            SetFieldValueInDb(entityType, entityID, fieldID, fieldValue);
        }

        private void SetFieldValueInDb(EntityType entityType, int entityID, int fieldID, String fieldValue)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            fieldValue = fieldValue.Trim();

            Db.ExecuteNonQuery(Delete("crm_field_value").Where(Exp.Eq("entity_id", entityID) & Exp.Eq("entity_type", (int)entityType) & Exp.Eq("field_id", fieldID)));

            if (!String.IsNullOrEmpty(fieldValue))
            {
                var lastModifiedOn = TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow());
                var id = Db.ExecuteScalar<int>(
                        Insert("crm_field_value")
                        .InColumnValue("id", 0)
                        .InColumnValue("entity_id", entityID)
                        .InColumnValue("value", fieldValue)
                        .InColumnValue("field_id", fieldID)
                        .InColumnValue("entity_type", (int)entityType)
                        .InColumnValue("last_modifed_on", lastModifiedOn)
                        .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                        .Identity(1, 0, true)
                        );

                FactoryIndexer<FieldsWrapper>.IndexAsync(new FieldsWrapper
                {
                    Id = id,
                    EntityId = entityID,
                    EntityType = (int)entityType,
                    Value = fieldValue,
                    FieldId = fieldID,
                    LastModifiedOn = lastModifiedOn,
                    TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
                });
            }
        }

        private string GetValidMask(CustomFieldType customFieldType, String mask)
        {
            var resultMask = new JObject();

            if (customFieldType == CustomFieldType.CheckBox || customFieldType == CustomFieldType.Heading || customFieldType == CustomFieldType.Date)
                return String.Empty;

            if(String.IsNullOrEmpty(mask))
                throw new ArgumentException(CRMErrorsResource.CustomFieldMaskNotValid);

            try
            {
                var maskObj = JToken.Parse(mask);
                if (customFieldType == CustomFieldType.TextField)
                {
                    var size = maskObj.Value<int>("size");
                    if (size == 0)
                    {
                        resultMask.Add("size", 1);
                    }
                    else if (size > Global.MaxCustomFieldSize)
                    {
                        resultMask.Add("size", Global.MaxCustomFieldSize);
                    }
                    else
                    {
                        resultMask.Add("size", size);
                    }
                }
                if (customFieldType == CustomFieldType.TextArea)
                {
                    var rows = maskObj.Value<int>("rows");
                    var cols = maskObj.Value<int>("cols");

                    if (rows == 0)
                    {
                        resultMask.Add("rows", 1);
                    }
                    else if (rows > Global.MaxCustomFieldRows)
                    {
                        resultMask.Add("rows", Global.MaxCustomFieldRows);
                    }
                    else
                    {
                        resultMask.Add("rows", rows);
                    }

                    if (cols == 0)
                    {
                        resultMask.Add("cols", 1);
                    }
                    else if (cols > Global.MaxCustomFieldCols)
                    {
                        resultMask.Add("cols", Global.MaxCustomFieldCols);
                    }
                    else
                    {
                        resultMask.Add("cols", cols);
                    }
                }
                if (customFieldType == CustomFieldType.SelectBox)
                {
                    if (maskObj is JArray)
                    {
                        return mask;
                    }
                    else
                    {
                        throw new ArgumentException(CRMErrorsResource.CustomFieldMaskNotValid);
                    }
                }
            }
            catch (Exception ex)
            {
                if (customFieldType == CustomFieldType.TextField)
                {
                    resultMask.Add("size", Global.DefaultCustomFieldSize);
                }
                if (customFieldType == CustomFieldType.TextArea)
                {
                    resultMask.Add("rows", Global.DefaultCustomFieldRows);
                    resultMask.Add("cols", Global.DefaultCustomFieldCols);
                }
                if (customFieldType == CustomFieldType.SelectBox)
                {
                    _log.Error(ex);
                    throw ex;
                }
            }
            return JsonConvert.SerializeObject(resultMask);
        }

        public int CreateField(EntityType entityType, String label, CustomFieldType customFieldType, String mask)
        {
            if (!_supportedEntityType.Contains(entityType) || String.IsNullOrEmpty(label))
                throw new ArgumentException();
            var resultMask = GetValidMask(customFieldType, mask);

            var sortOrder = Db.ExecuteScalar<int>(Query("crm_field_description").SelectMax("sort_order")) + 1;

            return Db.ExecuteScalar<int>(
                                                Insert("crm_field_description")
                                                .InColumnValue("id", 0)
                                                .InColumnValue("label", label)
                                                .InColumnValue("type", (int)customFieldType)
                                                .InColumnValue("mask", resultMask)
                                                .InColumnValue("sort_order", sortOrder)
                                                .InColumnValue("entity_type", (int)entityType)
                                                .Identity(1, 0, true));
        }

        public String GetValue(EntityType entityType, int entityID, int fieldID)
        {
            var sqlQuery = Query("crm_field_value")
                          .Select("value")
                          .Where(Exp.Eq("field_id", fieldID)
                                 & BuildEntityTypeConditions(entityType, "entity_type")
                                 & Exp.Eq("entity_id", entityID));

            return Db.ExecuteScalar<String>(sqlQuery);
        }

        public List<Int32> GetEntityIds(EntityType entityType, int fieldID, String fieldValue)
        {
            var sqlQuery = Query("crm_field_value")
                          .Select("entity_id")
                          .Where(Exp.Eq("field_id", fieldID)
                                 & BuildEntityTypeConditions(entityType, "entity_type")
                                 & Exp.Eq("value", fieldValue));

            return Db.ExecuteList(sqlQuery).ConvertAll(row => Convert.ToInt32(row[0]));
        }

        public bool IsExist(int id)
        {
            return Db.ExecuteScalar<bool>("select exists(select 1 from crm_field_description where tenant_id = @tid and id = @id)",
                new { tid = TenantID, id = id });
        }

        public int GetFieldId(EntityType entityType, String label, CustomFieldType customFieldType)
        {
            var result = Db.ExecuteList(GetFieldDescriptionSqlQuery(
                Exp.Eq("type", (int)customFieldType)
                & BuildEntityTypeConditions(entityType, "entity_type")
                & Exp.Eq("label", label))).ConvertAll(row => ToCustomField(row));

            if (result.Count == 0) return 0;
            else return result[0].ID;
        }

        public void EditItem(CustomField customField)
        {
            if (String.IsNullOrEmpty(customField.Label))
                throw new ArgumentException();

            if (HaveRelativeLink(customField.ID))
            {
                try
                {
                    var resultMask = "";

                    var row = Db.ExecuteList(Query("crm_field_description")
                    .Where(Exp.Eq("id", customField.ID))
                    .Select("type", "mask")).FirstOrDefault();

                    var fieldType = (CustomFieldType)Convert.ToInt32(row[0]);
                    var oldMask = Convert.ToString(row[1]);

                    if (fieldType == CustomFieldType.SelectBox)
                    {
                        if (oldMask == customField.Mask || customField.Mask == "")
                        {
                            resultMask = oldMask;
                        }
                        else
                        {
                            var maskObjOld = JToken.Parse(oldMask);
                            var maskObjNew =JToken.Parse(customField.Mask);

                            if (!(maskObjOld is JArray && maskObjNew is JArray))
                            {
                                throw new ArgumentException(CRMErrorsResource.CustomFieldMaskNotValid);
                            }
                            var inm = (((JArray)maskObjNew).ToList()).Intersect(((JArray)maskObjOld).ToList()).ToList();
                            if (inm.Count == ((JArray)maskObjOld).ToList().Count)
                            {
                                resultMask = customField.Mask;
                            }
                            else
                            {
                                throw new ArgumentException(CRMErrorsResource.CustomFieldMaskNotValid);
                            }
                        }
                    }
                    else
                    {
                        resultMask = GetValidMask(fieldType, customField.Mask);
                    }

                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    throw ex;
                }

                Db.ExecuteNonQuery(
                    Update("crm_field_description")
                    .Set("label", customField.Label)
                    .Set("mask", customField.Mask)
                    .Where(Exp.Eq("id", customField.ID)));
            }
            else
            {
                var resultMask = GetValidMask(customField.FieldType, customField.Mask);
                Db.ExecuteNonQuery(
                    Update("crm_field_description")
                    .Set("label", customField.Label)
                    .Set("type", (int)customField.FieldType)
                    .Set("mask", resultMask)
                    .Where(Exp.Eq("id", customField.ID)));
            }
        }

        public void ReorderFields(int[] fieldID)
        {
            for (int index = 0; index < fieldID.Length; index++)
                Db.ExecuteNonQuery(Update("crm_field_description")
                                            .Set("sort_order", index)
                                            .Where(Exp.Eq("id", fieldID[index])));
        }

        private bool HaveRelativeLink(int fieldID)
        {
            return
                Db.ExecuteScalar<int>(
                    Query("crm_field_value").Where(Exp.Eq("field_id", fieldID)).SelectCount()) > 0;
        }

        public String GetContactLinkCountJSON(EntityType entityType)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            var sqlQuery = Query("crm_field_description tblFD")
                .Select("count(tblFV.field_id)")
                .LeftOuterJoin("crm_field_value tblFV", Exp.EqColumns("tblFD.id", "tblFV.field_id"))
                .OrderBy("tblFD.sort_order", true)
                .GroupBy("tblFD.id");

            sqlQuery.Where(BuildEntityTypeConditions(entityType, "tblFD.entity_type"));

            var queryResult = Db.ExecuteList(sqlQuery);

            return JsonConvert.SerializeObject(queryResult.ConvertAll(row => row[0]));
        }

        public int GetContactLinkCount(EntityType entityType, int entityID)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            var sqlQuery = Query("crm_field_description tblFD")
                .Select("count(tblFV.field_id)")
                .LeftOuterJoin("crm_field_value tblFV", Exp.EqColumns("tblFD.id", "tblFV.field_id"))
                .Where(Exp.Eq("tblFD.id", entityID))
                .OrderBy("tblFD.sort_order", true);

            sqlQuery.Where(BuildEntityTypeConditions(entityType, "tblFD.entity_type"));

            return Db.ExecuteScalar<int>(sqlQuery);
        }

        public List<CustomField> GetEnityFields(EntityType entityType, int entityID, bool includeEmptyFields)
        {
            return GetEnityFields(entityType, entityID == 0 ? null : new[] { entityID }, includeEmptyFields);
        }

        public List<CustomField> GetEnityFields(EntityType entityType, int[] entityID)
        {
            return GetEnityFields(entityType, entityID, false);
        }

        private List<CustomField> GetEnityFields(EntityType entityType, int[] entityID, bool includeEmptyFields)
        {
            // TODO: Refactoring Query!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            SqlQuery sqlQuery = Query("crm_field_description tbl_field")
                .Select("tbl_field.id",
                        "tbl_field_value.entity_id",
                        "tbl_field.label",
                        "tbl_field_value.value",
                        "tbl_field.type",
                        "tbl_field.sort_order",
                        "tbl_field.mask",
                        "tbl_field.entity_type");

            sqlQuery.Where(BuildEntityTypeConditions(entityType, "tbl_field.entity_type"));

            if (entityID != null && entityID.Length > 0)
                sqlQuery.LeftOuterJoin("crm_field_value tbl_field_value",
                                   Exp.EqColumns("tbl_field_value.field_id", "tbl_field.id") &
                                   Exp.In("tbl_field_value.entity_id", entityID))
                .OrderBy("tbl_field.sort_order", true);
            else
                sqlQuery.LeftOuterJoin("crm_field_value tbl_field_value",
                                      Exp.EqColumns("tbl_field_value.field_id", "tbl_field.id"))
               .Where(Exp.Eq("tbl_field_value.tenant_id", TenantID))
               .OrderBy("tbl_field_value.entity_id", true)
               .OrderBy("tbl_field.sort_order", true);

            if (!includeEmptyFields)
                return Db.ExecuteList(sqlQuery)
                        .ConvertAll(row => ToCustomField(row)).FindAll(item =>
                        {
                            if (item.FieldType == CustomFieldType.Heading)
                                return true;

                            return !String.IsNullOrEmpty(item.Value.Trim());

                        }).ToList();

            return Db.ExecuteList(sqlQuery)
                    .ConvertAll(row => ToCustomField(row));
        }

        public CustomField GetFieldDescription(int fieldID)
        {

            var sqlQuery = GetFieldDescriptionSqlQuery(null);

            sqlQuery.Where(Exp.Eq("id", fieldID));

            var fields = Db.ExecuteList(sqlQuery).ConvertAll(row => ToCustomField(row));

            return fields.Count == 0 ? null : fields[0];
        }

        public List<CustomField> GetFieldsDescription(EntityType entityType)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            SqlQuery sqlQuery = GetFieldDescriptionSqlQuery(null);

            sqlQuery.Where(BuildEntityTypeConditions(entityType, "entity_type"));

            return Db.ExecuteList(sqlQuery)
                .ConvertAll(row => ToCustomField(row));
        }

        private SqlQuery GetFieldDescriptionSqlQuery(Exp where)
        {
            var sqlQuery = Query("crm_field_description")
                .Select("id",
                        "-1",
                        "label",
                        "\" \"",
                        "type",
                        "sort_order",
                        "mask",
                        "entity_type")
                .OrderBy("sort_order", true);

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        private Exp BuildEntityTypeConditions(EntityType entityType, String dbFieldName)
        {
            switch (entityType)
            {
                case EntityType.Company:
                case EntityType.Person:
                    return Exp.In(dbFieldName, new[] { (int)entityType, (int)EntityType.Contact });

                default:
                    return Exp.Eq(dbFieldName, (int)entityType);

            }

        }

        public void DeleteField(int fieldID)
        {
            //if (HaveRelativeLink(fieldID))
            //    throw new ArgumentException();

            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(Delete("crm_field_description").Where(Exp.Eq("id", fieldID)));
                Db.ExecuteNonQuery(Delete("crm_field_value").Where(Exp.Eq("field_id", fieldID)));

                tx.Commit();
            }
        }

        public static CustomField ToCustomField(object[] row)
        {
            return new CustomField
            {
                ID = Convert.ToInt32(row[0]),
                EntityID = Convert.ToInt32(row[1]),
                EntityType = (EntityType)Convert.ToInt32(row[7]),
                Label = Convert.ToString(row[2]),
                Value = Convert.ToString(row[3]),
                FieldType = (CustomFieldType)Convert.ToInt32(row[4]),
                Position = Convert.ToInt32(row[5]),
                Mask = Convert.ToString(row[6])
            };
        }
    }

}