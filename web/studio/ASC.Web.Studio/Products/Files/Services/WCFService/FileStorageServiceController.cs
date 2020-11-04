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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Http;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.ElasticSearch;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Core.Users;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core.Entries;
using ASC.Web.Files.Core.Search;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Services.NotifyService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using Resources;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;
using UrlShortener = ASC.Web.Core.Utility.UrlShortener;

namespace ASC.Web.Files.Services.WCFService
{
    [Authorize]
    [FileExceptionFilter]
    public class FileStorageServiceController : ApiController, IFileStorageService
    {
        private static readonly FileOperationsManager fileOperations = new FileOperationsManager();
        private static readonly FileEntrySerializer serializer = new FileEntrySerializer();

        #region Folder Manager

        [ActionName("folders-folder"), HttpGet]
        public Folder GetFolder(String folderId)
        {
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(folderId);

                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanRead(folder), FilesCommonResource.ErrorMassage_SecurityException_ReadFolder);

                return folder;
            }
        }

        [ActionName("folders-subfolders"), HttpGet]
        public ItemList<Folder> GetFolders(String parentId)
        {
            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                try
                {
                    int total;
                    var folders = EntryManager.GetEntries(folderDao, fileDao, folderDao.GetFolder(parentId), 0, 0, FilterType.FoldersOnly, false, Guid.Empty, string.Empty, false, false, new OrderBy(SortedByType.AZ, true), out total);
                    return new ItemList<Folder>(folders.OfType<Folder>());
                }
                catch (Exception e)
                {
                    throw GenerateException(e);
                }
            }
        }

        [ActionName("folders-path"), HttpGet]
        public ItemList<object> GetPath(String folderId)
        {
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(folderId);

                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanRead(folder), FilesCommonResource.ErrorMassage_SecurityException_ViewFolder);

                return new ItemList<object>(EntryManager.GetBreadCrumbs(folderId, folderDao).Select(f => f.ID));
            }
        }

        public DataWrapper GetFolderItems(String parentId, int from, int count, FilterType filter, bool subjectGroup, String ssubject, String searchText, bool searchInContent, bool withSubfolders, OrderBy orderBy)
        {
            var subjectId = string.IsNullOrEmpty(ssubject) ? Guid.Empty : new Guid(ssubject);

            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                Folder parent = null;
                try
                {
                    parent = folderDao.GetFolder(parentId);
                    if (parent != null && !string.IsNullOrEmpty(parent.Error)) throw new Exception(parent.Error);
                }
                catch (Exception e)
                {
                    if (parent != null && parent.ProviderEntry)
                    {
                        throw GenerateException(new Exception(FilesCommonResource.ErrorMassage_SharpBoxException, e));
                    }
                    throw GenerateException(e);
                }

                ErrorIf(parent == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanRead(parent), FilesCommonResource.ErrorMassage_SecurityException_ViewFolder);
                ErrorIf(parent.RootFolderType == FolderType.TRASH && !Equals(parent.ID, Global.FolderTrash), FilesCommonResource.ErrorMassage_ViewTrashItem);

                if (orderBy != null)
                {
                    FilesSettings.DefaultOrder = orderBy;
                }
                else
                {
                    orderBy = FilesSettings.DefaultOrder;
                }
                if (Equals(parent.ID, Global.FolderShare) && orderBy.SortedBy == SortedByType.DateAndTime)
                    orderBy.SortedBy = SortedByType.New;

                int total;
                IEnumerable<FileEntry> entries;
                try
                {
                    entries = EntryManager.GetEntries(folderDao, fileDao, parent, from, count, filter, subjectGroup, subjectId, searchText, searchInContent, withSubfolders, orderBy, out total);
                }
                catch (Exception e)
                {
                    if (parent.ProviderEntry)
                    {
                        throw GenerateException(new Exception(FilesCommonResource.ErrorMassage_SharpBoxException, e));
                    }
                    throw GenerateException(e);
                }

                var breadCrumbs = EntryManager.GetBreadCrumbs(parentId, folderDao);

                var prevVisible = breadCrumbs.ElementAtOrDefault(breadCrumbs.Count() - 2);
                if (prevVisible != null)
                {
                    parent.ParentFolderID = prevVisible.ID;
                }

                parent.Shareable = FileSharing.CanSetAccess(parent)
                    || parent.FolderType == FolderType.SHARE
                    || parent.RootFolderType == FolderType.Privacy;

                entries = entries.Where(x => x.FileEntryType == FileEntryType.Folder || !FileConverter.IsConverting((File)x));

                var result = new DataWrapper
                    {
                        Total = total,
                        Entries = new ItemList<FileEntry>(entries.ToList()),
                        FolderPathParts = new ItemList<object>(breadCrumbs.Select(f => f.ID)),
                        FolderInfo = parent,
                        RootFoldersIdMarkedAsNew = FileMarker.GetRootFoldersIdMarkedAsNew()
                    };

                return result;
            }
        }

        [ActionName("folders"), HttpPost]
        public object GetFolderItemsXml(String parentId, int from, int count, FilterType filter, bool subjectGroup, String subjectID, String search, bool searchInContent, bool withSubfolders, [FromBody] OrderBy orderBy)
        {
            var folderItems = GetFolderItems(parentId, from, count, filter, subjectGroup, subjectID, search, searchInContent, withSubfolders, orderBy);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(serializer.ToXml(folderItems))
                };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            return response;
        }

        [ActionName("folders-entries"), HttpPost]
        public ItemList<FileEntry> GetItems([FromBody] ItemList<String> items, FilterType filter, bool subjectGroup, String subjectID, String search)
        {
            List<object> filesId;
            List<object> foldersId;
            ParseArrayItems(items, out foldersId, out filesId);

            var subjectId = string.IsNullOrEmpty(subjectID) ? Guid.Empty : new Guid(subjectID);

            var entries = Enumerable.Empty<FileEntry>();

            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var folders = folderDao.GetFolders(foldersId.ToArray()).Cast<FileEntry>();
                folders = FileSecurity.FilterRead(folders);
                entries = entries.Concat(folders);

                var files = fileDao.GetFiles(filesId.ToArray()).Cast<FileEntry>();
                files = FileSecurity.FilterRead(files);
                entries = entries.Concat(files);

                entries = EntryManager.FilterEntries(entries, filter, subjectGroup, subjectId, search, true);

                foreach (var fileEntry in entries)
                {
                    if (fileEntry.RootFolderType == FolderType.USER
                        && !Equals(fileEntry.RootFolderCreator, SecurityContext.CurrentAccount.ID)
                        && !Global.GetFilesSecurity().CanRead(folderDao.GetFolder(fileEntry.FolderIdDisplay)))
                        fileEntry.FolderIdDisplay = Global.FolderShare;
                }
            }

            EntryManager.SetFileStatus(entries.OfType<File>().Where(r => r.ID != null).ToList());

            return new ItemList<FileEntry>(entries);
        }

        [ActionName("folders-create"), HttpGet]
        public Folder CreateNewFolder(String parentId, String title)
        {
            if (string.IsNullOrEmpty(title) || String.IsNullOrEmpty(parentId)) throw new ArgumentException();

            using (var folderDao = GetFolderDao())
            {
                var parent = folderDao.GetFolder(parentId);
                ErrorIf(parent == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanCreate(parent), FilesCommonResource.ErrorMassage_SecurityException_Create);

                try
                {
                    var folderId = folderDao.SaveFolder(new Folder { Title = title, ParentFolderID = parent.ID });
                    var folder = folderDao.GetFolder(folderId);
                    FilesMessageService.Send(folder, GetHttpHeaders(), MessageAction.FolderCreated, folder.Title);

                    return folder;
                }
                catch (Exception e)
                {
                    throw GenerateException(e);
                }
            }
        }

        [ActionName("folders-rename"), HttpGet]
        public Folder FolderRename(String folderId, String title)
        {
            using (var tagDao = GetTagDao())
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(folderId);
                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanEdit(folder), FilesCommonResource.ErrorMassage_SecurityException_RenameFolder);
                if (!Global.GetFilesSecurity().CanDelete(folder) && CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_RenameFolder);
                ErrorIf(folder.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem);

                var folderAccess = folder.Access;

                if (String.Compare(folder.Title, title, false) != 0)
                {
                    var newFolderID = folderDao.RenameFolder(folder, title);
                    folder = folderDao.GetFolder(newFolderID);
                    folder.Access = folderAccess;

                    FilesMessageService.Send(folder, GetHttpHeaders(), MessageAction.FolderRenamed, folder.Title);

                    if (!folder.ProviderEntry)
                    {
                        FactoryIndexer<FoldersWrapper>.IndexAsync(folder);
                    }
                }

                var tag = tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, folder).FirstOrDefault();
                if (tag != null)
                {
                    folder.NewForMe = tag.Count;
                }

                if (folder.RootFolderType == FolderType.USER
                    && !Equals(folder.RootFolderCreator, SecurityContext.CurrentAccount.ID)
                    && !Global.GetFilesSecurity().CanRead(folderDao.GetFolder(folder.ParentFolderID)))
                    folder.FolderIdDisplay = Global.FolderShare;

                return folder;
            }
        }

        #endregion

        #region File Manager

        [ActionName("folders-files-getversion"), HttpGet]
        public File GetFile(String fileId, int version)
        {
            using (var fileDao = GetFileDao())
            {
                fileDao.InvalidateCache(fileId);

                var file = version > 0
                               ? fileDao.GetFile(fileId, version)
                               : fileDao.GetFile(fileId);
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                EntryManager.SetFileStatus(file);

                if (file.RootFolderType == FolderType.USER
                    && !Equals(file.RootFolderCreator, SecurityContext.CurrentAccount.ID))
                {
                    using (var folderDao = GetFolderDao())
                    {
                        if (!Global.GetFilesSecurity().CanRead(folderDao.GetFolder(file.FolderID)))
                            file.FolderIdDisplay = Global.FolderShare;
                    }
                }

                return file;
            }
        }

        [ActionName("folders-files-siblings"), HttpPost]
        public ItemList<File> GetSiblingsFile(String fileId, String parentId, FilterType filter, bool subjectGroup, String subjectID, String search, bool searchInContent, bool withSubfolders, [FromBody] OrderBy orderBy)
        {
            var subjectId = string.IsNullOrEmpty(subjectID) ? Guid.Empty : new Guid(subjectID);

            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var file = fileDao.GetFile(fileId);
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                var parent = folderDao.GetFolder(string.IsNullOrEmpty(parentId) ? file.FolderID : parentId);
                ErrorIf(parent == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(parent.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem);

                if (filter == FilterType.FoldersOnly)
                {
                    return new ItemList<File>();
                }
                if (filter == FilterType.None)
                {
                    filter = FilterType.FilesOnly;
                }

                if (orderBy == null)
                {
                    orderBy = FilesSettings.DefaultOrder;
                }
                if (Equals(parent.ID, Global.FolderShare) && orderBy.SortedBy == SortedByType.DateAndTime)
                {
                    orderBy.SortedBy = SortedByType.New;
                }

                var entries = Enumerable.Empty<FileEntry>();

                if (!FileSecurity.CanRead(parent))
                {
                    file.FolderID = Global.FolderShare;
                    entries = entries.Concat(new[] { file });
                }
                else
                {
                    try
                    {
                        int total;
                        entries = EntryManager.GetEntries(folderDao, fileDao, parent, 0, 0, filter, subjectGroup, subjectId, search, searchInContent, withSubfolders, orderBy, out total);
                    }
                    catch (Exception e)
                    {
                        if (parent.ProviderEntry)
                        {
                            throw GenerateException(new Exception(FilesCommonResource.ErrorMassage_SharpBoxException, e));
                        }
                        throw GenerateException(e);
                    }
                }

                var previewedType = new[] { FileType.Image, FileType.Audio, FileType.Video };

                var result =
                    FileSecurity.FilterRead(entries)
                                .OfType<File>()
                                .Where(f => previewedType.Contains(FileUtility.GetFileTypeByFileName(f.Title)));

                return new ItemList<File>(result);
            }
        }

        [ActionName("folders-files-createfile"), HttpGet]
        public File CreateNewFile(String parentId, String title, String templateId)
        {
            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                Folder folder = null;
                if (!string.IsNullOrEmpty(parentId))
                {
                    folder = folderDao.GetFolder(parentId);

                    if (!FileSecurity.CanCreate(folder))
                    {
                        folder = null;
                    }
                }
                if (folder == null)
                {
                    folder = folderDao.GetFolder(Global.FolderMy);
                }

                var file = new File
                    {
                        FolderID = folder.ID,
                        Comment = FilesCommonResource.CommentCreate,
                    };

                if (string.IsNullOrEmpty(title))
                {
                    title = UserControlsCommonResource.NewDocument + ".docx";
                }

                var fileExt = FileUtility.GetInternalExtension(title);
                if (!FileUtility.InternalExtension.Values.Contains(fileExt))
                {
                    fileExt = FileUtility.InternalExtension[FileType.Document];
                    file.Title = title + fileExt;
                }
                else
                {
                    file.Title = FileUtility.ReplaceFileExtension(title, fileExt);
                }

                if (string.IsNullOrEmpty(templateId))
                {
                    var culture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();
                    var storeTemplate = GetStoreTemplate();

                    var path = FileConstant.NewDocPath + culture + "/";
                    if (!storeTemplate.IsDirectory(path))
                    {
                        path = FileConstant.NewDocPath + "default/";
                    }

                    path += "new" + fileExt;

                    try
                    {
                        using (var stream = storeTemplate.GetReadStream("", path))
                        {
                            file.ContentLength = stream.CanSeek ? stream.Length : storeTemplate.GetFileSize(path);
                            file = fileDao.SaveFile(file, stream);
                        }
                    }
                    catch (Exception e)
                    {
                        throw GenerateException(e);
                    }
                }
                else
                {
                    var template = fileDao.GetFile(templateId);
                    ErrorIf(template == null, FilesCommonResource.ErrorMassage_FileNotFound);
                    ErrorIf(!FileSecurity.CanRead(template), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                    try
                    {
                        using (var stream = fileDao.GetFileStream(template))
                        {
                            file.ContentLength = template.ContentLength;
                            file = fileDao.SaveFile(file, stream);
                        }
                    }
                    catch (Exception e)
                    {
                        throw GenerateException(e);
                    }
                }

                FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileCreated, file.Title);

                FileMarker.MarkAsNew(file);

                return file;
            }
        }

        [ActionName("trackeditfile"), HttpGet, AllowAnonymous]
        public KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String doc = null, bool isFinish = false)
        {
            try
            {
                var id = FileShareLink.Parse(doc);
                if (string.IsNullOrEmpty(id))
                {
                    if (!SecurityContext.IsAuthenticated) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);
                    if (!string.IsNullOrEmpty(doc)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);
                    id = fileId;
                }

                if (docKeyForTrack != DocumentServiceHelper.GetDocKey(id, -1, DateTime.MinValue)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

                if (isFinish)
                {
                    FileTracker.Remove(id, tabId);
                    Global.SocketManager.FilesChangeEditors(id, true);
                }
                else
                {
                    EntryManager.TrackEditing(id, tabId, SecurityContext.CurrentAccount.ID, doc);
                }

                return new KeyValuePair<bool, string>(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }

        [ActionName("checkediting"), HttpPost]
        public ItemDictionary<String, String> CheckEditing([FromBody] ItemList<String> filesId)
        {
            ErrorIf(!SecurityContext.IsAuthenticated, FilesCommonResource.ErrorMassage_SecurityException);
            var result = new ItemDictionary<string, string>();

            using (var fileDao = GetFileDao())
            {
                var ids = filesId.Where(FileTracker.IsEditing).Select(id => (object)id).ToArray();

                foreach (var file in fileDao.GetFiles(ids))
                {
                    if (file == null
                        || !FileSecurity.CanEdit(file)
                            && !FileSecurity.CanCustomFilterEdit(file)
                            && !FileSecurity.CanReview(file)
                            && !FileSecurity.CanFillForms(file)
                            && !FileSecurity.CanComment(file)) continue;

                    var usersId = FileTracker.GetEditingBy(file.ID);
                    var value = string.Join(", ", usersId.Select(userId => Global.GetUserName(userId, true)).ToArray());
                    result[file.ID.ToString()] = value;
                }
            }

            return result;
        }

        public File SaveEditing(String fileId, string fileExtension, string fileuri, Stream stream, String doc = null, bool forcesave = false)
        {
            try
            {
                if (!forcesave && FileTracker.IsEditingAlone(fileId))
                {
                    FileTracker.Remove(fileId);
                }

                var file = EntryManager.SaveEditing(fileId, fileExtension, fileuri, stream, doc, forcesave: forcesave ? ForcesaveType.User : ForcesaveType.None);

                if (file != null)
                    FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileUpdated, file.Title);

                Global.SocketManager.FilesChangeEditors(fileId, !forcesave);
                return file;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        public File UpdateFileStream(String fileId, Stream stream, bool encrypted, bool forcesave)
        {
            try
            {
                if (!forcesave && FileTracker.IsEditing(fileId))
                {
                    FileTracker.Remove(fileId);
                }

                var file = EntryManager.SaveEditing(fileId,
                    null,
                    null,
                    stream,
                    null,
                    encrypted ? FilesCommonResource.CommentEncrypted : null,
                    encrypted: encrypted,
                    forcesave: forcesave ? ForcesaveType.User : ForcesaveType.None);

                if (file != null)
                    FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileUpdated, file.Title);

                Global.SocketManager.FilesChangeEditors(fileId, true);
                return file;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        [ActionName("startedit"), HttpGet, AllowAnonymous]
        public string StartEdit(String fileId, bool editingAlone = false, String doc = null)
        {
            try
            {
                IThirdPartyApp app;
                if (editingAlone)
                {
                    ErrorIf(FileTracker.IsEditing(fileId), FilesCommonResource.ErrorMassage_SecurityException_EditFileTwice);

                    app = ThirdPartySelector.GetAppByFileId(fileId);
                    if (app == null)
                    {
                        EntryManager.TrackEditing(fileId, Guid.Empty, SecurityContext.CurrentAccount.ID, doc, true);
                    }

                    //without StartTrack, track via old scheme
                    return DocumentServiceHelper.GetDocKey(fileId, -1, DateTime.MinValue);
                }

                DocumentService.Configuration configuration;

                app = ThirdPartySelector.GetAppByFileId(fileId);
                if (app == null)
                {
                    DocumentServiceHelper.GetParams(fileId, -1, doc, true, true, false, out configuration);
                }
                else
                {
                    bool editable;
                    var file = app.GetFile(fileId, out editable);
                    DocumentServiceHelper.GetParams(file, true, editable ? FileShare.ReadWrite : FileShare.Read, false, editable, editable, editable, false, out configuration);
                }

                ErrorIf(!configuration.EditorConfig.ModeWrite
                        || !(configuration.Document.Permissions.Edit
                             || configuration.Document.Permissions.ModifyFilter
                             || configuration.Document.Permissions.Review
                             || configuration.Document.Permissions.FillForms
                             || configuration.Document.Permissions.Comment),
                        !string.IsNullOrEmpty(configuration.ErrorMessage) ? configuration.ErrorMessage : FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                var key = configuration.Document.Key;

                if (!DocumentServiceTracker.StartTrack(fileId, key))
                {
                    throw new Exception(FilesCommonResource.ErrorMassage_StartEditing);
                }

                return key;
            }
            catch (Exception e)
            {
                FileTracker.Remove(fileId);
                throw GenerateException(e);
            }
        }

        [ActionName("folders-files-rename"), HttpGet]
        public File FileRename(String fileId, String title)
        {
            try
            {
                File file;
                var renamed = EntryManager.FileRename(fileId, title, out file);
                if (renamed)
                {
                    FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileRenamed, file.Title);

                    if (!file.ProviderEntry)
                    {
                        FactoryIndexer<FilesWrapper>.UpdateAsync(file, true, r=> r.Title);
                    }
                }

                if (file.RootFolderType == FolderType.USER
                    && !Equals(file.RootFolderCreator, SecurityContext.CurrentAccount.ID))
                {
                    using (var folderDao = GetFolderDao())
                    {
                        if (!Global.GetFilesSecurity().CanRead(folderDao.GetFolder(file.FolderID)))
                            file.FolderIdDisplay = Global.FolderShare;
                    }
                }

                return file;
            }
            catch (Exception ex)
            {
                throw GenerateException(ex);
            }
        }

        [ActionName("folders-files-history"), HttpGet]
        public ItemList<File> GetFileHistory(String fileId)
        {
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                return new ItemList<File>(fileDao.GetFileHistory(fileId));
            }
        }

        [ActionName("folders-files-updateToVersion"), HttpGet]
        public KeyValuePair<File, ItemList<File>> UpdateToVersion(String fileId, int version)
        {
            var file = EntryManager.UpdateToVersionFile(fileId, version);
            FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileRestoreVersion, file.Title, version.ToString(CultureInfo.InvariantCulture));

            if (file.RootFolderType == FolderType.USER
                && !Equals(file.RootFolderCreator, SecurityContext.CurrentAccount.ID))
            {
                using (var folderDao = GetFolderDao())
                {
                    if (!Global.GetFilesSecurity().CanRead(folderDao.GetFolder(file.FolderID)))
                        file.FolderIdDisplay = Global.FolderShare;
                }
            }

            return new KeyValuePair<File, ItemList<File>>(file, GetFileHistory(fileId));
        }

        [ActionName("folders-files-updateComment"), HttpGet]
        public string UpdateComment(String fileId, int version, String comment)
        {
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId, version);
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanEdit(file) || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor(), FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                ErrorIf(EntryManager.FileLockedForMe(file.ID), FilesCommonResource.ErrorMassage_LockedFile);
                ErrorIf(file.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem);

                comment = fileDao.UpdateComment(fileId, version, comment);

                FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileUpdatedRevisionComment, file.Title, version.ToString(CultureInfo.InvariantCulture));
            }

            return comment;
        }

        [ActionName("folders-files-completeVersion"), HttpGet]
        public KeyValuePair<File, ItemList<File>> CompleteVersion(String fileId, int version, bool continueVersion)
        {
            var file = EntryManager.CompleteVersionFile(fileId, version, continueVersion);

            FilesMessageService.Send(file, GetHttpHeaders(),
                                     continueVersion ? MessageAction.FileDeletedVersion : MessageAction.FileCreatedVersion,
                                     file.Title, version == 0 ? (file.Version - 1).ToString(CultureInfo.InvariantCulture) : version.ToString(CultureInfo.InvariantCulture));

            if (file.RootFolderType == FolderType.USER
                && !Equals(file.RootFolderCreator, SecurityContext.CurrentAccount.ID))
            {
                using (var folderDao = GetFolderDao())
                {
                    if (!Global.GetFilesSecurity().CanRead(folderDao.GetFolder(file.FolderID)))
                        file.FolderIdDisplay = Global.FolderShare;
                }
            }

            return new KeyValuePair<File, ItemList<File>>(file, GetFileHistory(fileId));
        }

        [ActionName("folders-files-lock"), HttpGet]
        public File LockFile(String fileId, bool lockfile)
        {
            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId);

                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanEdit(file) || lockfile && CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor(), FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                ErrorIf(file.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem);

                var tagLocked = tagDao.GetTags(file.ID, FileEntryType.File, TagType.Locked).FirstOrDefault();

                ErrorIf(tagLocked != null
                        && tagLocked.Owner != SecurityContext.CurrentAccount.ID
                        && !Global.IsAdministrator
                        && (file.RootFolderType != FolderType.USER || file.RootFolderCreator != SecurityContext.CurrentAccount.ID), FilesCommonResource.ErrorMassage_LockedFile);

                if (lockfile)
                {
                    if (tagLocked == null)
                    {
                        tagLocked = new Tag("locked", TagType.Locked, SecurityContext.CurrentAccount.ID, file, 0);

                        tagDao.SaveTags(tagLocked);
                    }

                    var usersDrop = FileTracker.GetEditingBy(file.ID).Where(uid => uid != SecurityContext.CurrentAccount.ID).Select(u => u.ToString()).ToArray();
                    if (usersDrop.Any())
                    {
                        var fileStable = file.Forcesave == ForcesaveType.None ? file : fileDao.GetFileStable(file.ID, file.Version);
                        var docKey = DocumentServiceHelper.GetDocKey(fileStable);
                        DocumentServiceHelper.DropUser(docKey, usersDrop, file.ID);
                    }

                    FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileLocked, file.Title);
                }
                else
                {
                    if (tagLocked != null)
                    {
                        tagDao.RemoveTags(tagLocked);

                        FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileUnlocked, file.Title);
                    }

                    if (!file.ProviderEntry)
                    {
                        file = EntryManager.CompleteVersionFile(file.ID, 0, false);
                        UpdateComment(file.ID.ToString(), file.Version, FilesCommonResource.UnlockComment);
                    }
                }

                EntryManager.SetFileStatus(file);

                if (file.RootFolderType == FolderType.USER
                    && !Equals(file.RootFolderCreator, SecurityContext.CurrentAccount.ID))
                {
                    using (var folderDao = GetFolderDao())
                    {
                        if (!Global.GetFilesSecurity().CanRead(folderDao.GetFolder(file.FolderID)))
                            file.FolderIdDisplay = Global.FolderShare;
                    }
                }

                return file;
            }
        }

        [ActionName("edit-history"), HttpGet, AllowAnonymous]
        public ItemList<EditHistory> GetEditHistory(String fileId, String doc = null)
        {
            using (var fileDao = GetFileDao())
            {
                File file;
                var readLink = FileShareLink.Check(doc, true, fileDao, out file);
                if (file == null)
                    file = fileDao.GetFile(fileId);

                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!readLink && !FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                ErrorIf(file.ProviderEntry, FilesCommonResource.ErrorMassage_BadRequest);

                return new ItemList<EditHistory>(fileDao.GetEditHistory(file.ID));
            }
        }

        [ActionName("edit-diff-url"), HttpGet, AllowAnonymous]
        public EditHistoryData GetEditDiffUrl(String fileId, int version = 0, String doc = null)
        {
            using (var fileDao = GetFileDao())
            {
                File file;
                var readLink = FileShareLink.Check(doc, true, fileDao, out file);

                if (file != null)
                {
                    fileId = file.ID.ToString();
                }

                if (file == null
                    || version > 0 && file.Version != version)
                {
                    file = version > 0
                               ? fileDao.GetFile(fileId, version)
                               : fileDao.GetFile(fileId);
                }

                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!readLink && !FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                ErrorIf(file.ProviderEntry, FilesCommonResource.ErrorMassage_BadRequest);

                var result = new EditHistoryData
                    {
                        Key = DocumentServiceHelper.GetDocKey(file),
                        Url = DocumentServiceConnector.ReplaceCommunityAdress(PathProvider.GetFileStreamUrl(file, doc)),
                        Version = version,
                    };

                if (fileDao.ContainChanges(file.ID, file.Version))
                {
                    string previouseKey;
                    string sourceFileUrl;
                    if (file.Version > 1)
                    {
                        var previousFileStable = fileDao.GetFileStable(file.ID, file.Version - 1);
                        ErrorIf(previousFileStable == null, FilesCommonResource.ErrorMassage_FileNotFound);

                        sourceFileUrl = PathProvider.GetFileStreamUrl(previousFileStable, doc);

                        previouseKey = DocumentServiceHelper.GetDocKey(previousFileStable);
                    }
                    else
                    {
                        var culture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();
                        var storeTemplate = GetStoreTemplate();

                        var path = FileConstant.NewDocPath + culture + "/";
                        if (!storeTemplate.IsDirectory(path))
                        {
                            path = FileConstant.NewDocPath + "default/";
                        }

                        var fileExt = FileUtility.GetFileExtension(file.Title);

                        path += "new" + fileExt;

                        sourceFileUrl = storeTemplate.GetUri("", path).ToString();
                        sourceFileUrl = CommonLinkUtility.GetFullAbsolutePath(sourceFileUrl);

                        previouseKey = DocumentServiceConnector.GenerateRevisionId(Guid.NewGuid().ToString());
                    }

                    result.Previous = new EditHistoryUrl
                        {
                            Key = previouseKey,
                            Url = DocumentServiceConnector.ReplaceCommunityAdress(sourceFileUrl),
                        };
                    result.ChangesUrl = PathProvider.GetFileChangesUrl(file, doc);
                }

                result.Token = DocumentServiceHelper.GetSignature(result);

                return result;
            }
        }

        [ActionName("restore-version"), HttpGet, AllowAnonymous]
        public ItemList<EditHistory> RestoreVersion(String fileId, int version, String url = null, String doc = null)
        {
            File file;
            if (string.IsNullOrEmpty(url))
            {
                file = EntryManager.UpdateToVersionFile(fileId, version, doc);
            }
            else
            {
                string modifiedOnString;
                using (var fileDao = GetFileDao())
                {
                    var fromFile = fileDao.GetFile(fileId, version);
                    modifiedOnString = fromFile.ModifiedOnString;
                }
                file = EntryManager.SaveEditing(fileId, null, url, null, doc, string.Format(FilesCommonResource.CommentRevertChanges, modifiedOnString));
            }

            FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileRestoreVersion, file.Title, version.ToString(CultureInfo.InvariantCulture));

            using (var fileDao = GetFileDao())
            {
                return new ItemList<EditHistory>(fileDao.GetEditHistory(file.ID));
            }
        }

        [ActionName("presigned"), HttpGet]
        public Web.Core.Files.DocumentService.FileLink GetPresignedUri(String fileId)
        {
            var file = GetFile(fileId, -1);
            var result = new Web.Core.Files.DocumentService.FileLink
                {
                    FileType = FileUtility.GetFileExtension(file.Title),
                    Url = DocumentServiceConnector.ReplaceCommunityAdress(PathProvider.GetFileStreamUrl(file))
                };

            result.Token = DocumentServiceHelper.GetSignature(result);

            return result;
        }

        #endregion

        #region News

        [ActionName("getnews"), HttpGet]
        public object GetNewItems(String folderId)
        {
            try
            {
                Folder folder;
                using (var folderDao = GetFolderDao())
                {
                    folder = folderDao.GetFolder(folderId);
                }

                var result = FileMarker.MarkedItems(folder);

                result = new List<FileEntry>(EntryManager.SortEntries(result, new OrderBy(SortedByType.DateAndTime, false)));

                if (!result.ToList().Any())
                {
                    MarkAsRead(new ItemList<string> { "folder_" + folderId });
                }

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(serializer.ToXml(new ItemList<FileEntry>(result)));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                return response;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        [ActionName("markasread"), HttpPost]
        public ItemList<FileOperationResult> MarkAsRead([FromBody] ItemList<String> items)
        {
            if (items.Count == 0) return GetTasksStatuses();

            List<object> foldersId;
            List<object> filesId;
            ParseArrayItems(items, out foldersId, out filesId);

            return fileOperations.MarkAsRead(foldersId, filesId);
        }

        #endregion

        #region ThirdParty

        public ItemList<ThirdPartyParams> GetThirdParty()
        {
            using (var providerDao = GetProviderDao())
            {
                if (providerDao == null) return new ItemList<ThirdPartyParams>();

                var providersInfo = providerDao.GetProvidersInfo();

                var resultList = providersInfo
                    .Select(r =>
                            new ThirdPartyParams
                                {
                                    CustomerTitle = r.CustomerTitle,
                                    Corporate = r.RootFolderType == FolderType.COMMON,
                                    ProviderId = r.ID.ToString(),
                                    ProviderKey = r.ProviderKey
                                }
                    );
                return new ItemList<ThirdPartyParams>(resultList.ToList());
            }
        }

        [ActionName("thirdparty-list"), HttpGet]
        public ItemList<Folder> GetThirdPartyFolder(int folderType = 0)
        {
            if (!FilesSettings.EnableThirdParty) return new ItemList<Folder>();

            using (var providerDao = GetProviderDao())
            {
                if (providerDao == null) return new ItemList<Folder>();

                var providersInfo = providerDao.GetProvidersInfo((FolderType)folderType);

                var folders = providersInfo.Select(providerInfo =>
                    {
                        var folder = EntryManager.GetFakeThirdpartyFolder(providerInfo);
                        folder.NewForMe = folder.RootFolderType == FolderType.COMMON ? 1 : 0;
                        return folder;
                    });

                return new ItemList<Folder>(folders);
            }
        }

        [ActionName("thirdparty-save"), HttpPost]
        public Folder SaveThirdParty([FromBody] ThirdPartyParams thirdPartyParams)
        {
            using (var folderDao = GetFolderDao())
            using (var providerDao = GetProviderDao())
            {
                if (providerDao == null) return null;

                ErrorIf(thirdPartyParams == null, FilesCommonResource.ErrorMassage_BadRequest);
                var parentFolder = folderDao.GetFolder(thirdPartyParams.Corporate && !CoreContext.Configuration.Personal ? Global.FolderCommon : Global.FolderMy);
                ErrorIf(!FileSecurity.CanCreate(parentFolder), FilesCommonResource.ErrorMassage_SecurityException_Create);
                ErrorIf(!FilesSettings.EnableThirdParty, FilesCommonResource.ErrorMassage_SecurityException_Create);

                var lostFolderType = FolderType.USER;
                var folderType = thirdPartyParams.Corporate ? FolderType.COMMON : FolderType.USER;

                int curProviderId;

                MessageAction messageAction;
                if (string.IsNullOrEmpty(thirdPartyParams.ProviderId))
                {
                    ErrorIf(!ThirdpartyConfiguration.SupportInclusion
                            ||
                            (!FilesSettings.EnableThirdParty
                             && !CoreContext.Configuration.Personal)
                            , FilesCommonResource.ErrorMassage_SecurityException_Create);

                    thirdPartyParams.CustomerTitle = Global.ReplaceInvalidCharsAndTruncate(thirdPartyParams.CustomerTitle);
                    ErrorIf(string.IsNullOrEmpty(thirdPartyParams.CustomerTitle), FilesCommonResource.ErrorMassage_InvalidTitle);

                    try
                    {
                        curProviderId = providerDao.SaveProviderInfo(thirdPartyParams.ProviderKey, thirdPartyParams.CustomerTitle, thirdPartyParams.AuthData, folderType);
                        messageAction = MessageAction.ThirdPartyCreated;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw GenerateException(e, true);
                    }
                    catch (Exception e)
                    {
                        throw GenerateException(e);
                    }
                }
                else
                {
                    curProviderId = Convert.ToInt32(thirdPartyParams.ProviderId);

                    var lostProvider = providerDao.GetProviderInfo(curProviderId);
                    ErrorIf(lostProvider.Owner != SecurityContext.CurrentAccount.ID, FilesCommonResource.ErrorMassage_SecurityException);

                    lostFolderType = lostProvider.RootFolderType;
                    if (lostProvider.RootFolderType == FolderType.COMMON && !thirdPartyParams.Corporate)
                    {
                        var lostFolder = folderDao.GetFolder(lostProvider.RootFolderId);
                        FileMarker.RemoveMarkAsNewForAll(lostFolder);
                    }

                    curProviderId = providerDao.UpdateProviderInfo(curProviderId, thirdPartyParams.CustomerTitle, thirdPartyParams.AuthData, folderType);
                    messageAction = MessageAction.ThirdPartyUpdated;
                }

                var provider = providerDao.GetProviderInfo(curProviderId);
                provider.InvalidateStorage();

                var folder = folderDao.GetFolder(provider.RootFolderId);
                ErrorIf(!FileSecurity.CanRead(folder), FilesCommonResource.ErrorMassage_SecurityException_ViewFolder);

                FilesMessageService.Send(parentFolder, GetHttpHeaders(), messageAction, folder.ID.ToString(), provider.ProviderKey);

                if (thirdPartyParams.Corporate && lostFolderType != FolderType.COMMON)
                {
                    FileMarker.MarkAsNew(folder);
                }

                return folder;
            }
        }

        [ActionName("thirdparty-delete"), HttpGet]
        public object DeleteThirdParty(String providerId)
        {
            using (var providerDao = GetProviderDao())
            {
                if (providerDao == null) return null;

                var curProviderId = Convert.ToInt32(providerId);
                var providerInfo = providerDao.GetProviderInfo(curProviderId);

                var folder = EntryManager.GetFakeThirdpartyFolder(providerInfo);
                ErrorIf(!FileSecurity.CanDelete(folder), FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder);

                if (providerInfo.RootFolderType == FolderType.COMMON)
                {
                    FileMarker.RemoveMarkAsNewForAll(folder);
                }

                providerDao.RemoveProviderInfo(folder.ProviderId);
                FilesMessageService.Send(folder, GetHttpHeaders(), MessageAction.ThirdPartyDeleted, folder.ID.ToString(), providerInfo.ProviderKey);

                return folder.ID;
            }
        }

        [ActionName("thirdparty"), HttpGet]
        public bool ChangeAccessToThirdparty(bool enable)
        {
            ErrorIf(!Global.IsAdministrator, FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.EnableThirdParty = enable;
            FilesMessageService.Send(GetHttpHeaders(), MessageAction.DocumentsThirdPartySettingsUpdated);

            return FilesSettings.EnableThirdParty;
        }

        [ActionName("docusign-save"), HttpPost]
        public bool SaveDocuSign([FromBody] String code)
        {
            ErrorIf(!SecurityContext.IsAuthenticated
                    || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                    || !FilesSettings.EnableThirdParty
                    || !ThirdpartyConfiguration.SupportDocuSignInclusion, FilesCommonResource.ErrorMassage_SecurityException_Create);

            var token = DocuSignLoginProvider.Instance.GetAccessToken(code);
            DocuSignHelper.ValidateToken(token);
            DocuSignToken.SaveToken(token);
            return true;
        }

        [ActionName("docusign-delete"), HttpGet]
        public object DeleteDocuSign()
        {
            DocuSignToken.DeleteToken();
            return null;
        }

        [ActionName("docusign"), HttpPost]
        public String SendDocuSign(String fileId, [FromBody] DocuSignData docuSignData)
        {
            try
            {
                ErrorIf (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                    || !FilesSettings.EnableThirdParty
                    || !ThirdpartyConfiguration.SupportDocuSignInclusion, FilesCommonResource.ErrorMassage_SecurityException_Create);

                return DocuSignHelper.SendDocuSign(fileId, docuSignData, GetHttpHeaders());
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        #endregion

        #region Operation

        [ActionName("tasks-statuses"), HttpGet]
        public ItemList<FileOperationResult> GetTasksStatuses()
        {
            ErrorIf(!SecurityContext.IsAuthenticated, FilesCommonResource.ErrorMassage_SecurityException);

            return fileOperations.GetOperationResults();
        }

        [ActionName("tasks"), HttpGet]
        public ItemList<FileOperationResult> TerminateTasks()
        {
            ErrorIf(!SecurityContext.IsAuthenticated, FilesCommonResource.ErrorMassage_SecurityException);

            return fileOperations.CancelOperations();
        }

        [ActionName("bulkdownload"), HttpPost]
        public ItemList<FileOperationResult> BulkDownload([FromBody] Dictionary<String, String> items)
        {
            Dictionary<object, string> folders;
            Dictionary<object, string> files;

            ParseArrayItems(items, out folders, out files);
            ErrorIf(folders.Count == 0 && files.Count == 0, FilesCommonResource.ErrorMassage_BadRequest);

            return fileOperations.Download(folders, files, GetHttpHeaders());
        }

        [ActionName("folders-files-moveOrCopyFilesCheck"), HttpPost]
        public ItemDictionary<String, String> MoveOrCopyFilesCheck([FromBody] ItemList<String> items, String destFolderId)
        {
            if (items.Count == 0) return new ItemDictionary<String, String>();

            List<object> foldersId;
            List<object> filesId;
            ParseArrayItems(items, out foldersId, out filesId);

            return new ItemDictionary<String, String>(MoveOrCopyFilesCheck(filesId, foldersId, destFolderId));
        }

        private Dictionary<String, String> MoveOrCopyFilesCheck(IEnumerable<object> filesId, IEnumerable<object> foldersId, object destFolderId)
        {
            var result = new Dictionary<string, string>();
            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var toFolder = folderDao.GetFolder(destFolderId);
                ErrorIf(toFolder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanCreate(toFolder), FilesCommonResource.ErrorMassage_SecurityException_Create);

                foreach (var id in filesId)
                {
                    var file = fileDao.GetFile(id);
                    if (file != null
                        && !file.Encrypted
                        && fileDao.IsExist(file.Title, toFolder.ID))
                    {
                        result.Add(id.ToString(), file.Title);
                    }
                }

                var folders = folderDao.GetFolders(foldersId.ToArray());
                var foldersProject = folders.Where(folder => folder.FolderType == FolderType.BUNCH).ToList();
                if (foldersProject.Any())
                {
                    var toSubfolders = folderDao.GetFolders(toFolder.ID);

                    foreach (var folderProject in foldersProject)
                    {
                        var toSub = toSubfolders.FirstOrDefault(to => Equals(to.Title, folderProject.Title));
                        if (toSub == null) continue;

                        var filesPr = fileDao.GetFiles(folderProject.ID);
                        var foldersPr = folderDao.GetFolders(folderProject.ID).Select(d => d.ID);

                        var recurseItems = MoveOrCopyFilesCheck(filesPr, foldersPr, toSub.ID);
                        foreach (var recurseItem in recurseItems)
                        {
                            result.Add(recurseItem.Key, recurseItem.Value);
                        }
                    }
                }
                try
                {
                    foreach (var pair in folderDao.CanMoveOrCopy(foldersId.ToArray(), toFolder.ID))
                    {
                        result.Add(pair.Key.ToString(), pair.Value);
                    }
                }
                catch (Exception e)
                {
                    throw GenerateException(e);
                }
            }
            return result;
        }

        [ActionName("moveorcopy"), HttpPost]
        public ItemList<FileOperationResult> MoveOrCopyItems([FromBody] ItemList<string> items, string destFolderId, FileConflictResolveType resolve, bool ic, bool deleteAfter = false)
        {
            ItemList<FileOperationResult> result;
            if (items.Count != 0)
            {
                List<object> foldersId;
                List<object> filesId;
                ParseArrayItems(items, out foldersId, out filesId);

                result = fileOperations.MoveOrCopy(foldersId, filesId, destFolderId, ic, resolve, !deleteAfter, GetHttpHeaders());
            }
            else
            {
                result = fileOperations.GetOperationResults();
            }
            return result;
        }

        [ActionName("folders-files"), HttpPost]
        public ItemList<FileOperationResult> DeleteItems(string action, [FromBody] ItemList<String> items, bool ignoreException = false, bool deleteAfter = false, bool immediately = false)
        {
            List<object> foldersId;
            List<object> filesId;
            ParseArrayItems(items, out foldersId, out filesId);

            return fileOperations.Delete(foldersId, filesId, ignoreException, !deleteAfter, immediately, GetHttpHeaders());
        }

        [ActionName("emptytrash"), HttpGet]
        public ItemList<FileOperationResult> EmptyTrash()
        {
            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var trashId = folderDao.GetFolderIDTrash(true);
                var foldersId = folderDao.GetFolders(trashId).Select(f => f.ID).ToList();
                var filesId = fileDao.GetFiles(trashId).ToList();

                return fileOperations.Delete(foldersId, filesId, false,  true, false, GetHttpHeaders());
            }
        }

        [ActionName("checkconversion"), HttpPost]
        public ItemList<FileOperationResult> CheckConversion([FromBody] ItemList<ItemList<string>> filesInfoJSON)
        {
            if (filesInfoJSON == null || filesInfoJSON.Count == 0) return new ItemList<FileOperationResult>();

            using (var fileDao = GetFileDao())
            {
                var files = new List<KeyValuePair<File, bool>>();
                foreach (var fileInfo in filesInfoJSON)
                {
                    object fileId;

                    var fileIdAsString = fileInfo[0];

                    int fileIdAsInt;
                    if (int.TryParse(fileIdAsString, out fileIdAsInt))
                        fileId = fileIdAsInt;
                    else
                        fileId = fileIdAsString;

                    int version;
                    var file = int.TryParse(fileInfo[1], out version) && version > 0
                                   ? fileDao.GetFile(fileId, version)
                                   : fileDao.GetFile(fileId);

                    if (file == null)
                    {
                        files.Add(new KeyValuePair<File, bool>(
                                      new File
                                          {
                                              ID = fileId,
                                              Version = version
                                          },
                                      true));
                        continue;
                    }

                    ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                    var startConvert = Convert.ToBoolean(fileInfo[2]);
                    if (startConvert && FileConverter.MustConvert(file))
                    {
                        try
                        {
                            FileConverter.ExecAsync(file, false, fileInfo.Count > 3 ? fileInfo[3] : null);
                        }
                        catch (Exception e)
                        {
                            throw GenerateException(e);
                        }
                    }

                    files.Add(new KeyValuePair<File, bool>(file, false));
                }

                var results = FileConverter.GetStatus(files).ToList();

                return new ItemList<FileOperationResult>(results);
            }
        }

        public void ReassignStorage(Guid userFromId, Guid userToId)
        {
            //check current user have access
            ErrorIf(!Global.IsAdministrator, FilesCommonResource.ErrorMassage_SecurityException);

            //check exist userFrom
            var userFrom = CoreContext.UserManager.GetUsers(userFromId);
            ErrorIf(Equals(userFrom, Constants.LostUser), FilesCommonResource.ErrorMassage_UserNotFound);

            //check exist userTo
            var userTo = CoreContext.UserManager.GetUsers(userToId);
            ErrorIf(Equals(userTo, Constants.LostUser), FilesCommonResource.ErrorMassage_UserNotFound);
            ErrorIf(userTo.IsVisitor(), FilesCommonResource.ErrorMassage_SecurityException);

            using (var providerDao = GetProviderDao())
            {
                if (providerDao != null)
                {
                    var providersInfo = providerDao.GetProvidersInfo(userFrom.ID);
                    var commonProvidersInfo = providersInfo.Where(provider => provider.RootFolderType == FolderType.COMMON).ToList();

                    //move common thirdparty storage userFrom
                    foreach (var commonProviderInfo in commonProvidersInfo)
                    {
                        Global.Logger.InfoFormat("Reassign provider {0} from {1} to {2}", commonProviderInfo.ID, userFrom.ID, userTo.ID);
                        providerDao.UpdateProviderInfo(commonProviderInfo.ID, null, null, FolderType.DEFAULT, userTo.ID);
                    }
                }
            }

            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                if (!userFrom.IsVisitor())
                {
                    var folderIdFromMy = folderDao.GetFolderIDUser(false, userFrom.ID);

                    if (!Equals(folderIdFromMy, 0))
                    {
                        //create folder with name userFrom in folder userTo
                        var folderIdToMy = folderDao.GetFolderIDUser(true, userTo.ID);
                        var newFolderTo = folderDao.SaveFolder(new Folder
                            {
                                Title = string.Format(CustomNamingPeople.Substitute<FilesCommonResource>("TitleDeletedUserFolder"), userFrom.DisplayUserName(false)),
                                ParentFolderID = folderIdToMy
                            });

                        //move items from userFrom to userTo
                        EntryManager.MoveSharedItems(folderIdFromMy, newFolderTo, folderDao, fileDao);

                        EntryManager.ReassignItems(newFolderTo, userFrom.ID, userTo.ID, folderDao, fileDao);
                    }
                }

                EntryManager.ReassignItems(Global.FolderCommon, userFrom.ID, userTo.ID, folderDao, fileDao);
            }
        }

        public void DeleteStorage(Guid userId)
        {
            //check current user have access
            ErrorIf(!Global.IsAdministrator, FilesCommonResource.ErrorMassage_SecurityException);

            //delete docuSign
            DocuSignToken.DeleteToken(userId);

            using (var providerDao = GetProviderDao())
            {
                if (providerDao != null)
                {
                    var providersInfo = providerDao.GetProvidersInfo(userId);

                    //delete thirdparty storage
                    foreach (var myProviderInfo in providersInfo)
                    {
                        Global.Logger.InfoFormat("Delete provider {0} for {1}", myProviderInfo.ID, userId);
                        providerDao.RemoveProviderInfo(myProviderInfo.ID);
                    }
                }
            }

            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                //delete all markAsNew
                var rootFoldersId = new List<object>
                    {
                        Global.FolderShare,
                        Global.FolderCommon,
                        Global.FolderProjects,
                    };

                var folderIdFromMy = folderDao.GetFolderIDUser(false, userId);
                if (!Equals(folderIdFromMy, 0))
                {
                    rootFoldersId.Add(folderIdFromMy);
                }

                var rootFolders = folderDao.GetFolders(rootFoldersId.ToArray());
                foreach (var rootFolder in rootFolders)
                {
                    FileMarker.RemoveMarkAsNew(rootFolder, userId);
                }

                //delete all from My
                if (!Equals(folderIdFromMy, 0))
                {
                    EntryManager.DeleteSubitems(folderIdFromMy, folderDao, fileDao);

                    //delete My userFrom folder
                    folderDao.DeleteFolder(folderIdFromMy);
                    Global.FolderMy = userId;
                }

                //delete all from Trash
                var folderIdFromTrash = folderDao.GetFolderIDTrash(false, userId);
                if (!Equals(folderIdFromTrash, 0))
                {
                    EntryManager.DeleteSubitems(folderIdFromTrash, folderDao, fileDao);
                    folderDao.DeleteFolder(folderIdFromTrash);
                    Global.FolderTrash = userId;
                }

                EntryManager.ReassignItems(Global.FolderCommon, userId, SecurityContext.CurrentAccount.ID, folderDao, fileDao);
            }
        }

        #endregion

        #region Favorites Manager

        public ItemList<FileEntry> AddToFavorites(ItemList<String> foldersId, ItemList<String> filesId)
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var entries = Enumerable.Empty<FileEntry>();

                var files = fileDao.GetFiles(filesId.ToArray());
                entries = entries.Concat(files);

                var folders = folderDao.GetFolders(foldersId.ToArray());
                entries = entries.Concat(folders);

                entries = FileSecurity.FilterRead(entries);

                var tags = entries.Select(entry => Tag.Favorite(SecurityContext.CurrentAccount.ID, entry));

                tagDao.SaveTags(tags);

                return new ItemList<FileEntry>(entries);
            }
        }

        public ItemList<FileEntry> DeleteFavorites(ItemList<String> foldersId, ItemList<String> filesId)
        {
            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var entries = Enumerable.Empty<FileEntry>();

                var files = fileDao.GetFiles(filesId.ToArray());
                entries = entries.Concat(files);

                var folders = folderDao.GetFolders(foldersId.ToArray());
                entries = entries.Concat(folders);

                entries = FileSecurity.FilterRead(entries);

                var tags = entries.Select(entry => Tag.Favorite(SecurityContext.CurrentAccount.ID, entry));

                tagDao.RemoveTags(tags);

                return new ItemList<FileEntry>(entries);
            }
        }

        #endregion

        #region Templates Manager

        public ItemList<FileEntry> AddToTemplates(ItemList<String> filesId)
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            {
                var files = fileDao.GetFiles(filesId.ToArray());

                files = FileSecurity.FilterRead(files)
                    .Where(file => FileUtility.ExtsWebTemplate.Contains(FileUtility.GetFileExtension(file.Title), StringComparer.CurrentCultureIgnoreCase))
                    .ToList();

                var tags = files.Select(file => Tag.Template(SecurityContext.CurrentAccount.ID, file));

                tagDao.SaveTags(tags);

                return new ItemList<FileEntry>(files);
            }
        }

        public ItemList<FileEntry> DeleteTemplates(ItemList<String> filesId)
        {
            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            {
                var files = fileDao.GetFiles(filesId.ToArray());

                files = FileSecurity.FilterRead(files).ToList();

                var tags = files.Select(file => Tag.Template(SecurityContext.CurrentAccount.ID, file));

                tagDao.RemoveTags(tags);

                return new ItemList<FileEntry>(files);
            }
        }

        [ActionName("gettemplates"), HttpGet]
        public object GetTemplates(FilterType filter, int from, int count, bool subjectGroup, String subjectID, String search, bool searchInContent)
        {
            try
            {
                IEnumerable<File> result;

                var subjectId = string.IsNullOrEmpty(subjectID) ? Guid.Empty : new Guid(subjectID);
                using (var fileDao = GetFileDao())
                {
                    result = EntryManager.GetTemplates(fileDao, filter, subjectGroup, subjectId, search, searchInContent);
                }

                if (result.Count() <= from)
                    return null;

                result = result.Skip(from).Take(count);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(serializer.ToXml(new ItemList<FileEntry>(result)));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                return response;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        #endregion

        #region Ace Manager

        [ActionName("sharedinfo"), HttpPost]
        public ItemList<AceWrapper> GetSharedInfo([FromBody] ItemList<String> objectIds)
        {
            ErrorIf(!SecurityContext.IsAuthenticated, FilesCommonResource.ErrorMassage_SecurityException);

            var result = new List<AceWrapper>();

            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                foreach (var objectId in objectIds)
                {
                    ErrorIf(string.IsNullOrEmpty(objectId), FilesCommonResource.ErrorMassage_BadRequest);

                    Debug.Assert(objectId != null, "objectId != null");
                    var entryType = objectId.StartsWith("file_") ? FileEntryType.File : FileEntryType.Folder;
                    var entryId = objectId.Substring((entryType == FileEntryType.File ? "file_" : "folder_").Length);

                    var entry = entryType == FileEntryType.File
                                    ? (FileEntry)fileDao.GetFile(entryId)
                                    : (FileEntry)folderDao.GetFolder(entryId);

                    IEnumerable<AceWrapper> acesForObject;
                    try
                    {
                        acesForObject = FileSharing.GetSharedInfo(entry);
                    }
                    catch (Exception e)
                    {
                        throw GenerateException(e);
                    }

                    foreach (var aceForObject in acesForObject)
                    {
                        var duplicate = result.FirstOrDefault(ace => ace.SubjectId == aceForObject.SubjectId);
                        if (duplicate == null)
                        {
                            if (result.Any())
                            {
                                aceForObject.Owner = false;
                                aceForObject.Share = FileShare.Varies;
                            }
                            continue;
                        }

                        if (duplicate.Share != aceForObject.Share)
                        {
                            aceForObject.Share = FileShare.Varies;
                        }
                        if (duplicate.Owner != aceForObject.Owner)
                        {
                            aceForObject.Owner = false;
                            aceForObject.Share = FileShare.Varies;
                        }
                        result.Remove(duplicate);
                    }

                    var withoutAce = result.Where(ace =>
                                                  acesForObject.FirstOrDefault(aceForObject =>
                                                                               aceForObject.SubjectId == ace.SubjectId) == null);
                    foreach (var ace in withoutAce)
                    {
                        ace.Share = FileShare.Varies;
                    }

                    var notOwner = result.Where(ace =>
                                                ace.Owner &&
                                                acesForObject.FirstOrDefault(aceForObject =>
                                                                             aceForObject.Owner
                                                                             && aceForObject.SubjectId == ace.SubjectId) == null);
                    foreach (var ace in notOwner)
                    {
                        ace.Owner = false;
                        ace.Share = FileShare.Varies;
                    }

                    result.AddRange(acesForObject);
                }
            }


            var ownerAce = result.FirstOrDefault(ace => ace.Owner);
            result.Remove(ownerAce);

            var meAce = result.FirstOrDefault(ace => ace.SubjectId == SecurityContext.CurrentAccount.ID);
            result.Remove(meAce);

            AceWrapper linkAce = null;
            if (objectIds.Count > 1)
            {
                result.RemoveAll(ace => ace.SubjectId == FileConstant.ShareLinkId);
            }
            else
            {
                linkAce = result.FirstOrDefault(ace => ace.SubjectId == FileConstant.ShareLinkId);
            }

            result.Sort((x, y) => string.Compare(x.SubjectName, y.SubjectName));

            if (ownerAce != null)
            {
                result = new List<AceWrapper> {ownerAce}.Concat(result).ToList();
            }
            if (meAce != null)
            {
                result = new List<AceWrapper> {meAce}.Concat(result).ToList();
            }
            if (linkAce != null)
            {
                result.Remove(linkAce);
                result = new List<AceWrapper> {linkAce}.Concat(result).ToList();
            }

            return new ItemList<AceWrapper>(result);
        }

        [ActionName("sharedinfoshort"), HttpGet]
        public ItemList<AceShortWrapper> GetSharedInfoShort(String objectId)
        {
            var aces = GetSharedInfo(new ItemList<string> { objectId });

            return new ItemList<AceShortWrapper>(
                aces.Where(aceWrapper => !aceWrapper.SubjectId.Equals(FileConstant.ShareLinkId) || aceWrapper.Share != FileShare.Restrict)
                    .Select(aceWrapper => new AceShortWrapper(aceWrapper)));
        }

        [ActionName("setaceobject"), HttpPost]
        public ItemList<string> SetAceObject([FromBody] AceCollection aceCollection, bool notify)
        {
            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var result = new ItemList<string>();
                foreach (var objectId in aceCollection.Entries)
                {
                    Debug.Assert(objectId != null, "objectId != null");
                    var entryType = objectId.StartsWith("file_") ? FileEntryType.File : FileEntryType.Folder;
                    var entryId = objectId.Substring((entryType == FileEntryType.File ? "file_" : "folder_").Length);
                    var entry = entryType == FileEntryType.File
                                    ? (FileEntry)fileDao.GetFile(entryId)
                                    : (FileEntry)folderDao.GetFolder(entryId);

                    try
                    {
                        var changed = FileSharing.SetAceObject(aceCollection.Aces, entry, notify, aceCollection.Message);
                        if (changed)
                        {
                            FilesMessageService.Send(entry, GetHttpHeaders(),
                                                     entryType == FileEntryType.Folder ? MessageAction.FolderUpdatedAccess : MessageAction.FileUpdatedAccess,
                                                     entry.Title);
                        }
                    }
                    catch (Exception e)
                    {
                        throw GenerateException(e);
                    }

                    using (var securityDao = GetSecurityDao())
                    {
                        if (securityDao.IsShared(entry.ID, entryType))
                        {
                            result.Add(objectId);
                        }
                    }
                }
                return result;
            }
        }

        [ActionName("removeace"), HttpPost]
        public void RemoveAce([FromBody] ItemList<String> items)
        {
            ErrorIf(!SecurityContext.IsAuthenticated, FilesCommonResource.ErrorMassage_SecurityException);
            List<object> filesId;
            List<object> foldersId;
            ParseArrayItems(items, out foldersId, out filesId);

            var entries = new List<FileEntry>();

            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                entries.AddRange(filesId.Select(fileId => fileDao.GetFile(fileId)));
                entries.AddRange(foldersId.Select(folderDao.GetFolder));

                FileSharing.RemoveAce(entries);
            }
        }

        [ActionName("shorten"), HttpGet]
        public String GetShortenLink(String fileId)
        {
            File file;
            using (var fileDao = GetFileDao())
            {
                file = fileDao.GetFile(fileId);
                ErrorIf(!FileSharing.CanSetAccess(file), FilesCommonResource.ErrorMassage_SecurityException);
            }
            var shareLink = FileShareLink.GetLink(file);

            try
            {
                return UrlShortener.Instance.GetShortenLink(shareLink);
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        [ActionName("setacelink"), HttpGet]
        public bool SetAceLink(String fileId, FileShare share)
        {
            FileEntry file;
            using (var fileDao = GetFileDao())
            {
                file = fileDao.GetFile(fileId);
            }
            var aces = new List<AceWrapper>
                {
                    new AceWrapper
                        {
                            Share = share,
                            SubjectId = FileConstant.ShareLinkId,
                            SubjectGroup = true,
                        }
                };

            try
            {

                var changed = FileSharing.SetAceObject(aces, file, false, null);
                if (changed)
                {
                    FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileUpdatedAccess, file.Title);
                }
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }

            using (var securityDao = GetSecurityDao())
            {
                return securityDao.IsShared(file.ID, FileEntryType.File);
            }
        }

        [ActionName("sharedusers"), HttpGet]
        public ItemList<MentionWrapper> SharedUsers(String fileId)
        {
            if (!SecurityContext.IsAuthenticated || CoreContext.Configuration.Personal)
                return null;

            FileEntry file;
            using (var fileDao = GetFileDao())
            {
                file = fileDao.GetFile(fileId);
            }

            ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);

            var usersIdWithAccess = new List<Guid>();
            if (FileSharing.CanSetAccess(file))
            {
                var access = FileSharing.GetSharedInfo(file);
                usersIdWithAccess = access.Where(aceWrapper => !aceWrapper.SubjectGroup && aceWrapper.Share != FileShare.Restrict)
                                          .Select(aceWrapper => aceWrapper.SubjectId)
                                          .ToList();
            }
            else
            {
                usersIdWithAccess.Add(file.CreateBy);
            }

            var users = CoreContext.UserManager.GetUsersByGroup(Constants.GroupEveryone.ID)
                                   .Where(user => !user.ID.Equals(SecurityContext.CurrentAccount.ID)
                                                  && !user.ID.Equals(Constants.LostUser.ID))
                                   .Select(user => new MentionWrapper(user) { HasAccess = usersIdWithAccess.Contains(user.ID) })
                                   .ToList();

            users = users
                .OrderBy(user => !user.HasAccess)
                .ThenBy(user => user.User, UserInfoComparer.Default)
                .ToList();

            return new ItemList<MentionWrapper>(users);
        }

        [ActionName("sendeditornotify"), HttpPost]
        public ItemList<AceShortWrapper> SendEditorNotify(String fileId, [FromBody] MentionMessageWrapper mentionMessage)
        {
            ErrorIf(!SecurityContext.IsAuthenticated, FilesCommonResource.ErrorMassage_SecurityException);

            File file;
            using (var fileDao = GetFileDao())
            {
                file = fileDao.GetFile(fileId);
            }

            ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);

            var fileSecurity = Global.GetFilesSecurity();
            ErrorIf(!fileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
            ErrorIf(mentionMessage == null || mentionMessage.Emails == null, FilesCommonResource.ErrorMassage_BadRequest);

            var showSharingSettings = false;
            bool? canShare = null;

            if (file.Encrypted)
            {
                canShare = false;
                showSharingSettings = true;
            }

            var recipients = new List<Guid>();
            foreach (var email in mentionMessage.Emails)
            {
                if (!canShare.HasValue)
                {
                    canShare = FileSharing.CanSetAccess(file);
                }

                var recipient = CoreContext.UserManager.GetUserByEmail(email);
                if (recipient == null || recipient.ID == Constants.LostUser.ID)
                {
                    showSharingSettings = canShare.Value;
                    continue;
                }

                if (!fileSecurity.CanRead(file, recipient.ID))
                {
                    if (!canShare.Value)
                    {
                        continue;
                    }

                    try
                    {
                        var aces = new List<AceWrapper>
                            {
                                new AceWrapper
                                    {
                                        Share = FileShare.Read,
                                        SubjectId = recipient.ID,
                                        SubjectGroup = false,
                                    }
                            };

                        showSharingSettings |= FileSharing.SetAceObject(aces, file, false, null);

                        recipients.Add(recipient.ID);
                    }
                    catch (Exception e)
                    {
                        throw GenerateException(e);
                    }
                }
                else
                {
                    recipients.Add(recipient.ID);
                }
            }

            if (showSharingSettings)
            {
                FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileUpdatedAccess, file.Title);
            }

            var fileLink = FilesLinkUtility.GetFileWebEditorUrl(file.ID);
            if (mentionMessage.ActionLink != null)
            {
                fileLink += "&" + FilesLinkUtility.Anchor + "=" + HttpUtility.UrlEncode(
                    DocumentService.Configuration.EditorConfiguration.ActionLinkConfig.Serialize(mentionMessage.ActionLink));
            }

            var message = (mentionMessage.Message ?? "").Trim();
            const int maxMessageLength = 200;
            if (message.Length > maxMessageLength)
            {
                message = message.Substring(0, maxMessageLength) + "...";
            }

            NotifyClient.SendEditorMentions(file, fileLink, recipients, message);

            return showSharingSettings ? GetSharedInfoShort("file_" + fileId) : null;
        }

        [ActionName("publickeys"), HttpGet]
        public ItemList<EncryptionKeyPair> GetEncryptionAccess(String fileId)
        {
            ErrorIf(!PrivacyRoomSettings.Enabled, FilesCommonResource.ErrorMassage_SecurityException);

            var fileKeyPair = EncryptionKeyPair.GetKeyPair(fileId);
            return new ItemList<EncryptionKeyPair>(fileKeyPair);
        }

        #endregion

        #region MailMerge

        [ActionName("mailaccounts")]
        public ItemList<string> GetMailAccounts()
        {
            var apiServer = new ASC.Api.ApiServer();
            var apiUrl = String.Format("{0}mail/accounts.json", SetupInfo.WebApiBaseUrl);

            var accounts = new List<string>();

            var responseBody = apiServer.GetApiResponse(apiUrl, "GET");
            if (responseBody != null)
            {
                var responseApi = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(responseBody)));

                var responseData = responseApi["response"];
                if (responseData is JArray)
                {
                    accounts.AddRange(
                        from account in responseData.Children()
                        orderby account["isDefault"].Value<Boolean>() descending
                        where account["enabled"].Value<Boolean>() && !account["isGroup"].Value<Boolean>()
                        select account["email"].Value<String>()
                        );
                }
            }
            ErrorIf(!accounts.Any(), FilesCommonResource.ErrorMassage_MailAccountNotFound);

            return new ItemList<string>(accounts);
        }

        #endregion

        [ActionName("changeowner"), HttpPost]
        public ItemList<FileEntry> ChangeOwner([FromBody] ItemList<String> items, Guid userId)
        {
            var userInfo = CoreContext.UserManager.GetUsers(userId);
            ErrorIf(Equals(userInfo, Constants.LostUser) || userInfo.IsVisitor(), FilesCommonResource.ErrorMassage_ChangeOwner);

            List<object> filesId;
            List<object> foldersId;
            ParseArrayItems(items, out foldersId, out filesId);
            
            var entries = new List<FileEntry>();

            using (var folderDao = GetFolderDao())
            {
                var folders = folderDao.GetFolders(foldersId.ToArray());

                foreach (var folder in folders)
                {
                    ErrorIf(!FileSecurity.CanEdit(folder), FilesCommonResource.ErrorMassage_SecurityException);
                    ErrorIf(folder.RootFolderType != FolderType.COMMON, FilesCommonResource.ErrorMassage_SecurityException);
                    if (folder.ProviderEntry) continue;

                    var newFolder = folder;
                    if (folder.CreateBy != userInfo.ID)
                    {
                        var folderAccess = folder.Access;

                        newFolder.CreateBy = userInfo.ID;
                        var newFolderID = folderDao.SaveFolder((Folder)newFolder);

                        newFolder = folderDao.GetFolder(newFolderID);
                        newFolder.Access = folderAccess;

                        FilesMessageService.Send(newFolder, GetHttpHeaders(), MessageAction.FileChangeOwner, new[] { newFolder.Title, userInfo.DisplayUserName(false) });
                    }
                    entries.Add(newFolder);
                }
            }

            using (var fileDao = GetFileDao())
            {
                var files = fileDao.GetFiles(filesId.ToArray());

                foreach (var file in files)
                {
                    ErrorIf(!FileSecurity.CanEdit(file), FilesCommonResource.ErrorMassage_SecurityException);
                    ErrorIf(EntryManager.FileLockedForMe(file.ID), FilesCommonResource.ErrorMassage_LockedFile);
                    ErrorIf(FileTracker.IsEditing(file.ID), FilesCommonResource.ErrorMassage_UpdateEditingFile);
                    ErrorIf(file.RootFolderType != FolderType.COMMON, FilesCommonResource.ErrorMassage_SecurityException);
                    if (file.ProviderEntry) continue;

                    var newFile = file;
                    if (file.CreateBy != userInfo.ID)
                    {
                        newFile = new File
                            {
                                ID = file.ID,
                                Version = file.Version + 1,
                                VersionGroup = file.VersionGroup + 1,
                                Title = file.Title,
                                FileStatus = file.FileStatus,
                                FolderID = file.FolderID,
                                CreateBy = userInfo.ID,
                                CreateOn = file.CreateOn,
                                ConvertedType = file.ConvertedType,
                                Comment = FilesCommonResource.CommentChangeOwner,
                                Encrypted = file.Encrypted,
                            };

                        using (var stream = fileDao.GetFileStream(file))
                        {
                            newFile.ContentLength = stream.CanSeek ? stream.Length : file.ContentLength;
                            newFile = fileDao.SaveFile(newFile, stream);
                        }

                        FileMarker.MarkAsNew(newFile);

                        EntryManager.SetFileStatus(newFile);

                        FilesMessageService.Send(newFile, GetHttpHeaders(), MessageAction.FileChangeOwner, new[] {newFile.Title, userInfo.DisplayUserName(false)});
                    }
                    entries.Add(newFile);
                }
            }

            return new ItemList<FileEntry>(entries);
        }

        public bool StoreOriginal(bool set)
        {
            FilesSettings.StoreOriginalFiles = set;
            FilesMessageService.Send(GetHttpHeaders(), MessageAction.DocumentsUploadingFormatsSettingsUpdated);

            return FilesSettings.StoreOriginalFiles;
        }

        public bool HideConfirmConvert(bool isForSave)
        {
            if (isForSave)
            {
                FilesSettings.HideConfirmConvertSave = true;
            }
            else
            {
                FilesSettings.HideConfirmConvertOpen = true;
            }
            
            return true;
        }

        [ActionName("updateifexist"), HttpGet]
        public bool UpdateIfExist(bool set)
        {
            FilesSettings.UpdateIfExist = set;
            FilesMessageService.Send(GetHttpHeaders(), MessageAction.DocumentsOverwritingSettingsUpdated);

            return FilesSettings.UpdateIfExist;
        }

        [ActionName("forcesave"), HttpGet]
        public bool Forcesave(bool set)
        {
            FilesSettings.Forcesave = set;
            FilesMessageService.Send(GetHttpHeaders(), MessageAction.DocumentsForcesave);

            return FilesSettings.Forcesave;
        }

        [ActionName("storeforcesave"), HttpGet]
        public bool StoreForcesave(bool set)
        {
            ErrorIf(!Global.IsAdministrator, FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.StoreForcesave = set;
            FilesMessageService.Send(GetHttpHeaders(), MessageAction.DocumentsStoreForcesave);

            return FilesSettings.StoreForcesave;
        }

        public bool DisplayRecent(bool set)
        {
            if (!SecurityContext.IsAuthenticated) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.RecentSection = set;

            return FilesSettings.RecentSection;
        }

        public bool DisplayFavorite(bool set)
        {
            if (!SecurityContext.IsAuthenticated) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.FavoritesSection = set;

            return FilesSettings.FavoritesSection;
        }

        public bool DisplayTemplates(bool set)
        {
            if (!SecurityContext.IsAuthenticated) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.TemplatesSection = set;

            return FilesSettings.TemplatesSection;
        }

        [ActionName("changedeleteconfrim"), HttpGet]
        public bool ChangeDeleteConfrim(bool set)
        {
            FilesSettings.ConfirmDelete = set;

            return FilesSettings.ConfirmDelete;
        }

        public String GetHelpCenter()
        {
            return Studio.UserControls.Common.HelpCenter.HelpCenter.RenderControlToString();
        }

        #region private

        private static FileSecurity FileSecurity
        {
            get { return Global.GetFilesSecurity(); }
        }

        private static IFolderDao GetFolderDao()
        {
            return Global.DaoFactory.GetFolderDao();
        }

        private static IFileDao GetFileDao()
        {
            return Global.DaoFactory.GetFileDao();
        }

        private static ITagDao GetTagDao()
        {
            return Global.DaoFactory.GetTagDao();
        }

        private static IDataStore GetStoreTemplate()
        {
            return Global.GetStoreTemplate();
        }

        private static IProviderDao GetProviderDao()
        {
            return Global.DaoFactory.GetProviderDao();
        }

        private static ISecurityDao GetSecurityDao()
        {
            return Global.DaoFactory.GetSecurityDao();
        }

        private static void ParseArrayItems(IEnumerable<string> data, out List<object> foldersId, out List<object> filesId)
        {
            //TODO:!!!!Fix
            foldersId = new List<object>();
            filesId = new List<object>();
            foreach (var id in data)
            {
                if (id.StartsWith("file_")) filesId.Add(id.Substring("file_".Length));
                if (id.StartsWith("folder_")) foldersId.Add(id.Substring("folder_".Length));
            }
        }

        private static void ParseArrayItems(Dictionary<String, String> items, out Dictionary<object, String> folders, out Dictionary<object, String> files)
        {
            //TODO:!!!!Fix
            folders = new Dictionary<object, String>();
            files = new Dictionary<object, String>();
            foreach (var item in (items ?? new Dictionary<String, String>()))
            {
                if (item.Key.StartsWith("file_")) files.Add(item.Key.Substring("file_".Length), item.Value);
                if (item.Key.StartsWith("folder_")) folders.Add(item.Key.Substring("folder_".Length), item.Value);
            }
        }

        private static void ErrorIf(bool condition, string errorMessage)
        {
            if (condition) throw new InvalidOperationException(errorMessage);
        }

        private static Exception GenerateException(Exception error, bool warning = false)
        {
            if (warning)
            {
                Global.Logger.Info(error);
            }
            else
            {
                Global.Logger.Error(error);
            }
            return new InvalidOperationException(error.Message, error);
        }

        private Dictionary<string, string> GetHttpHeaders()
        {
            if (Request != null && Request.Headers != null)
            {
                return Request.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value.ToArray()));
            }
            else if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers != null)
            {
                var headers = new Dictionary<string, string>();
                foreach (var k in HttpContext.Current.Request.Headers.AllKeys)
                {
                    headers[k] = string.Join(", ", HttpContext.Current.Request.Headers.GetValues(k));
                }
                return headers;
            }
            return null;
        }

        #endregion
    }
}