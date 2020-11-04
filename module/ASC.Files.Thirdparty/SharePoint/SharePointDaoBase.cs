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
