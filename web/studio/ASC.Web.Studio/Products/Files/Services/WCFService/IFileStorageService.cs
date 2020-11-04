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
using ASC.Files.Core;
using ASC.Web.Files.Core.Entries;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Services.WCFService.FileOperations;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;

namespace ASC.Web.Files.Services.WCFService
{
    public interface IFileStorageService
    {
        #region Folder Manager

        Folder GetFolder(String folderId);

        ItemList<Folder> GetFolders(String parentId);

        ItemList<object> GetPath(String folderId);

        Folder CreateNewFolder(String parentId, String title);

        Folder FolderRename(String folderId, String title);

        DataWrapper GetFolderItems(String parentId, int from, int count, FilterType filter, bool subjectGroup, String subjectID, String searchText, bool searchInContent, bool withSubfolders, OrderBy orderBy);

        object GetFolderItemsXml(String parentId, int from, int count, FilterType filter, bool subjectGroup, String subjectID, String searchText, bool searchInContent, bool withSubfolders, OrderBy orderBy);

        ItemList<FileEntry> GetItems(ItemList<String> items, FilterType filter, bool subjectGroup, String subjectID, String searchText);

        ItemDictionary<String, String> MoveOrCopyFilesCheck(ItemList<String> items, String destFolderId);

        ItemList<FileOperationResult> MoveOrCopyItems(ItemList<String> items, String destFolderId, FileConflictResolveType resolveType, bool isCopyOperation, bool deleteAfter = false);

        ItemList<FileOperationResult> DeleteItems(string action, ItemList<String> items, bool ignoreException = false, bool deleteAfter = false, bool immediately = false);

        void ReassignStorage(Guid userFromId, Guid userToId);

        void DeleteStorage(Guid userId);

        #endregion

        #region File Manager

        File GetFile(String fileId, int version);

        File CreateNewFile(String parentId, String fileTitle, string templateId);

        File FileRename(String fileId, String title);

        KeyValuePair<File, ItemList<File>> UpdateToVersion(String fileId, int version);

        KeyValuePair<File, ItemList<File>> CompleteVersion(String fileId, int version, bool continueVersion);

        String UpdateComment(String fileId, int version, String comment);

        ItemList<File> GetFileHistory(String fileId);

        ItemList<File> GetSiblingsFile(String fileId, String folderId, FilterType filter, bool subjectGroup, String subjectID, String searchText, bool searchInContent, bool withSubfolders, OrderBy orderBy);

        KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String doc, bool isFinish);

        ItemDictionary<String, String> CheckEditing(ItemList<String> filesId);

        File SaveEditing(String fileId, string fileExtension, string fileuri, Stream stream, String doc, bool forcesave);

        File UpdateFileStream(String fileId, Stream stream, bool encrypted, bool forcesave);

        string StartEdit(String fileId, bool editingAlone, String doc);

        ItemList<FileOperationResult> CheckConversion(ItemList<ItemList<String>> filesIdVersion);

        File LockFile(String fileId, bool lockFile);

        ItemList<EditHistory> GetEditHistory(String fileId, String doc);

        EditHistoryData GetEditDiffUrl(String fileId, int version, String doc = null);

        ItemList<EditHistory> RestoreVersion(String fileId, int version, String url, String doc = null);

        Web.Core.Files.DocumentService.FileLink GetPresignedUri(String fileId);

        #endregion

        #region Favorites Manager

        ItemList<FileEntry> AddToFavorites(ItemList<String> foldersId, ItemList<String> filesId);

        ItemList<FileEntry> DeleteFavorites(ItemList<String> foldersId, ItemList<String> filesId);

        #endregion

        #region Templates Manager

        ItemList<FileEntry> AddToTemplates(ItemList<String> filesId);

        ItemList<FileEntry> DeleteTemplates(ItemList<String> filesId);

        object GetTemplates(FilterType filter, int from, int count, bool subjectGroup, String ssubject, String searchText, bool searchInContent);

        #endregion

        #region Utils

        ItemList<FileEntry> ChangeOwner(ItemList<String> items, Guid userId);

        ItemList<FileOperationResult> BulkDownload(Dictionary<String, String> items);

        ItemList<FileOperationResult> GetTasksStatuses();

        ItemList<FileOperationResult> EmptyTrash();

        ItemList<FileOperationResult> TerminateTasks();

        String GetShortenLink(String fileId);

        bool StoreOriginal(bool store);

        bool HideConfirmConvert(bool isForSave);

        bool UpdateIfExist(bool update);

        bool Forcesave(bool value);

        bool StoreForcesave(bool value);

        bool DisplayRecent(bool value);

        bool DisplayFavorite(bool value);

        bool DisplayTemplates(bool value);

        bool ChangeDeleteConfrim(bool update);

        String GetHelpCenter();

        #endregion

        #region Ace Manager

        ItemList<AceWrapper> GetSharedInfo(ItemList<String> objectId);

        ItemList<AceShortWrapper> GetSharedInfoShort(String objectId);

        ItemList<String> SetAceObject(AceCollection aceCollection, bool notify);

        void RemoveAce(ItemList<String> items);

        ItemList<FileOperationResult> MarkAsRead(ItemList<String> items);

        object GetNewItems(String folderId);

        bool SetAceLink(String fileId, FileShare share);

        ItemList<MentionWrapper> SharedUsers(String fileId);

        ItemList<AceShortWrapper> SendEditorNotify(String fileId, MentionMessageWrapper mentionMessage);

        ItemList<EncryptionKeyPair> GetEncryptionAccess(String fileId);

        #endregion

        #region ThirdParty

        ItemList<ThirdPartyParams> GetThirdParty();

        ItemList<Folder> GetThirdPartyFolder(int folderType);

        Folder SaveThirdParty(ThirdPartyParams thirdPartyParams);

        object DeleteThirdParty(String providerId);

        bool ChangeAccessToThirdparty(bool enableThirdpartySettings);

        bool SaveDocuSign(String code);

        object DeleteDocuSign();

        String SendDocuSign(string fileId, DocuSignData docuSignData);

        #endregion

        #region MailMerge

        ItemList<String> GetMailAccounts();

        #endregion
    }
}