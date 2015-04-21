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
using System.Configuration;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Web.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Files
{
    public class FilesSpaceUsageStatManager : SpaceUsageStatManager
    {
        public override List<UsageSpaceStatItem> GetStatData()
        {
            using (var db = new DbManager(FileConstant.DatabaseId))
            {
                var myFiles = new SqlQuery("files_file f")
                    .Select("f.create_by")
                    .Select("sum(f.content_length) as size")
                    .InnerJoin("files_folder_tree t", Exp.EqColumns("f.folder_id", "t.folder_id"))
                    .InnerJoin("files_bunch_objects b", Exp.EqColumns("f.tenant_id", "b.tenant_id") & Exp.EqColumns("t.parent_id", "b.left_node"))
                    .Where("b.tenant_id", TenantProvider.CurrentTenantID)
                    .Where(Exp.Like("b.right_node", "files/my/", SqlLike.StartWith) | Exp.Like("b.right_node", "files/trash/", SqlLike.StartWith))
                    .GroupBy(1);

                var commonFiles = new SqlQuery("files_file f")
                    .Select("'" + Constants.LostUser.ID + "'")
                    .Select("sum(f.content_length) as size")
                    .InnerJoin("files_folder_tree t", Exp.EqColumns("f.folder_id", "t.folder_id"))
                    .InnerJoin("files_bunch_objects b", Exp.EqColumns("f.tenant_id", "b.tenant_id") & Exp.EqColumns("t.parent_id", "b.left_node"))
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
                                          item.ImgUrl = PathProvider.GetImagePath("corporatefiles_big.png");
                                          item.Url = PathProvider.GetFolderUrl(Global.FolderCommon);
                                      }
                                      else
                                      {
                                          item.Name = user.DisplayUserName(false);
                                          item.ImgUrl = user.GetSmallPhotoURL();
                                          item.Url = user.GetUserProfilePageURL();
                                      }
                                      return item;
                                  })
                         .OrderByDescending(i => i.SpaceUsage)
                         .ToList();
            }
        }
    }
}