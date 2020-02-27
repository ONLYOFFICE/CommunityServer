/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


#region Import

using System;
using System.Collections.Generic;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CachedTaskTemplateContainerDao : TaskTemplateContainerDao
    {
        #region Constructor

        public CachedTaskTemplateContainerDao(int tenantID)
            : base(tenantID)
        {


        }

        #endregion

    }

    public class CachedTaskTemplateDao : TaskTemplateDao
    {

        #region Constructor

        public CachedTaskTemplateDao(int tenantID)
            : base(tenantID)
        {


        }

        #endregion
    }

    public class TaskTemplateContainerDao : AbstractDao
    {
        #region Constructor

        public TaskTemplateContainerDao(int tenantID)
            : base(tenantID)
        {


        }

        #endregion

        #region Methods

        public virtual int SaveOrUpdate(TaskTemplateContainer item)
        {
            if (item.ID == 0 && Db.ExecuteScalar<int>(Query("crm_task_template_container").SelectCount().Where(Exp.Eq("id", item.ID))) == 0)
            {

                item.ID = Db.ExecuteScalar<int>(
                                    Insert("crm_task_template_container")
                                    .InColumnValue("id", 0)
                                    .InColumnValue("title", item.Title)
                                    .InColumnValue("entity_type", (int)item.EntityType)
                                    .InColumnValue("create_on", DateTime.UtcNow)
                                    .InColumnValue("create_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                                    .InColumnValue("last_modifed_on", DateTime.UtcNow)
                                    .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                                    .Identity(1, 0, true));

            }
            else
            {

                Db.ExecuteScalar<int>(
                    Update("crm_task_template_container")
                        .Set("title", item.Title)
                        .Set("entity_type", (int)item.EntityType)
                        .Set("last_modifed_on", DateTime.UtcNow)
                        .Set("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                        .Where(Exp.Eq("id", item.ID)));



            }


            return item.ID;
        }

        public virtual void Delete(int id)
        {

            if (id <= 0)
                throw new ArgumentException();

            Db.ExecuteNonQuery(Delete("crm_task_template_container").Where("id", id));
        }

        public virtual TaskTemplateContainer GetByID(int id)
        {

            if (id <= 0)
                throw new ArgumentException();

            var result = Db.ExecuteList(GetQuery(null).Where(Exp.Eq("id", id))).ConvertAll(row => ToObject(row));

            if (result.Count == 0) return null;

            return result[0];
        }

        public virtual List<TaskTemplateContainer> GetItems(EntityType entityType)
        {
            if (!_supportedEntityType.Contains(entityType))
                throw new ArgumentException("", entityType.ToString());

                return Db.ExecuteList(GetQuery(Exp.Eq("entity_type", (int)entityType)))
                                                  .ConvertAll(row => ToObject(row));
        }

        #endregion

        protected SqlQuery GetQuery(Exp where)
        {

            var sqlQuery = Query("crm_task_template_container")
                          .Select("id",
                                  "title",
                                  "entity_type");

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        protected TaskTemplateContainer ToObject(object[] row)
        {
            return new TaskTemplateContainer
                       {
                           ID = Convert.ToInt32(row[0]),
                           Title = Convert.ToString(row[1]),
                           EntityType = (EntityType)Convert.ToInt32(row[2])
                       };
        }
    }

    public class TaskTemplateDao : AbstractDao
    {
        #region Constructor

        public TaskTemplateDao(int tenantID)
            : base(tenantID)
        {

        }

        #endregion

        #region Methods

        public int SaveOrUpdate(TaskTemplate item)
        {
            if (item.ID == 0 && Db.ExecuteScalar<int>(Query("crm_task_template").SelectCount().Where(Exp.Eq("id", item.ID))) == 0)
            {

                item.ID = Db.ExecuteScalar<int>(
                                    Insert("crm_task_template")
                                    .InColumnValue("id", 0)
                                    .InColumnValue("title", item.Title)
                                    .InColumnValue("category_id", item.CategoryID)
                                    .InColumnValue("description", item.Description)
                                    .InColumnValue("responsible_id", item.ResponsibleID)
                                    .InColumnValue("is_notify", item.isNotify)
                                    .InColumnValue("offset", item.Offset.Ticks)
                                    .InColumnValue("deadLine_is_fixed", item.DeadLineIsFixed)
                                    .InColumnValue("container_id", item.ContainerID)
                                    .InColumnValue("create_on", DateTime.UtcNow)
                                    .InColumnValue("create_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                                    .InColumnValue("last_modifed_on", DateTime.UtcNow)
                                    .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                                    .Identity(1, 0, true));

            }
            else
            {

                Db.ExecuteNonQuery(
                    Update("crm_task_template")
                        .Set("title", item.Title)
                        .Set("category_id", item.CategoryID)
                        .Set("description", item.Description)
                        .Set("responsible_id", item.ResponsibleID)
                        .Set("is_notify", item.isNotify)
                        .Set("offset", item.Offset.Ticks)
                        .Set("deadLine_is_fixed", item.DeadLineIsFixed)
                        .Set("container_id", item.ContainerID)
                        .Set("last_modifed_on", DateTime.UtcNow)
                        .Set("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                        .Where("id", item.ID));

            }

            return item.ID;
        }

        public TaskTemplate GetNext(int taskID)
        {
            using (var tx = Db.BeginTransaction())
            {
                var sqlResult = Db.ExecuteList(
                     Query("crm_task_template_task tblTTT")
                     .Select("tblTT.container_id")
                     .Select("tblTT.sort_order")
                     .LeftOuterJoin("crm_task_template tblTT", Exp.EqColumns("tblTT.tenant_id", "tblTTT.tenant_id") & Exp.EqColumns("tblTT.id", "tblTTT.task_template_id"))
                     .Where(Exp.Eq("tblTTT.task_id", taskID) & Exp.Eq("tblTT.tenant_id", TenantID)));

                if (sqlResult.Count == 0) return null;

                var result = Db.ExecuteList(GetQuery(Exp.Eq("container_id", sqlResult[0][0]) &
                                                Exp.Gt("sort_order", sqlResult[0][1]) &
                                                Exp.Eq("deadLine_is_fixed", false)).SetMaxResults(1)).ConvertAll(
                                                    row => ToObject(row));

                Db.ExecuteNonQuery(Delete("crm_task_template_task").Where(Exp.Eq("task_id", taskID)));

                tx.Commit();

                if (result.Count == 0) return null;

                return result[0];
            }
        }

        public List<TaskTemplate> GetAll()
        {
            return Db.ExecuteList(GetQuery(null))
                .ConvertAll(row => ToObject(row));
        }

        public List<TaskTemplate> GetList(int containerID)
        {
            if (containerID <= 0)
                throw new NotImplementedException();

            return Db.ExecuteList(GetQuery(Exp.Eq("container_id", containerID)))
                .ConvertAll(row => ToObject(row));
        }

        public virtual TaskTemplate GetByID(int id)
        {
            if (id <= 0)
                throw new NotImplementedException();

            var items = Db.ExecuteList(GetQuery(Exp.Eq("id", id))).ConvertAll(row => ToObject(row));

            if (items.Count == 0)
                return null;

            return items[0];
        }

        public virtual void Delete(int id)
        {
            if (id <= 0)
                throw new NotImplementedException();

            Db.ExecuteNonQuery(Delete("crm_task_template").Where("id", id));
        }

        protected TaskTemplate ToObject(object[] row)
        {
            return new TaskTemplate
                       {
                           ID = Convert.ToInt32(row[0]),
                           Title = Convert.ToString(row[1]),
                           CategoryID = Convert.ToInt32(row[2]),
                           Description = Convert.ToString(row[3]),
                           ResponsibleID = ToGuid(row[4]),
                           isNotify = Convert.ToBoolean(row[5]),
                           Offset = TimeSpan.FromTicks((long)row[6]),
                           DeadLineIsFixed = Convert.ToBoolean(row[7]),
                           ContainerID = Convert.ToInt32(row[8]),
                           CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[9])),
                           CreateBy = ToGuid(row[10])
                       };
        }

        protected SqlQuery GetQuery(Exp where)
        {
            var sqlQuery = Query("crm_task_template")
                          .Select("id",
                                  "title",
                                  "category_id",
                                  "description",
                                  "responsible_id",
                                  "is_notify",
                                  "offset",
                                  "deadLine_is_fixed",
                                  "container_id",
                                  "create_on",
                                  "create_by"
                                  );

            if (where != null)
                sqlQuery.Where(where);

            sqlQuery.OrderBy("sort_order", true);

            return sqlQuery;
        }

        #endregion

    }

}