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
