/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Security.Cryptography;
using ASC.Web.Files.Core;
using Autofac;

namespace ASC.Files.Core.Data
{
    public class AbstractDao : IDisposable
    {
        protected int TenantID { get; private set; }
        protected readonly ICache cache = AscCache.Default;
        protected IDbManager dbManager { get; private set; }
        protected ILifetimeScope scope { get; private set; }


        protected AbstractDao(int tenantID, String storageKey)
        {
            TenantID = tenantID;
            scope = DIHelper.Resolve();
            dbManager = scope.Resolve<IDbManager>();
        }

        public void Dispose()
        {
            scope.Dispose();
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

        protected SqlQuery GetFileQuery(Exp where, bool checkShared = true)
        {
            return Query("files_file f")
                .Select("f.id")
                .Select("f.title")
                .Select("f.folder_id")
                .Select("f.create_on")
                .Select("f.create_by")
                .Select("f.version")
                .Select("f.version_group")
                .Select("f.content_length")
                .Select("f.modified_on")
                .Select("f.modified_by")
                .Select(GetRootFolderType("folder_id"))
                .Select(checkShared ? GetSharedQuery(FileEntryType.File) : new SqlQuery().Select("1"))
                .Select("converted_type")
                .Select("f.comment")
                .Select("f.encrypted")
                .Select("f.forcesave")
                .Where(where);
        }

        protected SqlQuery GetRootFolderType(string parentFolderColumnName)
        {
            return new SqlQuery("files_folder d")
                .From("files_folder_tree t")
                .Select("concat(cast(d.folder_type as char),d.create_by,cast(d.id as char))")
                .Where(Exp.EqColumns("d.tenant_id", "f.tenant_id") &
                        Exp.EqColumns("d.id", "t.parent_id") &
                       Exp.EqColumns("t.folder_id", "f." + parentFolderColumnName))
                .OrderBy("level", false)
                .GroupBy("level")
                .SetMaxResults(1);
        }

        protected FolderType ParseRootFolderType(object v)
        {
            return v != null
                       ? (FolderType)Enum.Parse(typeof(FolderType), v.ToString().Substring(0, 1))
                       : default(FolderType);
        }

        protected Guid ParseRootFolderCreator(object v)
        {
            return v != null ? new Guid(v.ToString().Substring(1, 36)) : default(Guid);
        }

        protected int ParseRootFolderId(object v)
        {
            return v != null ? int.Parse(v.ToString().Substring(1 + 36)) : default(int);
        }

        protected SqlQuery GetSharedQuery(FileEntryType type)
        {
            var result = Query("files_security s")
                .SelectCount()
                .Where(Exp.EqColumns("s.entry_id", "cast(f.id as char)"))
                .Where("s.entry_type", (int) type)
                .SetMaxResults(1);

            return result;
        }

        protected SqlUpdate GetRecalculateFilesCountUpdate(object folderId)
        {
            if (DbRegistry.GetSqlDialect("default").SupportMultiTableUpdate)
            {
                return new SqlUpdate("files_folder d, files_folder_tree t")
                    .Set(
                        "d.filesCount = (select count(distinct f.id) from files_file f, files_folder_tree t2 where f.tenant_id = d.tenant_id and f.folder_id = t2.folder_id and t2.parent_id = d.id)")
                    .Where(Exp.EqColumns("d.id", "t.parent_id") & Exp.Eq("t.folder_id", folderId) &
                           Exp.Eq("d.tenant_id", TenantID));
            }
            else
            {
                return new SqlUpdate("files_folder")
                    .Set(
                        "filesCount = (select count(distinct f.id) from files_file f, files_folder_tree t2 where f.tenant_id = files_folder.tenant_id and f.folder_id = t2.folder_id and t2.parent_id = files_folder.id)")
                    .Where(Exp.Eq("files_folder.tenant_id", TenantID) &
                           Exp.In("files_folder.id",
                                  new SqlQuery("files_folder_tree t").Select("t.parent_id").Where("t.folder_id",
                                                                                                  folderId)));
            }
        }

        protected object MappingID(object id, bool saveIfNotExist)
        {
            if (id == null) return null;

            int n;

            var isNumeric = int.TryParse(id.ToString(), out n);

            if (isNumeric) return n;

            object result;

            if (id.ToString().StartsWith("sbox")
                || id.ToString().StartsWith("box")
                || id.ToString().StartsWith("dropbox")
                || id.ToString().StartsWith("spoint")
                || id.ToString().StartsWith("drive")
                || id.ToString().StartsWith("onedrive"))
            {
                result = Regex.Replace(BitConverter.ToString(Hasher.Hash(id.ToString(), HashAlg.MD5)), "-", "").ToLower();
            }
            else
            {
                result = dbManager.ExecuteScalar<String>(
                    Query("files_thirdparty_id_mapping")
                        .Select("id")
                        .Where(Exp.Eq("hash_id", id))
                    );
            }

            if (saveIfNotExist)
            {
                dbManager.ExecuteNonQuery(
                    Insert("files_thirdparty_id_mapping")
                        .InColumnValue("id", id)
                        .InColumnValue("hash_id", result)
                    );
            }

            return result;
        }

        protected object MappingID(object id)
        {
            return MappingID(id, false);
        }

        public static Exp BuildSearch(string column, string text, SqlLike like = SqlLike.AnyWhere)
        {
            return Exp.Like(string.Format("lower({0})", column), text.ToLower().Trim().Replace("%", "\\%").Replace("_", "\\_"), like);
        }
    }
}