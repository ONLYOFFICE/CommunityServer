/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using Antlr.Runtime.Tree;

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
using ASC.Web.Files.Core;
using ASC.Web.Files.Core.Compress;
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
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json.Linq;

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

        [ActionName("folders-folder"), HttpGet, AllowAnonymous]
        public Folder GetFolder(String folderId)
        {
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(folderId);
                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanRead(folder), FilesCommonResource.ErrorMassage_SecurityException_ReadFolder);
                EntryManager.SetIsFavoriteFolder(folder);

                return folder;
            }
        }

        [ActionName("folders-subfolders"), HttpGet, AllowAnonymous]
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

        [ActionName("folders-path"), HttpGet, AllowAnonymous]
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

        [ActionName("folders"), HttpPost, AllowAnonymous]
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

        [ActionName("folders-entries"), HttpPost, AllowAnonymous]
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
                var folders = folderDao.GetFolders(foldersId);
                folders = FileSecurity.FilterRead(folders);
                entries = entries.Concat(folders);

                var files = fileDao.GetFiles(filesId);
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
            EntryManager.SetIsFavoriteFolders(entries.OfType<Folder>().Where(r => r.ID != null).ToList());

            return new ItemList<FileEntry>(entries);
        }

        [ActionName("folders-create"), HttpPost, AllowAnonymous]
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

        [ActionName("folders-structure-create"), HttpPost]
        public Folder CreateNewFolders(String parentId, [FromBody] IEnumerable<string> relativePaths)
        {
            if (relativePaths == null || relativePaths.Count() == 0 || String.IsNullOrEmpty(parentId)) throw new ArgumentException();

            Folder folder = null;
            using (var folderDao = GetFolderDao())
            {
                var parent = folderDao.GetFolder(parentId);
                ErrorIf(parent == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanCreate(parent), FilesCommonResource.ErrorMassage_SecurityException_Create);

                try
                {
                    var relativePath = relativePaths.First();
                    var folderTitle = string.IsNullOrEmpty(relativePath) ? null : relativePath.Split('/').ToList();
                    var folderId = FileUploader.GetFolderId(parentId, folderTitle);
                    relativePaths = relativePaths.Where(x => !x.Equals(relativePath));

                    foreach (var folderPath in relativePaths)
                    {
                        FileUploader.GetFolderId(parentId, string.IsNullOrEmpty(folderPath) ? null : folderPath.Split('/').ToList());
                    }

                    folder = folderDao.GetFolder(folderId);
                    return folder;
                }
                catch (Exception e)
                {
                    throw GenerateException(e);
                }
            }
        }

        [ActionName("folders-rename"), HttpPut, AllowAnonymous]
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

                EntryManager.SetIsFavoriteFolder(folder);

                return folder;
            }
        }

        #endregion

        #region File Manager

        [ActionName("folders-files-getversion"), HttpGet, AllowAnonymous]
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

        [ActionName("folders-files-siblings"), HttpPost, AllowAnonymous]
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

                var entries = new List<FileEntry>();

                if (!FileSecurity.CanRead(parent))
                {
                    file.FolderID = Global.FolderShare;
                    entries.Add(file);
                }
                else
                {
                    try
                    {
                        int total;
                        entries = EntryManager.GetEntries(folderDao, fileDao, parent, 0, 0, filter, subjectGroup, subjectId, search, searchInContent, withSubfolders, orderBy, out total).ToList();
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

        [ActionName("folders-files-createfile"), HttpPost, AllowAnonymous]
        public File CreateNewFile(String parentId, String title, String templateId, bool enableExternalExt = false)
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

                var fileExt = FileUtility.GetFileExtension(title);

                ErrorIf(fileExt == FileUtility.MasterFormExtension && CoreContext.Configuration.CustomMode, FilesCommonResource.ErrorMassage_BadRequest);

                if (!enableExternalExt && fileExt != FileUtility.MasterFormExtension)
                {
                    fileExt = FileUtility.GetInternalExtension(title);
                    if (!FileUtility.InternalExtension.Values.Contains(fileExt))
                    {
                        fileExt = FileUtility.InternalExtension[FileType.Document];
                        file.Title = title + fileExt;
                    }
                    else
                    {
                        file.Title = FileUtility.ReplaceFileExtension(title, fileExt);
                    }
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
                        path = FileConstant.NewDocPath + "en-US/";
                    }

                    try
                    {
                        var pathNew = path + "new" + fileExt;

                        if (!enableExternalExt)
                        {
                            using (var stream = storeTemplate.GetReadStream("", pathNew))
                            {
                                file.ContentLength = stream.CanSeek ? stream.Length : storeTemplate.GetFileSize(pathNew);
                                file = fileDao.SaveFile(file, stream);
                            }
                        }
                        else
                        {
                            using (var stream = new MemoryStream())
                            {
                                file = fileDao.SaveFile(file, stream);
                            }
                        }

                        var pathThumb = path + fileExt.Trim('.') + "." + Global.ThumbnailExtension;
                        if (storeTemplate.IsFile("", pathThumb))
                        {
                            using (var streamThumb = storeTemplate.GetReadStream("", pathThumb))
                            {
                                fileDao.SaveThumbnail(file, streamThumb);
                            }
                            file.ThumbnailStatus = Thumbnail.Created;
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

                        if (template.ThumbnailStatus == Thumbnail.Created)
                        {
                            using (var thumb = fileDao.GetThumbnail(template))
                            {
                                fileDao.SaveThumbnail(file, thumb);
                            }
                            file.ThumbnailStatus = Thumbnail.Created;
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
                var id = FileShareLink.Parse(doc, out _, out _);
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

        [ActionName("checkediting"), HttpPost, AllowAnonymous]
        public ItemDictionary<String, String> CheckEditing([FromBody] ItemList<String> filesId)
        {

            var result = new ItemDictionary<string, string>();

            using (var fileDao = GetFileDao())
            {
                var ids = filesId.Where(FileTracker.IsEditing).Select(id => (object)id).ToList();

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

                var file = EntryManager.SaveEditing(fileId, fileExtension, fileuri, stream, doc, forcesave: forcesave ? ForcesaveType.User : ForcesaveType.None, keepLink: true);

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

        public File UpdateFileStream(String fileId, Stream stream, String fileExtension, bool encrypted, bool forcesave)
        {
            try
            {
                if (!forcesave && FileTracker.IsEditing(fileId))
                {
                    FileTracker.Remove(fileId);
                }

                var file = EntryManager.SaveEditing(fileId,
                    fileExtension,
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

        [ActionName("startedit"), HttpPost, AllowAnonymous]
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

                if (!DocumentServiceTracker.StartTrack(fileId, key, doc))
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

        [ActionName("folders-files-rename"), HttpPut, AllowAnonymous]
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
                        FactoryIndexer<FilesWrapper>.UpdateAsync(file, true, r => r.Title);
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

        [ActionName("folders-files-history"), HttpGet, AllowAnonymous]
        public ItemList<File> GetFileHistory(String fileId)
        {
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                return new ItemList<File>(fileDao.GetFileHistory(fileId));
            }
        }

        [ActionName("folders-files-updateToVersion"), HttpPut, AllowAnonymous]
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

        [ActionName("folders-files-updateComment"), HttpPut, AllowAnonymous]
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

        [ActionName("folders-files-completeVersion"), HttpPut, AllowAnonymous]
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

        [ActionName("folders-files-lock"), HttpPut]
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
                var readLink = FileShareLink.Check(doc, true, fileDao, out File file, out FileShare linkShare, out Guid linkId);
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
                var readLink = FileShareLink.Check(doc, true, fileDao, out File file, out FileShare linkShare, out Guid linkId);

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
                    FileType = file.ConvertedExtension.Trim('.'),
                    Key = DocumentServiceHelper.GetDocKey(file),
                    Url = DocumentServiceConnector.ReplaceCommunityAdress(PathProvider.GetFileStreamUrl(file, doc)),
                    Version = version,
                };

                if (fileDao.ContainChanges(file.ID, file.Version))
                {
                    string previouseKey;
                    string sourceFileUrl;
                    string sourceExt;
                    if (file.Version > 1)
                    {
                        var previousFileStable = fileDao.GetFileStable(file.ID, file.Version - 1);
                        ErrorIf(previousFileStable == null, FilesCommonResource.ErrorMassage_FileNotFound);

                        sourceFileUrl = PathProvider.GetFileStreamUrl(previousFileStable, doc);
                        sourceExt = previousFileStable.ConvertedExtension;

                        previouseKey = DocumentServiceHelper.GetDocKey(previousFileStable);
                    }
                    else
                    {
                        var culture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();
                        var storeTemplate = GetStoreTemplate();

                        var path = FileConstant.NewDocPath + culture + "/";
                        if (!storeTemplate.IsDirectory(path))
                        {
                            path = FileConstant.NewDocPath + "en-US/";
                        }

                        var fileExt = FileUtility.GetFileExtension(file.Title);

                        path += "new" + fileExt;

                        sourceFileUrl = storeTemplate.GetUri("", path).ToString();
                        sourceFileUrl = CommonLinkUtility.GetFullAbsolutePath(sourceFileUrl);
                        sourceExt = fileExt.Trim('.');

                        previouseKey = DocumentServiceConnector.GenerateRevisionId(Guid.NewGuid().ToString());
                    }

                    result.Previous = new EditHistoryUrl
                    {
                        FileType = sourceExt.Trim('.'),
                        Key = previouseKey,
                        Url = DocumentServiceConnector.ReplaceCommunityAdress(sourceFileUrl),
                    };
                    result.ChangesUrl = DocumentServiceConnector.ReplaceCommunityAdress(PathProvider.GetFileChangesUrl(file, doc));
                }

                result.Token = DocumentServiceHelper.GetSignature(result);

                return result;
            }
        }

        [ActionName("restore-version"), HttpPut, AllowAnonymous]
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

        public EntryProperties GetFileProperties(String fileId)
        {
            using (var fileDao = GetFileDao())
            {
                fileDao.InvalidateCache(fileId);

                var file = fileDao.GetFile(fileId);
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                var properties = fileDao.GetProperties(fileId) ?? new EntryProperties();

                if (properties.FormFilling != null)
                {
                    if (!FileSharing.CanSetAccess(file)
                        || !FileUtility.CanWebRestrictedEditing(file.Title))
                    {
                        properties.FormFilling = null;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(properties.FormFilling.ToFolderId))
                        {
                            using (var folderDao = GetFolderDao())
                            {
                                var folder = folderDao.GetFolder(properties.FormFilling.ToFolderId);

                                if (folder == null)
                                {
                                    properties.FormFilling.ToFolderId = null;
                                }
                                else if (FileSecurity.CanCreate(folder))
                                {
                                    properties.FormFilling.ToFolderPath = null;
                                    var breadCrumbs = EntryManager.GetBreadCrumbs(folder.ID, folderDao);
                                    properties.FormFilling.ToFolderPath = string.Join("/", breadCrumbs.Select(f => f.Title));
                                }
                            }
                        }
                    }
                }

                return properties;
            }
        }

        public EntryProperties SetFileProperties(String fileId, EntryProperties fileProperties)
        {
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId);
                if (file == null) throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
                if (!Global.GetFilesSecurity().CanEdit(file)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
                if (EntryManager.FileLockedForMe(file.ID)) throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
                if (file.ProviderEntry) throw new Exception(FilesCommonResource.ErrorMassage_BadRequest);
                if (file.RootFolderType == FolderType.TRASH) throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);

                var currentProperies = fileDao.GetProperties(fileId) ?? new EntryProperties();
                if (fileProperties != null)
                {
                    if (fileProperties.FormFilling != null)
                    {
                        if (!FileSharing.CanSetAccess(file)) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);
                        if (!FileUtility.CanWebRestrictedEditing(file.Title)) throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);

                        if (currentProperies.FormFilling == null)
                        {
                            currentProperies.FormFilling = new EntryProperties.FormFillingProperties();
                        }

                        currentProperies.FormFilling.CollectFillForm = fileProperties.FormFilling.CollectFillForm;

                        if (!string.IsNullOrEmpty(fileProperties.FormFilling.ToFolderId))
                        {
                            using (var folderDao = GetFolderDao())
                            {
                                var folder = folderDao.GetFolder(fileProperties.FormFilling.ToFolderId);

                                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                                ErrorIf(!FileSecurity.CanCreate(folder), FilesCommonResource.ErrorMassage_SecurityException_Create);

                                currentProperies.FormFilling.ToFolderId = folder.ID.ToString();
                            }
                        }

                        currentProperies.FormFilling.CreateFolderTitle = Global.ReplaceInvalidCharsAndTruncate(fileProperties.FormFilling.CreateFolderTitle);

                        currentProperies.FormFilling.CreateFileMask = fileProperties.FormFilling.CreateFileMask;
                        currentProperies.FormFilling.FixFileMask();
                    }

                    fileDao.SaveProperties(file.ID, currentProperies);
                }

                return currentProperies;
            }
        }

        [ActionName("reference-data"), HttpGet]
        public FileReference GetReferenceData(string fileKey, string instanceId, string sourceFileId, string path)
        {
            File file = null;
            using (var fileDao = GetFileDao())
            {
                if (!string.IsNullOrEmpty(fileKey)
                    && instanceId == CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString())
                {
                    file = fileDao.GetFile(fileKey);
                }

                if (file == null
                    && !string.IsNullOrEmpty(sourceFileId)
                    && !string.IsNullOrEmpty(path))
                {
                    var source = fileDao.GetFile(sourceFileId);
                    if (!FileSecurity.CanRead(source))
                    {
                        return new FileReference
                        {
                            Error = FilesCommonResource.ErrorMassage_SecurityException_ReadFile
                        };
                    }

                    using (var folderDao = GetFolderDao())
                    {
                        var folder = folderDao.GetFolder(source.FolderID);
                        if (!Global.GetFilesSecurity().CanRead(folder))
                        {
                            return new FileReference
                            {
                                Error = FilesCommonResource.ErrorMassage_SecurityException_ReadFolder
                            };
                        }

                        var list = fileDao.GetFiles(folder.ID, new OrderBy(SortedByType.AZ, true), FilterType.FilesOnly, false, Guid.Empty, path, false, false);
                        file = list.FirstOrDefault(fileItem => fileItem.Title == path);
                    }
                }
            }

            if (file == null)
            {
                return new FileReference
                {
                    Error = FilesCommonResource.ErrorMassage_FileNotFound
                };
            }

            if (!FileSecurity.CanRead(file))
            {
                return new FileReference
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_ReadFile
                };
            }

            var fileReference = new FileReference
            {
                FileType = file.ConvertedExtension.Trim('.'),
                Path = file.Title,
                ReferenceData = new FileReference.FileReferenceData
                {
                    FileKey = file.ID.ToString(),
                    InstanceId = CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString()
                },
                Url = DocumentServiceConnector.ReplaceCommunityAdress(PathProvider.GetFileStreamUrl(file, lastVersion: true))
            };

            fileReference.Token = DocumentServiceHelper.GetSignature(fileReference);

            return fileReference;
        }

        public IEnumerable<FileEntry> GetFilterReadFiles(IEnumerable<string> fileIds)
        {
            using (var fileDao = GetFileDao())
            {
                var files = fileDao.GetFiles(fileIds);

                files = FileSecurity.FilterRead(files);

                return files;
            }
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

        [ActionName("markasread"), HttpPut]
        public ItemList<FileOperationResult> MarkAsRead([FromBody] ItemList<String> items)
        {
            if (items.Count == 0) return GetTasksStatuses();

            List<object> foldersId;
            List<object> filesId;
            ParseArrayItems(items, out foldersId, out filesId);

            return fileOperations.MarkAsRead(foldersId, filesId, GetHttpHeaders());
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

        [ActionName("thirdparty-delete"), HttpDelete]
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

        [ActionName("thirdparty"), HttpPut]
        public bool ChangeAccessToThirdparty(bool enable)
        {
            ErrorIf(!Global.IsAdministrator, FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.EnableThirdParty = enable;
            MessageService.Send(GetHttpHeaders(), MessageAction.DocumentsThirdPartySettingsUpdated);

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

        [ActionName("docusign-delete"), HttpDelete]
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
                ErrorIf(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
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

        [ActionName("tasks-statuses"), HttpGet, AllowAnonymous]
        public ItemList<FileOperationResult> GetTasksStatuses()
        {
            ErrorIf(!SecurityContext.IsAuthenticated && !FileShareLink.TryGetSessionId(out _), FilesCommonResource.ErrorMassage_SecurityException);

            return fileOperations.GetOperationResults();
        }

        [ActionName("tasks"), HttpPut, AllowAnonymous]
        public ItemList<FileOperationResult> TerminateTasks()
        {
            ErrorIf(!SecurityContext.IsAuthenticated && !FileShareLink.TryGetSessionId(out _), FilesCommonResource.ErrorMassage_SecurityException);

            return fileOperations.CancelOperations();
        }

        [ActionName("bulkdownload"), HttpPut, AllowAnonymous]
        public ItemList<FileOperationResult> BulkDownload([FromBody] Dictionary<String, String> items)
        {
            Dictionary<object, string> folders;
            Dictionary<object, string> files;

            ParseArrayItems(items, out folders, out files);
            ErrorIf(folders.Count == 0 && files.Count == 0, FilesCommonResource.ErrorMassage_BadRequest);

            return fileOperations.Download(folders, files, GetHttpHeaders(), GetCookies());
        }

        [ActionName("folders-files-moveOrCopyFilesCheck"), HttpPost, AllowAnonymous]
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

                var folders = folderDao.GetFolders(foldersId);
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

        [ActionName("moveorcopy"), HttpPut, AllowAnonymous]
        public ItemList<FileOperationResult> MoveOrCopyItems([FromBody] ItemList<string> items, string destFolderId, FileConflictResolveType resolve, bool ic, bool deleteAfter = false)
        {
            ErrorIf(resolve == FileConflictResolveType.Overwrite && CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor(), FilesCommonResource.ErrorMassage_SecurityException);

            ItemList<FileOperationResult> result;
            if (items.Count != 0)
            {
                List<object> foldersId;
                List<object> filesId;
                ParseArrayItems(items, out foldersId, out filesId);

                result = fileOperations.MoveOrCopy(foldersId, filesId, destFolderId, ic, resolve, !deleteAfter, GetHttpHeaders(), GetCookies());
            }
            else
            {
                result = fileOperations.GetOperationResults();
            }
            return result;
        }

        [ActionName("folders-files"), HttpPut]
        public ItemList<FileOperationResult> DeleteItems(string action, [FromBody] ItemList<String> items, bool ignoreException = false, bool deleteAfter = false, bool immediately = false)
        {
            List<object> foldersId;
            List<object> filesId;
            ParseArrayItems(items, out foldersId, out filesId);

            return fileOperations.Delete(foldersId, filesId, ignoreException, !deleteAfter, immediately, GetHttpHeaders());
        }

        [ActionName("emptytrash"), HttpPut]
        public ItemList<FileOperationResult> EmptyTrash()
        {
            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var trashId = folderDao.GetFolderIDTrash(true);
                var foldersId = folderDao.GetFolders(trashId).Select(f => f.ID).ToList();
                var filesId = fileDao.GetFiles(trashId).ToList();

                return fileOperations.Delete(foldersId, filesId, false, true, false, GetHttpHeaders(), true);
            }
        }

        [ActionName("checkconversion"), HttpPost, AllowAnonymous]
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

        public void DeleteStorage(Guid userId, Guid initiatorId)
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
            using (var linkDao = GetLinkDao())
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

                var rootFolders = folderDao.GetFolders(rootFoldersId);
                foreach (var rootFolder in rootFolders)
                {
                    FileMarker.RemoveMarkAsNew(rootFolder, userId);
                }

                //delete all from My
                if (!Equals(folderIdFromMy, 0))
                {
                    EntryManager.DeleteSubitems(folderIdFromMy, folderDao, fileDao, linkDao);

                    //delete My userFrom folder
                    folderDao.DeleteFolder(folderIdFromMy);
                    Global.FolderMy = userId;
                }

                //delete all from Trash
                var folderIdFromTrash = folderDao.GetFolderIDTrash(false, userId);
                if (!Equals(folderIdFromTrash, 0))
                {
                    EntryManager.DeleteSubitems(folderIdFromTrash, folderDao, fileDao, linkDao);
                    folderDao.DeleteFolder(folderIdFromTrash);
                    Global.FolderTrash = userId;
                }

                EntryManager.ReassignItems(Global.FolderCommon, userId, initiatorId, folderDao, fileDao);
            }
        }

        #endregion

        #region Favorites Manager

        [ActionName("file-favorite"), HttpPut]
        public bool ToggleFileFavorite(String fileId, bool favorite)
        {
            if (favorite)
            {
                AddToFavorites(new ItemList<object> { }, new ItemList<object> { fileId });
            }
            else
            {
                DeleteFavorites(new ItemList<object> { }, new ItemList<object> { fileId });
            }
            return favorite;
        }

        public ItemList<FileEntry> AddToFavorites(ItemList<object> foldersId, ItemList<object> filesId)
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var entries = new List<FileEntry>();

                var files = fileDao.GetFiles(filesId)
                    .Where(file => !file.Encrypted);
                entries.AddRange(files);

                var folders = folderDao.GetFolders(foldersId);
                entries.AddRange(folders);

                entries = FileSecurity.FilterRead(entries);

                var tags = entries.Select(entry => Tag.Favorite(SecurityContext.CurrentAccount.ID, entry));

                tagDao.SaveTags(tags);
                foreach (var entry in entries)
                {
                    FilesMessageService.Send(entry, HttpContext.Current.Request, MessageAction.FileMarkedAsFavorite, entry.Title);
                }

                return new ItemList<FileEntry>(entries);
            }
        }

        public ItemList<FileEntry> DeleteFavorites(ItemList<object> foldersId, ItemList<object> filesId)
        {
            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var entries = new List<FileEntry>();

                var files = fileDao.GetFiles(filesId);
                entries.AddRange(files);

                var folders = folderDao.GetFolders(foldersId);
                entries.AddRange(folders);

                entries = FileSecurity.FilterRead(entries);

                var tags = entries.Select(entry => Tag.Favorite(SecurityContext.CurrentAccount.ID, entry));

                tagDao.RemoveTags(tags);

                foreach (var entry in entries)
                {
                    FilesMessageService.Send(entry, HttpContext.Current.Request, MessageAction.FileRemovedFromFavorite, entry.Title);
                }

                return new ItemList<FileEntry>(entries);
            }
        }

        #endregion

        #region Templates Manager

        public ItemList<FileEntry> AddToTemplates(ItemList<object> filesId)
        {
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            {
                var files = fileDao.GetFiles(filesId);

                files = FileSecurity.FilterRead(files)
                    .Where(file => FileUtility.ExtsWebTemplate.Contains(FileUtility.GetFileExtension(file.Title), StringComparer.CurrentCultureIgnoreCase))
                    .ToList();

                var tags = files.Select(file => Tag.Template(SecurityContext.CurrentAccount.ID, file));

                tagDao.SaveTags(tags);

                return new ItemList<FileEntry>(files);
            }
        }

        public ItemList<FileEntry> DeleteTemplates(ItemList<object> filesId)
        {
            using (var tagDao = GetTagDao())
            using (var fileDao = GetFileDao())
            {
                var files = fileDao.GetFiles(filesId);

                files = FileSecurity.FilterRead(files);

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
                using (var folderDao = GetFolderDao())
                using (var fileDao = GetFileDao())
                {
                    result = EntryManager.GetTemplates(folderDao, fileDao, filter, subjectGroup, subjectId, search, searchInContent);
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
                                    ? fileDao.GetFile(entryId)
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
                result.RemoveAll(ace => ace.IsLink);
            }
            else
            {
                linkAce = result.FirstOrDefault(ace => ace.SubjectId == FileConstant.ShareLinkId);
            }

            result.Sort((x, y) => string.Compare(x.SubjectName, y.SubjectName));

            if (ownerAce != null)
            {
                result = new List<AceWrapper> { ownerAce }.Concat(result).ToList();
            }
            if (meAce != null)
            {
                result = new List<AceWrapper> { meAce }.Concat(result).ToList();
            }
            if (linkAce != null)
            {
                result.Remove(linkAce);
                result = new List<AceWrapper> { linkAce }.Concat(result).ToList();
            }

            return new ItemList<AceWrapper>(result.OrderByDescending(x => x.IsLink));
        }

        [ActionName("sharedinfoshort"), HttpGet]
        public ItemList<AceShortWrapper> GetSharedInfoShort(String objectId)
        {
            var aces = GetSharedInfo(new ItemList<string> { objectId });

            return new ItemList<AceShortWrapper>(
                aces.Where(aceWrapper => !aceWrapper.IsLink || aceWrapper.Share != FileShare.Restrict)
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
                                    ? fileDao.GetFile(entryId)
                                    : (FileEntry)folderDao.GetFolder(entryId);

                    try
                    {
                        var changed = FileSharing.SetAceObject(aceCollection.Aces, entry, notify, aceCollection.Message, aceCollection.AdvancedSettings);
                        if (changed)
                        {
                            foreach (var ace in aceCollection.Aces)
                            {
                                var name = ace.SubjectGroup ? CoreContext.UserManager.GetGroupInfo(ace.SubjectId).Name : CoreContext.UserManager.GetUsers(ace.SubjectId).DisplayUserName(false);
                                FilesMessageService.Send(entry, GetHttpHeaders(),
                                                     entryType == FileEntryType.Folder ? MessageAction.FolderUpdatedAccessFor : MessageAction.FileUpdatedAccessFor,
                                                     entry.Title, name, GetAccessString(ace.Share));
                            }
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
                foreach (var entry in entries)
                {
                    if (entry.FileEntryType == FileEntryType.File)
                    {
                        FilesMessageService.Send(entry, GetHttpHeaders(), MessageAction.FileRemovedFromList, entry.Title);
                    }
                    else
                    {
                        FilesMessageService.Send(entry, GetHttpHeaders(), MessageAction.FolderRemovedFromList, entry.Title);
                    }
                }
            }
        }

        [ActionName("shorten"), HttpGet]
        public string GetShortenLink(String fileId, String linkId, bool isFolder)
        {
            FileEntry fileEntry;

            if (!isFolder)
            {
                using (var fileDao = GetFileDao())
                {
                    fileEntry = fileDao.GetFile(fileId);
                }
            }
            else
            {
                using (var fileDao = GetFolderDao())
                {
                    fileEntry = fileDao.GetFolder(fileId);
                }
            }

            ErrorIf(fileEntry == null, !isFolder ? FilesCommonResource.ErrorMassage_FileNotFound : FilesCommonResource.ErrorMassage_FolderNotFound);
            ErrorIf(!FileSharing.CanSetAccess(fileEntry), FilesCommonResource.ErrorMassage_SecurityException);

            Guid.TryParse(linkId, out Guid shareLinkId);

            //if (shareLinkId != FileConstant.ShareLinkId && shareLinkId != Guid.Empty)
            //{
            //    using (var securityDao = GetSecurityDao())
            //    {
            //        var shareRecord = securityDao.GetShares(new[] { shareLinkId }).FirstOrDefault();
            //        ErrorIf(shareRecord == null || shareRecord.SubjectType != SubjectType.ExternalLink, FilesCommonResource.ErrorMassage_ShareLinkNotFound);
            //    }
            //}

            var link = !isFolder ? FileShareLink.GetLink((File)fileEntry, true, shareLinkId) : FileShareLink.GetLink((Folder)fileEntry, shareLinkId);

            try
            {
                return UrlShortener.Instance.GetShortenLink(link);
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        [ActionName("setacelink"), HttpPut]
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
                var changed = FileSharing.SetAceObject(aces, file, false, null, null);
                if (changed)
                {
                    FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileExternalLinkAccessUpdated, file.Title, GetAccessString(share));
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

        [ActionName("getexternallink"), HttpGet, AllowAnonymous]
        public string GetExternalLink(string entryId)
        {
            ErrorIf(!FileShareLink.TryGetCurrentLinkId(out var linkId), FilesCommonResource.ErrorMassage_SecurityException);
            ErrorIf(string.IsNullOrEmpty(entryId), FilesCommonResource.ErrorMassage_BadRequest);

            var entryType = entryId.StartsWith("file_") ? FileEntryType.File : FileEntryType.Folder;
            var id = entryId.Substring((entryType == FileEntryType.File ? "file_" : "folder_").Length);
            var entry = entryType == FileEntryType.File
                            ? GetFile(id, -1)
                            : (FileEntry)GetFolder(id);
            
            // only editable format on SaaS trial/startup
            ErrorIf(entry is File && !FileUtility.CanWebView(entry.Title) && TenantExtra.Saas && 
                    (TenantExtra.GetTenantQuota().Trial || TenantExtra.GetTenantQuota().Free), FilesCommonResource.ErrorMassage_BadRequest);

            return entryType == FileEntryType.File ? FileShareLink.GetLink((File)entry, true, linkId)
                    : FileShareLink.GetLink((Folder)entry, linkId);
        }

        public Tuple<File, FileShareRecord> ParseFileShareLinkKey(string key)
        {
            var result = ParseShareLinkKey(key);

            return GetFileShareLink(result.Item1, result.Item2);
        }

        public Tuple<Folder, FileShareRecord> ParseFolderShareLinkKey(string key)
        {
            var result = ParseShareLinkKey(key);

            return GetFolderShareLink(result.Item1, result.Item2);
        }

        public Tuple<File, FileShareRecord> GetFileShareLink(string fileId, Guid shareLinkId)
        {
            using (var fileDao = GetFileDao())
            using (var securityDao = GetSecurityDao())
            {
                var file = fileDao.GetFile(fileId);

                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);

                var shareRecord = securityDao.GetShares(new[] { shareLinkId }).FirstOrDefault();

                ErrorIf(shareRecord == null || shareRecord.SubjectType != SubjectType.ExternalLink, FilesCommonResource.ErrorMassage_ShareLinkNotFound);

                ErrorIf(shareRecord.Options != null && shareRecord.Options.IsExpired(), FilesCommonResource.ErrorMassage_ShareLinkExpired);

                return new Tuple<File, FileShareRecord>(file, shareRecord);
            }
        }

        public Tuple<Folder, FileShareRecord> GetFolderShareLink(string folderId, Guid shareLinkId)
        {
            using (var fileDao = GetFolderDao())
            using (var securityDao = GetSecurityDao())
            {
                var folder = fileDao.GetFolder(folderId);

                ErrorIf(folder == null || folder.RootFolderType == FolderType.TRASH || (folder.ProviderEntry && !FilesSettings.EnableThirdParty)
                    || !FilesSettings.ExternalShare, FilesCommonResource.ErrorMassage_FolderNotAvailable);

                var shareRecord = securityDao.GetShares(new[] { shareLinkId }).FirstOrDefault();

                ErrorIf(shareRecord == null || shareRecord.SubjectType != SubjectType.ExternalLink, FilesCommonResource.ErrorMassage_ShareLinkNotFound);

                ErrorIf(shareRecord?.Options != null && shareRecord.Options.IsExpired(), FilesCommonResource.ErrorMassage_ShareLinkExpired);
                
                ErrorIf(shareRecord?.Share == FileShare.Restrict, FilesCommonResource.ErrorMassage_FolderNotAvailable);

                return new Tuple<Folder, FileShareRecord>(folder, shareRecord);
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

                        showSharingSettings |= FileSharing.SetAceObject(aces, file, false, null, null);
                        if (showSharingSettings)
                        {
                            foreach (var ace in aces)
                            {
                                FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileUpdatedAccessFor, file.Title, CoreContext.UserManager.GetUsers(ace.SubjectId).DisplayUserName(false), GetAccessString(ace.Share));
                            }
                        }
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

        [ActionName("external"), HttpPut]
        public bool ChangeExternalShareSettings(bool enable)
        {
            ErrorIf(!Global.IsAdministrator, FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.ExternalShare = enable;

            if (!enable)
            {
                FilesSettings.ExternalShareSocialMedia = false;
            }

            MessageService.Send(GetHttpHeaders(), MessageAction.DocumentsExternalShareSettingsUpdated);

            return FilesSettings.ExternalShare;
        }

        [ActionName("externalsocialmedia"), HttpPut]
        public bool ChangeExternalShareSocialMediaSettings(bool enable)
        {
            ErrorIf(!Global.IsAdministrator, FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.ExternalShareSocialMedia = FilesSettings.ExternalShare && enable;

            MessageService.Send(GetHttpHeaders(), MessageAction.DocumentsExternalShareSettingsUpdated);

            return FilesSettings.ExternalShareSocialMedia;
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
                var folders = folderDao.GetFolders(foldersId);

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
                        var newFolderID = folderDao.SaveFolder(newFolder);

                        newFolder = folderDao.GetFolder(newFolderID);
                        newFolder.Access = folderAccess;

                        EntryManager.SetIsFavoriteFolder(folder);

                        FilesMessageService.Send(newFolder, GetHttpHeaders(), MessageAction.FileChangeOwner, new[] { newFolder.Title, userInfo.DisplayUserName(false) });
                    }
                    entries.Add(newFolder);
                }
            }

            using (var fileDao = GetFileDao())
            {
                var files = fileDao.GetFiles(filesId);

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

                        if (file.ThumbnailStatus == Thumbnail.Created)
                        {
                            using (var thumbnail = fileDao.GetThumbnail(file))
                            {
                                fileDao.SaveThumbnail(newFile, thumbnail);
                            }
                            newFile.ThumbnailStatus = Thumbnail.Created;
                        }

                        FileMarker.MarkAsNew(newFile);

                        EntryManager.SetFileStatus(newFile);

                        FilesMessageService.Send(newFile, GetHttpHeaders(), MessageAction.FileChangeOwner, new[] { newFile.Title, userInfo.DisplayUserName(false) });
                    }
                    entries.Add(newFile);
                }
            }

            return new ItemList<FileEntry>(entries);
        }

        public bool StoreOriginal(bool set)
        {
            FilesSettings.StoreOriginalFiles = set;
            MessageService.Send(GetHttpHeaders(), MessageAction.DocumentsUploadingFormatsSettingsUpdated);

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

        [ActionName("updateifexist"), HttpPut]
        public bool UpdateIfExist(bool set)
        {
            ErrorIf(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor(), FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.UpdateIfExist = set;
            MessageService.Send(GetHttpHeaders(), MessageAction.DocumentsOverwritingSettingsUpdated);

            return FilesSettings.UpdateIfExist;
        }

        [ActionName("forcesave"), HttpPut]
        public bool Forcesave(bool set)
        {
            FilesSettings.Forcesave = set;
            MessageService.Send(GetHttpHeaders(), MessageAction.DocumentsForcesave);

            return FilesSettings.Forcesave;
        }

        [ActionName("storeforcesave"), HttpPut]
        public bool StoreForcesave(bool set)
        {
            ErrorIf(!Global.IsAdministrator, FilesCommonResource.ErrorMassage_SecurityException);

            FilesSettings.StoreForcesave = set;
            MessageService.Send(GetHttpHeaders(), MessageAction.DocumentsStoreForcesave);

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

        [ActionName("changedeleteconfrim"), HttpPut]
        public bool ChangeDeleteConfrim(bool set)
        {
            FilesSettings.ConfirmDelete = set;

            return FilesSettings.ConfirmDelete;
        }

        public AutoCleanUpData ChangeAutomaticallyCleanUp(bool set, DateToAutoCleanUp gap)
        {
            FilesSettings.AutomaticallyCleanUp = new AutoCleanUpData() { IsAutoCleanUp = set, Gap = gap };

            return FilesSettings.AutomaticallyCleanUp;
        }

        [ActionName("autocleanup"), HttpGet]
        public AutoCleanUpData GetSettingsAutomaticallyCleanUp()
        {
            return FilesSettings.AutomaticallyCleanUp;
        }

        public List<FileShare> ChangeDafaultAccessRights(List<FileShare> value)
        {
            FilesSettings.DefaultSharingAccessRights = value;

            return FilesSettings.DefaultSharingAccessRights;
        }

        [ActionName("downloadtargz"), HttpPut]
        public ICompress ChangeDownloadTarGz(bool set)
        {
            FilesSettings.DownloadTarGz = set;

            return CompressToArchive.Instance;
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

        private static ILinkDao GetLinkDao()
        {
            return Global.GetLinkDao();
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

        private IEnumerable<HttpCookie> GetCookies()
        {
            var contextCookie = HttpContext.Current.Request.Cookies;
            var cookies = new List<HttpCookie>();
            
            foreach (var key in contextCookie.AllKeys)
            {
                var cookie = contextCookie[key];
                cookies.Add(new HttpCookie(cookie.Name, cookie.Value));
            }

            return cookies;
        }

        private static string GetAccessString(FileShare fileShare)
        {
            switch (fileShare)
            {
                case FileShare.Read:
                case FileShare.ReadWrite:
                case FileShare.CustomFilter:
                case FileShare.Review:
                case FileShare.FillForms:
                case FileShare.Comment:
                case FileShare.Restrict:
                case FileShare.None:
                    return FilesCommonResource.ResourceManager.GetString("AceStatusEnum_" + fileShare.ToString());
                default:
                    return string.Empty;
            }
        }

        private static Tuple<string, Guid> ParseShareLinkKey(string key)
        {
            ErrorIf(string.IsNullOrEmpty(key), FilesCommonResource.ErrorMassage_ShareLinkInvalid);

            var entryId = FileShareLink.Parse(key, out Guid shareLinkId, out _);

            ErrorIf(string.IsNullOrEmpty(entryId), FilesCommonResource.ErrorMassage_ShareLinkInvalid);

            return new Tuple<string, Guid>(entryId, shareLinkId);
        }

        #endregion
    }
}