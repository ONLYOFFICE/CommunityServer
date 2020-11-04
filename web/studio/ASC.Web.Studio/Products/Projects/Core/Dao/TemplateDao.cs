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
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class TemplateDao : BaseDao, ITemplateDao
    {
        private readonly string[] templateColumns = new[] { "id", "title", "description", "create_by", "create_on" };
        private readonly Converter<object[], Template> converter;

        public TemplateDao(int tenant) : base(tenant)
        {
            converter = ToTemplate;
        }

        public List<Template> GetAll()
        {
            var q = Query(TemplatesTable + " p").Select(templateColumns).OrderBy("create_on", false);

            return Db.ExecuteList(q).ConvertAll(converter);
        }

        public int GetCount()
        {
            var q = Query(TemplatesTable + " p").SelectCount();

            return Db.ExecuteScalar<int>(q);
        }

        public Template GetByID(int id)
        {
            var query = Query(TemplatesTable + " p").Select(templateColumns).Where("p.id", id);
            return Db.ExecuteList(query).ConvertAll(converter).SingleOrDefault();
        }

        public Template Save(Template template)
        {
            var insert = Insert(TemplatesTable)
                .InColumnValue("id", template.Id)
                .InColumnValue("title", template.Title)
                .InColumnValue("description", template.Description)
                .InColumnValue("create_by", template.CreateBy.ToString())
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(template.CreateOn))
                .InColumnValue("last_modified_by", template.LastModifiedBy.ToString())
                .InColumnValue("last_modified_on", TenantUtil.DateTimeToUtc(template.LastModifiedOn))
                .Identity(1, 0, true);

            template.Id = Db.ExecuteScalar<int>(insert);

            return template;
        }

        public void Delete(int id)
        {
            Db.ExecuteNonQuery(Delete(TemplatesTable).Where("id", id));
        }

        private static Template ToTemplate(IList<object> r)
        {
            return new Template
            {
                Id = Convert.ToInt32(r[0]),
                Title = (string)r[1],
                Description = (string)r[2],
                CreateBy = new Guid((string)r[3]),
                CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[4]))
            };
        }
    }
}
