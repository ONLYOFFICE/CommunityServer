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
using Box.V2.Models;

namespace ASC.Files.Thirdparty.Box
{
    internal abstract class BoxDaoBase : IDisposable
    {
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);
        private static readonly ICache CacheFile = AscCache.Memory;
        private static readonly ICache CacheFolder = AscCache.Memory;
        private static readonly ICache CacheChildItems = AscCache.Memory;
        private static readonly ICacheNotify CacheNotify;

        protected readonly BoxDaoSelector BoxDaoSelector;

        public int TenantID { get; private set; }
        public BoxProviderInfo BoxProviderInfo { get; private set; }
        public string PathPrefix { get; private set; }


        static BoxDaoBase()
        {
            CacheNotify = AscCache.Notify;
            CacheNotify.Subscribe<BoxCacheItem>((i, action) =>
                {
                    if (action != CacheNotifyAction.Remove) return;
                    if (i.ResetAll)
                    {
                        CacheChildItems.Remove(new Regex("^box-.*"));
                        CacheFile.Remove(new Regex("^boxf-.*"));
                        CacheFolder.Remove(new Regex("^boxd-.*"));
                    }

                    if (!i.IsFile.HasValue)
                    {
                        CacheChildItems.Remove("box-" + i.Key);

                        CacheFolder.Remove("boxd-" + i.Key);
                    }
                    else
                    {
                        if (i.IsFile.Value)
                        {
                            CacheFile.Remove("boxf-" + i.Key);
                        }
                        else
                        {
                            CacheFolder.Remove("boxd-" + i.Key);
                        }
                    }
                });
        }

        protected BoxDaoBase(BoxDaoSelector.BoxInfo boxInfo, BoxDaoSelector boxDaoSelector)
        {
            BoxProviderInfo = boxInfo.BoxProviderInfo;
            PathPrefix = boxInfo.PathPrefix;
            BoxDaoSelector = boxDaoSelector;
            TenantID = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        public void Dispose()
        {
            BoxProviderInfo.Storage.Close();
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
                var folder = CacheFolder.Get<BoxFolder>("boxd-" + BoxProviderInfo.ID + boxFolderId);
                if (folder == null)
                {
                    folder = BoxProviderInfo.Storage.GetFolder(boxFolderId);
                    CacheFolder.Insert("boxd-" + BoxProviderInfo.ID + boxFolderId, folder, DateTime.UtcNow.Add(CacheExpiration));
                }
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
                var file = CacheFile.Get<BoxFile>("boxf-" + BoxProviderInfo.ID + boxFileId);
                if (file == null)
                {
                    file = BoxProviderInfo.Storage.GetFile(boxFileId);
                    CacheFile.Insert("boxf-" + BoxProviderInfo.ID + boxFileId, file, DateTime.UtcNow.Add(CacheExpiration));
                }
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
            var items = CacheChildItems.Get<List<BoxItem>>("box-" + BoxProviderInfo.ID + boxFolderId);

            if (items == null)
            {
                items = BoxProviderInfo.Storage.GetItems(boxFolderId);
                CacheChildItems.Insert("box-" + BoxProviderInfo.ID + boxFolderId, items, DateTime.UtcNow.Add(CacheExpiration));
            }

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


        protected void CacheInsert(BoxItem boxItem)
        {
            if (boxItem != null)
            {
                CacheNotify.Publish(new BoxCacheItem { IsFile = boxItem is BoxFile, Key = BoxProviderInfo.ID + boxItem.Id }, CacheNotifyAction.Remove);
            }
        }

        protected void CacheReset(string boxId = null, bool? isFile = null)
        {
            if (boxId == null)
            {
                CacheNotify.Publish(new BoxCacheItem { ResetAll = true }, CacheNotifyAction.Remove);
            }
            else
            {
                if (boxId == BoxProviderInfo.BoxRootId)
                {
                    boxId = "0";
                }
                var key = BoxProviderInfo.ID + boxId;

                CacheNotify.Publish(new BoxCacheItem { IsFile = isFile, Key = key }, CacheNotifyAction.Remove);
            }
        }

        private class BoxCacheItem
        {
            public bool ResetAll { get; set; }
            public bool? IsFile { get; set; }
            public string Key { get; set; }
        }
    }
}