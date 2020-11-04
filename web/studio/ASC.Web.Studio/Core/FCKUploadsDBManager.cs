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


using ASC.Common.Data;
using ASC.Web.Studio.Utility;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Web.Studio.Core
{
    class FCKUploadsDBManager
    {
        private static readonly string _databaseID = "default";

        public static void SetUploadRelations(string storeDomain, string folderID, string itemID)
        {
            using (var _dbManager = DbManager.FromHttpContext(_databaseID))
            {
                _dbManager.Connection.CreateCommand(
                    @"insert into webstudio_fckuploads (TenantID, StoreDomain, FolderID, ItemID)
                                                      values (@tid, @sd, @fid, @iid)")
                    .AddParameter("tid", TenantProvider.CurrentTenantID)
                    .AddParameter("sd", storeDomain.ToLower())
                    .AddParameter("fid", folderID.ToLower())
                    .AddParameter("iid", itemID.ToLower()).ExecuteNonQuery();
            }
        }

        public static string GetFolderID(string storeDomain, string itemID)
        {
            using (var _dbManager = DbManager.FromHttpContext(_databaseID))
            {
                return _dbManager.ExecuteScalar<string>(new SqlQuery("webstudio_fckuploads").Select("FolderID")
                    .Where(Exp.Eq("TenantID", TenantProvider.CurrentTenantID) &
                           Exp.Eq("StoreDomain", storeDomain.ToLower()) &
                           Exp.Eq("ItemID", itemID.ToLower())));
            }
        }

        public static void RemoveUploadRelation(string storeDomain, string folderID, string itemID)
        {
            using (var _dbManager = DbManager.FromHttpContext(_databaseID))
            {
                _dbManager.ExecuteNonQuery(new SqlDelete("webstudio_fckuploads")
                    .Where(Exp.Eq("TenantID", TenantProvider.CurrentTenantID) &
                           Exp.Eq("StoreDomain", storeDomain.ToLower()) &
                           Exp.Eq("FolderID", folderID.ToLower()) &
                           Exp.Eq("ItemID", itemID.ToLower())));
            }
        }
    }
}
