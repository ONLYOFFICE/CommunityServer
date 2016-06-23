/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Files.Core;
using ASC.Web.Core;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Web.Projects.Classes;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Configuration
{
    public class ProjectsSpaceUsageStatManager : SpaceUsageStatManager
    {
        public override List<UsageSpaceStatItem> GetStatData()
        {
            using (var filedb = new DbManager(FileConstant.DatabaseId))
            using (var projdb = new DbManager(Global.DbID))
            {
                var q = new SqlQuery("files_file f")
                    .Select("b.right_node")
                    .SelectSum("f.content_length")
                    .InnerJoin("files_folder_tree t", Exp.EqColumns("f.folder_id", "t.folder_id"))
                    .InnerJoin("files_bunch_objects b", Exp.EqColumns("t.parent_id", "b.left_node"))
                    .Where("b.tenant_id", TenantProvider.CurrentTenantID)
                    .Where(Exp.Like("b.right_node", "projects/project/", SqlLike.StartWith))
                    .GroupBy(1);

                var sizes = filedb.ExecuteList(q)
                    .Select(r => new {ProjectId = Convert.ToInt32(((string) r[0]).Substring(17)), Size = Convert.ToInt64(r[1])})
                    .GroupBy(r => r.ProjectId)
                    .ToDictionary(g => g.Key, g => g.Sum(a => a.Size));

                q = new SqlQuery("projects_projects")
                    .Select("id", "title")
                    .Where("tenant_id", TenantProvider.CurrentTenantID)
                    .Where(Exp.In("id", sizes.Keys));

                return projdb.ExecuteList(q)
                    .Select(r => new UsageSpaceStatItem
                        {
                            Name = Convert.ToString(r[1]),
                            SpaceUsage = sizes[Convert.ToInt32(r[0])],
                            Url = String.Concat(PathProvider.BaseAbsolutePath, "projects.aspx?prjID=" + Convert.ToInt32(r[0]))
                        })
                    .OrderByDescending(i => i.SpaceUsage)
                    .ToList();
            }
        }
    }
}