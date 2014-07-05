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
            if (!DbRegistry.IsDatabaseRegistered(FileConstant.DatabaseId))
            {
                DbRegistry.RegisterDatabase(FileConstant.DatabaseId, ConfigurationManager.ConnectionStrings[FileConstant.DatabaseId]);
            }

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