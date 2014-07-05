/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;

namespace ASC.Web.Files.Services.WCFService
{
    internal class UriTemplates
    {
        #region Folder Template

        //  [Create("{folderid}")]
        public const String XMLCreateFolder = "folders/create?parentId={parentId}&title={title}";

        // [Read("{folderid}")]?&fields=folders
        public const String XMLGetSubFolders = "folders/subfolders?parentId={parentId}";

        public const String XMLGetPath = "folders/path?folderId={folderId}";

        //  [Update("{folderid}")]?&fields=folders
        public const String XMLRenameFolder = "folders/rename?folderId={folderId}&title={title}";

        // [Read("{folderid}")]
        public const String XMLPostFolderItems = "folders?parentId={parentId}&from={from}&count={count}&filter={filter}&subjectID={subjectID}&search={searchText}";

        #endregion

        #region File Template

        //  [Create("{folderid}/file")]
        public const String XMLCreateNewFile = "folders/files/createfile?parentId={parentId}&title={fileTitle}";

        //  [Update("file/{fileid}")]
        public const String XMLRenameFile = "folders/files/rename?fileId={fileId}&title={title}";

        // [Read("file/{fileid}/history")]
        public const String XMLGetFileHistory = "folders/files/history?fileId={fileId}";

        public const String JSONPostCheckMoveFiles = "folders/files/moveOrCopyFilesCheck?destFolderId={destFolderId}";

        // [Read("{folderid}")]?filterType=ImagesOnly&fields=files
        public const String JSONPostGetSiblingsFile = "folders/files/siblings?fileId={fileId}&filter={filter}&subjectID={subjectID}&search={searchText}";

        // [Update("file/{fileid}")]?lastVersion={version}
        public const String JSONUpdateToVersion = "folders/files/updateToVersion?fileId={fileId}&version={version}";

        public const String JSONUpdateComment = "folders/files/updateComment?fileId={fileId}&version={version}&comment={comment}";

        public const String JSONCompleteVersion = "folders/files/completeVersion?fileId={fileId}&version={version}&continueVersion={continueVersion}";

        // [Read("file/{fileid}/history")]?count=1
        public const String XMLLastFileVersion = "folders/files/getversion?fileId={fileId}&version={version}";

        public const String XMLGetLockFile = "folders/files/lock?fileId={fileId}&lockfile={lockFile}";

        #endregion

        #region Utils Template

        //[Read("settings/import/{source:(boxnet|google|zoho)}/data")]
        public const String XMLPostGetImportDocs = "import?source={source}";

        //  [Update("settings/import/{source:(boxnet|google|zoho)}/data")]
        public const String JSONPostExecImportDocs = "import/exec?login={login}&password={password}&token={token}&source={source}&tofolder={parentId}&ignoreCoincidenceFiles={ignoreCoincidenceFiles}";

        //  [Read("fileops")]
        public const String JSONGetTasksStatuses = "tasks/statuses";

        // [Update("fileops/terminate")] or [Update("settings/import/terminate")]
        public const String JSONTerminateTasks = "tasks?terminate={import}";

        // [Update("fileops/bulkdownload")]
        public const String JSONPostBulkDownload = "bulkdownload";

        // [Update("fileops/delete")]
        public const String JSONPostDeleteItems = "folders/files?action=delete";

        // [Update("fileops/move")] or [Update("fileops/copy")]
        public const String JSONPostMoveItems = "moveorcopy?destFolderId={destFolderId}&ow={overwriteFiles}&ic={isCopyOperation}";

        // [Update("fileops/emptytrash")]
        public const String JSONEmptyTrash = "emptytrash";

        public const String JSONGetShortenLink = "shorten?fileId={fileId}";

        public const String JSONGetTrackEditFile = "trackeditfile?fileID={fileId}&tabId={tabId}&docKeyForTrack={docKeyForTrack}&doc={shareLinkKey}&isFinish={isFinish}&fixedVersion={fixedVersion}";

        public const String JSONPostCheckConversion = "checkconversion";

        public const String JSONPostCheckEditing = "checkediting";

        public const String JSONGetCanEdit = "canedit?fileId={fileId}&doc={shareLinkKey}";

        public const String JSONGetSaveEditing = "saveediting?fileId={fileId}&version={version}&tabId={tabId}&fileuri={downloadUri}&asNew={asNew}&doc={shareLinkKey}";

        public const String JSONGetStartEdit = "startedit?fileId={fileId}&docKeyForTrack={docKeyForTrack}&asNew={asNew}&doc={shareLinkKey}";

        public const String XMLPostLinkToEmail = "sendlinktoemail?fileId={fileId}";

        public const String JSONGetStoreOriginal = "storeoriginal?set={store}";

        public const String JSONGetUpdateIfExist = "updateifexist?set={update}";

        public const String JSONGetHelpCenter = "gethelpcenter";

        #endregion

        #region Ace Tempate

        //  [Read("file/{fileid}/share")]   [Read("folder/{folderid}/share")]
        public const String JSONPostSharedInfo = "sharedinfo";

        public const String JSONGetSharedInfoShort = "sharedinfoshort?objectId={objectId}";

        // [Update("file/{fileid}/share")]  [Update("folder/{folderid}/share")]
        public const String JSONPostSetAceObject = "setaceobject?notify={notify}";

        //  [Delete("folder/{folderid}/share")]  [Delete("file/{fileid}/share")] 
        public const String JSONPostRemoveAce = "removeace";

        // [Update("fileops/markasread")]
        public const String JSONPostMarkAsRead = "markasread";

        public const String JSONGetNewItems = "getnews?folderId={folderId}";

        #endregion

        #region ThirdParty

        // [Read("settings/thirdparty")]
        public const String JSONGetThirdParty = "thirdparty";

        // [Create("settings/thirdparty")]
        public const String JSONPostSaveThirdParty = "thirdparty/save";

        // [Delete("settings/thirdparty/{folderid}")]
        public const String JSONDeleteThirdParty = "thirdparty/delete?providerId={providerId}";

        public const String JSONGetChangeAccessToThirdparty = "thirdparty?enable={enableThirdpartySettings}";

        #endregion
    }
}