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


using ASC.Files.Core;
using ASC.Web.Files.Services.WCFService.FileOperations;
using System;
using System.Collections.Generic;
using System.IO;
using File = ASC.Files.Core.File;

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

        DataWrapper GetFolderItems(String parentId, int from, int count, FilterType filter, OrderBy orderBy, String subjectID, String searchText);

        object GetFolderItemsXml(String parentId, int from, int count, FilterType filter, OrderBy orderBy, String subjectID, String searchText);

        ItemList<FileEntry> GetItems(ItemList<String> items, FilterType filter, String subjectID, String searchText);

        ItemDictionary<String, String> MoveOrCopyFilesCheck(ItemList<String> items, String destFolderId);

        ItemList<FileOperationResult> MoveOrCopyItems(ItemList<String> items, String destFolderId, FileConflictResolveType resolveType, bool isCopyOperation);

        ItemList<FileOperationResult> DeleteItems(string action, ItemList<String> items);

        ItemList<FileOperationResult> DeleteItems(string action, ItemList<String> items, bool ignoreException);

        #endregion

        #region File Manager

        File GetFile(String fileId, int version);

        File CreateNewFile(String parentId, String fileTitle);

        File FileRename(String fileId, String title);

        KeyValuePair<File, ItemList<File>> UpdateToVersion(String fileId, int version);

        KeyValuePair<File, ItemList<File>> CompleteVersion(String fileId, int version, bool continueVersion);

        String UpdateComment(String fileId, int version, String comment);

        ItemList<File> GetFileHistory(String fileId);

        KeyValuePair<String, ItemDictionary<String, String>> GetSiblingsFile(String fileId, FilterType filter, OrderBy orderBy, String subjectID, String searchText);

        KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String shareLinkKey, bool isFinish, bool fixedVersion);

        ItemDictionary<String, String> CheckEditing(ItemList<String> filesId);

        File SaveEditing(String fileId, int version, Guid tabId, string fileExtension, string fileuri, Stream stream, bool asNew, String shareLinkKey);

        string StartEdit(String fileId, String docKeyForTrack, bool asNew, bool editingAlone, String shareLinkKey);

        ItemList<FileOperationResult> CheckConversion(ItemList<ItemList<String>> filesIdVersion);

        File LockFile(String fileId, bool lockFile);

        ItemList<EditHistory> GetEditHistory(String fileId, String shareLinkKey);

        EditHistoryData GetEditDiffUrl(String fileId, int version, String doc = null);

        #endregion

        #region Utils

        ItemList<FileOperationResult> BulkDownload(Dictionary<String, String> items);

        ItemList<FileOperationResult> GetTasksStatuses();

        ItemList<FileOperationResult> EmptyTrash();

        ItemList<FileOperationResult> TerminateTasks();

        String GetShortenLink(String fileId);

        void SendLinkToEmail(String fileId, MessageParams messageAddresses);

        bool StoreOriginal(bool store);

        bool UpdateIfExist(bool update);

        String GetHelpCenter();

        #endregion

        #region Ace Manager

        ItemList<AceWrapper> GetSharedInfo(ItemList<String> objectId);

        ItemList<AceShortWrapper> GetSharedInfoShort(String objectId);

        ItemList<String> SetAceObject(AceCollection aceCollection, bool notify);

        void RemoveAce(ItemList<String> items);

        ItemList<FileOperationResult> MarkAsRead(ItemList<String> items);

        object GetNewItems(String folderId);

        #endregion

        #region ThirdParty

        ItemList<ThirdPartyParams> GetThirdParty();

        ItemList<Folder> GetThirdPartyFolder(int folderType);

        Folder SaveThirdParty(ThirdPartyParams thirdPartyParams);

        object DeleteThirdParty(String providerId);

        bool ChangeAccessToThirdparty(bool enableThirdpartySettings);

        #endregion

        #region MailMerge

        ItemList<String> GetMailAccounts();

        #endregion
    }
}