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
