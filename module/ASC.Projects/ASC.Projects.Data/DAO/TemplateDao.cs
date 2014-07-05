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
            var q = Query(TemplatesTable + " p").Select(templateColumns);

            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(q).ConvertAll(ToTemplate);
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
