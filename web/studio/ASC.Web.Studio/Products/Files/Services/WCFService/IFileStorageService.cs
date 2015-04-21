/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

namespace ASC.Web.Files.Services.WCFService
{
    //[ServiceContract(Namespace = "")]
    public interface IFileStorageService
    {
        #region Folder Manager

        //[OperationContract]
        Folder GetFolder(String folderId);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLGetSubFolders)]
        ItemList<Folder> GetFolders(String parentId);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLGetPath, ResponseFormat = WebMessageFormat.Json)]
        ItemList<object> GetPath(String folderId);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLCreateFolder)]
        Folder CreateNewFolder(String parentId, String title);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLRenameFolder)]
        Folder FolderRename(String folderId, String title);

        DataWrapper GetFolderItems(String parentId, int from, int count, FilterType filter, OrderBy orderBy, String subjectID, String searchText);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.XMLPostFolderItems, Method = "POST")]
        object GetFolderItemsXml(String parentId, int from, int count, FilterType filter, OrderBy orderBy, String subjectID, String searchText);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostItems, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileEntry> GetItems(ItemList<String> items, FilterType filter, String subjectID, String searchText);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostCheckMoveFiles, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemDictionary<String, String> MoveOrCopyFilesCheck(ItemList<String> items, String destFolderId);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostMoveItems, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> MoveOrCopyItems(ItemList<String> items, String destFolderId, FileConflictResolveType resolveType, bool isCopyOperation);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostDeleteItems, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> DeleteItems(string action, ItemList<String> items);

        ItemList<FileOperationResult> DeleteItems(string action, ItemList<String> items, bool ignoreException);

        #endregion

        #region File Manager

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLLastFileVersion)]
        File GetFile(String fileId, int version);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLCreateNewFile)]
        File CreateNewFile(String parentId, String fileTitle);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLRenameFile)]
        File FileRename(String fileId, String title);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONUpdateToVersion, ResponseFormat = WebMessageFormat.Json)]
        KeyValuePair<File, ItemList<File>> UpdateToVersion(String fileId, int version);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONCompleteVersion, ResponseFormat = WebMessageFormat.Json)]
        KeyValuePair<File, ItemList<File>> CompleteVersion(String fileId, int version, bool continueVersion);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONUpdateComment, ResponseFormat = WebMessageFormat.Json)]
        String UpdateComment(String fileId, int version, String comment);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLGetFileHistory)]
        ItemList<File> GetFileHistory(String fileId);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostGetSiblingsFile, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        KeyValuePair<String, ItemDictionary<String, String>> GetSiblingsFile(String fileId, FilterType filter, OrderBy orderBy, String subjectID, String searchText);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetTrackEditFile, ResponseFormat = WebMessageFormat.Json)]
        KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String shareLinkKey, bool isFinish, bool fixedVersion);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostCheckEditing, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemDictionary<String, String> CheckEditing(ItemList<String> filesId);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetCanEdit, ResponseFormat = WebMessageFormat.Json)]
        Guid CanEdit(String fileId, String shareLinkKey);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetSaveEditing, ResponseFormat = WebMessageFormat.Json)]
        File SaveEditing(String fileId, int version, Guid tabId, string fileuri, bool asNew, String shareLinkKey);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetStartEdit, ResponseFormat = WebMessageFormat.Json)]
        string StartEdit(String fileId, String docKeyForTrack, bool asNew, bool editingAlone, String shareLinkKey);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostCheckConversion, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> CheckConversion(ItemList<ItemList<String>> filesIdVersion);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.XMLGetLockFile)]
        File LockFile(String fileId, bool lockFile);

        ItemList<EditHistory> GetEditHistory(String fileId, String shareLinkKey);

        KeyValuePair<string, string> GetEditDiffUrl(String fileId, int version, String doc = null);

        #endregion

        #region Utils

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostBulkDownload, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> BulkDownload(Dictionary<String, String> items);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetTasksStatuses, ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> GetTasksStatuses();

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONEmptyTrash, ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> EmptyTrash();

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONTerminateTasks, ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> TerminateTasks();

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetShortenLink, ResponseFormat = WebMessageFormat.Json)]
        String GetShortenLink(String fileId);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.XMLPostLinkToEmail, Method = "POST")]
        void SendLinkToEmail(String fileId, MessageParams messageAddresses);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetStoreOriginal, ResponseFormat = WebMessageFormat.Json)]
        bool StoreOriginal(bool store);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetUpdateIfExist, ResponseFormat = WebMessageFormat.Json)]
        bool UpdateIfExist(bool update);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetHelpCenter, ResponseFormat = WebMessageFormat.Json)]
        String GetHelpCenter();

        #endregion

        #region Ace Manager

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostSharedInfo, ResponseFormat = WebMessageFormat.Json)]
        ItemList<AceWrapper> GetSharedInfo(ItemList<String> objectId);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetSharedInfoShort, ResponseFormat = WebMessageFormat.Json)]
        ItemList<AceShortWrapper> GetSharedInfoShort(String objectId);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostSetAceObject, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<String> SetAceObject(AceCollection aceCollection, bool notify);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostRemoveAce, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        void RemoveAce(ItemList<String> items);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostMarkAsRead, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> MarkAsRead(ItemList<String> items);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetNewItems)]
        object GetNewItems(String folderId);

        #endregion

        #region ThirdParty

        ItemList<ThirdPartyParams> GetThirdParty();

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetThirdParty, ResponseFormat = WebMessageFormat.Json)]
        ItemList<Folder> GetThirdPartyFolder(int folderType);

        //[OperationContract]
        //[WebInvoke(UriTemplate = UriTemplates.JSONPostSaveThirdParty, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        Folder SaveThirdParty(ThirdPartyParams thirdPartyParams);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONDeleteThirdParty, ResponseFormat = WebMessageFormat.Json)]
        object DeleteThirdParty(String providerId);

        //[OperationContract]
        //[WebGet(UriTemplate = UriTemplates.JSONGetChangeAccessToThirdparty, ResponseFormat = WebMessageFormat.Json)]
        bool ChangeAccessToThirdparty(bool enableThirdpartySettings);

        #endregion
    }
}