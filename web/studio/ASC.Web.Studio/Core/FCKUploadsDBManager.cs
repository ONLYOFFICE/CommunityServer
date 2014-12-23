/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
        private static readonly string _databaseID = "webstudio";

        private static DbManager _dbManager
        {
            get
            {
                return DbManager.FromHttpContext(_databaseID);
            }
        }

        public static void SetUploadRelations(string storeDomain, string folderID, string itemID)
        {
            _dbManager.Connection.CreateCommand(@"insert into webstudio_fckuploads (TenantID, StoreDomain, FolderID, ItemID)
                                                      values (@tid, @sd, @fid, @iid)")
                                                            .AddParameter("tid", TenantProvider.CurrentTenantID)
                                                            .AddParameter("sd", storeDomain.ToLower())
                                                            .AddParameter("fid", folderID.ToLower())
                                                            .AddParameter("iid", itemID.ToLower()).ExecuteNonQuery();
        }

        public static string GetFolderID(string storeDomain, string itemID)
        { 
            return _dbManager.ExecuteScalar<string>(new SqlQuery("webstudio_fckuploads").Select("FolderID" )
                                            .Where(Exp.Eq("TenantID", TenantProvider.CurrentTenantID) &
                                                   Exp.Eq("StoreDomain", storeDomain.ToLower()) &
                                                   Exp.Eq("ItemID", itemID.ToLower())));
        }

        public static void RemoveUploadRelation(string storeDomain, string folderID, string itemID)
        { 
            _dbManager.ExecuteNonQuery(new SqlDelete("webstudio_fckuploads")
                                            .Where(Exp.Eq("TenantID", TenantProvider.CurrentTenantID) &
                                                   Exp.Eq("StoreDomain", storeDomain.ToLower()) &
                                                   Exp.Eq("FolderID", folderID.ToLower()) &
                                                   Exp.Eq("ItemID", itemID.ToLower())));
        }

        
    }
}
