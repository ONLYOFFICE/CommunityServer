/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Web.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Files
{
    public class FilesSpaceUsageStatManager : SpaceUsageStatManager, IUserSpaceUsage
    {
        public override List<UsageSpaceStatItem> GetStatData()
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                var myFiles = new SqlQuery("files_file f")
                    .Select("f.create_by")
                    .Select("sum(f.content_length) as size")
                    .InnerJoin("files_folder_tree t", Exp.EqColumns("f.folder_id", "t.folder_id"))
                    .InnerJoin("files_bunch_objects b", Exp.EqColumns("f.tenant_id", "b.tenant_id") & Exp.EqColumns("CONVERT(t.parent_id USING utf8)", "b.left_node"))
                    .Where("b.tenant_id", TenantProvider.CurrentTenantID)
                    .Where(Exp.Like("b.right_node", "files/my/", SqlLike.StartWith) | Exp.Like("b.right_node", "files/trash/", SqlLike.StartWith))
                    .GroupBy(1);

                var commonFiles = new SqlQuery("files_file f")
                    .Select("'" + Constants.LostUser.ID + "'")
                    .Select("sum(f.content_length) as size")
                    .InnerJoin("files_folder_tree t", Exp.EqColumns("f.folder_id", "t.folder_id"))
                    .InnerJoin("files_bunch_objects b", Exp.EqColumns("f.tenant_id", "b.tenant_id") & Exp.EqColumns("CONVERT(t.parent_id USING utf8)", "b.left_node"))
                    .Where("b.tenant_id", TenantProvider.CurrentTenantID)
                    .Where("b.right_node", "files/common/");

                var query = myFiles.UnionAll(commonFiles);

                return db.ExecuteList(query)
                         .GroupBy(r => CoreContext.UserManager.GetUsers(new Guid(Convert.ToString(r[0]))),
                                  r => Convert.ToInt64(r[1]),
                                  (user, items) =>
                                  {
                                      var item = new UsageSpaceStatItem { SpaceUsage = items.Sum() };
                                      if (user.Equals(Constants.LostUser))
                                      {
                                          item.Name = FilesUCResource.CorporateFiles;
                                          item.ImgUrl = PathProvider.GetImagePath("corporatefiles_big.svg");
                                          item.Url = PathProvider.GetFolderUrl(Global.FolderCommon);
                                      }
                                      else
                                      {
                                          item.Name = user.DisplayUserName(false);
                                          item.ImgUrl = user.GetSmallPhotoURL();
                                          item.Url = user.GetUserProfilePageURL();
                                          item.Disabled = user.Status == EmployeeStatus.Terminated;
                                      }
                                      return item;
                                  })
                         .OrderByDescending(i => i.SpaceUsage)
                         .ToList();
            }
        }

        public long GetUserSpaceUsage(Guid userId)
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                var query = new SqlQuery("files_file f")
                    .Select("sum(f.content_length) as size")
                    .InnerJoin("files_folder_tree t", Exp.EqColumns("f.folder_id", "t.folder_id"))
                    .InnerJoin("files_bunch_objects b", Exp.EqColumns("f.tenant_id", "b.tenant_id") & Exp.EqColumns("CONVERT(t.parent_id USING utf8)", "b.left_node"))
                    .Where("b.tenant_id", TenantProvider.CurrentTenantID)
                    .Where(Exp.Like("b.right_node", "files/my/"+ userId, SqlLike.StartWith) | Exp.Like("b.right_node", "files/trash/"+ userId, SqlLike.StartWith));

                return db.ExecuteScalar<long>(query);
            }
        }

        public void RecalculateUserQuota(int TenantId, Guid userId)
        {
            CoreContext.TenantManager.SetCurrentTenant(TenantId);

            var size = GetUserSpaceUsage(userId);

            CoreContext.TenantManager.SetTenantQuotaRow(
                new TenantQuotaRow { 
                    Tenant = TenantId, 
                    Path = $"/{FileConstant.ModuleId}/", 
                    Counter = size, 
                    Tag = WebItemManager.DocumentsProductID.ToString(), 
                    UserId = userId
                },
               false);
        }
    }
}