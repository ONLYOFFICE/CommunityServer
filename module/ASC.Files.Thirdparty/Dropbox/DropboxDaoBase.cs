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


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using Dropbox.Api.Files;

namespace ASC.Files.Thirdparty.Dropbox
{
    internal abstract class DropboxDaoBase : IDisposable
    {
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);
        private static readonly ICache CacheFile = AscCache.Memory;
        private static readonly ICache CacheFolder = AscCache.Memory;
        private static readonly ICache CacheChildItems = AscCache.Memory;
        private static readonly ICacheNotify CacheNotify;

        protected readonly DropboxDaoSelector DropboxDaoSelector;

        public int TenantID { get; private set; }
        public DropboxProviderInfo DropboxProviderInfo { get; private set; }
        public string PathPrefix { get; private set; }


        static DropboxDaoBase()
        {
            CacheNotify = AscCache.Notify;
            CacheNotify.Subscribe<DropboxCacheItem>((i, action) =>
                {
                    if (action != CacheNotifyAction.Remove) return;
                    if (i.ResetAll)
                    {
                        CacheFile.Remove(new Regex("^dropboxf-.*"));
                        CacheFolder.Remove(new Regex("^dropboxd-.*"));
                        CacheChildItems.Remove(new Regex("^dropbox-.*"));
                    }

                    if (!i.IsFile.HasValue)
                    {
                        CacheChildItems.Remove("dropbox-" + i.Key);

                        CacheFolder.Remove("dropboxd-" + i.Key);
                    }
                    else
                    {
                        if (i.IsFile.Value)
                        {
                            CacheFile.Remove("dropboxf-" + i.Key);
                        }
                        else
                        {
                            CacheFolder.Remove("dropboxd-" + i.Key);
                        }
                    }
                });
        }

        protected DropboxDaoBase(DropboxDaoSelector.DropboxInfo dropboxInfo, DropboxDaoSelector dropboxDaoSelector)
        {
            DropboxProviderInfo = dropboxInfo.DropboxProviderInfo;
            PathPrefix = dropboxInfo.PathPrefix;
            DropboxDaoSelector = dropboxDaoSelector;
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public void Dispose()
        {
            DropboxProviderInfo.Storage.Close();
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
                if (id.ToString().StartsWith("dropbox"))
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


        protected static string GetParentFolderPath(Metadata dropboxItem)
        {
            if (dropboxItem == null || IsRoot(dropboxItem.AsFolder))
                return null;

            var pathLength = dropboxItem.PathDisplay.Length - dropboxItem.Name.Length;
            return dropboxItem.PathDisplay.Substring(0, pathLength > 1 ? pathLength - 1 : 0);
        }

        protected static string MakeDropboxPath(object entryId)
        {
            return Convert.ToString(entryId, CultureInfo.InvariantCulture);
        }

        protected string MakeDropboxPath(Metadata dropboxItem)
        {
            string path = null;
            if (dropboxItem != null)
            {
                path = dropboxItem.PathDisplay;
            }

            return path;
        }

        protected string MakeId(Metadata dropboxItem)
        {
            return MakeId(MakeDropboxPath(dropboxItem));
        }

        protected string MakeId(string path = null)
        {
            return string.Format("{0}{1}", PathPrefix, string.IsNullOrEmpty(path) || path == "/" ? "" : ("-" + path.Replace('/', '|')));
        }

        protected String MakeFolderTitle(FolderMetadata dropboxFolder)
        {
            if (dropboxFolder == null || IsRoot(dropboxFolder))
            {
                return DropboxProviderInfo.CustomerTitle;
            }

            return Global.ReplaceInvalidCharsAndTruncate(dropboxFolder.Name);
        }

        protected String MakeFileTitle(FileMetadata dropboxFile)
        {
            if (dropboxFile == null || string.IsNullOrEmpty(dropboxFile.Name))
            {
                return DropboxProviderInfo.ProviderKey;
            }

            return Global.ReplaceInvalidCharsAndTruncate(dropboxFile.Name);
        }

        protected Folder ToFolder(FolderMetadata dropboxFolder)
        {
            if (dropboxFolder == null) return null;
            if (dropboxFolder is ErrorFolder)
            {
                //Return error entry
                return ToErrorFolder(dropboxFolder as ErrorFolder);
            }

            var isRoot = IsRoot(dropboxFolder);

            var folder = new Folder
                {
                    ID = MakeId(dropboxFolder),
                    ParentFolderID = isRoot ? null : MakeId(GetParentFolderPath(dropboxFolder)),
                    CreateBy = DropboxProviderInfo.Owner,
                    CreateOn = isRoot ? DropboxProviderInfo.CreateOn : default(DateTime),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = DropboxProviderInfo.Owner,
                    ModifiedOn = isRoot ? DropboxProviderInfo.CreateOn : default(DateTime),
                    ProviderId = DropboxProviderInfo.ID,
                    ProviderKey = DropboxProviderInfo.ProviderKey,
                    RootFolderCreator = DropboxProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = DropboxProviderInfo.RootFolderType,

                    Shareable = false,
                    Title = MakeFolderTitle(dropboxFolder),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                };

            if (folder.CreateOn != DateTime.MinValue && folder.CreateOn.Kind == DateTimeKind.Utc)
                folder.CreateOn = TenantUtil.DateTimeFromUtc(folder.CreateOn);

            if (folder.ModifiedOn != DateTime.MinValue && folder.ModifiedOn.Kind == DateTimeKind.Utc)
                folder.ModifiedOn = TenantUtil.DateTimeFromUtc(folder.ModifiedOn);

            return folder;
        }

        protected static bool IsRoot(FolderMetadata dropboxFolder)
        {
            return dropboxFolder != null && dropboxFolder.Id == "/";
        }

        private File ToErrorFile(ErrorFile dropboxFile)
        {
            if (dropboxFile == null) return null;
            return new File
                {
                    ID = MakeId(dropboxFile.ErrorId),
                    CreateBy = DropboxProviderInfo.Owner,
                    CreateOn = TenantUtil.DateTimeNow(),
                    ModifiedBy = DropboxProviderInfo.Owner,
                    ModifiedOn = TenantUtil.DateTimeNow(),
                    ProviderId = DropboxProviderInfo.ID,
                    ProviderKey = DropboxProviderInfo.ProviderKey,
                    RootFolderCreator = DropboxProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = DropboxProviderInfo.RootFolderType,
                    Title = MakeFileTitle(dropboxFile),
                    Error = dropboxFile.Error
                };
        }

        private Folder ToErrorFolder(ErrorFolder dropboxFolder)
        {
            if (dropboxFolder == null) return null;
            return new Folder
                {
                    ID = MakeId(dropboxFolder.ErrorId),
                    ParentFolderID = null,
                    CreateBy = DropboxProviderInfo.Owner,
                    CreateOn = TenantUtil.DateTimeNow(),
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = DropboxProviderInfo.Owner,
                    ModifiedOn = TenantUtil.DateTimeNow(),
                    ProviderId = DropboxProviderInfo.ID,
                    ProviderKey = DropboxProviderInfo.ProviderKey,
                    RootFolderCreator = DropboxProviderInfo.Owner,
                    RootFolderId = MakeId(),
                    RootFolderType = DropboxProviderInfo.RootFolderType,
                    Shareable = false,
                    Title = MakeFolderTitle(dropboxFolder),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                    Error = dropboxFolder.Error
                };
        }

        public File ToFile(FileMetadata dropboxFile)
        {
            if (dropboxFile == null) return null;

            if (dropboxFile is ErrorFile)
            {
                //Return error entry
                return ToErrorFile(dropboxFile as ErrorFile);
            }

            return new File
                {
                    ID = MakeId(dropboxFile),
                    Access = FileShare.None,
                    ContentLength = (long)dropboxFile.Size,
                    CreateBy = DropboxProviderInfo.Owner,
                    CreateOn = dropboxFile.ServerModified,
                    FileStatus = FileStatus.None,
                    FolderID = MakeId(GetParentFolderPath(dropboxFile)),
                    ModifiedBy = DropboxProviderInfo.Owner,
                    ModifiedOn = dropboxFile.ServerModified,
                    NativeAccessor = dropboxFile,
                    ProviderId = DropboxProviderInfo.ID,
                    ProviderKey = DropboxProviderInfo.ProviderKey,
                    Title = MakeFileTitle(dropboxFile),
                    RootFolderId = MakeId(),
                    RootFolderType = DropboxProviderInfo.RootFolderType,
                    RootFolderCreator = DropboxProviderInfo.Owner,
                    Shared = false,
                    Version = 1
                };
        }

        public Folder GetRootFolder(object folderId)
        {
            return ToFolder(GetDropboxFolder(string.Empty));
        }

        protected FolderMetadata GetDropboxFolder(object folderId)
        {
            var dropboxFolderPath = MakeDropboxPath(folderId);
            try
            {
                var folder = CacheFolder.Get<FolderMetadata>("dropboxd-" + DropboxProviderInfo.ID + dropboxFolderPath);
                if (folder == null)
                {
                    folder = DropboxProviderInfo.Storage.GetFolder(dropboxFolderPath);
                    CacheFolder.Insert("dropboxd-" + DropboxProviderInfo.ID + dropboxFolderPath, folder, DateTime.UtcNow.Add(CacheExpiration));
                }
                return folder;
            }
            catch (Exception ex)
            {
                return new ErrorFolder(ex, dropboxFolderPath);
            }
        }

        protected FileMetadata GetDropboxFile(object fileId)
        {
            var dropboxFilePath = MakeDropboxPath(fileId);
            try
            {
                var file = CacheFile.Get<FileMetadata>("dropboxf-" + DropboxProviderInfo.ID + dropboxFilePath);
                if (file == null)
                {
                    file = DropboxProviderInfo.Storage.GetFile(dropboxFilePath);
                    CacheFile.Insert("dropboxf-" + DropboxProviderInfo.ID + dropboxFilePath, file, DateTime.UtcNow.Add(CacheExpiration));
                }
                return file;
            }
            catch (Exception ex)
            {
                return new ErrorFile(ex, dropboxFilePath);
            }
        }

        protected IEnumerable<string> GetChildren(object folderId)
        {
            return GetDropboxItems(folderId).Select(MakeId);
        }

        protected List<Metadata> GetDropboxItems(object parentId, bool? folder = null)
        {
            var dropboxFolderPath = MakeDropboxPath(parentId);
            var items = CacheChildItems.Get<List<Metadata>>("dropbox-" + DropboxProviderInfo.ID + dropboxFolderPath);

            if (items == null)
            {
                items = DropboxProviderInfo.Storage.GetItems(dropboxFolderPath);
                CacheChildItems.Insert("dropbox-" + DropboxProviderInfo.ID + dropboxFolderPath, items, DateTime.UtcNow.Add(CacheExpiration));
            }

            if (folder.HasValue)
            {
                if (folder.Value)
                {
                    return items.Where(i => i.AsFolder != null).ToList();
                }

                return items.Where(i => i.AsFile != null).ToList();
            }

            return items;
        }

        protected sealed class ErrorFolder : FolderMetadata
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

        protected sealed class ErrorFile : FileMetadata
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

        protected String GetAvailableTitle(String requestTitle, string parentFolderPath, Func<string, string, bool> isExist)
        {
            if (!isExist(requestTitle, parentFolderPath)) return requestTitle;

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

            while (isExist(requestTitle, parentFolderPath))
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


        protected void CacheInsert(Metadata dropboxItem)
        {
            if (dropboxItem != null)
            {
                CacheNotify.Publish(new DropboxCacheItem { IsFile = dropboxItem.AsFolder != null, Key = DropboxProviderInfo.ID + dropboxItem.PathDisplay + "/" + dropboxItem.Name }, CacheNotifyAction.Remove);
            }
        }

        protected void CacheReset(string dropboxPath = null, bool? isFile = null)
        {
            if (dropboxPath == null)
            {
                CacheNotify.Publish(new DropboxCacheItem { ResetAll = true }, CacheNotifyAction.Remove);
            }
            else
            {
                var key = DropboxProviderInfo.ID + dropboxPath;

                CacheNotify.Publish(new DropboxCacheItem { IsFile = isFile, Key = key }, CacheNotifyAction.Remove);
            }
        }

        private class DropboxCacheItem
        {
            public bool ResetAll { get; set; }
            public bool? IsFile { get; set; }
            public string Key { get; set; }
        }
    }
}