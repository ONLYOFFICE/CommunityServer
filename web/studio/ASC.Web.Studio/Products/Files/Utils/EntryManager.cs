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
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Studio.Core;
using Newtonsoft.Json.Linq;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Utils
{
    public class EntryManager
    {
        private const string UPDATE_LIST = "filesUpdateList";
        private static readonly ICache cache = AscCache.Default;


        public static IEnumerable<FileEntry> GetEntries(IFolderDao folderDao, IFileDao fileDao, Folder parent, int from, int count, FilterType filter, bool subjectGroup, Guid subjectId, String searchText, bool searchInContent, bool withSubfolders, OrderBy orderBy, out int total)
        {
            total = 0;

            if (parent == null) throw new ArgumentNullException("parent", FilesCommonResource.ErrorMassage_FolderNotFound);
            if (parent.ProviderEntry && !FilesSettings.EnableThirdParty) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFolder);
            if (parent.RootFolderType == FolderType.Privacy && (!PrivacyRoomSettings.Available || !PrivacyRoomSettings.Enabled)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFolder);

            var fileSecurity = Global.GetFilesSecurity();
            var entries = Enumerable.Empty<FileEntry>();

            searchInContent = searchInContent && filter != FilterType.ByExtension && !Equals(parent.ID, Global.FolderTrash);

            if (parent.FolderType == FolderType.Projects && parent.ID.Equals(Global.FolderProjects))
            {
                var apiServer = new ASC.Api.ApiServer();
                var apiUrl = string.Format("{0}project/maxlastmodified.json", SetupInfo.WebApiBaseUrl);

                var responseBody = apiServer.GetApiResponse(apiUrl, "GET");
                if (responseBody != null)
                {
                    var responseApi = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(responseBody)));

                    var projectLastModified = responseApi["response"].Value<String>();
                    const string projectLastModifiedCacheKey = "documents/projectFolders/projectLastModified";
                    if (HttpRuntime.Cache.Get(projectLastModifiedCacheKey) == null || !HttpRuntime.Cache.Get(projectLastModifiedCacheKey).Equals(projectLastModified))
                    {
                        HttpRuntime.Cache.Remove(projectLastModifiedCacheKey);
                        HttpRuntime.Cache.Insert(projectLastModifiedCacheKey, projectLastModified);
                    }
                    var projectListCacheKey = string.Format("documents/projectFolders/{0}", SecurityContext.CurrentAccount.ID);
                    var folderIDProjectTitle = (Dictionary<object, KeyValuePair<int, string>>)HttpRuntime.Cache.Get(projectListCacheKey);

                    if (folderIDProjectTitle == null)
                    {
                        apiUrl = string.Format("{0}project/filter.json?sortBy=title&sortOrder=ascending&status=open&fields=id,title,security,projectFolder", SetupInfo.WebApiBaseUrl);

                        responseApi = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiServer.GetApiResponse(apiUrl, "GET"))));

                        var responseData = responseApi["response"];

                        if (!(responseData is JArray)) return entries.ToList();

                        folderIDProjectTitle = new Dictionary<object, KeyValuePair<int, string>>();
                        foreach (JObject projectInfo in responseData.Children())
                        {
                            var projectID = projectInfo["id"].Value<int>();
                            var projectTitle = Global.ReplaceInvalidCharsAndTruncate(projectInfo["title"].Value<String>());

                            JToken projectSecurityJToken;
                            if (projectInfo.TryGetValue("security", out projectSecurityJToken))
                            {
                                var projectSecurity = projectInfo["security"].Value<JObject>();
                                JToken projectCanFileReadJToken;
                                if (projectSecurity.TryGetValue("canReadFiles", out projectCanFileReadJToken))
                                {
                                    if (!projectSecurity["canReadFiles"].Value<bool>())
                                    {
                                        continue;
                                    }
                                }
                            }

                            int projectFolderID;
                            JToken projectFolderIDjToken;
                            if (projectInfo.TryGetValue("projectFolder", out projectFolderIDjToken))
                                projectFolderID = projectFolderIDjToken.Value<int>();
                            else
                                projectFolderID = (int)FilesIntegration.RegisterBunch("projects", "project", projectID.ToString());

                            if (!folderIDProjectTitle.ContainsKey(projectFolderID))
                                folderIDProjectTitle.Add(projectFolderID, new KeyValuePair<int, string>(projectID, projectTitle));

                            AscCache.Default.Remove("documents/folders/" + projectFolderID);
                            AscCache.Default.Insert("documents/folders/" + projectFolderID, projectTitle, TimeSpan.FromMinutes(30));
                        }

                        HttpRuntime.Cache.Remove(projectListCacheKey);
                        HttpRuntime.Cache.Insert(projectListCacheKey, folderIDProjectTitle, new CacheDependency(null, new[] { projectLastModifiedCacheKey }), Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15));
                    }

                    var rootKeys = folderIDProjectTitle.Keys.ToArray();
                    if (filter == FilterType.None || filter == FilterType.FoldersOnly)
                    {
                        var folders = folderDao.GetFolders(rootKeys, filter, subjectGroup, subjectId, searchText, withSubfolders, false);

                        var emptyFilter = string.IsNullOrEmpty(searchText) && filter == FilterType.None && subjectId == Guid.Empty;
                        if (!emptyFilter)
                        {
                            var projectFolderIds =
                                folderIDProjectTitle
                                    .Where(projectFolder => string.IsNullOrEmpty(searchText)
                                                            || (projectFolder.Value.Value ?? "").ToLower().Trim().Contains(searchText.ToLower().Trim()))
                                    .Select(projectFolder => projectFolder.Key);

                            folders.RemoveAll(folder => rootKeys.Contains(folder.ID));

                            var projectFolders = folderDao.GetFolders(projectFolderIds.ToArray(), filter, subjectGroup, subjectId, null, false, false);
                            folders.AddRange(projectFolders);
                        }

                        folders.ForEach(x =>
                            {
                                x.Title = folderIDProjectTitle.ContainsKey(x.ID) ? folderIDProjectTitle[x.ID].Value : x.Title;
                                x.FolderUrl = folderIDProjectTitle.ContainsKey(x.ID) ? PathProvider.GetFolderUrl(x, folderIDProjectTitle[x.ID].Key) : string.Empty;
                            });

                        if (withSubfolders)
                        {
                            folders = fileSecurity.FilterRead(folders).ToList();
                        }

                        entries = entries.Concat(folders);
                    }

                    if (filter != FilterType.FoldersOnly && withSubfolders)
                    {
                        var files = fileDao.GetFiles(rootKeys, filter, subjectGroup, subjectId, searchText, searchInContent).ToList();
                        files = fileSecurity.FilterRead(files).ToList();
                        entries = entries.Concat(files);
                    }
                }

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalSubFolders + 1 : 0));
            }
            else if (parent.FolderType == FolderType.SHARE)
            {
                //share
                var shared = (IEnumerable<FileEntry>)fileSecurity.GetSharesForMe(filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders);

                entries = entries.Concat(shared);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalSubFolders + 1 : 0));
            }
            else if (parent.FolderType == FolderType.Recent)
            {
                var files = GetRecent(fileDao, filter, subjectGroup, subjectId, searchText, searchInContent);
                entries = entries.Concat(files);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalFiles : 1));
            }
            else if (parent.FolderType == FolderType.Favorites)
            {
                IEnumerable<Folder> folders;
                IEnumerable<File> files;
                GetFavorites(folderDao, fileDao, filter, subjectGroup, subjectId, searchText, searchInContent, out folders, out files);

                entries = entries.Concat(folders);
                entries = entries.Concat(files);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalSubFolders + 1 : 0));
            }
            else if (parent.FolderType == FolderType.Templates)
            {
                var files = GetTemplates(fileDao, filter, subjectGroup, subjectId, searchText, searchInContent);
                entries = entries.Concat(files);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalFiles : 1));
                parent.TotalSubFolders = 0;
            }
            else if (parent.FolderType == FolderType.Privacy)
            {
                var folders = folderDao.GetFolders(parent.ID, orderBy, filter, subjectGroup, subjectId, searchText, withSubfolders).Cast<FileEntry>();
                folders = fileSecurity.FilterRead(folders);
                entries = entries.Concat(folders);

                var files = fileDao.GetFiles(parent.ID, orderBy, filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders).Cast<FileEntry>();
                files = fileSecurity.FilterRead(files);
                entries = entries.Concat(files);

                //share
                var shared = (IEnumerable<FileEntry>)fileSecurity.GetPrivacyForMe(filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders);

                entries = entries.Concat(shared);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f.FileEntryType == FileEntryType.Folder ? ((Folder)f).TotalSubFolders + 1 : 0));
            }
            else
            {
                if (parent.FolderType == FolderType.TRASH)
                    withSubfolders = false;

                var folders = folderDao.GetFolders(parent.ID, orderBy, filter, subjectGroup, subjectId, searchText, withSubfolders).Cast<FileEntry>();
                folders = fileSecurity.FilterRead(folders);
                entries = entries.Concat(folders);

                var files = fileDao.GetFiles(parent.ID, orderBy, filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders).Cast<FileEntry>();
                files = fileSecurity.FilterRead(files);
                entries = entries.Concat(files);

                if (filter == FilterType.None || filter == FilterType.FoldersOnly)
                {
                    var folderList = GetThirpartyFolders(parent, searchText);

                    var thirdPartyFolder = FilterEntries(folderList, filter, subjectGroup, subjectId, searchText, searchInContent);
                    entries = entries.Concat(thirdPartyFolder);
                }
            }

            if (orderBy.SortedBy != SortedByType.New)
            {
                if (parent.FolderType != FolderType.Recent)
                {
                    entries = SortEntries(entries, orderBy);
                }

                total = entries.Count();
                if (0 < from) entries = entries.Skip(from);
                if (0 < count) entries = entries.Take(count);
            }

            entries = FileMarker.SetTagsNew(folderDao, parent, entries);

            //sorting after marking
            if (orderBy.SortedBy == SortedByType.New)
            {
                entries = SortEntries(entries, orderBy);

                total = entries.Count();
                if (0 < from) entries = entries.Skip(from);
                if (0 < count) entries = entries.Take(count);
            }

            SetFileStatus(entries.Where(r => r != null && r.ID != null && r.FileEntryType == FileEntryType.File).Select(r => r as File).ToList());

            return entries;
        }

        public static IEnumerable<File> GetTemplates(IFileDao fileDao, FilterType filter, bool subjectGroup, Guid subjectId, String searchText, bool searchInContent)
        {
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var tags = tagDao.GetTags(SecurityContext.CurrentAccount.ID, TagType.Template);

                var fileIds = tags.Where(tag => tag.EntryType == FileEntryType.File).Select(tag => tag.EntryId).ToArray();

                var files = fileDao.GetFilesFiltered(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent);
                files = files.Where(file => file.RootFolderType != FolderType.TRASH).ToList();

                files = Global.GetFilesSecurity().FilterRead(files).ToList();

                return files;
            }
        }

        public static IEnumerable<Folder> GetThirpartyFolders(Folder parent, string searchText = null)
        {
            var folderList = new List<Folder>();

            if ((parent.ID.Equals(Global.FolderMy) || parent.ID.Equals(Global.FolderCommon))
                && ThirdpartyConfiguration.SupportInclusion
                && (FilesSettings.EnableThirdParty
                    || CoreContext.Configuration.Personal))
            {
                using (var providerDao = Global.DaoFactory.GetProviderDao())
                {
                    if (providerDao == null) return folderList;

                    var fileSecurity = Global.GetFilesSecurity();

                    var providers = providerDao.GetProvidersInfo(parent.RootFolderType, searchText);
                    folderList = providers
                        .Select(providerInfo => GetFakeThirdpartyFolder(providerInfo, parent.ID))
                        .Where(fileSecurity.CanRead).ToList();
                }

                if (folderList.Any())
                    using (var securityDao = Global.DaoFactory.GetSecurityDao())
                    {
                        securityDao.GetPureShareRecords(folderList.Cast<FileEntry>().ToArray())
                                   //.Where(x => x.Owner == SecurityContext.CurrentAccount.ID)
                                   .Select(x => x.EntryId).Distinct().ToList()
                                   .ForEach(id =>
                                       {
                                           folderList.First(y => y.ID.Equals(id)).Shared = true;
                                       });
                    }
            }

            return folderList;
        }

        public static IEnumerable<File> GetRecent(IFileDao fileDao, FilterType filter, bool subjectGroup, Guid subjectId, String searchText, bool searchInContent)
        {
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var tags = tagDao.GetTags(SecurityContext.CurrentAccount.ID, TagType.Recent).ToList();

                var fileIds = tags.Where(tag => tag.EntryType == FileEntryType.File).Select(tag => tag.EntryId).ToArray();
                var files = fileDao.GetFilesFiltered(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent);
                files = files.Where(file => file.RootFolderType != FolderType.TRASH).ToList();

                files = Global.GetFilesSecurity().FilterRead(files).ToList();

                var listFileIds = fileIds.ToList();
                files = files.OrderBy(file => listFileIds.IndexOf(file.ID)).ToList();

                return files;
            }
        }

        public static void GetFavorites(IFolderDao folderDao, IFileDao fileDao, FilterType filter, bool subjectGroup, Guid subjectId, String searchText, bool searchInContent, out IEnumerable<Folder> folders, out IEnumerable<File> files)
        {
            folders = new List<Folder>();
            files = new List<File>();
            var fileSecurity = Global.GetFilesSecurity();
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var tags = tagDao.GetTags(SecurityContext.CurrentAccount.ID, TagType.Favorite);

                if (filter == FilterType.None || filter == FilterType.FoldersOnly)
                {
                    var folderIds = tags.Where(tag => tag.EntryType == FileEntryType.Folder).Select(tag => tag.EntryId).ToArray();
                    folders = folderDao.GetFolders(folderIds, filter, subjectGroup, subjectId, searchText, false, false);
                    folders = folders.Where(folder => folder.RootFolderType != FolderType.TRASH).ToList();

                    folders = fileSecurity.FilterRead(folders).ToList();
                }

                if (filter != FilterType.FoldersOnly)
                {
                    var fileIds = tags.Where(tag => tag.EntryType == FileEntryType.File).Select(tag => tag.EntryId).ToArray();
                    files = fileDao.GetFilesFiltered(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent);
                    files = files.Where(file => file.RootFolderType != FolderType.TRASH).ToList();

                    files = fileSecurity.FilterRead(files).ToList();
                }
            }
        }

        public static IEnumerable<FileEntry> FilterEntries(IEnumerable<FileEntry> entries, FilterType filter, bool subjectGroup, Guid subjectId, String searchText, bool searchInContent)
        {
            if (entries == null || !entries.Any()) return entries;

            if (subjectId != Guid.Empty)
            {
                entries = entries.Where(f =>
                                        subjectGroup
                                            ? CoreContext.UserManager.GetUsersByGroup(subjectId).Any(s => s.ID == f.CreateBy)
                                            : f.CreateBy == subjectId
                    )
                                 .ToList();
            }

            Func<FileEntry, bool> where = null;

            switch (filter)
            {
                case FilterType.SpreadsheetsOnly:
                case FilterType.PresentationsOnly:
                case FilterType.ImagesOnly:
                case FilterType.DocumentsOnly:
                case FilterType.ArchiveOnly:
                case FilterType.FilesOnly:
                case FilterType.MediaOnly:
                    where = f => f.FileEntryType == FileEntryType.File && (((File)f).FilterType == filter || filter == FilterType.FilesOnly);
                    break;
                case FilterType.FoldersOnly:
                    where = f => f.FileEntryType == FileEntryType.Folder;
                    break;
                case FilterType.ByExtension:
                    var filterExt = (searchText ?? string.Empty).ToLower().Trim();
                    where = f => !string.IsNullOrEmpty(filterExt) && f.FileEntryType == FileEntryType.File && FileUtility.GetFileExtension(f.Title).Contains(filterExt);
                    break;
            }

            if (where != null)
            {
                entries = entries.Where(where).ToList();
            }

            if ((!searchInContent || filter == FilterType.ByExtension) && !string.IsNullOrEmpty(searchText = (searchText ?? string.Empty).ToLower().Trim()))
            {
                entries = entries.Where(f => f.Title.ToLower().Contains(searchText)).ToList();
            }
            return entries;
        }

        public static IEnumerable<FileEntry> SortEntries(IEnumerable<FileEntry> entries, OrderBy orderBy)
        {
            if (entries == null || !entries.Any()) return entries;

            Comparison<FileEntry> sorter;

            if (orderBy == null)
            {
                orderBy = FilesSettings.DefaultOrder;
            }

            var c = orderBy.IsAsc ? 1 : -1;
            switch (orderBy.SortedBy)
            {
                case SortedByType.Type:
                    sorter = (x, y) =>
                             {
                                 var cmp = 0;
                                 if (x.FileEntryType == FileEntryType.File && y.FileEntryType == FileEntryType.File)
                                     cmp = c * (FileUtility.GetFileExtension((x.Title)).CompareTo(FileUtility.GetFileExtension(y.Title)));
                                 return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                             };
                    break;
                case SortedByType.Author:
                    sorter = (x, y) =>
                             {
                                 var cmp = c * string.Compare(x.ModifiedByString, y.ModifiedByString);
                                 return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                             };
                    break;
                case SortedByType.Size:
                    sorter = (x, y) =>
                             {
                                 var cmp = 0;
                                 if (x.FileEntryType == FileEntryType.File && y.FileEntryType == FileEntryType.File)
                                     cmp = c * ((File)x).ContentLength.CompareTo(((File)y).ContentLength);
                                 return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                             };
                    break;
                case SortedByType.AZ:
                    sorter = (x, y) => c * x.Title.EnumerableComparer(y.Title);
                    break;
                case SortedByType.DateAndTime:
                    sorter = (x, y) =>
                             {
                                 var cmp = c * DateTime.Compare(x.ModifiedOn, y.ModifiedOn);
                                 return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                             };
                    break;
                case SortedByType.DateAndTimeCreation:
                    sorter = (x, y) =>
                    {
                        var cmp = c * DateTime.Compare(x.CreateOn, y.CreateOn);
                        return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
                    };
                    break;
                case SortedByType.New:
                    sorter = (x, y) =>
                        {
                            var isNewSortResult = x.IsNew.CompareTo(y.IsNew);
                            return c * (isNewSortResult == 0 ? DateTime.Compare(x.ModifiedOn, y.ModifiedOn) : isNewSortResult);
                        };
                    break;
                default:
                    sorter = (x, y) => c * x.Title.EnumerableComparer(y.Title);
                    break;
            }

            if (orderBy.SortedBy != SortedByType.New)
            {
                // folders on top
                var folders = entries.Where(r => r.FileEntryType == FileEntryType.Folder).ToList();
                var files = entries.Where(r => r.FileEntryType == FileEntryType.File).ToList();
                folders.Sort(sorter);
                files.Sort(sorter);

                return folders.Concat(files);
            }

            var result = entries.ToList();

            result.Sort(sorter);

            return result;
        }

        public static Folder GetFakeThirdpartyFolder(IProviderInfo providerInfo, object parentFolderId = null)
        {
            //Fake folder. Don't send request to third party
            return new Folder
                {
                    ParentFolderID = parentFolderId,

                    ID = providerInfo.RootFolderId,
                    CreateBy = providerInfo.Owner,
                    CreateOn = providerInfo.CreateOn,
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = providerInfo.Owner,
                    ModifiedOn = providerInfo.CreateOn,
                    ProviderId = providerInfo.ID,
                    ProviderKey = providerInfo.ProviderKey,
                    RootFolderCreator = providerInfo.Owner,
                    RootFolderId = providerInfo.RootFolderId,
                    RootFolderType = providerInfo.RootFolderType,
                    Shareable = false,
                    Title = providerInfo.CustomerTitle,
                    TotalFiles = 0,
                    TotalSubFolders = 0
                };
        }


        public static List<Folder> GetBreadCrumbs(object folderId)
        {
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                return GetBreadCrumbs(folderId, folderDao);
            }
        }

        public static List<Folder> GetBreadCrumbs(object folderId, IFolderDao folderDao)
        {
            if (folderId == null) return new List<Folder>();
            var breadCrumbs = Global.GetFilesSecurity().FilterRead(folderDao.GetParentFolders(folderId)).ToList();

            var firstVisible = breadCrumbs.ElementAtOrDefault(0);

            object rootId = null;
            if (firstVisible == null)
            {
                rootId = Global.FolderShare;
            }
            else
            {
                switch (firstVisible.FolderType)
                {
                    case FolderType.DEFAULT:
                        if (!firstVisible.ProviderEntry)
                        {
                            rootId = Global.FolderShare;
                        }
                        else
                        {
                            switch (firstVisible.RootFolderType)
                            {
                                case FolderType.USER:
                                    rootId = SecurityContext.CurrentAccount.ID == firstVisible.RootFolderCreator
                                        ? Global.FolderMy
                                        : Global.FolderShare;
                                    break;
                                case FolderType.COMMON:
                                    rootId = Global.FolderCommon;
                                    break;
                            }
                        }
                        break;

                    case FolderType.BUNCH:
                        rootId = Global.FolderProjects;
                        break;
                }
            }

            if (rootId != null)
            {
                breadCrumbs.Insert(0, folderDao.GetFolder(rootId));
            }

            return breadCrumbs;
        }


        public static void SetFileStatus(File file)
        {
            if (file == null || file.ID == null) return;

            SetFileStatus(new List<File>(1) { file });
        }

        public static void SetFileStatus(IEnumerable<File> files)
        {
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var tagsFavorite = tagDao.GetTags(SecurityContext.CurrentAccount.ID, TagType.Favorite, files);

                var tagsTemplate = tagDao.GetTags(SecurityContext.CurrentAccount.ID, TagType.Template, files);

                var tagsNew = tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, files);

                var tagsLocked = tagDao.GetTags(TagType.Locked, files.ToArray());

                foreach (var file in files)
                {
                    if (tagsFavorite.Any(r => r.EntryId.Equals(file.ID)))
                    {
                        file.IsFavorite = true;
                    }

                    if (tagsTemplate.Any(r => r.EntryId.Equals(file.ID)))
                    {
                        file.IsTemplate = true;
                    }

                    if (tagsNew.Any(r => r.EntryId.Equals(file.ID)))
                    {
                        file.IsNew = true;
                    }

                    var tagLocked = tagsLocked.FirstOrDefault(t => t.EntryId.Equals(file.ID));

                    var lockedBy = tagLocked != null ? tagLocked.Owner : Guid.Empty;
                    file.Locked = lockedBy != Guid.Empty;
                    file.LockedBy = lockedBy != Guid.Empty && lockedBy != SecurityContext.CurrentAccount.ID
                        ? Global.GetUserName(lockedBy)
                        : null;
                }
            }
        }

        public static bool FileLockedForMe(object fileId, Guid userId = default(Guid))
        {
            var app = ThirdPartySelector.GetAppByFileId(fileId.ToString());
            if (app != null)
            {
                return false;
            }

            userId = userId == default(Guid) ? SecurityContext.CurrentAccount.ID : userId;
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var lockedBy = FileLockedBy(fileId, tagDao);
                return lockedBy != Guid.Empty && lockedBy != userId;
            }
        }

        public static Guid FileLockedBy(object fileId, ITagDao tagDao)
        {
            var tagLock = tagDao.GetTags(fileId, FileEntryType.File, TagType.Locked).FirstOrDefault();
            return tagLock != null ? tagLock.Owner : Guid.Empty;
        }


        public static File SaveEditing(String fileId, string fileExtension, string downloadUri, Stream stream, String doc, string comment = null, bool checkRight = true, bool encrypted = false, ForcesaveType? forcesave = null)
        {
            var newExtension = string.IsNullOrEmpty(fileExtension)
                              ? FileUtility.GetFileExtension(downloadUri)
                              : fileExtension;

            var app = ThirdPartySelector.GetAppByFileId(fileId);
            if (app != null)
            {
                app.SaveFile(fileId, newExtension, downloadUri, stream);
                return null;
            }

            File file;
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                var editLink = FileShareLink.Check(doc, false, fileDao, out file);
                if (file == null)
                {
                    file = fileDao.GetFile(fileId);
                }

                if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                var fileSecurity = Global.GetFilesSecurity();
                if (checkRight && !editLink && (!fileSecurity.CanEdit(file) || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                if (checkRight && FileLockedForMe(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
                if (checkRight && (!forcesave.HasValue || forcesave.Value == ForcesaveType.None) && FileTracker.IsEditing(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);
                if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

                var currentExt = file.ConvertedExtension;
                if (string.IsNullOrEmpty(newExtension)) newExtension = FileUtility.GetInternalExtension(file.Title);

                var replaceVersion = false;
                if (file.Forcesave != ForcesaveType.None)
                {
                    if (file.Forcesave == ForcesaveType.User && FilesSettings.StoreForcesave
                        || encrypted)
                    {
                        file.Version++;
                    }
                    else
                    {
                        replaceVersion = true;
                    }
                }
                else
                {
                    if (file.Version != 1)
                    {
                        file.VersionGroup++;
                    }
                    else
                    {
                        var storeTemplate = Global.GetStoreTemplate();

                        var path = FileConstant.NewDocPath + Thread.CurrentThread.CurrentCulture + "/";
                        if (!storeTemplate.IsDirectory(path))
                        {
                            path = FileConstant.NewDocPath + "default/";
                        }
                        path += "new" + FileUtility.GetInternalExtension(file.Title);

                        //todo: think about the criteria for saving after creation
                        if (file.ContentLength != storeTemplate.GetFileSize("", path))
                        {
                            file.VersionGroup++;
                        }
                    }
                    file.Version++;
                }
                file.Forcesave = forcesave.HasValue ? forcesave.Value : ForcesaveType.None;

                if (string.IsNullOrEmpty(comment))
                    comment = FilesCommonResource.CommentEdit;

                file.Encrypted = encrypted;

                file.ConvertedType = FileUtility.GetFileExtension(file.Title) != newExtension ? newExtension : null;

                if (file.ProviderEntry && !newExtension.Equals(currentExt))
                {
                    if (FileUtility.ExtsConvertible.Keys.Contains(newExtension)
                        && FileUtility.ExtsConvertible[newExtension].Contains(currentExt))
                    {
                        if (stream != null)
                        {
                            downloadUri = PathProvider.GetTempUrl(stream, newExtension);
                            downloadUri = DocumentServiceConnector.ReplaceCommunityAdress(downloadUri);
                        }

                        var key = DocumentServiceConnector.GenerateRevisionId(downloadUri);
                        DocumentServiceConnector.GetConvertedUri(downloadUri, newExtension, currentExt, key, null, false, out downloadUri);

                        stream = null;
                    }
                    else
                    {
                        file.ID = null;
                        file.Title = FileUtility.ReplaceFileExtension(file.Title, newExtension);
                    }

                    file.ConvertedType = null;
                }

                using (var tmpStream = new MemoryStream())
                {
                    if (stream != null)
                    {
                        stream.CopyTo(tmpStream);
                    }
                    else
                    {
                        // hack. http://ubuntuforums.org/showthread.php?t=1841740
                        if (WorkContext.IsMono)
                        {
                            ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                        }

                        var req = (HttpWebRequest)WebRequest.Create(downloadUri);
                        using (var editedFileStream = new ResponseStream(req.GetResponse()))
                        {
                            editedFileStream.CopyTo(tmpStream);
                        }
                    }
                    tmpStream.Position = 0;

                    file.ContentLength = tmpStream.Length;
                    file.Comment = string.IsNullOrEmpty(comment) ? null : comment;
                    if (replaceVersion)
                    {
                        file = fileDao.ReplaceFileVersion(file, tmpStream);
                    }
                    else
                    {
                        file = fileDao.SaveFile(file, tmpStream);
                    }
                }
            }

            FileMarker.MarkAsNew(file);
            FileMarker.RemoveMarkAsNew(file);
            return file;
        }

        public static void TrackEditing(string fileId, Guid tabId, Guid userId, string doc, bool editingAlone = false)
        {
            bool checkRight;
            if (FileTracker.GetEditingBy(fileId).Contains(userId))
            {
                checkRight = FileTracker.ProlongEditing(fileId, tabId, userId, editingAlone);
                if (!checkRight) return;
            }

            File file;
            bool editLink;
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                editLink = FileShareLink.Check(doc, false, fileDao, out file);
                if (file == null)
                    file = fileDao.GetFile(fileId);
            }

            if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            var fileSecurity = Global.GetFilesSecurity();
            if (!editLink
                && (!fileSecurity.CanEdit(file, userId)
                    && !fileSecurity.CanCustomFilterEdit(file, userId)
                    && !fileSecurity.CanReview(file, userId)
                    && !fileSecurity.CanFillForms(file, userId)
                    && !fileSecurity.CanComment(file, userId)
                    || CoreContext.UserManager.GetUsers(userId).IsVisitor()))
            {
                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
            }
            if (FileLockedForMe(file.ID, userId)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
            if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

            checkRight = FileTracker.ProlongEditing(fileId, tabId, userId, editingAlone);
            if (checkRight)
            {
                FileTracker.ChangeRight(fileId, userId, false);
            }
        }


        public static File UpdateToVersionFile(object fileId, int version, String doc = null, bool checkRight = true)
        {
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                if (version < 1) throw new ArgumentNullException("version");

                File fromFile;
                var editLink = FileShareLink.Check(doc, false, fileDao, out fromFile);

                if (fromFile == null)
                    fromFile = fileDao.GetFile(fileId);

                if (fromFile == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);

                if (fromFile.Version != version)
                    fromFile = fileDao.GetFile(fromFile.ID, Math.Min(fromFile.Version, version));

                if (fromFile == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                if (checkRight && !editLink && (!Global.GetFilesSecurity().CanEdit(fromFile) || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                if (FileLockedForMe(fromFile.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
                if (checkRight && FileTracker.IsEditing(fromFile.ID)) throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);
                if (fromFile.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
                if (fromFile.ProviderEntry) throw new Exception(FilesCommonResource.ErrorMassage_BadRequest);
                if (fromFile.Encrypted) throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);

                var exists = cache.Get<string>(UPDATE_LIST + fileId.ToString()) != null;
                if (exists)
                {
                    throw new Exception(FilesCommonResource.ErrorMassage_UpdateEditingFile);
                }
                else
                {
                    cache.Insert(UPDATE_LIST + fileId.ToString(), fileId.ToString(), TimeSpan.FromMinutes(2));
                }

                try
                {
                    var currFile = fileDao.GetFile(fileId);
                    var newFile = new File
                    {
                        ID = fromFile.ID,
                        Version = currFile.Version + 1,
                        VersionGroup = currFile.VersionGroup,
                        Title = FileUtility.ReplaceFileExtension(currFile.Title, FileUtility.GetFileExtension(fromFile.Title)),
                        FileStatus = currFile.FileStatus,
                        FolderID = currFile.FolderID,
                        CreateBy = currFile.CreateBy,
                        CreateOn = currFile.CreateOn,
                        ModifiedBy = fromFile.ModifiedBy,
                        ModifiedOn = fromFile.ModifiedOn,
                        ConvertedType = fromFile.ConvertedType,
                        Comment = string.Format(FilesCommonResource.CommentRevert, fromFile.ModifiedOnString),
                        Encrypted = fromFile.Encrypted,
                    };

                    using (var stream = fileDao.GetFileStream(fromFile))
                    {
                        newFile.ContentLength = stream.CanSeek ? stream.Length : fromFile.ContentLength;
                        newFile = fileDao.SaveFile(newFile, stream);
                    }

                    FileMarker.MarkAsNew(newFile);

                    SetFileStatus(newFile);

                    newFile.Access = fromFile.Access;

                    if (newFile.IsTemplate
                        && !FileUtility.ExtsWebTemplate.Contains(FileUtility.GetFileExtension(newFile.Title), StringComparer.CurrentCultureIgnoreCase))
                    {
                        var tagTemplate = Tag.Template(SecurityContext.CurrentAccount.ID, newFile);
                        using (var tagDao = Global.DaoFactory.GetTagDao())
                        {
                            tagDao.RemoveTags(tagTemplate);
                        }

                        newFile.IsTemplate = false;
                    }

                    return newFile;
                }
                catch (Exception e)
                {
                    Global.Logger.Error(string.Format("Error on update {0} to version {1}", fileId, version), e);
                    throw new Exception(e.Message, e);
                }
                finally
                {
                    cache.Remove(UPDATE_LIST + fromFile.ID);
                }
            }
        }

        public static File CompleteVersionFile(object fileId, int version, bool continueVersion, bool checkRight = true)
        {
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                var fileVersion = version > 0
                    ? fileDao.GetFile(fileId, version)
                    : fileDao.GetFile(fileId);
                if (fileVersion == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                if (checkRight && (!Global.GetFilesSecurity().CanEdit(fileVersion) || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                if (FileLockedForMe(fileVersion.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
                if (fileVersion.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
                if (fileVersion.ProviderEntry) throw new Exception(FilesCommonResource.ErrorMassage_BadRequest);

                var lastVersionFile = fileDao.GetFile(fileVersion.ID);

                if (continueVersion)
                {
                    if (lastVersionFile.VersionGroup > 1)
                    {
                        fileDao.ContinueVersion(fileVersion.ID, fileVersion.Version);
                        lastVersionFile.VersionGroup--;
                    }
                }
                else
                {
                    if (!FileTracker.IsEditing(lastVersionFile.ID))
                    {
                        if (fileVersion.Version == lastVersionFile.Version)
                        {
                            lastVersionFile = UpdateToVersionFile(fileVersion.ID, fileVersion.Version, null, checkRight);
                        }

                        fileDao.CompleteVersion(fileVersion.ID, fileVersion.Version);
                        lastVersionFile.VersionGroup++;
                    }
                }

                SetFileStatus(lastVersionFile);

                return lastVersionFile;
            }
        }

        public static bool FileRename(object fileId, String title, out File file)
        {
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                file = fileDao.GetFile(fileId);
                if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                if (!Global.GetFilesSecurity().CanEdit(file)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
                if (!Global.GetFilesSecurity().CanDelete(file) && CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
                if (FileLockedForMe(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
                if (file.ProviderEntry && FileTracker.IsEditing(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_UpdateEditingFile);
                if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

                title = Global.ReplaceInvalidCharsAndTruncate(title);

                var ext = FileUtility.GetFileExtension(file.Title);
                if (string.Compare(ext, FileUtility.GetFileExtension(title), true) != 0)
                {
                    title += ext;
                }

                var fileAccess = file.Access;

                var renamed = false;
                if (String.Compare(file.Title, title, false) != 0)
                {
                    var newFileID = fileDao.FileRename(file, title);

                    file = fileDao.GetFile(newFileID);
                    file.Access = fileAccess;

                    DocumentServiceHelper.RenameFile(file, fileDao);

                    renamed = true;
                }

                SetFileStatus(file);

                return renamed;
            }
        }


        public static void MarkAsRecent(FileEntry fileEntry)
        {
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var userID = SecurityContext.CurrentAccount.ID;

                var tag = Tag.Recent(userID, fileEntry);
                tagDao.SaveTags(tag);
            }
        }

        //Long operation
        public static void DeleteSubitems(object parentId, IFolderDao folderDao, IFileDao fileDao)
        {
            var folders = folderDao.GetFolders(parentId);
            foreach (var folder in folders)
            {
                DeleteSubitems(folder.ID, folderDao, fileDao);

                Global.Logger.InfoFormat("Delete folder {0} in {1}", folder.ID, parentId);
                folderDao.DeleteFolder(folder.ID);
            }

            var files = fileDao.GetFiles(parentId, null, FilterType.None, false, Guid.Empty, string.Empty, true);
            foreach (var file in files)
            {
                Global.Logger.InfoFormat("Delete file {0} in {1}", file.ID, parentId);
                fileDao.DeleteFile(file.ID);
            }
        }

        public static void MoveSharedItems(object parentId, object toId, IFolderDao folderDao, IFileDao fileDao)
        {
            var fileSecurity = Global.GetFilesSecurity();

            var folders = folderDao.GetFolders(parentId);
            foreach (var folder in folders)
            {
                var shared = folder.Shared
                             && fileSecurity.GetShares(folder).Any(record => record.Share != FileShare.Restrict);
                if (shared)
                {
                    Global.Logger.InfoFormat("Move shared folder {0} from {1} to {2}", folder.ID, parentId, toId);
                    folderDao.MoveFolder(folder.ID, toId, null);
                }
                else
                {
                    MoveSharedItems(folder.ID, toId, folderDao, fileDao);
                }
            }

            var files = fileDao.GetFiles(parentId, null, FilterType.None, false, Guid.Empty, string.Empty, true);
            foreach (var file
                in files.Where(file =>
                               file.Shared
                               && fileSecurity.GetShares(file)
                                              .Any(record =>
                                                   record.Subject != FileConstant.ShareLinkId
                                                   && record.Share != FileShare.Restrict)))
            {
                Global.Logger.InfoFormat("Move shared file {0} from {1} to {2}", file.ID, parentId, toId);
                fileDao.MoveFile(file.ID, toId);
            }
        }

        public static void ReassignItems(object parentId, Guid fromUserId, Guid toUserId, IFolderDao folderDao, IFileDao fileDao)
        {
            var fileIds = fileDao.GetFiles(parentId, new OrderBy(SortedByType.AZ, true), FilterType.ByUser, false, fromUserId, null, true, true)
                                 .Where(file => file.CreateBy == fromUserId).Select(file => file.ID);

            fileDao.ReassignFiles(fileIds.ToArray(), toUserId);

            var folderIds = folderDao.GetFolders(parentId, new OrderBy(SortedByType.AZ, true), FilterType.ByUser, false, fromUserId, null, true)
                                     .Where(folder => folder.CreateBy == fromUserId).Select(folder => folder.ID);

            folderDao.ReassignFolders(folderIds.ToArray(), toUserId);
        }
    }
}