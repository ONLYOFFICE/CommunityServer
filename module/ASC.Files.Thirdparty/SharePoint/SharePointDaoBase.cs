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

using System;
using System.Text.RegularExpressions;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using Folder = Microsoft.SharePoint.Client.Folder;

namespace ASC.Files.Thirdparty.SharePoint
{
    internal class SharePointDaoBase
    {
        public SharePointProviderInfo ProviderInfo { get; private set; }
        public SharePointDaoSelector SharePointDaoSelector { get; private set; }

        public SharePointDaoBase(SharePointProviderInfo sharePointInfo, SharePointDaoSelector sharePointDaoSelector)
        {
            ProviderInfo = sharePointInfo;
            SharePointDaoSelector = sharePointDaoSelector;
        }

        protected int TenantID
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        protected SqlQuery Query(string table)
        {
            return new SqlQuery(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected SqlDelete Delete(string table)
        {
            return new SqlDelete(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected SqlInsert Insert(string table)
        {
            return new SqlInsert(table, true).InColumns(GetTenantColumnName(table)).Values(TenantID);
        }

        protected SqlUpdate Update(string table)
        {
            return new SqlUpdate(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected string GetTenantColumnName(string table)
        {
            var tenant = "tenant_id";
            if (!table.Contains(" ")) return tenant;
            return table.Substring(table.IndexOf(" ")).Trim() + "." + tenant;
        }

        protected String GetAvailableTitle(String requestTitle, Folder parentFolderID, Func<string, Folder, bool> isExist)
        {
            if (!isExist(requestTitle, parentFolderID)) return requestTitle;

            var re = new Regex(@"( \(((?<index>[0-9])+)\)(\.[^\.]*)?)$");
            var match = re.Match(requestTitle);

            if (!match.Success)
            {
                var insertIndex = requestTitle.Length;
                if (requestTitle.LastIndexOf(".", StringComparison.Ordinal) != -1)
                {
                    insertIndex = requestTitle.LastIndexOf(".", StringComparison.Ordinal);
                }
                requestTitle = requestTitle.Insert(insertIndex, " (1)");
            }

            while (isExist(requestTitle, parentFolderID))
            {
                requestTitle = re.Replace(requestTitle, MatchEvaluator);
            }
            return requestTitle;
        }

        private static String MatchEvaluator(Match match)
        {
            var index = Convert.ToInt32(match.Groups[2].Value);
            var staticText = match.Value.Substring(String.Format(" ({0})", index).Length);
            return String.Format(" ({0}){1}", index + 1, staticText);
        }

        protected object MappingID(object id, bool saveIfNotExist)
        {
            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                if (id == null) return null;
                int n;

                var isNumeric = int.TryParse(id.ToString(), out n);

                if (isNumeric) return n;

                object result;

                if (id.ToString().StartsWith("spoint"))
                    result = Regex.Replace(BitConverter.ToString(Hasher.Hash(id.ToString(), HashAlg.MD5)), "-", "").ToLower();
                else
                    result = dbManager.ExecuteScalar<String>(Query("files_thirdparty_id_mapping")
                                                                 .Select("id")
                                                                 .Where(Exp.Eq("hash_id", id)));

                if (saveIfNotExist)
                    dbManager.ExecuteNonQuery(Insert("files_thirdparty_id_mapping")
                                                  .InColumnValue("id", id)
                                                  .InColumnValue("hash_id", result));

                return result;
            }
        }

        protected void UpdatePathInDB(String oldValue, String newValue)
        {
            if (oldValue.Equals(newValue)) return;

            using (var dbManager = new DbManager(FileConstant.DatabaseId))
            {
                using (var tx = dbManager.BeginTransaction())
                {
                    var oldIDs = dbManager.ExecuteList(Query("files_thirdparty_id_mapping")
                                                           .Select("id")
                                                           .Where(Exp.Like("id", oldValue, SqlLike.StartWith)))
                                          .ConvertAll(x => x[0].ToString());

                    foreach (var oldID in oldIDs)
                    {
                        var oldHashID = MappingID(oldID);
                        var newID = oldID.Replace(oldValue, newValue);
                        var newHashID = MappingID(newID);

                        dbManager.ExecuteNonQuery(Update("files_thirdparty_id_mapping")
                                                      .Set("id", newID)
                                                      .Set("hash_id", newHashID)
                                                      .Where(Exp.Eq("hash_id", oldHashID)));

                        dbManager.ExecuteNonQuery(Update("files_security")
                                                      .Set("entry_id", newHashID)
                                                      .Where(Exp.Eq("entry_id", oldHashID)));

                        dbManager.ExecuteNonQuery(Update("files_tag_link")
                                                      .Set("entry_id", newHashID)
                                                      .Where(Exp.Eq("entry_id", oldHashID)));
                    }

                    tx.Commit();
                }
            }
        }

        protected object MappingID(object id)
        {
            return MappingID(id, false);
        }
    }
}
