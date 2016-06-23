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


using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core;
using ASC.Web.Files.Import;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Studio.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Caching;
using File = ASC.Files.Core.File;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Utils
{
    public class EntryManager
    {
        private const string UPDATE_LIST = "filesUpdateList";
        private static readonly ICache cache = AscCache.Default;
        
        
        public static IEnumerable<FileEntry> GetEntries(IFolderDao folderDao, IFileDao fileDao, Folder parent, FilterType filter, Guid subjectId, OrderBy orderBy, String searchText, int from, int count, out int total)
        {
            total = 0;

            if (parent == null) throw new ArgumentNullException("parent", FilesCommonResource.ErrorMassage_FolderNotFound);

            var fileSecurity = Global.GetFilesSecurity();
            var entries = Enumerable.Empty<FileEntry>();

            if (parent.FolderType == FolderType.Projects && parent.ID.Equals(Global.FolderProjects))
            {
                var apiServer = new ASC.Api.ApiServer();
                var apiUrl = String.Format("{0}project/maxlastmodified.json", SetupInfo.WebApiBaseUrl);

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
                    var projectListCacheKey = String.Format("documents/projectFolders/{0}", SecurityContext.CurrentAccount.ID);
                    var fromCache = HttpRuntime.Cache.Get(projectListCacheKey);

                    if (fromCache == null || !string.IsNullOrEmpty(searchText))
                    {
                        apiUrl = String.Format("{0}project/filter.json?sortBy=title&sortOrder=ascending&status=open", SetupInfo.WebApiBaseUrl);

                        responseApi = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiServer.GetApiResponse(apiUrl, "GET"))));

                        var responseData = responseApi["response"];

                        if (!(responseData is JArray)) return entries.ToList();

                        var folderIDProjectTitle = new Dictionary<object, String>();

                        foreach (JObject projectInfo in responseData.Children())
                        {
                            var projectID = projectInfo["id"].Value<String>();
                            var projectTitle = Global.ReplaceInvalidCharsAndTruncate(projectInfo["title"].Value<String>());
                            int projectFolderID;

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

                            JToken projectFolderIDJToken;

                            if (projectInfo.TryGetValue("projectFolder", out projectFolderIDJToken))
                                projectFolderID = projectInfo["projectFolder"].Value<int>();
                            else
                                projectFolderID = (int)FilesIntegration.RegisterBunch("projects", "project", projectID);

                            if (!folderIDProjectTitle.ContainsKey(projectFolderID))
                                folderIDProjectTitle.Add(projectFolderID, projectTitle);
                            HttpRuntime.Cache.Remove("documents/folders/" + projectFolderID.ToString());
                            HttpRuntime.Cache.Insert("documents/folders/" + projectFolderID.ToString(), projectTitle, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30));
                        }

                        var folders = folderDao.GetFolders(folderIDProjectTitle.Keys.ToArray(), searchText, !string.IsNullOrEmpty(searchText));
                        folders.ForEach(x =>
                            {
                                x.Title = folderIDProjectTitle.ContainsKey(x.ID) ? folderIDProjectTitle[x.ID] : x.Title;
                                x.FolderUrl = PathProvider.GetFolderUrl(x);
                            });

                        folders = fileSecurity.FilterRead(folders).ToList();

                        entries = entries.Concat(folders);

                        if (!string.IsNullOrEmpty(searchText))
                        {
                            var files = fileDao.GetFiles(folderIDProjectTitle.Keys.ToArray(), searchText, !string.IsNullOrEmpty(searchText)).ToList();
                            files = fileSecurity.FilterRead(files).ToList();
                            entries = entries.Concat(files);
                        }

                        if (entries.Any() && string.IsNullOrEmpty(searchText))
                        {
                            HttpRuntime.Cache.Remove(projectListCacheKey);
                            HttpRuntime.Cache.Insert(projectListCacheKey, entries, new CacheDependency(null, new[] { projectLastModifiedCacheKey }), Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15));
                        }
                    }
                    else
                    {
                        entries = entries.Concat((IEnumerable<FileEntry>)fromCache);
                    }
                }

                entries = FilterEntries(entries, filter, subjectId, searchText);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f is Folder ? ((Folder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f is Folder ? ((Folder)f).TotalSubFolders + 1 : 0));
            }
            else if (parent.FolderType == FolderType.SHARE)
            {
                //share
                var shared = (IEnumerable<FileEntry>) fileSecurity.GetSharesForMe(searchText, !string.IsNullOrEmpty(searchText));
                if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
                {
                    shared = shared.Where(r => !r.ProviderEntry);
                }

                shared = FilterEntries(shared, filter, subjectId, searchText)
                    .Where(f => f.CreateBy != SecurityContext.CurrentAccount.ID && // don't show my files
                                f.RootFolderType == FolderType.USER); // don't show common files (common files can read)
                entries = entries.Concat(shared);

                parent.TotalFiles = entries.Aggregate(0, (a, f) => a + (f is Folder ? ((Folder)f).TotalFiles : 1));
                parent.TotalSubFolders = entries.Aggregate(0, (a, f) => a + (f is Folder ? ((Folder)f).TotalSubFolders + 1 : 0));
            }
            else
            {
                var folders = folderDao.GetFolders(parent.ID, orderBy, filter, subjectId, searchText, !string.IsNullOrEmpty(searchText) && parent.FolderType != FolderType.TRASH).Cast<FileEntry>();
                folders = fileSecurity.FilterRead(folders);
                entries = entries.Concat(folders);

                //TODO:Optimize
                var files = fileDao.GetFiles(parent.ID, orderBy, filter, subjectId, searchText, !string.IsNullOrEmpty(searchText) && parent.FolderType != FolderType.TRASH).Cast<FileEntry>();
                files = fileSecurity.FilterRead(files);
                entries = entries.Concat(files);

                if (filter == FilterType.None || filter == FilterType.FoldersOnly)
                {
                    var folderList = GetThirpartyFolders(parent, searchText);

                    var thirdPartyFolder = FilterEntries(folderList, filter, subjectId, searchText);
                    entries = entries.Concat(thirdPartyFolder);
                }
            }

            if (orderBy.SortedBy != SortedByType.New)
            {
                entries = SortEntries(entries, orderBy);

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

            SetFileStatus(entries.Select(r => r as File).Where(r => r != null && r.ID != null));

            return entries;
        }

        public static IEnumerable<Folder> GetThirpartyFolders(Folder parent, string searchText = null)
        {
            var folderList = new List<Folder>();

            if ((parent.ID.Equals(Global.FolderMy) || parent.ID.Equals(Global.FolderCommon))
                && ImportConfiguration.SupportInclusion
                && (Global.IsAdministrator
                    || CoreContext.Configuration.Personal
                    || FilesSettings.EnableThirdParty))
            {
                using (var providerDao = Global.DaoFactory.GetProviderDao())
                {
                    var fileSecurity = Global.GetFilesSecurity();

                    var providers = providerDao.GetProvidersInfo(parent.RootFolderType, searchText);
                    folderList = providers
                        .Select(providerInfo =>
                                //Fake folder. Don't send request to third party
                                new Folder
                                {
                                    ID = providerInfo.RootFolderId,
                                    ParentFolderID = parent.ID,
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
                                }
                        )
                        .Where(fileSecurity.CanRead).ToList();
                }

                if (folderList.Any())
                    using (var securityDao = Global.DaoFactory.GetSecurityDao())
                    {
                        securityDao.GetPureShareRecords(folderList.Cast<FileEntry>().ToArray())
                                   .Where(x => x.Owner == SecurityContext.CurrentAccount.ID)
                                   .Select(x => x.EntryId).Distinct().ToList()
                                   .ForEach(id =>
                                       {
                                           folderList.First(y => y.ID.Equals(id)).SharedByMe = true;
                                       });
                    }
            }

            return folderList;
        }

        public static IEnumerable<FileEntry> FilterEntries(IEnumerable<FileEntry> entries, FilterType filter, Guid subjectId, String searchText)
        {
            if (entries == null || !entries.Any()) return entries;

            Func<FileEntry, bool> where = null;

            switch (filter)
            {
                case FilterType.ByUser:
                    where = f => f.CreateBy == subjectId;
                    break;
                case FilterType.ByDepartment:
                    where = f => CoreContext.UserManager.GetUsersByGroup(subjectId).Any(s => s.ID == f.CreateBy);
                    break;
                case FilterType.SpreadsheetsOnly:
                case FilterType.PresentationsOnly:
                case FilterType.ImagesOnly:
                case FilterType.DocumentsOnly:
                case FilterType.ArchiveOnly:
                case FilterType.FilesOnly:
                    where = f => f is File && (((File)f).FilterType == filter || filter == FilterType.FilesOnly);
                    break;
                case FilterType.FoldersOnly:
                    where = f => f is Folder;
                    break;
                case FilterType.ByExtension:
                    var filterExt = (searchText ?? string.Empty).ToLower().Trim();
                    where = f => !string.IsNullOrEmpty(filterExt) && f is File && FileUtility.GetFileExtension(f.Title).Contains(filterExt);
                    break;
            }

            if (where != null)
            {
                entries = entries.Where(where);
            }

            if (!string.IsNullOrEmpty(searchText = (searchText ?? string.Empty).ToLower().Trim()))
            {
                entries = entries.Where(f => f.Title.ToLower().Contains(searchText));
            }
            return entries;
        }

        public static IEnumerable<FileEntry> SortEntries(IEnumerable<FileEntry> entries, OrderBy orderBy)
        {
            if (entries == null || !entries.Any()) return entries;

            Comparison<FileEntry> sorter;

            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            var c = orderBy.IsAsc ? 1 : -1;
            switch (orderBy.SortedBy)
            {
                case SortedByType.Type:
                    sorter = (x, y) =>
                             {
                                 var cmp = 0;
                                 if (x is File && y is File)
                                     cmp = c * (FileUtility.GetFileExtension((x.Title)).CompareTo(FileUtility.GetFileExtension(y.Title)));
                                 return cmp == 0 ? x.Title.CompareTo(y.Title) : cmp;
                             };
                    break;
                case SortedByType.Author:
                    sorter = (x, y) =>
                             {
                                 var cmp = c * string.Compare(x.ModifiedByString, y.ModifiedByString);
                                 return cmp == 0 ? x.Title.CompareTo(y.Title) : cmp;
                             };
                    break;
                case SortedByType.Size:
                    sorter = (x, y) =>
                             {
                                 var cmp = 0;
                                 if (x is File && y is File)
                                     cmp = c * ((File)x).ContentLength.CompareTo(((File)y).ContentLength);
                                 return cmp == 0 ? x.Title.CompareTo(y.Title) : cmp;
                             };
                    break;
                case SortedByType.AZ:
                    sorter = (x, y) => c * x.Title.CompareTo(y.Title);
                    break;
                case SortedByType.DateAndTime:
                    sorter = (x, y) =>
                             {
                                 var cmp = c * DateTime.Compare(x.ModifiedOn, y.ModifiedOn);
                                 return cmp == 0 ? x.Title.CompareTo(y.Title) : cmp;
                             };
                    break;
                case SortedByType.New:
                    sorter = (x, y) =>
                        {
                            var isNewSortResult = c * x.IsNew.CompareTo(y.IsNew);
                            if (isNewSortResult == 0)
                            {
                                var dataTimeSortResult = (-1) * DateTime.Compare(x.ModifiedOn, y.ModifiedOn);

                                return dataTimeSortResult == 0
                                    ? String.Compare(x.Title, y.Title, StringComparison.Ordinal)
                                    : dataTimeSortResult;
                            }

                            return isNewSortResult;
                        };
                    break;
                default:
                    sorter = (x, y) => c * x.Title.CompareTo(y.Title);
                    break;
            }

            if (orderBy.SortedBy != SortedByType.New)
            {
                // folders on top
                var folders = entries.OfType<Folder>().Cast<FileEntry>().ToList();
                var files = entries.OfType<File>().Cast<FileEntry>().ToList();
                folders.Sort(sorter);
                files.Sort(sorter);

                return folders.Concat(files);
            }

            var result = entries.ToList();

            result.Sort(sorter);

            return result;
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

            SetFileStatus(new List<File> { file });
        }

        public static void SetFileStatus(IEnumerable<File> files)
        {
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var tagsNew = tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, files.ToArray());

                var tagsLocked = tagDao.GetTags(TagType.Locked, files.ToArray());

                foreach (var file in files)
                {
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


        public static File SaveEditing(String fileId, int version, Guid tabId, string fileExtension, string downloadUri, Stream stream, bool asNew, String shareLinkKey, string comment = null, bool checkRight = true)
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
                var editLink = FileShareLink.Check(shareLinkKey, false, fileDao, out file);
                if (file == null)
                {
                    file = fileDao.GetFile(fileId);
                }

                if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                var fileSecurity = Global.GetFilesSecurity();
                if (checkRight && !editLink && (!(fileSecurity.CanEdit(file) || fileSecurity.CanReview(file)) || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                if (checkRight && FileLockedForMe(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
                if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

                var currentExt = file.ConvertedExtension;
                if (string.IsNullOrEmpty(newExtension)) newExtension = FileUtility.GetInternalExtension(file.Title);

                if ((file.Version <= version || !newExtension.Equals(currentExt) || version < 1)
                    && (file.Version > 1 || !asNew)
                    && !FileTracker.FixedVersion(file.ID))
                {
                    file.Version++;
                    if (string.IsNullOrEmpty(comment))
                        comment = FilesCommonResource.CommentEdit;
                }
                else
                {
                    if (string.IsNullOrEmpty(comment))
                        comment = FilesCommonResource.CommentCreate;
                }

                file.ConvertedType = FileUtility.GetFileExtension(file.Title) != newExtension ? newExtension : null;

                if (file.ProviderEntry && !newExtension.Equals(currentExt))
                {
                    if (FileUtility.ExtsConvertible.Keys.Contains(newExtension)
                        && FileUtility.ExtsConvertible[newExtension].Contains(currentExt))
                    {
                        var key = DocumentServiceConnector.GenerateRevisionId(downloadUri ?? Guid.NewGuid().ToString());
                        if (stream != null)
                        {
                            using (var tmpStream = new MemoryStream())
                            {
                                stream.CopyTo(tmpStream);
                                downloadUri = DocumentServiceConnector.GetExternalUri(tmpStream, newExtension, key);
                            }
                        }

                        DocumentServiceConnector.GetConvertedUri(downloadUri, newExtension, currentExt, key, false, out downloadUri);

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
                    file = fileDao.SaveFile(file, tmpStream);
                }
            }

            checkRight = FileTracker.ProlongEditing(file.ID, tabId, true, SecurityContext.CurrentAccount.ID);
            if (checkRight) FileTracker.ChangeRight(file.ID, SecurityContext.CurrentAccount.ID, false);

            FileMarker.MarkAsNew(file);
            FileMarker.RemoveMarkAsNew(file);
            return file;
        }

        public static void TrackEditing(string fileId, Guid tabId, Guid userId, bool fixedVersion, string shareLinkKey, bool editingAlone = false)
        {
            bool checkRight;
            if (FileTracker.GetEditingBy(fileId).Contains(userId))
            {
                checkRight = FileTracker.ProlongEditing(fileId, tabId, fixedVersion, userId, editingAlone);
                if (!checkRight) return;
            }

            File file;
            bool editLink;
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                editLink = FileShareLink.Check(shareLinkKey, false, fileDao, out file);
                if (file == null)
                    file = fileDao.GetFile(fileId);
            }

            if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            var fileSecurity = Global.GetFilesSecurity();
            if (!editLink && (!fileSecurity.CanEdit(file, userId) && !fileSecurity.CanReview(file, userId) || CoreContext.UserManager.GetUsers(userId).IsVisitor())) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
            if (FileLockedForMe(file.ID, userId)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
            if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

            checkRight = FileTracker.ProlongEditing(fileId, tabId, fixedVersion, userId, editingAlone);
            if (checkRight)
            {
                FileTracker.ChangeRight(fileId, userId, false);
            }
        }


        public static File UpdateToVersionFile(object fileId, int version, bool checkRight = true)
        {
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                if (version < 1) throw new ArgumentNullException("version");

                var fromFile = fileDao.GetFile(fileId);
                if (fromFile == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);

                if (fromFile.Version != version)
                    fromFile = fileDao.GetFile(fromFile.ID, Math.Min(fromFile.Version, version));

                if (fromFile == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                if (checkRight && (!Global.GetFilesSecurity().CanEdit(fromFile) || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                if (FileLockedForMe(fromFile.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
                if (checkRight && FileTracker.IsEditing(fromFile.ID)) throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);
                if (fromFile.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

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
                        Title = fromFile.Title,
                        ContentLength = fromFile.ContentLength,
                        FileStatus = fromFile.FileStatus,
                        FolderID = fromFile.FolderID,
                        CreateBy = fromFile.CreateBy,
                        CreateOn = fromFile.CreateOn,
                        ModifiedBy = fromFile.ModifiedBy,
                        ModifiedOn = fromFile.ModifiedOn,
                        ConvertedType = fromFile.ConvertedType,
                        Comment = string.Format(FilesCommonResource.CommentRevert, fromFile.ModifiedOnString),
                    };

                    using (var stream = fileDao.GetFileStream(fromFile))
                    {
                        newFile = fileDao.SaveFile(newFile, stream);
                    }

                    FileMarker.MarkAsNew(newFile);

                    SetFileStatus(newFile);

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
                    fileDao.ContinueVersion(fileVersion.ID, fileVersion.Version);
                    lastVersionFile.VersionGroup--;
                }
                else
                {
                    if (!FileTracker.IsEditing(lastVersionFile.ID))
                    {
                        if (fileVersion.Version == lastVersionFile.Version)
                        {
                            lastVersionFile = UpdateToVersionFile(fileVersion.ID, fileVersion.Version, checkRight);
                        }

                        fileDao.CompleteVersion(fileVersion.ID, fileVersion.Version);
                        lastVersionFile.VersionGroup++;
                    }
                }

                SetFileStatus(lastVersionFile);

                return lastVersionFile;
            }
        }
    }
}