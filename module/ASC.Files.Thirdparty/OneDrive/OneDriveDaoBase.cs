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
using Microsoft.OneDrive.Sdk;
using File = ASC.Files.Core.File;
using Folder = ASC.Files.Core.Folder;

namespace ASC.Files.Thirdparty.OneDrive
{
    internal abstract class OneDriveDaoBase
    {
        protected readonly OneDriveDaoSelector OneDriveDaoSelector;

        public int TenantID { get; private set; }
        public OneDriveProviderInfo OneDriveProviderInfo { get; private set; }
        public string PathPrefix { get; private set; }


        protected OneDriveDaoBase(OneDriveDaoSelector.OneDriveInfo onedriveInfo, OneDriveDaoSelector onedriveDaoSelector)
        {
            OneDriveProviderInfo = onedriveInfo.OneDriveProviderInfo;
            PathPrefix = onedriveInfo.PathPrefix;
            OneDriveDaoSelector = onedriveDaoSelector;
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public void Dispose()
        {
            OneDriveProviderInfo.Dispose();
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
                if (id.ToString().StartsWith("onedrive"))
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


        protected static string MakeOneDriveId(object entryId)
        {
            var id = Convert.ToString(entryId, CultureInfo.InvariantCulture);
            return string.IsNullOrEmpty(id)
                       ? string.Empty
                       : id.TrimStart('/');
        }

        protected static string GetParentFolderId(Item onedriveItem)
        {
            return onedriveItem == null || IsRoot(onedriveItem)
                       ? null
                       : (onedriveItem.ParentReference.Path.Equals(OneDriveStorage.RootPath, StringComparison.InvariantCultureIgnoreCase)
                              ? String.Empty
                              : onedriveItem.ParentReference.Id);
        }

        protected string MakeId(Item onedriveItem)
        {
            var id = string.Empty;
            if (onedriveItem != null)
            {
                id = onedriveItem.Id;
            }

            return MakeId(id);
        }

        protected string MakeId(string id = null)
        {
            return string.Format("{0}{1}", PathPrefix,
                                 string.IsNullOrEmpty(id) || id == ""
                                     ? "" : ("-|" + id.TrimStart('/')));
        }

        public string MakeOneDrivePath(Item onedriveItem)
        {
            return onedriveItem == null || IsRoot(onedriveItem)
                       ? string.Empty
                       : (OneDriveStorage.MakeOneDrivePath(
                           new Regex("^" + OneDriveStorage.RootPath).Replace(onedriveItem.ParentReference.Path, ""),
                           onedriveItem.Name));
        }

        protected String MakeItemTitle(Item onedriveItem)
        {
            if (onedriveItem == null || IsRoot(onedriveItem))
            {
                return OneDriveProviderInfo.CustomerTitle;
            }

            return Global.ReplaceInvalidCharsAndTruncate(onedriveItem.Name);
        }

        protected Folder ToFolder(Item onedriveFolder)
        {
            if (onedriveFolder == null) return null;
            if (onedriveFolder is ErrorItem)
            {
                //Return error entry
                return ToErrorFolder(onedriveFolder as ErrorItem);
            }

            if (onedriveFolder.Folder == null) return null;

            var isRoot = IsRoot(onedriveFolder);

            var folder = new Folder
                {
                    ID = MakeId(isRoot ? string.Empty : onedriveFolder.Id),
                    ParentFolderID = isRoot ? null : MakeId(GetParentFolderId(onedriveFolder)),
                    CreateBy = OneDriveProviderInfo.Owner,
                    CreateOn = isRoot ? OneDriveProviderInfo.CreateOn : (onedriveFolder.CreatedDateTime.HasValue ? TenantUtil.DateTimeFromUtc(onedriveFolder.CreatedDateTime.Value.DateTime) : default(DateTime)),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = OneDriveProviderInfo.Owner,
                    ModifiedOn = isRoot ? OneDriveProviderInfo.CreateOn : (onedriveFolder.LastModifiedDateTime.HasValue ? TenantUtil.DateTimeFromUtc(onedriveFolder.LastModifiedDateTime.Value.DateTime) : default(DateTime)),
                    ProviderId = OneDriveProviderInfo.ID,
                    ProviderKey = OneDriveProviderInfo.ProviderKey,
                    RootFolderCreator = OneDriveProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = OneDriveProviderInfo.RootFolderType,

                    Shareable = false,
                    Title = MakeItemTitle(onedriveFolder),
                    TotalFiles = 0,
                    TotalSubFolders = 0
                };

            return folder;
        }

        protected static bool IsRoot(Item onedriveFolder)
        {
            return onedriveFolder.ParentReference == null || onedriveFolder.ParentReference.Id == null;
        }

        private File ToErrorFile(ErrorItem onedriveFile)
        {
            if (onedriveFile == null) return null;
            return new File
                {
                    ID = MakeId(onedriveFile.ErrorId),
                    CreateBy = OneDriveProviderInfo.Owner,
                    CreateOn = TenantUtil.DateTimeNow(),
                    ModifiedBy = OneDriveProviderInfo.Owner,
                    ModifiedOn = TenantUtil.DateTimeNow(),
                    ProviderId = OneDriveProviderInfo.ID,
                    ProviderKey = OneDriveProviderInfo.ProviderKey,
                    RootFolderCreator = OneDriveProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = OneDriveProviderInfo.RootFolderType,
                    Title = MakeItemTitle(onedriveFile),
                    Error = onedriveFile.Error
                };
        }

        private Folder ToErrorFolder(ErrorItem onedriveFolder)
        {
            if (onedriveFolder == null) return null;
            return new Folder
                {
                    ID = MakeId(onedriveFolder.ErrorId),
                    ParentFolderID = null,
                    CreateBy = OneDriveProviderInfo.Owner,
                    CreateOn = TenantUtil.DateTimeNow(),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = OneDriveProviderInfo.Owner,
                    ModifiedOn = TenantUtil.DateTimeNow(),
                    ProviderId = OneDriveProviderInfo.ID,
                    ProviderKey = OneDriveProviderInfo.ProviderKey,
                    RootFolderCreator = OneDriveProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = OneDriveProviderInfo.RootFolderType,
                    Shareable = false,
                    Title = MakeItemTitle(onedriveFolder),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                    Error = onedriveFolder.Error
                };
        }

        public File ToFile(Item onedriveFile)
        {
            if (onedriveFile == null) return null;

            if (onedriveFile is ErrorItem)
            {
                //Return error entry
                return ToErrorFile(onedriveFile as ErrorItem);
            }

            if (onedriveFile.File == null) return null;

            return new File
                {
                    ID = MakeId(onedriveFile.Id),
                    Access = FileShare.None,
                    ContentLength = onedriveFile.Size.HasValue ? (long)onedriveFile.Size : 0,
                    CreateBy = OneDriveProviderInfo.Owner,
                    CreateOn = onedriveFile.CreatedDateTime.HasValue ? TenantUtil.DateTimeFromUtc(onedriveFile.CreatedDateTime.Value.DateTime) : default(DateTime),
                    FileStatus = FileStatus.None,
                    FolderID = MakeId(GetParentFolderId(onedriveFile)),
                    ModifiedBy = OneDriveProviderInfo.Owner,
                    ModifiedOn = onedriveFile.LastModifiedDateTime.HasValue ? TenantUtil.DateTimeFromUtc(onedriveFile.LastModifiedDateTime.Value.DateTime) : default(DateTime),
                    NativeAccessor = onedriveFile,
                    ProviderId = OneDriveProviderInfo.ID,
                    ProviderKey = OneDriveProviderInfo.ProviderKey,
                    Title = MakeItemTitle(onedriveFile),
                    RootFolderId = MakeId(),
                    RootFolderType = OneDriveProviderInfo.RootFolderType,
                    RootFolderCreator = OneDriveProviderInfo.Owner,
                    Shared = false,
                    Version = 1
                };
        }

        public Folder GetRootFolder(object folderId)
        {
            return ToFolder(GetOneDriveItem(""));
        }

        protected Item GetOneDriveItem(object itemId)
        {
            var onedriveId = MakeOneDriveId(itemId);
            try
            {
                return OneDriveProviderInfo.GetOneDriveItem(onedriveId);
            }
            catch (Exception ex)
            {
                return new ErrorItem(ex, onedriveId);
            }
        }

        protected IEnumerable<string> GetChildren(object folderId)
        {
            return GetOneDriveItems(folderId).Select(entry => MakeId(entry.Id));
        }

        protected List<Item> GetOneDriveItems(object parentId, bool? folder = null)
        {
            var onedriveFolderId = MakeOneDriveId(parentId);
            var items = OneDriveProviderInfo.GetOneDriveItems(onedriveFolderId);

            if (folder.HasValue)
            {
                if (folder.Value)
                {
                    return items.Where(i => i.Folder != null).ToList();
                }

                return items.Where(i => i.File != null).ToList();
            }

            return items;
        }

        protected sealed class ErrorItem : Item
        {
            public string Error { get; set; }

            public string ErrorId { get; private set; }


            public ErrorItem(Exception e, object id)
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
            requestTitle = new Regex("\\.$").Replace(requestTitle, "_");
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