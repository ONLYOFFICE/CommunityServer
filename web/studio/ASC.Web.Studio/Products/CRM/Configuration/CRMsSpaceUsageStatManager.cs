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
using System.Configuration;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Files.Core;
using ASC.Web.Core;
using System.Web;
using ASC.Web.Studio.Utility;

namespace ASC.Web.CRM.Configuration
{
    public class CRMSpaceUsageStatManager : SpaceUsageStatManager
    {        
        public override List<UsageSpaceStatItem> GetStatData()
        {
            using (var filedb = new DbManager(FileConstant.DatabaseId))
            {
                var q = new SqlQuery("files_file f")
                    .Select("b.right_node")
                    .SelectSum("f.content_length")
                    .InnerJoin("files_folder_tree t", Exp.EqColumns("f.folder_id", "t.folder_id"))
                    .InnerJoin("files_bunch_objects b", Exp.EqColumns("t.parent_id", "b.left_node"))
                    .Where("b.tenant_id", TenantProvider.CurrentTenantID)
                    .Where(Exp.Like("b.right_node", "crm/crm_common/", SqlLike.StartWith))
                    .GroupBy(1);

                return filedb.ExecuteList(q)
                    .Select(r => new UsageSpaceStatItem
                        {

                            Name = Resources.CRMCommonResource.WholeCRMModule,
                            SpaceUsage = Convert.ToInt64(r[1]),
                            Url = VirtualPathUtility.ToAbsolute(PathProvider.StartURL())
                        })
                    .ToList();
            }        
        }
    }
}
