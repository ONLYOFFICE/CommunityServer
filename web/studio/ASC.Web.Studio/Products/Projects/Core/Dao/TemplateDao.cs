/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class TemplateDao : BaseDao, ITemplateDao
    {
        private readonly string[] templateColumns = new[] { "id", "title", "description", "create_by", "create_on" };

        public TemplateDao(string dbId, int tenant)
            : base(dbId, tenant)
        {

        }

        public List<Template> GetAll()
        {
            var q = Query(TemplatesTable + " p").Select(templateColumns).OrderBy("create_on", false);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(q).ConvertAll(ToTemplate);
            }
        }

        public int GetCount()
        {
            var q = Query(TemplatesTable + " p").SelectCount();

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<int>(q);
            }
        }

        public Template GetByID(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var query = Query(TemplatesTable + " p").Select(templateColumns).Where("p.id", id);
                return db.ExecuteList(query).ConvertAll(ToTemplate).SingleOrDefault();
            }
        }

        public Template Save(Template template)
        {
            using (var db = new DbManager(DatabaseId))
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

                template.Id = db.ExecuteScalar<int>(insert);

                return template;
            }
        }

        public void Delete(int id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(Delete(TemplatesTable).Where("id", id));
            }
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
