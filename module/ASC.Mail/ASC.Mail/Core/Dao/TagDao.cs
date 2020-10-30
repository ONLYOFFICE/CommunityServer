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
using ASC.CRM.Core;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class TagDao : BaseDao, ITagDao
    {
        protected static ITable table = new MailTableFactory().Create<TagTable>();

        protected string CurrentUserId { get; private set; }

        public TagDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public Tag GetTag(int id)
        {
            if (id < 0)
                return GetCrmTag(id);

            var query = Query()
                .Where(TagTable.Columns.Tenant, Tenant)
                .Where(TagTable.Columns.User, CurrentUserId)
                .Where(TagTable.Columns.Id, id);

            return Db.ExecuteList(query)
                .ConvertAll(ToTag)
                .SingleOrDefault();
        }

        public Tag GetCrmTag(int id)
        {
            var crmTagId = id < 0 ? -id : id;

            var query = new SqlQuery(CrmTagTable.TABLE_NAME)
                   .Select(CrmTagTable.Columns.Id, CrmTagTable.Columns.Title)
                   .Where(CrmTagTable.Columns.Tenant, Tenant)
                   .Where(CrmTagTable.Columns.EntityType, (int)EntityType.Contact)
                   .Where(CrmTagTable.Columns.Id, crmTagId);

            return Db.ExecuteList(query)
                .ConvertAll(ToCrmTag)
                .SingleOrDefault();
        }

        public Tag GetTag(string name)
        {
            var query = Query()
                .Where(TagTable.Columns.Tenant, Tenant)
                .Where(TagTable.Columns.User, CurrentUserId)
                .Where(TagTable.Columns.TagName, name);

            return Db.ExecuteList(query)
                .ConvertAll(ToTag)
                .SingleOrDefault();
        }

        public List<Tag> GetTags()
        {
            var query = Query()
                .Where(TagTable.Columns.Tenant, Tenant)
                .Where(TagTable.Columns.User, CurrentUserId);

            return Db.ExecuteList(query)
                .ConvertAll(ToTag);
        }

        public List<Tag> GetCrmTags()
        {
            var query = new SqlQuery(CrmTagTable.TABLE_NAME)
                   .Select(CrmTagTable.Columns.Id, CrmTagTable.Columns.Title)
                   .Where(CrmTagTable.Columns.Tenant, Tenant)
                   .Where(CrmTagTable.Columns.EntityType, (int)EntityType.Contact);

            return Db.ExecuteList(query)
                .ConvertAll(ToCrmTag);
        }

        public List<Tag> GetCrmTags(List<int> contactIds)
        {
            const string cet = "cet";
            const string ct = "ct";

            var query = new SqlQuery(CrmEntityTagTable.TABLE_NAME.Alias(cet))
                .InnerJoin(CrmTagTable.TABLE_NAME.Alias(ct),
                    Exp.EqColumns(CrmEntityTagTable.Columns.TagId.Prefix(cet), CrmTagTable.Columns.Id.Prefix(ct)))
                .Distinct()
                .Select(CrmTagTable.Columns.Id.Prefix(ct), CrmTagTable.Columns.Title.Prefix(ct))
                .Where(CrmTagTable.Columns.Tenant.Prefix(ct), Tenant)
                .Where(CrmEntityTagTable.Columns.EntityType.Prefix(cet), (int) EntityType.Contact)
                .Where(Exp.In(CrmEntityTagTable.Columns.EntityId.Prefix(cet), contactIds));

            return Db.ExecuteList(query)
                .ConvertAll(ToCrmTag);
        }

        public int SaveTag(Tag tag)
        {
            var query = new SqlInsert(TagTable.TABLE_NAME, true)
                .InColumnValue(TagTable.Columns.Id, tag.Id)
                .InColumnValue(TagTable.Columns.Tenant, tag.Tenant)
                .InColumnValue(TagTable.Columns.User, tag.User)
                .InColumnValue(TagTable.Columns.TagName, tag.TagName)
                .InColumnValue(TagTable.Columns.Style, tag.Style)
                .InColumnValue(TagTable.Columns.Addresses, tag.Addresses)
                .InColumnValue(TagTable.Columns.Count, tag.Count)
                .InColumnValue(TagTable.Columns.CrmId, tag.CrmId)
                .Identity(0, 0, true);

            return Db.ExecuteScalar<int>(query);
        }

        public int DeleteTag(int id)
        {
            var query = new SqlDelete(TagTable.TABLE_NAME)
                .Where(TagTable.Columns.Tenant, Tenant)
                .Where(TagTable.Columns.User, CurrentUserId)
                .Where(TagTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        public int DeleteTags(List<int> tagIds)
        {
            var query = new SqlDelete(TagTable.TABLE_NAME)
                .Where(TagTable.Columns.Tenant, Tenant)
                .Where(TagTable.Columns.User, CurrentUserId)
                .Where(Exp.In(TagTable.Columns.Id, tagIds));

            return Db.ExecuteNonQuery(query);
        }

        protected Tag ToTag(object[] r)
        {
            var f = new Tag
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                User = Convert.ToString(r[2]),
                TagName = Convert.ToString(r[3]),
                Style = Convert.ToString(r[4]),
                Addresses = Convert.ToString(r[5]),
                Count = Convert.ToInt32(r[6]),
                CrmId = Convert.ToInt32(r[7])
            };

            return f;
        }

        protected Tag ToCrmTag(object[] r)
        {
            var f = new Tag
            {
                Id = -Convert.ToInt32(r[0]),
                TagName = Convert.ToString(r[1]),
                Tenant = Tenant,
                User = CurrentUserId,
                Style = "",
                Addresses = "",
                Count = 0,
                CrmId = 0
            };

            return f;
        }
    }
}