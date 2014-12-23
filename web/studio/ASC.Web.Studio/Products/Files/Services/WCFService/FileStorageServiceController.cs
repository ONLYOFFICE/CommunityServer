/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Import;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Services.NotifyService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Files.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Services.WCFService
{
    [Authorize]
    [FileExceptionFilter]
    public class FileStorageServiceController : ApiController, IFileStorageService
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Files");
        private static readonly ProgressQueue Tasks = new ProgressQueue(10, TimeSpan.FromMinutes(5), true);
        private static readonly FileEntrySerializer serializer = new FileEntrySerializer();

        #region Folder Manager

        public Folder GetFolder(String folderId)
        {
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(folderId);

                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanRead(folder), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                return folder;
            }
        }

        [ActionName("folders-subfolders"), HttpGet]
        public ItemList<Folder> GetFolders(String parentId)
        {
            using (var folderDao = GetFolderDao())
            {
                try
                {
                    int total;
                    var folders = EntryManager.GetEntries(folderDao, folderDao.GetFolder(parentId), FilterType.FoldersOnly, Guid.Empty, new OrderBy(SortedByType.AZ, true), string.Empty, 0, 0, out total);
                    return new ItemList<Folder>(folders.OfType<Folder>());
                }
                catch (Exception e)
                {
                    Global.Logger.Error(e);
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

        public DataWrapper GetFolderItems(String parentId, int from, int count, FilterType filter, OrderBy orderBy, String ssubject, String searchText)
        {
            var subjectId = string.IsNullOrEmpty(ssubject) ? Guid.Empty : new Guid(ssubject);

            using (var folderDao = GetFolderDao())
            {
                Folder parent;
                try
                {
                    parent = folderDao.GetFolder(parentId);
                }
                catch (Exception e)
                {
                    throw GenerateException(e);
                }

                ErrorIf(parent == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanRead(parent), FilesCommonResource.ErrorMassage_SecurityException_ViewFolder);
                ErrorIf(parent.RootFolderType == FolderType.TRASH && !Equals(parent.ID, Global.FolderTrash), FilesCommonResource.ErrorMassage_ViewTrashItem);

                if (Equals(parent.ID, Global.FolderShare))
                    orderBy = new OrderBy(SortedByType.New, false);
                else if (orderBy.SortedBy == SortedByType.New)
                    orderBy = new OrderBy(SortedByType.DateAndTime, true);

                int total;
                IEnumerable<FileEntry> entries;
                try
                {
                    entries = EntryManager.GetEntries(folderDao, parent, filter, subjectId, orderBy, searchText, from, count, out total);
                }
                catch (Exception e)
                {
                    throw GenerateException(e);
                }

                var breadCrumbs = EntryManager.GetBreadCrumbs(parentId, folderDao);

                var prevVisible = breadCrumbs.ElementAtOrDefault(breadCrumbs.Count() - 2);
                if (prevVisible != null)
                {
                    parent.ParentFolderID = prevVisible.ID;
                }

                parent.Shareable = FileSharing.CanSetAccess(parent) || parent.FolderType == FolderType.SHARE;

                entries = entries.Where(x => x is Folder || !FileConverter.IsConverting((File)x));

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
        public object GetFolderItemsXml(String parentId, int from, int count, FilterType filter, [FromBody] OrderBy orderBy, String subjectID, String search)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(serializer.ToXml(GetFolderItems(parentId, from, count, filter, orderBy, subjectID, search)));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            return response;
        }

        [ActionName("folders-intries"), HttpPost]
        public ItemList<FileEntry> GetItems([FromBody] ItemList<String> items, FilterType filter, String subjectID, String search)
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

                entries = EntryManager.FilterEntries(entries, filter, subjectId, search);
            }

            EntryManager.SetFileStatus(entries.Select(r => r as File).Where(r => r != null && r.ID != null));

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
                ErrorIf(folder.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem);

                var folderAccess = folder.Access;

                if (String.Compare(folder.Title, title, false) != 0)
                {
                    var newFolderID = folderDao.RenameFolder(folder.ID, title);
                    folder = folderDao.GetFolder(newFolderID);
                    folder.Access = folderAccess;

                    FilesMessageService.Send(folder, GetHttpHeaders(), MessageAction.FolderRenamed, folder.Title);
                }

                var tag = tagDao.GetNewTags(SecurityContext.CurrentAccount.ID, folder).FirstOrDefault();
                if (tag != null)
                {
                    folder.NewForMe = tag.Count;
                }

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

                return file;
            }
        }

        [ActionName("folders-files-siblings"), HttpPost]
        public KeyValuePair<String, ItemDictionary<String, String>> GetSiblingsFile(String fileId, FilterType filter, [FromBody] OrderBy orderBy, String subjectID, String search)
        {
            var subjectId = string.IsNullOrEmpty(subjectID) ? Guid.Empty : new Guid(subjectID);

            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var file = fileDao.GetFile(fileId);
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                var folder = folderDao.GetFolder(file.FolderID);
                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(folder.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem);

                var folderId = file.FolderID;
                var entries = Enumerable.Empty<FileEntry>();
                if (!FileSecurity.CanRead(folder) &&
                    folder.RootFolderType == FolderType.USER && !Equals(folder.RootFolderId, Global.FolderMy))
                {
                    folderId = Global.FolderShare;
                    orderBy = new OrderBy(SortedByType.DateAndTime, false);

                    var shared = (IEnumerable<FileEntry>)FileSecurity.GetSharesForMe();
                    shared = EntryManager.FilterEntries(shared, filter, subjectId, search)
                                         .Where(f => f is File &&
                                                     f.CreateBy != SecurityContext.CurrentAccount.ID && // don't show my files
                                                     f.RootFolderType == FolderType.USER); // don't show common files (common files can read)
                    entries = entries.Concat(shared);
                }
                else if (folder.FolderType == FolderType.BUNCH)
                {
                    var path = folderDao.GetBunchObjectID(folder.RootFolderId);

                    var projectID = path.Split('/').Last();

                    if (String.IsNullOrEmpty(projectID))
                    {
                        folderId = Global.FolderMy;
                        entries = entries.Concat(new List<FileEntry> { file });
                    }
                    else
                    {
                        entries = entries.Concat(folderDao.GetFiles(folder.ID, orderBy, filter, subjectId, search));
                    }
                }
                else
                {
                    entries = entries.Concat(folderDao.GetFiles(folder.ID, orderBy, filter, subjectId, search));
                }

                entries = EntryManager.SortEntries(entries, orderBy);

                var siblingType = FileUtility.GetFileTypeByFileName(file.Title);

                var result = new ItemDictionary<String, String>();
                FileSecurity.FilterRead(entries)
                            .OfType<File>()
                            .Where(f => siblingType.Equals(FileUtility.GetFileTypeByFileName(f.Title)))
                            .ToList()
                            .ForEach(f => result.Add(f.ID.ToString(), f.Version + "&" + f.Title));

                return new KeyValuePair<string, ItemDictionary<string, string>>(folderId.ToString(), result);
            }
        }

        [ActionName("folders-files-createfile"), HttpGet]
        public File CreateNewFile(String parentId, String title)
        {
            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(parentId);
                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(folder.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_CreateNewFolderInTrash);
                ErrorIf(!FileSecurity.CanCreate(folder), FilesCommonResource.ErrorMassage_SecurityException_Create);

                var file = new File
                    {
                        FolderID = folder.ID
                    };

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

                file.ConvertedType = fileExt;

                if (folder.ProviderEntry)
                {
                    file.ConvertedType = null;
                }

                var culture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();
                var storeTemp = GetStoreTemplate();

                var path = FileConstant.NewDocPath + culture + "/";
                if (!storeTemp.IsDirectory(path))
                {
                    path = FileConstant.NewDocPath + "default/";
                }

                path += "new" + fileExt;

                file.ContentLength = storeTemp.GetFileSize(path);

                try
                {
                    using (var stream = storeTemp.IronReadStream("", path, 10))
                    {
                        file = fileDao.SaveFile(file, stream);
                    }
                }
                catch (Exception e)
                {
                    throw GenerateException(e);
                }
                FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileCreated, file.Title);

                FileMarker.MarkAsNew(file);

                return file;
            }
        }

        [ActionName("trackeditfile"), HttpGet, AllowAnonymous]
        public KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String doc = null, bool isFinish = false, bool fixedVersion = false)
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
                }
                else
                {
                    EntryManager.TrackEditing(id, tabId, SecurityContext.CurrentAccount.ID, fixedVersion, doc);
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

            foreach (var id in filesId.Where(FileTracker.IsEditing))
            {
                var usersId = FileTracker.GetEditingBy(id);
                var value = string.Join(", ", usersId.Select(Global.GetUserName).ToArray());
                result[id] = value;
            }

            return result;
        }

        [ActionName("canedit"), HttpGet, AllowAnonymous]
        public Guid CanEdit(String fileId, String doc = null)
        {
            File file;
            using (var fileDao = GetFileDao())
            {
                var editLink = FileShareLink.Check(doc, false, fileDao, out file);
                if (file == null)
                    file = fileDao.GetFile(fileId);

                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!editLink && (!FileSecurity.CanEdit(file) || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()), FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                ErrorIf(EntryManager.FileLockedForMe(file.ID), FilesCommonResource.ErrorMassage_LockedFile);
                ErrorIf(file.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem);
                ErrorIf(!FileUtility.CanWebEdit(file.Title) && !FileConverter.MustConvert(file), FilesCommonResource.ErrorMassage_NotSupportedFormat);
                ErrorIf((!FileUtility.CanCoAuhtoring(file.Title) || FileTracker.IsEditingAlone(file.ID))
                    && FileTracker.IsEditing(file.ID), FilesCommonResource.ErrorMassage_UpdateEditingFile);
            }

            return FileTracker.Add(file.ID, false);
        }

        [ActionName("saveediting"), HttpGet, AllowAnonymous]
        public File SaveEditing(String fileId, int version, Guid tabId, string fileuri, bool asNew, String doc = null)
        {
            try
            {
                var file = EntryManager.SaveEditing(fileId, version, tabId, fileuri, asNew, doc);

                if (file != null)
                    FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileUpdated, file.Title);
                return file;
            }
            catch (Exception ex)
            {
                Global.Logger.Error(string.Format("Error on save. File id: {0}. DownloadUri: {1}", fileId, fileuri), ex);
                throw GenerateException(ex);
            }
        }

        [ActionName("startedit"), HttpGet, AllowAnonymous]
        public string StartEdit(String fileId, String docKeyForTrack, bool asNew, bool editingAlone = false, String doc = null)
        {
            try
            {
                ErrorIf(editingAlone && FileTracker.IsEditing(fileId), FilesCommonResource.ErrorMassage_SecurityException_EditFileTwice);

                //lonely editing only for old scheme
                var tabId = editingAlone ? Guid.Empty : SecurityContext.CurrentAccount.ID;

                EntryManager.TrackEditing(fileId, tabId, SecurityContext.CurrentAccount.ID, asNew, doc, editingAlone);
                if (!editingAlone && !DocumentServiceTracker.StartTrack(fileId, docKeyForTrack, asNew))
                {
                    throw new Exception(FilesCommonResource.ErrorMassage_StartEditing);
                }
            }
            catch (Exception ex)
            {
                FileTracker.Remove(fileId);
                throw GenerateException(ex);
            }

            return string.IsNullOrEmpty(docKeyForTrack) ? DocumentServiceHelper.GetDocKey(fileId, -1, DateTime.MinValue) : docKeyForTrack;
        }

        [ActionName("folders-files-rename"), HttpGet]
        public File FileRename(String fileId, String title)
        {
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId);
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanEdit(file), FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
                ErrorIf(EntryManager.FileLockedForMe(file.ID), FilesCommonResource.ErrorMassage_LockedFile);
                ErrorIf(FileTracker.IsEditing(file.ID), FilesCommonResource.ErrorMassage_UpdateEditingFile);
                ErrorIf(file.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem);

                title = Global.ReplaceInvalidCharsAndTruncate(title);

                var ext = FileUtility.GetFileExtension(file.Title);
                if (string.Compare(ext, FileUtility.GetFileExtension(title), true) != 0)
                {
                    title += ext;
                }

                var fileAccess = file.Access;

                if (String.Compare(file.Title, title, false) != 0)
                {
                    var newFileID = fileDao.FileRename(file.ID, title);

                    file = fileDao.GetFile(newFileID);
                    file.Access = fileAccess;

                    FilesMessageService.Send(file, GetHttpHeaders(), MessageAction.FileRenamed, file.Title);
                }

                EntryManager.SetFileStatus(file);

                return file;
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
            return new KeyValuePair<File, ItemList<File>>(file, GetFileHistory(fileId));
        }

        [ActionName("folders-files-updateComment"), HttpGet]
        public string UpdateComment(String fileId, int version, String comment)
        {
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId, version);
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanEdit(file), FilesCommonResource.ErrorMassage_SecurityException_EditFile);
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

                ErrorIf(tagLocked != null && tagLocked.Owner != SecurityContext.CurrentAccount.ID && !Global.IsAdministrator, FilesCommonResource.ErrorMassage_LockedFile);

                if (lockfile)
                {
                    if (tagLocked == null)
                    {
                        tagLocked = new Tag("locked", TagType.Locked, SecurityContext.CurrentAccount.ID, file, 0);

                        tagDao.SaveTags(tagLocked);
                    }
                    FileTracker.RemoveAllOther(file.ID);

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
                return file;
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
                Global.Logger.Error(e);
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

            var task = new FileMarkAsReadOperation(
                CoreContext.TenantManager.GetCurrentTenant(),
                foldersId,
                filesId
                );

            Tasks.Add(task);

            return GetTasksStatuses();
        }

        #endregion

        #region ThirdParty

        public ItemList<ThirdPartyParams> GetThirdParty()
        {
            using (var providerDao = GetProviderDao())
            {
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
            using (var folderDao = GetFolderDao())
            using (var providerDao = GetProviderDao())
            {
                var providersInfo =
                    (FolderType)folderType == FolderType.DEFAULT
                        ? providerDao.GetProvidersInfo()
                        : providerDao.GetProvidersInfo((FolderType)folderType);

                var folders = folderDao.GetFolders(providersInfo.Select(r => r.RootFolderId).ToArray());
                foreach (var folder in folders)
                {
                    folder.NewForMe = folder.RootFolderType == FolderType.COMMON ? 1 : 0;
                }

                return new ItemList<Folder>(folders);
            }
        }

        [ActionName("thirdparty-save"), HttpPost]
        public Folder SaveThirdParty([FromBody] ThirdPartyParams thirdPartyParams)
        {
            using (var folderDao = GetFolderDao())
            using (var providerDao = GetProviderDao())
            {
                var parentFolder = folderDao.GetFolder(thirdPartyParams.Corporate && !CoreContext.Configuration.Personal ? Global.FolderCommon : Global.FolderMy);
                ErrorIf(!FileSecurity.CanCreate(parentFolder), FilesCommonResource.ErrorMassage_SecurityException_Create);
                ErrorIf(!Global.IsAdministrator && !FilesSettings.EnableThirdParty, FilesCommonResource.ErrorMassage_SecurityException_Create);

                var lostFolderType = FolderType.USER;
                var folderType = thirdPartyParams.Corporate ? FolderType.COMMON : FolderType.USER;

                int curProviderId;

                MessageAction messageAction;
                if (string.IsNullOrEmpty(thirdPartyParams.ProviderId))
                {
                    ErrorIf(!ImportConfiguration.SupportInclusion
                            ||
                            (!Global.IsAdministrator
                             && !CoreContext.Configuration.Personal
                             && !FilesSettings.EnableThirdParty)
                            , FilesCommonResource.ErrorMassage_SecurityException_Create);
                    try
                    {
                        curProviderId = providerDao.SaveProviderInfo(thirdPartyParams.ProviderKey, thirdPartyParams.CustomerTitle, thirdPartyParams.AuthData, folderType);
                        messageAction = MessageAction.ThirdPartyCreated;
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
                    lostFolderType = lostProvider.RootFolderType;
                    if (!thirdPartyParams.Corporate)
                    {
                        var lostFolder = folderDao.GetFolder(lostProvider.RootFolderId);
                        FileMarker.RemoveMarkAsNewForAll(lostFolder);
                    }

                    curProviderId = providerDao.UpdateProviderInfo(curProviderId, thirdPartyParams.CustomerTitle, folderType);
                    messageAction = MessageAction.ThirdPartyUpdated;
                }

                var provider = providerDao.GetProviderInfo(curProviderId);

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
            using (var folderDao = GetFolderDao())
            using (var providerDao = GetProviderDao())
            {
                var curProviderId = Convert.ToInt32(providerId);
                var provider = providerDao.GetProviderInfo(curProviderId);

                var folder = folderDao.GetFolder(provider.RootFolderId);
                ErrorIf(!FileSecurity.CanDelete(folder), FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder);

                if (provider.RootFolderType == FolderType.COMMON)
                {
                    FileMarker.RemoveMarkAsNewForAll(folder);
                }

                providerDao.RemoveProviderInfo(folder.ProviderId);
                FilesMessageService.Send(folder, GetHttpHeaders(), MessageAction.ThirdPartyDeleted, folder.ID.ToString(), provider.ProviderKey);

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

        #endregion

        #region Operation

        [ActionName("tasks-statuses"), HttpGet]
        public ItemList<FileOperationResult> GetTasksStatuses()
        {
            ErrorIf(!SecurityContext.IsAuthenticated, FilesCommonResource.ErrorMassage_SecurityException);
            var operations = Tasks.GetItems()
                                  .Where(t => t is FileOperation && ((FileOperation)t).Owner == SecurityContext.CurrentAccount.ID)
                                  .Select(o => Tasks.GetStatus(o.Id.ToString()))
                                  .Select(o => ((FileOperation)o).GetResult())
                                  .ToList();

            return new ItemList<FileOperationResult>(operations);
        }

        [ActionName("tasks"), HttpGet]
        public ItemList<FileOperationResult> TerminateTasks()
        {
            var statuses = GetTasksStatuses().ToList();
            statuses.ForEach(s => { s.Progress = 100; });

            var myTasks = Tasks.GetItems()
                               .Where(t => t is FileOperation && ((FileOperation)t).Owner == SecurityContext.CurrentAccount.ID)
                               .Cast<FileOperation>();

            foreach (var task in myTasks)
            {
                try
                {
                    task.Terminate();
                    Tasks.Remove(task);
                }
                catch (Exception ex)
                {
                    Global.Logger.Error(ex);
                }
            }
            return new ItemList<FileOperationResult>(statuses);
        }

        [ActionName("bulkdownload"), HttpPost]
        public ItemList<FileOperationResult> BulkDownload([FromBody] Dictionary<String, String> items)
        {
            Dictionary<object, string> folders;
            Dictionary<object, string> files;

            ParseArrayItems(items, out folders, out files);
            ErrorIf(folders.Count == 0 && files.Count == 0, FilesCommonResource.ErrorMassage_BadRequest);

            var task = new FileDownloadOperation(CoreContext.TenantManager.GetCurrentTenant(), folders, files, GetHttpHeaders());

            lock (Tasks)
            {
                var oldTask = Tasks.GetStatus(task.Id.ToString());
                if (oldTask != null)
                {
                    ErrorIf(!oldTask.IsCompleted, FilesCommonResource.ErrorMassage_ManyDownloads);
                    Tasks.Remove(oldTask);
                }
                Tasks.Add(task);
            }

            return GetTasksStatuses();
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
                    if (file != null && fileDao.IsExist(file.Title, toFolder.ID))
                    {
                        result.Add(id.ToString(), file.Title);
                    }
                }

                var folders = folderDao.GetFolders(foldersId.ToArray());
                var foldersProject = folders.Where(folder => folder.RootFolderType == FolderType.BUNCH).ToList();
                if (foldersProject.Any())
                {
                    var toSubfolders = folderDao.GetFolders(toFolder.ID);

                    foreach (var folderProject in foldersProject)
                    {
                        var toSub = toSubfolders.FirstOrDefault(to => Equals(to.Title, folderProject.Title));
                        if (toSub == null) continue;

                        var filesPr = folderDao.GetFiles(folderProject.ID, false);
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
        public ItemList<FileOperationResult> MoveOrCopyItems([FromBody] ItemList<string> items, string destFolderId, FileConflictResolveType resolve, bool ic)
        {
            if (items.Count != 0)
            {
                List<object> foldersId;
                List<object> filesId;
                ParseArrayItems(items, out foldersId, out filesId);

                var task = new FileMoveCopyOperation(
                    CoreContext.TenantManager.GetCurrentTenant(),
                    foldersId,
                    filesId,
                    destFolderId,
                    ic,
                    resolve,
                    GetHttpHeaders());

                Tasks.Add(task);
            }

            return GetTasksStatuses();
        }

        [ActionName("folders-files"), HttpPost]
        public ItemList<FileOperationResult> DeleteItems(string action, [FromBody] ItemList<String> items)
        {
            return DeleteItems(action, items, false);
        }

        [ActionName("folders-files"), HttpPost]
        public ItemList<FileOperationResult> DeleteItems(string action, [FromBody] ItemList<String> items, bool ignoreException)
        {
            List<object> foldersId;
            List<object> filesId;
            ParseArrayItems(items, out foldersId, out filesId);

            var task = new FileDeleteOperation(CoreContext.TenantManager.GetCurrentTenant(), foldersId, filesId, ignoreException, GetHttpHeaders());
            Tasks.Add(task);

            return GetTasksStatuses();
        }

        [ActionName("emptytrash"), HttpGet]
        public ItemList<FileOperationResult> EmptyTrash()
        {
            using (var folderDao = GetFolderDao())
            {
                var trashId = folderDao.GetFolderIDTrash(true);
                var foldersId = folderDao.GetFolders(trashId).Select(f => f.ID).ToList();
                var filesId = folderDao.GetFiles(trashId, false).ToList();
                var task = new FileDeleteOperation(CoreContext.TenantManager.GetCurrentTenant(), foldersId, filesId, false, GetHttpHeaders());

                Tasks.Add(task);
            }
            return GetTasksStatuses();
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
                            FileConverter.ExecAsync(file, false);
                        }
                        catch (Exception e)
                        {
                            throw GenerateException(e);
                        }
                    }

                    files.Add(new KeyValuePair<File, bool>(file, false));
                }

                var results = FileConverter.GetStatus(files).ToList();
                for (var i = 0; i < results.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(results[i].Processed) && results[i].Processed == "1")
                    {
                        FilesMessageService.Send(files[i].Key, GetHttpHeaders(), MessageAction.FileConverted, files[i].Key.Title);
                    }
                }

                return new ItemList<FileOperationResult>(results);
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
                                aceForObject.Share = FileShare.Varies;
                            }
                            continue;
                        }

                        if (duplicate.Share != aceForObject.Share)
                        {
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

                    result.AddRange(acesForObject);
                }
            }

            if (objectIds.Count > 1)
            {
                result.RemoveAll(ace => ace.SubjectId == FileConstant.ShareLinkId);
            }

            result.Sort((x, y) => string.Compare(x.SubjectName, y.SubjectName));

            return new ItemList<AceWrapper>(result);
        }

        [ActionName("sharedinfoshort"), HttpGet]
        public ItemList<AceShortWrapper> GetSharedInfoShort(String objectId)
        {
            var aces = GetSharedInfo(new ItemList<string> { objectId });

            return new ItemList<AceShortWrapper>(
                aces.Where(aceWrapper => !aceWrapper.SubjectId.Equals(FileConstant.ShareLinkId))
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
                        FileSharing.SetAceObject(aceCollection.Aces, entry, notify, aceCollection.Message);
                        FilesMessageService.Send(entry, GetHttpHeaders(),
                                                 entryType == FileEntryType.Folder ? MessageAction.FolderUpdatedAccess : MessageAction.FileUpdatedAccess,
                                                 entry.Title);
                    }
                    catch (Exception e)
                    {
                        throw GenerateException(e);
                    }

                    //reget SharedByMe
                    entry = entryType == FileEntryType.File
                                ? (FileEntry)fileDao.GetFile(entryId)
                                : (FileEntry)folderDao.GetFolder(entryId);
                    if (entry.SharedByMe)
                    {
                        result.Add(objectId);
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

            var uri = new Uri(shareLink);

            var bitly = string.Format(Global.BitlyUrl, Uri.EscapeDataString(uri.ToString()));
            XDocument response;
            try
            {
                response = XDocument.Load(bitly);
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }

            ErrorIf(response.XPathSelectElement("/response/status_code").Value != ((int)HttpStatusCode.OK).ToString(CultureInfo.InvariantCulture), FilesCommonResource.ErrorMassage_BadRequest);
            var data = response.XPathSelectElement("/response/data/url");

            return data.Value;
        }

        [ActionName("sendlinktoemail"), HttpPost]
        public void SendLinkToEmail(String fileId, [FromBody] MessageParams messageAddresses)
        {
            ErrorIf(messageAddresses == null, FilesCommonResource.ErrorMassage_BadRequest);

            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(fileId);
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);

                var shareRecord = FileSecurity.GetShares(file).FirstOrDefault(r => r.Subject == FileConstant.ShareLinkId);
                ErrorIf(shareRecord == null, FilesCommonResource.ErrorMassage_SecurityException);

                ErrorIf(!FileSharing.CanSetAccess(file), FilesCommonResource.ErrorMassage_SecurityException);

                file.Access = shareRecord.Share;

                var shareLink = FileShareLink.GetLink(file);

                NotifyClient.SendLinkToEmail(file, shareLink, messageAddresses.Message, messageAddresses.Address);
            }
        }

        #endregion

        [ActionName("storeoriginal"), HttpGet]
        public bool StoreOriginal(bool set)
        {
            FilesSettings.StoreOriginalFiles = set;
            FilesMessageService.Send(GetHttpHeaders(), MessageAction.DocumentsUploadingFormatsSettingsUpdated);

            return FilesSettings.StoreOriginalFiles;
        }

        [ActionName("updateifexist"), HttpGet]
        public bool UpdateIfExist(bool set)
        {
            FilesSettings.UpdateIfExist = set;
            FilesMessageService.Send(GetHttpHeaders(), MessageAction.DocumentsOverwritingSettingsUpdated);

            return FilesSettings.UpdateIfExist;
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

        private void ErrorIf(bool condition, string errorMessage)
        {
            if (condition) throw new InvalidOperationException(errorMessage);
        }

        private Exception GenerateException(Exception error)
        {
            log.Error(error);
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