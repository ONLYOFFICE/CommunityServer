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
using Newtonsoft.Json.Linq;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CustomFieldDao : AbstractDao
    {
        public CustomFieldDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {

        }

        public void SaveList(List<CustomField> items)
        {
            if (items == null || items.Count == 0) return;

            using (var db = GetDb())
            using (var tx = db.BeginTransaction(true))
            {
                foreach (var customField in items)
                {
                    SetFieldValue(customField.EntityType, customField.EntityID, customField.ID, customField.Value, db);
                }

                tx.Commit();
            }

        }

        public void SetFieldValue(EntityType entityType, int entityID, int fieldID, String fieldValue)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            fieldValue = fieldValue.Trim();

            using (var db = GetDb())
            {
                SetFieldValue(entityType, entityID, fieldID, fieldValue, db);
            }
        }

        private void SetFieldValue(EntityType entityType, int entityID, int fieldID, String fieldValue, DbManager db)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            fieldValue = fieldValue.Trim();

            if (String.IsNullOrEmpty(fieldValue))
                db.ExecuteNonQuery(Delete("crm_field_value").Where(Exp.Eq("entity_id", entityID) & Exp.Eq("field_id", fieldID)));
            else
                db.ExecuteNonQuery(
                        Insert("crm_field_value")
                        .InColumnValue("entity_id", entityID)
                        .InColumnValue("value", fieldValue)
                        .InColumnValue("field_id", fieldID)
                        .InColumnValue("entity_type", (int)entityType)
                        .InColumnValue("last_modifed_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                        .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                        );
        }

        private string GetValidMask(CustomFieldType customFieldType, String mask)
        {
            var resultMask = new JObject();
            if (String.IsNullOrEmpty(mask) ||
                (customFieldType != CustomFieldType.TextField && customFieldType != CustomFieldType.TextArea && customFieldType != CustomFieldType.SelectBox))
            {
                return String.Empty;
            }

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
                        throw new ArgumentException("Mask is not valid");
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
                    throw ex;
                }
            }
            return JsonConvert.SerializeObject(resultMask);
        }

        public int CreateField(EntityType entityType, String label, CustomFieldType customFieldType, String mask)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();
            var resultMask = GetValidMask(customFieldType, mask);

            using (var db = GetDb())
            {
                var sortOrder = db.ExecuteScalar<int>(Query("crm_field_description").SelectMax("sort_order")) + 1;

                return db.ExecuteScalar<int>(
                                                  Insert("crm_field_description")
                                                  .InColumnValue("id", 0)
                                                  .InColumnValue("label", label)
                                                  .InColumnValue("type", (int)customFieldType)
                                                  .InColumnValue("mask", resultMask)
                                                  .InColumnValue("sort_order", sortOrder)
                                                  .InColumnValue("entity_type", (int)entityType)
                                                  .Identity(1, 0, true));
            }
        }

        public String GetValue(EntityType entityType, int entityID, int fieldID)
        {
            var sqlQuery = Query("crm_field_value")
                          .Select("value")
                          .Where(Exp.Eq("field_id", fieldID)
                                 & BuildEntityTypeConditions(entityType, "entity_type")
                                 & Exp.Eq("entity_id", entityID));

            using (var db = GetDb())
            {
                return db.ExecuteScalar<String>(sqlQuery);
            }
        }

        public List<Int32> GetEntityIds(EntityType entityType, int fieldID, String fieldValue)
        {
            var sqlQuery = Query("crm_field_value")
                          .Select("entity_id")
                          .Where(Exp.Eq("field_id", fieldID)
                                 & BuildEntityTypeConditions(entityType, "entity_type")
                                 & Exp.Eq("value", fieldValue));

            using (var db = GetDb())
            {
                return db.ExecuteList(sqlQuery).ConvertAll(row => Convert.ToInt32(row[0]));
            }
        }

        public bool IsExist(int id)
        {
            var q = new SqlExp(
                    string.Format(@"select exists(select 1 from crm_field_description where tenant_id = {0} and id = {1})",
                                TenantID,
                                id));

            using (var db = GetDb())
            {
                return db.ExecuteScalar<bool>(q);
            }
        }

        public int GetFieldId(EntityType entityType, String label, CustomFieldType customFieldType)
        {
            using (var db = GetDb())
            {
                var result = db.ExecuteList(GetFieldDescriptionSqlQuery(
                    Exp.Eq("type", (int)customFieldType)
                    & BuildEntityTypeConditions(entityType, "entity_type")
                    & Exp.Eq("label", label))).ConvertAll(row => ToCustomField(row));

                if (result.Count == 0) return 0;
                else return result[0].ID;
            }
        }

        public void EditItem(CustomField customField)
        {
            if (HaveRelativeLink(customField.ID))
            {
                using (var db = GetDb())
                {
                    try
                    {
                        var resultMask = "";

                        var row = db.ExecuteList(Query("crm_field_description")
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
                                    throw new ArgumentException("Mask is not valid");
                                }
                                var inm = (((JArray)maskObjNew).ToList()).Intersect(((JArray)maskObjOld).ToList()).ToList();
                                if (inm.Count == ((JArray)maskObjOld).ToList().Count)
                                {
                                    resultMask = customField.Mask;
                                }
                                else
                                { 
                                    throw new ArgumentException("Mask is not valid");
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
                        throw ex;
                    }

                    db.ExecuteNonQuery(
                       Update("crm_field_description")
                       .Set("label", customField.Label)
                       .Set("mask", customField.Mask)
                       .Where(Exp.Eq("id", customField.ID)));
                }
            }
            else
            {
                var resultMask = GetValidMask(customField.FieldType, customField.Mask);
                using (var db = GetDb())
                {
                    db.ExecuteNonQuery(
                       Update("crm_field_description")
                       .Set("label", customField.Label)
                       .Set("type", (int)customField.FieldType)
                       .Set("mask", resultMask)
                       .Where(Exp.Eq("id", customField.ID)));
                }
            }
        }

        public void ReorderFields(int[] fieldID)
        {
            using (var db = GetDb())
            {
                for (int index = 0; index < fieldID.Length; index++)
                    db.ExecuteNonQuery(Update("crm_field_description")
                                             .Set("sort_order", index)
                                             .Where(Exp.Eq("id", fieldID[index])));
            }
        }

        private bool HaveRelativeLink(int fieldID)
        {
            using (var db = GetDb())
            {
                return
                    db.ExecuteScalar<int>(
                        Query("crm_field_value").Where(Exp.Eq("field_id", fieldID)).SelectCount()) > 0;
            }
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

            using (var db = GetDb())
            {
                var queryResult = db.ExecuteList(sqlQuery);

                return JsonConvert.SerializeObject(queryResult.ConvertAll(row => row[0]));
            }
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

            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(sqlQuery);
            }
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
            using (var db = GetDb())
            {
                if (!includeEmptyFields)
                    return db.ExecuteList(sqlQuery)
                            .ConvertAll(row => ToCustomField(row)).FindAll(item =>
                            {
                                if (item.FieldType == CustomFieldType.Heading)
                                    return true;

                                return !String.IsNullOrEmpty(item.Value.Trim());

                            }).ToList();

                return db.ExecuteList(sqlQuery)
                       .ConvertAll(row => ToCustomField(row));
            }
        }

        public CustomField GetFieldDescription(int fieldID)
        {

            var sqlQuery = GetFieldDescriptionSqlQuery(null);

            sqlQuery.Where(Exp.Eq("id", fieldID));

            using (var db = GetDb())
            {
                var fields = db.ExecuteList(sqlQuery)
                           .ConvertAll(row => ToCustomField(row));

                return fields.Count == 0 ? null : fields[0];
            }
        }

        public List<CustomField> GetFieldsDescription(EntityType entityType)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException();

            SqlQuery sqlQuery = GetFieldDescriptionSqlQuery(null);

            sqlQuery.Where(BuildEntityTypeConditions(entityType, "entity_type"));

            using (var db = GetDb())
            {
                return db.ExecuteList(sqlQuery)
                     .ConvertAll(row => ToCustomField(row));
            }
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

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                db.ExecuteNonQuery(Delete("crm_field_description").Where(Exp.Eq("id", fieldID)));
                db.ExecuteNonQuery(Delete("crm_field_value").Where(Exp.Eq("field_id", fieldID)));

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