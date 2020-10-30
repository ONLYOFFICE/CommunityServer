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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using Box.V2.Models;

namespace ASC.Files.Thirdparty.Box
{
    internal abstract class BoxDaoBase
    {
        protected readonly BoxDaoSelector BoxDaoSelector;

        public int TenantID { get; private set; }
        public BoxProviderInfo BoxProviderInfo { get; private set; }
        public string PathPrefix { get; private set; }


        protected BoxDaoBase(BoxDaoSelector.BoxInfo boxInfo, BoxDaoSelector boxDaoSelector)
        {
            BoxProviderInfo = boxInfo.BoxProviderInfo;
            PathPrefix = boxInfo.PathPrefix;
            BoxDaoSelector = boxDaoSelector;
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public void Dispose()
        {
            BoxProviderInfo.Dispose();
        }

        protected DbManager GetDb()
        {
            return new DbManager(FileConstant.DatabaseId);
        }

        protected object MappingID(object id, bool saveIfNotExist = false)
        {
            if (id == null) return null;
            int n;

            var isNumeric = int.TryParse(id.ToString(), out n);

            if (isNumeric) return n;

            object result;
            using (var db = GetDb())
            {
                if (id.ToString().StartsWith("box"))
                {
                    result = Regex.Replace(BitConverter.ToString(Hasher.Hash(id.ToString(), HashAlg.MD5)), "-", "").ToLower();
                }
                else
                {
                    result = db.ExecuteScalar<String>(Query("files_thirdparty_id_mapping")
                                                          .Select("id")
                                                          .Where(Exp.Eq("hash_id", id)));
                }
                if (saveIfNotExist)
                {
                    db.ExecuteNonQuery(Insert("files_thirdparty_id_mapping")
                                           .InColumnValue("id", id)
                                           .InColumnValue("hash_id", result));
                }
            }
            return result;
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

        private static string GetTenantColumnName(string table)
        {
            const string tenant = "tenant_id";
            if (!table.Contains(" ")) return tenant;
            return table.Substring(table.IndexOf(" ", StringComparison.InvariantCulture)).Trim() + "." + tenant;
        }


        protected static string MakeBoxId(object entryId)
        {
            var id = Convert.ToString(entryId, CultureInfo.InvariantCulture);
            return string.IsNullOrEmpty(id)
                       ? "0"
                       : id.TrimStart('/');
        }

        protected static string GetParentFolderId(BoxItem boxItem)
        {
            return boxItem == null || boxItem.Parent == null
                       ? null
                       : boxItem.Parent.Id;
        }

        protected string MakeId(BoxItem boxItem)
        {
            var path = string.Empty;
            if (boxItem != null)
            {
                path = boxItem.Id;
            }

            return MakeId(path);
        }

        protected string MakeId(string path = null)
        {
            return string.Format("{0}{1}", PathPrefix,
                                 string.IsNullOrEmpty(path) || path == "0"
                                     ? "" : ("-|" + path.TrimStart('/')));
        }

        protected String MakeFolderTitle(BoxFolder boxFolder)
        {
            if (boxFolder == null || IsRoot(boxFolder))
            {
                return BoxProviderInfo.CustomerTitle;
            }

            return Global.ReplaceInvalidCharsAndTruncate(boxFolder.Name);
        }

        protected String MakeFileTitle(BoxFile boxFile)
        {
            if (boxFile == null || string.IsNullOrEmpty(boxFile.Name))
            {
                return BoxProviderInfo.ProviderKey;
            }

            return Global.ReplaceInvalidCharsAndTruncate(boxFile.Name);
        }

        protected Folder ToFolder(BoxFolder boxFolder)
        {
            if (boxFolder == null) return null;
            if (boxFolder is ErrorFolder)
            {
                //Return error entry
                return ToErrorFolder(boxFolder as ErrorFolder);
            }

            var isRoot = IsRoot(boxFolder);

            var folder = new Folder
                {
                    ID = MakeId(boxFolder.Id),
                    ParentFolderID = isRoot ? null : MakeId(GetParentFolderId(boxFolder)),
                    CreateBy = BoxProviderInfo.Owner,
                    CreateOn = isRoot ? BoxProviderInfo.CreateOn : (boxFolder.CreatedAt.HasValue ? boxFolder.CreatedAt.Value : default(DateTime)),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = BoxProviderInfo.Owner,
                    ModifiedOn = isRoot ? BoxProviderInfo.CreateOn : (boxFolder.ModifiedAt.HasValue ? boxFolder.ModifiedAt.Value : default(DateTime)),
                    ProviderId = BoxProviderInfo.ID,
                    ProviderKey = BoxProviderInfo.ProviderKey,
                    RootFolderCreator = BoxProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = BoxProviderInfo.RootFolderType,

                    Shareable = false,
                    Title = MakeFolderTitle(boxFolder),
                    TotalFiles = boxFolder.ItemCollection != null ? boxFolder.ItemCollection.Entries.Count(item => item is BoxFile) : 0,
                    TotalSubFolders = boxFolder.ItemCollection != null ? boxFolder.ItemCollection.Entries.Count(item => item is BoxFolder) : 0,
                };

            if (folder.CreateOn != DateTime.MinValue && folder.CreateOn.Kind == DateTimeKind.Utc)
                folder.CreateOn = TenantUtil.DateTimeFromUtc(folder.CreateOn);

            if (folder.ModifiedOn != DateTime.MinValue && folder.ModifiedOn.Kind == DateTimeKind.Utc)
                folder.ModifiedOn = TenantUtil.DateTimeFromUtc(folder.ModifiedOn);

            return folder;
        }

        protected static bool IsRoot(BoxFolder boxFolder)
        {
            return boxFolder.Id == "0";
        }

        private File ToErrorFile(ErrorFile boxFile)
        {
            if (boxFile == null) return null;
            return new File
                {
                    ID = MakeId(boxFile.ErrorId),
                    CreateBy = BoxProviderInfo.Owner,
                    CreateOn = TenantUtil.DateTimeNow(),
                    ModifiedBy = BoxProviderInfo.Owner,
                    ModifiedOn = TenantUtil.DateTimeNow(),
                    ProviderId = BoxProviderInfo.ID,
                    ProviderKey = BoxProviderInfo.ProviderKey,
                    RootFolderCreator = BoxProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = BoxProviderInfo.RootFolderType,
                    Title = MakeFileTitle(boxFile),
                    Error = boxFile.Error
                };
        }

        private Folder ToErrorFolder(ErrorFolder boxFolder)
        {
            if (boxFolder == null) return null;
            return new Folder
                {
                    ID = MakeId(boxFolder.ErrorId),
                    ParentFolderID = null,
                    CreateBy = BoxProviderInfo.Owner,
                    CreateOn = TenantUtil.DateTimeNow(),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = BoxProviderInfo.Owner,
                    ModifiedOn = TenantUtil.DateTimeNow(),
                    ProviderId = BoxProviderInfo.ID,
                    ProviderKey = BoxProviderInfo.ProviderKey,
                    RootFolderCreator = BoxProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = BoxProviderInfo.RootFolderType,
                    Shareable = false,
                    Title = MakeFolderTitle(boxFolder),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                    Error = boxFolder.Error
                };
        }

        public File ToFile(BoxFile boxFile)
        {
            if (boxFile == null) return null;

            if (boxFile is ErrorFile)
            {
                //Return error entry
                return ToErrorFile(boxFile as ErrorFile);
            }

            return new File
                {
                    ID = MakeId(boxFile.Id),
                    Access = FileShare.None,
                    ContentLength = boxFile.Size.HasValue ? (long)boxFile.Size : 0,
                    CreateBy = BoxProviderInfo.Owner,
                    CreateOn = boxFile.CreatedAt.HasValue ? TenantUtil.DateTimeFromUtc(boxFile.CreatedAt.Value) : default(DateTime),
                    FileStatus = FileStatus.None,
                    FolderID = MakeId(GetParentFolderId(boxFile)),
                    ModifiedBy = BoxProviderInfo.Owner,
                    ModifiedOn = boxFile.ModifiedAt.HasValue ? TenantUtil.DateTimeFromUtc(boxFile.ModifiedAt.Value) : default(DateTime),
                    NativeAccessor = boxFile,
                    ProviderId = BoxProviderInfo.ID,
                    ProviderKey = BoxProviderInfo.ProviderKey,
                    Title = MakeFileTitle(boxFile),
                    RootFolderId = MakeId(),
                    RootFolderType = BoxProviderInfo.RootFolderType,
                    RootFolderCreator = BoxProviderInfo.Owner,
                    Shared = false,
                    Version = 1
                };
        }

        public Folder GetRootFolder(object folderId)
        {
            return ToFolder(GetBoxFolder("0"));
        }

        protected BoxFolder GetBoxFolder(object folderId)
        {
            var boxFolderId = MakeBoxId(folderId);
            try
            {
                var folder = BoxProviderInfo.GetBoxFolder(boxFolderId);
                return folder;
            }
            catch (Exception ex)
            {
                return new ErrorFolder(ex, boxFolderId);
            }
        }

        protected BoxFile GetBoxFile(object fileId)
        {
            var boxFileId = MakeBoxId(fileId);
            try
            {
                var file = BoxProviderInfo.GetBoxFile(boxFileId);
                return file;
            }
            catch (Exception ex)
            {
                return new ErrorFile(ex, boxFileId);
            }
        }

        protected IEnumerable<string> GetChildren(object folderId)
        {
            return GetBoxItems(folderId).Select(entry => MakeId(entry.Id));
        }

        protected List<BoxItem> GetBoxItems(object parentId, bool? folder = null)
        {
            var boxFolderId = MakeBoxId(parentId);
            var items = BoxProviderInfo.GetBoxItems(boxFolderId);

            if (folder.HasValue)
            {
                if (folder.Value)
                {
                    return items.Where(i => i is BoxFolder).ToList();
                }

                return items.Where(i => i is BoxFile).ToList();
            }

            return items;
        }

        protected sealed class ErrorFolder : BoxFolder
        {
            public string Error { get; set; }

            public string ErrorId { get; private set; }


            public ErrorFolder(Exception e, object id)
            {
                ErrorId = id.ToString();
                if (e != null)
                {
                    Error = e.Message;
                }
            }
        }

        protected sealed class ErrorFile : BoxFile
        {
            public string Error { get; set; }

            public string ErrorId { get; private set; }


            public ErrorFile(Exception e, object id)
            {
                ErrorId = id.ToString();
                if (e != null)
                {
                    Error = e.Message;
                }
            }
        }

        protected String GetAvailableTitle(String requestTitle, string parentFolderId, Func<string, string, bool> isExist)
        {
            if (!isExist(requestTitle, parentFolderId)) return requestTitle;

            var re = new Regex(@"( \(((?<index>[0-9])+)\)(\.[^\.]*)?)$");
            var match = re.Match(requestTitle);

            if (!match.Success)
            {
                var insertIndex = requestTitle.Length;
                if (requestTitle.LastIndexOf(".", StringComparison.InvariantCulture) != -1)
                {
                    insertIndex = requestTitle.LastIndexOf(".", StringComparison.InvariantCulture);
                }
                requestTitle = requestTitle.Insert(insertIndex, " (1)");
            }

            while (isExist(requestTitle, parentFolderId))
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
    }
}