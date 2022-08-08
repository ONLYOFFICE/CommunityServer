/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core.Compress;
using ASC.Web.Files.Core.Entries;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.HttpHandlers;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json.Linq;

using FileShare = ASC.Files.Core.Security.FileShare;
using FilesNS = ASC.Web.Files.Services.WCFService;
using MimeMapping = ASC.Common.Web.MimeMapping;
using SortedByType = ASC.Files.Core.SortedByType;

namespace ASC.Api.Documents
{
    /// <summary>
    /// Provides access to documents.
    /// </summary>
    public class DocumentsApi : Interfaces.IApiEntryPoint
    {
        private readonly ApiContext _context;
        private readonly IFileStorageService _fileStorageService;

        /// <summary>
        /// </summary>
        public string Name
        {
            get { return "files"; }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileStorageService"></param>
        public DocumentsApi(ApiContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }


        /// <summary>
        /// Returns all the sections matching the parameters specified in the request.
        /// </summary>
        /// <short>Get sections</short>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <param name="withoutTrash">Root folders with or without trash</param>
        /// <param name="withoutAdditionalFolder">Root folders with or without additional folders</param>
        /// <category>Folders</category>
        /// <returns>Sections</returns>
        [Read("@root")]
        public IEnumerable<FolderContentWrapper> GetRootFolders(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders, bool withoutTrash, bool withoutAdditionalFolder)
        {
            var IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();
            var IsOutsider = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOutsider();
            var result = new SortedSet<object>();

            if (IsOutsider)
            {
                withoutTrash = true;
                withoutAdditionalFolder = true;
            }

            if (!IsVisitor)
            {
                result.Add(Global.FolderMy);
            }

            if (!CoreContext.Configuration.Personal && !IsOutsider)
            {
                result.Add(Global.FolderShare);
            }

            if (!IsVisitor && !withoutAdditionalFolder)
            {
                if (FilesSettings.FavoritesSection)
                {
                    result.Add(Global.FolderFavorites);
                }

                if (FilesSettings.RecentSection)
                {
                    result.Add(Global.FolderRecent);
                }

                if (PrivacyRoomSettings.Available)
                {
                    result.Add(Global.FolderPrivacy);
                }
            }

            if (!CoreContext.Configuration.Personal)
            {
                result.Add(Global.FolderCommon);
            }

            if (Global.FolderProjects != null)
            {
                result.Add(Global.FolderProjects);
            }

            if (!IsVisitor
               && !withoutAdditionalFolder
               && FileUtility.ExtsWebTemplate.Any()
               && FilesSettings.TemplatesSection)
            {
                result.Add(Global.FolderTemplates);
            }

            if (!withoutTrash)
            {
                result.Add((int)Global.FolderTrash);
            }

            return result.Select(r => ToFolderContentWrapper(r, userIdOrGroupId, filterType, searchInContent, withSubfolders));
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "My documents" section.
        /// </summary>
        /// <short>Get my section</short>
        /// <category>Folders</category>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>My documents section contents</returns>
        [Read("@my")]
        public FolderContentWrapper GetMyFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderMy, userIdOrGroupId, filterType, searchInContent, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "Projects" section.
        /// </summary>
        /// <short>Get project section</short>
        /// <category>Folders</category>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>Project section contents</returns>
        [Read("@projects")]
        public FolderContentWrapper GetProjectsFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderProjects, userIdOrGroupId, filterType, searchInContent, withSubfolders);
        }


        /// <summary>
        /// Returns the detailed list of files and folders located in the "Common" section.
        /// </summary>
        /// <short>Get common section</short>
        /// <category>Folders</category>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>Common section contents</returns>
        [Read("@common")]
        public FolderContentWrapper GetCommonFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderCommon, userIdOrGroupId, filterType, searchInContent, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "Shared with Me" section.
        /// </summary>
        /// <short>Get shared section</short>
        /// <category>Folders</category>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>Shared section contents</returns>
        [Read("@share")]
        public FolderContentWrapper GetShareFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderShare, userIdOrGroupId, filterType, searchInContent, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files located in the "Recent" section.
        /// </summary>
        /// <short>Get recent section</short>
        /// <category>Folders</category>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>Recent section contents</returns>
        [Read("@recent")]
        public FolderContentWrapper GetRecentFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderRecent, userIdOrGroupId, filterType, searchInContent, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files located in the "Favorites" section.
        /// </summary>
        /// <short>Get favorite section</short>
        /// <category>Folders</category>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>Favorite section contents</returns>
        [Read("@favorites")]
        public FolderContentWrapper GetFavoritesFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderFavorites, userIdOrGroupId, filterType, searchInContent, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files located in the "Templates" section.
        /// </summary>
        /// <short>Get template section</short>
        /// <category>Folders</category>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>Template section contents</returns>
        [Read("@templates")]
        public FolderContentWrapper GetTemplatesFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderTemplates, userIdOrGroupId, filterType, searchInContent, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "Trash" section.
        /// </summary>
        /// <short>Get trash section</short>
        /// <category>Folders</category>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>Trash section contents</returns>
        [Read("@trash")]
        public FolderContentWrapper GetTrashFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderTrash, userIdOrGroupId, filterType, searchInContent, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the folder with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a folder by ID
        /// </short>
        /// <category>Folders</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true">Filter type</param>
        /// <param name="searchInContent">Search in content files</param>
        /// <param name="withSubfolders">Root folders with or without subfolders</param>
        /// <returns>Folder contents</returns>
        [Read("{folderId}")]
        public FolderContentWrapper GetFolder(String folderId, Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            return ToFolderContentWrapper(folderId, userIdOrGroupId, filterType, searchInContent, withSubfolders).NotFoundIfNull();

        }

        /// <summary>
        /// Uploads a file specified in the request to the "My documents" section by single file uploading or standart multipart/form-data method.
        /// </summary>
        /// <short>Upload a file to my section</short>
        /// <category>Uploads</category>
        /// <param name="file" visible="false">Request input stream</param>
        /// <param name="contentType" visible="false">Content-Type header</param>
        /// <param name="contentDisposition" visible="false">Content-Disposition header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <remarks>
        /// <![CDATA[
        ///  Upload can be done in 2 different ways:
        ///  <ol>
        /// <li>Using single file upload. You should set the Content-Type &amp; Content-Disposition headers to specify the file name and content type, and send the file to the request body.</li>
        /// <li>Using standart multipart/form-data method.</li>
        /// </ol>]]>
        /// </remarks>
        /// <returns>Uploaded file</returns>
        [Create("@my/upload")]
        public object UploadFileToMy(Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<HttpPostedFileBase> files)
        {
            return UploadFile(Global.FolderMy.ToString(), file, contentType, contentDisposition, files, false, null);
        }

        /// <summary>
        /// Uploads a file specified in the request to the "Common" section by single file uploading or standart multipart/form-data method.
        /// </summary>
        /// <short>Upload a file to the common section</short>
        /// <category>Uploads</category>
        /// <param name="file" visible="false">Request input stream</param>
        /// <param name="contentType" visible="false">Content-Type header</param>
        /// <param name="contentDisposition" visible="false">Content-Disposition header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <remarks>
        /// <![CDATA[
        ///  Upload can be done in 2 different ways:
        ///  <ol>
        /// <li>Using single file upload. You should set the Content-Type &amp; Content-Disposition headers to specify the file name and content type, and send the file to the request body.</li>
        /// <li>Using standart multipart/form-data method.</li>
        /// </ol>]]>
        /// </remarks>
        /// <returns>Uploaded file</returns>
        [Create("@common/upload")]
        public object UploadFileToCommon(Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<HttpPostedFileBase> files)
        {
            return UploadFile(Global.FolderCommon.ToString(), file, contentType, contentDisposition, files, false, null);
        }


        /// <summary>
        /// Uploads a file specified in the request to the selected folder by single file uploading or standart multipart/form-data method.
        /// </summary>
        /// <short>Upload a file</short>
        /// <category>Uploads</category>
        /// <remarks>
        /// <![CDATA[
        ///  Upload can be done in 2 different ways:
        ///  <ol>
        /// <li>Using single file upload. You should set the Content-Type &amp; Content-Disposition headers to specify the file name and content type, and send the file to the request body.</li>
        /// <li>Using standart multipart/form-data method.</li>
        /// </ol>]]>
        /// </remarks>
        /// <param name="folderId">Folder ID</param>
        /// <param name="file" visible="false">Request input stream</param>
        /// <param name="contentType" visible="false">Content-Type header</param>
        /// <param name="contentDisposition" visible="false">Content-Disposition header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <param name="createNewIfExist" visible="false">Creates a new file if it already exists or not</param>
        /// <param name="storeOriginalFileFlag" visible="false">If true, uploads documents in the original formats as well</param>
        /// <param name="keepConvertStatus" visible="false">Keeps converting status after finishing or not</param>
        /// <returns>Uploaded file</returns>
        [Create("{folderId}/upload")]
        public object UploadFile(string folderId, Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<HttpPostedFileBase> files, bool? createNewIfExist, bool? storeOriginalFileFlag, bool keepConvertStatus = false)
        {
            if (storeOriginalFileFlag.HasValue)
            {
                FilesSettings.StoreOriginalFiles = storeOriginalFileFlag.Value;
            }

            if (files != null && files.Any())
            {
                if (files.Count() == 1)
                {
                    //Only one file. return it
                    var postedFile = files.First();
                    return InsertFile(folderId, postedFile.InputStream, postedFile.FileName, createNewIfExist, keepConvertStatus);
                }
                //For case with multiple files
                return files.Select(postedFile => InsertFile(folderId, postedFile.InputStream, postedFile.FileName, createNewIfExist, keepConvertStatus)).ToList();
            }
            if (file != null)
            {
                var fileName = "file" + MimeMapping.GetExtention(contentType.MediaType);
                if (contentDisposition != null)
                {
                    fileName = contentDisposition.FileName;
                }

                return InsertFile(folderId, file, fileName, createNewIfExist, keepConvertStatus);
            }
            throw new InvalidOperationException("No input files");
        }

        /// <summary>
        /// Inserts a file specified in the request to the "My documents" section by single file uploading.
        /// </summary>
        /// <short>Insert a file to my section</short>
        /// <param name="file" visible="false">Request input stream</param>
        /// <param name="title">File name</param>
        /// <param name="createNewIfExist" visible="false">Creates a new file if it already exists or not</param>
        /// <param name="keepConvertStatus" visible="false">Keeps converting status after finishing or not</param>
        /// <category>Uploads</category>
        /// <returns>Inserted file</returns>
        [Create("@my/insert")]
        public FileWrapper InsertFileToMy(Stream file, string title, bool? createNewIfExist, bool keepConvertStatus = false)
        {
            return InsertFile(Global.FolderMy.ToString(), file, title, createNewIfExist, keepConvertStatus);
        }

        /// <summary>
        /// Inserts a file specified in the request to the "Common" section by single file uploading.
        /// </summary>
        /// <short>Insert a file to the common section</short>
        /// <param name="file" visible="false">Request input stream</param>
        /// <param name="title">File name</param>
        /// <param name="createNewIfExist" visible="false">Creates a new file if it already exists or not</param>
        /// <param name="keepConvertStatus" visible="false">Keeps converting status after finishing or not</param>
        /// <category>Uploads</category>
        /// <returns>Inserted file</returns>
        [Create("@common/insert")]
        public FileWrapper InsertFileToCommon(Stream file, string title, bool? createNewIfExist, bool keepConvertStatus = false)
        {
            return InsertFile(Global.FolderCommon.ToString(), file, title, createNewIfExist, keepConvertStatus);
        }

        /// <summary>
        /// Inserts a file specified in the request to the selected folder by single file uploading.
        /// </summary>
        /// <short>Insert a file</short>
        /// <param name="folderId">Folder ID</param>
        /// <param name="file" visible="false">Request input stream</param>
        /// <param name="title">File name</param>
        /// <param name="createNewIfExist" visible="false">Creates a new file if it already exists or not</param>
        /// <param name="keepConvertStatus" visible="false">Keeps converting status after finishing or not</param>
        /// <category>Uploads</category>
        /// <returns>Inserted file</returns>
        [Create("{folderId}/insert")]
        public FileWrapper InsertFile(string folderId, Stream file, string title, bool? createNewIfExist, bool keepConvertStatus = false)
        {
            try
            {
                var resultFile = FileUploader.Exec(folderId, title, file.Length, file, createNewIfExist.HasValue ? createNewIfExist.Value : !FilesSettings.UpdateIfExist, !keepConvertStatus);
                return new FileWrapper(resultFile);
            }
            catch (FileNotFoundException e)
            {
                throw new ItemNotFoundException("File not found", e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new ItemNotFoundException("Folder not found", e);
            }
        }

        /// <summary>
        /// Updates the content of a file with the ID specified in the request.
        /// </summary>
        /// <short>Update file content</short>
        /// <category>Files</category>
        /// <param name="file">File stream</param>
        /// <param name="fileId">File ID</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="encrypted" visible="false">Encrypts a file or not</param>
        /// <param name="forcesave" visible="false">Forcibly saves a file or not</param>
        /// <returns>Updated file</returns>
        [Update("{fileId}/update")]
        public FileWrapper UpdateFileStream(Stream file, string fileId, string fileExtension, bool encrypted = false, bool forcesave = false)
        {
            try
            {
                var resultFile = _fileStorageService.UpdateFileStream(fileId, file, fileExtension, encrypted, forcesave);
                return new FileWrapper(resultFile);
            }
            catch (FileNotFoundException e)
            {
                throw new ItemNotFoundException("File not found", e);
            }
        }


        /// <summary>
        /// Saves editing of a file with the ID specified in the request.
        /// </summary>
        /// <short>Save editing</short>
        /// <param name="fileId">File ID</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="downloadUri">URI for downloading</param>
        /// <param name="stream">File stream</param>
        /// <param name="doc">Shared token</param>
        /// <param name="forcesave">Forcibly saves a file or not</param>
        /// <category>Files</category>
        /// <returns>Saved file</returns>
        [Update("file/{fileId}/saveediting")]
        public FileWrapper SaveEditing(String fileId, string fileExtension, string downloadUri, Stream stream, String doc, bool forcesave)
        {
            return new FileWrapper(_fileStorageService.SaveEditing(fileId, fileExtension, downloadUri, stream, doc, forcesave));
        }

        /// <summary>
        /// Informs about opening a file with the ID specified in the request for editing locking it from deletion or movement (this method is called by the mobile editors).
        /// </summary>
        /// <short>Start editing</short>
        /// <param name="fileId">File ID</param>
        /// <param name="editingAlone" visible="false">Shares a file with other users for editing or not</param>
        /// <param name="doc" visible="false">Shared token</param>
        /// <category>Files</category>
        /// <returns>File key for Document Service</returns>
        [Create("file/{fileId}/startedit")]
        public string StartEdit(String fileId, bool editingAlone, String doc)
        {
            return _fileStorageService.StartEdit(fileId, editingAlone, doc);
        }

        /// <summary>
        /// Tracks file changes when editing.
        /// </summary>
        /// <short>Track editing</short>
        /// <param name="fileId">File ID</param>
        /// <param name="tabId" visible="false">Tab ID</param>
        /// <param name="docKeyForTrack" visible="false">Document key for tracking</param>
        /// <param name="doc" visible="false">Shared token</param>
        /// <param name="isFinish">Finishes file editing or not</param>
        /// <category>Files</category>
        /// <returns>File changes</returns>
        [Read("file/{fileId}/trackeditfile")]
        public KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String doc, bool isFinish)
        {
            return _fileStorageService.TrackEditFile(fileId, tabId, docKeyForTrack, doc, isFinish);
        }

        /// <summary>
        /// Returns the initialization configuration to open the editor.
        /// </summary>
        /// <short>Open the editor</short>
        /// <param name="fileId">File ID</param>
        /// <param name="version">File version</param>
        /// <param name="doc" visible="false">Shared token</param>
        /// <category>Files</category>
        /// <returns>Configuration</returns>
        [Read("file/{fileId}/openedit", false)] // NOTE: This method doesn't require auth!!!
        public Configuration OpenEdit(String fileId, int version, String doc)
        {
            Configuration configuration;
            var file = DocumentServiceHelper.GetParams(fileId, version, doc, true, true, true, out configuration);
            if (configuration.EditorConfig.ModeWrite && FileConverter.MustConvert(file))
            {
                file = DocumentServiceHelper.GetParams(file.ID, file.Version, doc, false, false, false, out configuration);
            }

            configuration.Type = Configuration.EditorType.External;

            if (file.RootFolderType == FolderType.Privacy
                && PrivacyRoomSettings.Enabled)
            {
                var keyPair = EncryptionKeyPair.GetKeyPair();
                if (keyPair != null)
                {
                    configuration.EditorConfig.EncryptionKeys = new Configuration.EditorConfiguration.EncryptionKeysConfig
                    {
                        PrivateKeyEnc = keyPair.PrivateKeyEnc,
                        PublicKey = keyPair.PublicKey,
                    };
                }
            }

            if (!file.Encrypted && !file.ProviderEntry) EntryManager.MarkAsRecent(file);

            configuration.Token = DocumentServiceHelper.GetSignature(configuration);
            return configuration;
        }


        /// <summary>
        /// Creates a session to upload large files in multiple chunks to the folder with the ID specified in the request.
        /// </summary>
        /// <short>Chunked upload</short>
        /// <category>Uploads</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="fileName">File name</param>
        /// <param name="fileSize">File length in bytes</param>
        /// <param name="relativePath">Folder which is relative to the selected folder</param>
        /// <param name="encrypted" visible="false">Encrypts a file or not</param>
        /// <remarks>
        /// <![CDATA[
        /// Each chunk can have different length but the length should be multiple of <b>512</b> and greater or equal to <b>10 mb</b>. Last chunk can have any size.
        /// After the initial response to the request with the <b>200 OK</b> status, you must get the <em>location</em> field value from the response. Send all your chunks to this location.
        /// Each chunk must be sent in the exact order the chunks appear in the file.
        /// After receiving each chunk, the server will respond with the current information about the upload session if no errors occurred.
        /// When the number of bytes uploaded is equal to the number of bytes you sent in the initial request, the server responds with the <b>201 Created</b> status and sends you information about the uploaded file.
        /// ]]>
        /// </remarks>
        /// <returns>
        /// <![CDATA[
        /// Information about created session which includes:
        /// <ul>
        /// <li><b>id:</b> unique ID of this upload session</li>
        /// <li><b>created:</b> UTC time when the session was created</li>
        /// <li><b>expired:</b> UTC time when the session will expire if no chunks are sent before that time</li>
        /// <li><b>location:</b> URL where you should send your next chunk</li>
        /// <li><b>bytes_uploaded:</b> number of bytes uploaded for the specific upload ID</li>
        /// <li><b>bytes_total:</b> total number of bytes which will be uploaded</li>
        /// </ul>
        /// ]]>
        /// </returns>
        [Create("{folderId}/upload/create_session")]
        public object CreateUploadSession(string folderId, string fileName, long fileSize, string relativePath, bool encrypted)
        {
            var file = FileUploader.VerifyChunkedUpload(folderId, fileName, fileSize, FilesSettings.UpdateIfExist, relativePath);

            return CreateUploadSession(file, encrypted);
        }

        /// <summary>
        /// Creates a session to edit the existing file with multiple chunks (needed for WebDAV).
        /// </summary>
        /// <short>Create the editing session</short>
        /// <category>Files</category>
        /// <param name="fileId">File ID</param>
        /// <param name="fileSize">File size in bytes</param>
        /// <returns>Upload session</returns>
        /// <visible>false</visible>
        [Create("file/{fileId}/edit_session")]
        public object CreateEditSession(object fileId, long fileSize)
        {
            var file = FileUploader.VerifyChunkedUploadForEditing(fileId, fileSize);

            return CreateUploadSession(file, false, true);
        }

        private object CreateUploadSession(Files.Core.File file, bool encrypted, bool keepVersion = false)
        {
            if (FilesLinkUtility.IsLocalFileUploader)
            {
                var session = FileUploader.InitiateUpload(file.FolderID.ToString(), (file.ID ?? "").ToString(), file.Title, file.ContentLength, encrypted, keepVersion);

                var response = ChunkedUploaderHandler.ToResponseObject(session, true);
                return new
                {
                    success = true,
                    data = response
                };
            }

            var createSessionUrl = FilesLinkUtility.GetInitiateUploadSessionUrl(file.FolderID, file.ID, file.Title, file.ContentLength, encrypted);
            var request = (HttpWebRequest)WebRequest.Create(createSessionUrl);
            request.Method = "POST";
            request.ContentLength = 0;

            // hack for uploader.onlyoffice.com in api requests
            var rewriterHeader = _context.RequestContext.HttpContext.Request.Headers[HttpRequestExtensions.UrlRewriterHeader];
            if (!string.IsNullOrEmpty(rewriterHeader))
            {
                request.Headers[HttpRequestExtensions.UrlRewriterHeader] = rewriterHeader;
            }

            // hack. http://ubuntuforums.org/showthread.php?t=1841740
            if (WorkContext.IsMono)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            }

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                return JObject.Parse(new StreamReader(responseStream).ReadToEnd()); //result is json string
            }
        }

        /// <summary>
        /// Creates a text (.txt) file in the "My documents" section with the title and contents specified in the request.
        /// </summary>
        /// <short>Create a txt file in my section</short>
        /// <category>Files</category>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        /// <visible>false</visible>
        [Create("@my/text")]
        public FileWrapper CreateTextFileInMy(string title, string content)
        {
            return CreateTextFile(Global.FolderMy.ToString(), title, content);
        }

        /// <summary>
        /// Creates a text (.txt) file in the "Common" section with the title and contents specified in the request.
        /// </summary>
        /// <short>Create a txt file in the common section</short>
        /// <category>Files</category>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        /// <visible>false</visible>
        [Create("@common/text")]
        public FileWrapper CreateTextFileInCommon(string title, string content)
        {
            return CreateTextFile(Global.FolderCommon.ToString(), title, content);
        }

        /// <summary>
        /// Creates a text (.txt) file in the selected folder with the title and contents specified in the request.
        /// </summary>
        /// <short>Create a txt file</short>
        /// <category>Files</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        /// <visible>false</visible>
        [Create("{folderId}/text")]
        public FileWrapper CreateTextFile(string folderId, string title, string content)
        {
            if (title == null) throw new ArgumentNullException("title");
            //Try detect content
            var extension = ".txt";
            if (!string.IsNullOrEmpty(content))
            {
                if (Regex.IsMatch(content, @"<([^\s>]*)(\s[^<]*)>"))
                {
                    extension = ".html";
                }
            }
            return CreateFile(folderId, title, content, extension);
        }

        private static FileWrapper CreateFile(string folderId, string title, string content, string extension)
        {
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var file = FileUploader.Exec(folderId,
                                  title.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? title : (title + extension),
                                  memStream.Length, memStream);
                return new FileWrapper(file);
            }
        }

        /// <summary>
        /// Creates an html (.html) file in the selected folder with the title and contents specified in the request.
        /// </summary>
        /// <short>Create an html file</short>
        /// <category>Files</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        /// <visible>false</visible>
        [Create("{folderId}/html")]
        public FileWrapper CreateHtmlFile(string folderId, string title, string content)
        {
            if (title == null) throw new ArgumentNullException("title");
            return CreateFile(folderId, title, content, ".html");
        }

        /// <summary>
        /// Creates an html (.html) file in the "My documents" section with the title and contents specified in the request.
        /// </summary>
        /// <short>Create an html file in my section</short>
        /// <category>Files</category>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        /// <visible>false</visible>
        [Create("@my/html")]
        public FileWrapper CreateHtmlFileInMy(string title, string content)
        {
            return CreateHtmlFile(Global.FolderMy.ToString(), title, content);
        }


        /// <summary>
        /// Creates an html (.html) file in the common section with the title and contents specified in the request.
        /// </summary>
        /// <short>Create an html file in the common section</short>
        /// <category>Files</category>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        /// <visible>false</visible>
        [Create("@common/html")]
        public FileWrapper CreateHtmlFileInCommon(string title, string content)
        {
            return CreateHtmlFile(Global.FolderCommon.ToString(), title, content);
        }


        /// <summary>
        /// Creates a new folder with the title specified in the request. The parent folder ID can be also specified.
        /// </summary>
        /// <short>
        /// Create a folder
        /// </short>
        /// <category>Folders</category>
        /// <param name="folderId">Parent folder ID</param>
        /// <param name="title">Folder title</param>
        /// <returns>New folder contents</returns>
        [Create("folder/{folderId}")]
        public FolderWrapper CreateFolder(string folderId, string title)
        {
            var folder = _fileStorageService.CreateNewFolder(folderId, title);
            return new FolderWrapper(folder);
        }

        /// <summary>
        /// Creates a new file in the "My documents" section with the title specified in the request.
        /// </summary>
        /// <short>Create a file in my section</short>
        /// <category>Files</category>
        /// <param name="title" remark="Allowed values: the file must have one of the following extensions: DOCX, XLSX, PPTX">File title</param>
        /// <remarks>In case an extension for the file title differs from DOCX/XLSX/PPTX and belongs to one of the known text, spreadsheet or presentation formats, it will be changed to DOCX/XLSX/PPTX accordingly. If the file extension is not set or is unknown, the DOCX extension will be added to the file title.</remarks>
        /// <returns>New file information</returns>
        [Create("@my/file")]
        public FileWrapper CreateFile(string title)
        {
            return CreateFile(Global.FolderMy.ToString(), title, null, false);
        }

        /// <summary>
        /// Creates a new file in the specified folder with the title specified in the request.
        /// </summary>
        /// <short>Create a file</short>
        /// <category>Files</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="title" remark="Allowed values: the file must have one of the following extensions: DOCX, XLSX, PPTX">File title</param>
        /// <param name="templateId">Template file ID</param>
        /// <param name="enableExternalExt">The ability to create files of any extension</param>
        /// <remarks>In case an extension for the file title differs from DOCX/XLSX/PPTX and belongs to one of the known text, spreadsheet or presentation formats, it will be changed to DOCX/XLSX/PPTX accordingly. If the file extension is not set or is unknown, the DOCX extension will be added to the file title.</remarks>
        /// <returns>New file information</returns>
        [Create("{folderId}/file")]
        public FileWrapper CreateFile(string folderId, string title, string templateId, bool enableExternalExt)
        {
            var file = _fileStorageService.CreateNewFile(folderId, title, templateId, enableExternalExt);
            return new FileWrapper(file);
        }

        /// <summary>
        /// Renames the selected folder with a new title specified in the request.
        /// </summary>
        /// <short>
        /// Rename a folder
        /// </short>
        /// <category>Folders</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="title">New title</param>
        /// <returns>Folder contents</returns>
        [Update("folder/{folderId}")]
        public FolderWrapper RenameFolder(string folderId, string title)
        {
            var folder = _fileStorageService.FolderRename(folderId, title);
            return new FolderWrapper(folder);
        }

        /// <summary>
        /// Returns the detailed information about a folder with the ID specified in the request.
        /// </summary>
        /// <short>Get the folder information</short>
        /// <param name="folderId">Folder ID</param>
        /// <category>Folders</category>
        /// <returns>Folder information</returns>
        [Read("folder/{folderId}")]
        public FolderWrapper GetFolderInfo(string folderId)
        {
            var folder = _fileStorageService.GetFolder(folderId).NotFoundIfNull("Folder not found");

            return new FolderWrapper(folder);
        }

        /// <summary>
        /// Returns a path to the folder with the ID specified in the request.
        /// </summary>
        /// <short>Get the folder path</short>
        /// <param name="folderId">Folder ID</param>
        /// <category>Folders</category>
        /// <returns>Folder path</returns>
        [Read("folder/{folderId}/path")]
        public IEnumerable<FolderWrapper> GetFolderPath(string folderId)
        {
            return EntryManager.GetBreadCrumbs(folderId).Select(f => new FolderWrapper(f)).ToSmartList();
        }

        /// <summary>
        /// Returns the detailed information about a file with the ID specified in the request.
        /// </summary>
        /// <short>Get the file information</short>
        /// <param name="fileId">File ID</param>
        /// <param name="version">File version</param>
        /// <category>Files</category>
        /// <returns>File information</returns>
        [Read("file/{fileId}")]
        public FileWrapper GetFileInfo(string fileId, int version = -1)
        {
            var file = _fileStorageService.GetFile(fileId, version).NotFoundIfNull("File not found");
            return new FileWrapper(file);
        }

        /// <summary>
        /// Copies (and converts, if possible) an existing file to a new file.
        /// </summary>
        /// <short>Copy file</short>
        /// <category>Files</category>
        /// <param name="fileId">File ID</param>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="destTitle">Destination file Title</param>
        /// <returns></returns>
        [Create("file/{fileId}/copyas")]
        public FileWrapper CopyFileAs(string fileId, string destFolderId, string destTitle)
        {
            var file = _fileStorageService.GetFile(fileId, -1);
            var ext = FileUtility.GetFileExtension(file.Title);
            var destExt = FileUtility.GetFileExtension(destTitle);

            if (ext == destExt)
            {
                return CreateFile(destFolderId, destTitle, fileId, true);
            }

            using (var fileStream = FileConverter.Exec(file, destExt))
            {
                return InsertFile(destFolderId, fileStream, destTitle, true);
            }
        }

        /// <summary>
        /// Updates the information of the selected file with the parameters specified in the request.
        /// </summary>
        /// <short>Update a file</short>
        /// <category>Files</category>
        /// <param name="fileId">File ID</param>
        /// <param name="title">New file title</param>
        /// <param name="lastVersion">File last version number</param>
        /// <returns>File information</returns>
        [Update("file/{fileId}")]
        public FileWrapper UpdateFile(String fileId, String title, int lastVersion)
        {
            if (!String.IsNullOrEmpty(title))
                _fileStorageService.FileRename(fileId.ToString(CultureInfo.InvariantCulture), title);

            if (lastVersion > 0)
                _fileStorageService.UpdateToVersion(fileId.ToString(CultureInfo.InvariantCulture), lastVersion);

            return GetFileInfo(fileId);
        }

        /// <summary>
        /// Deletes a file with the ID specified in the request.
        /// </summary>
        /// <short>Delete a file</short>
        /// <category>Operations</category>
        /// <param name="fileId">File ID</param>
        /// <param name="deleteAfter">Deletes after finished</param>
        /// <param name="immediately">Doesn't move a file to the recycle bin</param>
        /// <returns>Operation result</returns>
        [Delete("file/{fileId}")]
        public IEnumerable<FileOperationWraper> DeleteFile(String fileId, bool deleteAfter, bool immediately)
        {
            return DeleteBatchItems(null, new[] { fileId }, deleteAfter, immediately);
        }

        /// <summary>
        /// Starts a conversion operation of a file with the ID specified in the request.
        /// </summary>
        /// <short>Start file converting</short>
        /// <category>Operations</category>
        /// <param name="fileId">File ID</param>
        /// <returns>Operation result</returns>
        [Update("file/{fileId}/checkconversion")]
        public IEnumerable<ConversationResult> StartConversion(String fileId)
        {
            return CheckConversion(fileId, true);
        }

        /// <summary>
        /// Checks a status of converting a file with the ID specified in the request.
        /// </summary>
        /// <short>Get converting status</short>
        /// <category>Operations</category>
        /// <param name="fileId">File ID</param>
        /// <param name="start">Starts a conversion operation or not</param>
        /// <returns>Operation result</returns>
        [Read("file/{fileId}/checkconversion")]
        public IEnumerable<ConversationResult> CheckConversion(String fileId, bool start)
        {
            return _fileStorageService.CheckConversion(new FilesNS.ItemList<FilesNS.ItemList<string>>
            {
                new FilesNS.ItemList<string> { fileId, "0", start.ToString() }
            })
            .Select(r =>
            {
                var o = new ConversationResult
                {
                    Id = r.Id,
                    Error = r.Error,
                    OperationType = r.OperationType,
                    Processed = r.Processed,
                    Progress = r.Progress,
                    Source = r.Source,
                };
                if (!string.IsNullOrEmpty(r.Result))
                {
                    var jResult = JObject.Parse(r.Result);
                    o.File = GetFileInfo(jResult.Value<string>("id"), jResult.Value<int>("version"));
                }
                return o;
            });
        }

        /// <summary>
        /// Returns a link to download a file with the ID specified in the request.
        /// </summary>
        /// <short>Get file download link</short>
        /// <category>Files</category>
        /// <param name="fileId">File ID</param>
        /// <returns>File download link</returns>
        [Read("file/{fileId}/presigneduri")]
        public string GetPresignedUri(String fileId)
        {
            var file = _fileStorageService.GetFile(fileId, -1);
            return PathProvider.GetFileStreamUrl(file);
        }

        /// <summary>
        /// Deletes a folder with the ID specified in the request.
        /// </summary>
        /// <short>Delete a folder</short>
        /// <category>Operations</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="deleteAfter">Deletes after finished</param>
        /// <param name="immediately">Doesn't move a folder to the recycle bin</param>
        /// <returns>Operation result</returns>
        [Delete("folder/{folderId}")]
        public IEnumerable<FileOperationWraper> DeleteFolder(String folderId, bool deleteAfter, bool immediately)
        {
            return DeleteBatchItems(new[] { folderId }, null, deleteAfter, immediately);
        }

        /// <summary>
        /// Checks a batch of files and folders for conflicts when moving or copying them to the folder with the ID specified in the request.
        /// </summary>
        /// <short>Check files and folders for conflicts</short>
        /// <category>Operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">List of folder IDs</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <returns>Conflict file IDs</returns>
        [Read("fileops/move")]
        public IEnumerable<FileWrapper> MoveOrCopyBatchCheck(String destFolderId, IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            var ids = _fileStorageService.MoveOrCopyFilesCheck(itemList, destFolderId).Keys.Select(id => "file_" + id);

            var entries = _fileStorageService.GetItems(new Web.Files.Services.WCFService.ItemList<string>(ids), FilterType.FilesOnly, false, "", "");
            return entries.Select(x => new FileWrapper((Files.Core.File)x)).ToSmartList();
        }

        /// <summary>
        /// Moves all the selected files and folders to the folder with the ID specified in the request.
        /// </summary>
        /// <short>Move to a folder</short>
        /// <category>Operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">List of folder IDs</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <param name="conflictResolveType">Overwriting behavior: skip (0), overwrite (1) or duplicate (2)</param>
        /// <param name="deleteAfter">Deletes after finished</param>
        /// <returns>Operation result</returns>
        [Update("fileops/move")]
        public IEnumerable<FileOperationWraper> MoveBatchItems(String destFolderId, IEnumerable<String> folderIds, IEnumerable<String> fileIds, FileConflictResolveType conflictResolveType, bool deleteAfter)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            return _fileStorageService.MoveOrCopyItems(itemList, destFolderId, conflictResolveType, false, deleteAfter).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Copies all the selected files and folders to the folder with the ID specified in the request.
        /// </summary>
        /// <short>Copy to a folder</short>
        /// <category>Operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">List of folder IDs</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <param name="conflictResolveType">Overwriting behavior: skip (0), overwrite (1) or duplicate (2)</param>
        /// <param name="deleteAfter">Deletes after finished</param>
        /// <returns>Operation result</returns>
        [Update("fileops/copy")]
        public IEnumerable<FileOperationWraper> CopyBatchItems(String destFolderId, IEnumerable<String> folderIds, IEnumerable<String> fileIds, FileConflictResolveType conflictResolveType, bool deleteAfter)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            return _fileStorageService.MoveOrCopyItems(itemList, destFolderId, conflictResolveType, true, deleteAfter).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Marks the files and folders with the IDs specified in the request as read.
        /// </summary>
        /// <short>Mark as read</short>
        /// <category>Operations</category>
        /// <param name="folderIds">List of folder IDs</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <returns>Operation result</returns>
        [Update("fileops/markasread")]
        public IEnumerable<FileOperationWraper> MarkAsRead(IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            return _fileStorageService.MarkAsRead(itemList).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Finishes all the active operations.
        /// </summary>
        /// <short>Finish all operations</short>
        /// <category>Operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/terminate")]
        public IEnumerable<FileOperationWraper> TerminateTasks()
        {
            return _fileStorageService.TerminateTasks().Select(o => new FileOperationWraper(o));
        }


        /// <summary>
        ///  Returns a list of all the active operations.
        /// </summary>
        /// <short>Get operations</short>
        /// <category>Operations</category>
        /// <returns>Operation result</returns>
        [Read("fileops")]
        public IEnumerable<FileOperationWraper> GetOperationStatuses()
        {
            return _fileStorageService.GetTasksStatuses().Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Starts the download process of files and folders with the IDs specified in the request.
        /// </summary>
        /// <short>Bulk download</short>
        /// <param name="fileConvertIds" visible="false">List of file IDs which will be converted</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <param name="folderIds">List of folder IDs</param>
        /// <category>Operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/bulkdownload")]
        public IEnumerable<FileOperationWraper> BulkDownload(
            IEnumerable<ItemKeyValuePair<String, String>> fileConvertIds,
            IEnumerable<String> fileIds,
            IEnumerable<String> folderIds)
        {
            var itemList = new Dictionary<String, String>();

            foreach (var fileId in fileConvertIds.Where(fileId => !itemList.ContainsKey(fileId.Key)))
            {
                itemList.Add("file_" + fileId.Key, fileId.Value);
            }

            foreach (var fileId in fileIds.Where(fileId => !itemList.ContainsKey(fileId)))
            {
                itemList.Add("file_" + fileId, string.Empty);
            }

            foreach (var folderId in folderIds.Where(folderId => !itemList.ContainsKey(folderId)))
            {
                itemList.Add("folder_" + folderId, String.Empty);
            }

            return _fileStorageService.BulkDownload(itemList).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Deletes the files and folders with the IDs specified in the request.
        /// </summary>
        /// <param name="folderIds">List of folder IDs</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <param name="deleteAfter">Deletes after finished</param>
        /// <param name="immediately">Doesn't move the files and folders to the recycle bin</param>
        /// <short>Delete files and folders</short>
        /// <category>Operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/delete")]
        public IEnumerable<FileOperationWraper> DeleteBatchItems(IEnumerable<String> folderIds, IEnumerable<String> fileIds, bool deleteAfter, bool immediately)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            return _fileStorageService.DeleteItems("delete", itemList, false, deleteAfter, immediately).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Deletes all the files and folders from the recycle bin.
        /// </summary>
        /// <short>Clear recycle bin</short>
        /// <category>Operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/emptytrash")]
        public IEnumerable<FileOperationWraper> EmptyTrash()
        {
            return _fileStorageService.EmptyTrash().Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Returns the detailed information about all the available file versions with the ID specified in the request.
        /// </summary>
        /// <short>Get file versions</short>
        /// <category>Files</category>
        /// <param name="fileId">File ID</param>
        /// <returns>File information</returns>
        [Read("file/{fileId}/history")]
        public IEnumerable<FileWrapper> GetFileVersionInfo(string fileId)
        {
            var files = _fileStorageService.GetFileHistory(fileId);
            return files.Select(x => new FileWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Changes version history of a file with the ID specified in the request.
        /// </summary>
        /// <short>Change history</short>
        /// <param name="fileId">File ID</param>
        /// <param name="version">History version</param>
        /// <param name="continueVersion">Marks as version or revision</param>
        /// <category>Files</category>
        /// <returns>File history</returns>
        [Update("file/{fileId}/history")]
        public IEnumerable<FileWrapper> ChangeHistory(string fileId, int version, bool continueVersion)
        {
            var history = _fileStorageService.CompleteVersion(fileId, version, continueVersion).Value;
            return history.Select(x => new FileWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Returns the detailed information about the shared file with the ID specified in the request.
        /// </summary>
        /// <short>Get the shared file information</short>
        /// <category>Sharing</category>
        /// <param name="fileId">File ID</param>
        /// <returns>Shared file information</returns>
        [Read("file/{fileId}/share")]
        public IEnumerable<FileShareWrapper> GetFileSecurityInfo(string fileId)
        {
            var fileShares = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { String.Format("file_{0}", fileId) });
            return fileShares.Select(x => new FileShareWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Returns the detailed information about the shared folder with the ID specified in the request.
        /// </summary>
        /// <short>Get the shared folder information</short>
        /// <param name="folderId">Folder ID</param>
        /// <category>Sharing</category>
        /// <returns>Shared folder information</returns>
        [Read("folder/{folderId}/share")]
        public IEnumerable<FileShareWrapper> GetFolderSecurityInfo(string folderId)
        {
            var fileShares = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { String.Format("folder_{0}", folderId) });
            return fileShares.Select(x => new FileShareWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Sets the sharing settings to a file with the ID specified in the request.
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <param name="share">Collection of sharing parameters</param>
        /// <param name="notify">Notifies users about the shared file</param>
        /// <param name="sharingMessage">Message to send when notifying about the shared file</param>
        /// <short>Share a file</short>
        /// <category>Sharing</category>
        /// <remarks>
        /// Each of the "share" parameters must contain two values: 'ShareTo' - ID of the user with whom we want to share the file and 'Access' - access type which we want to grant to the user (Read, ReadWrite, etc).
        /// </remarks>
        /// <returns>Shared file information</returns>
        [Update("file/{fileId}/share")]
        public IEnumerable<FileShareWrapper> SetFileSecurityInfo(string fileId, IEnumerable<FileShareParams> share, bool notify, string sharingMessage)
        {
            if (share != null && share.Any())
            {
                var list = new Web.Files.Services.WCFService.ItemList<AceWrapper>(share.Select(x => x.ToAceObject()));
                var aceCollection = new AceCollection
                {
                    Entries = new Web.Files.Services.WCFService.ItemList<string> { "file_" + fileId },
                    Aces = list,
                    Message = sharingMessage
                };
                _fileStorageService.SetAceObject(aceCollection, notify);
            }
            return GetFileSecurityInfo(fileId);
        }

        /// <summary>
        /// Sets the sharing settings to a folder with the ID specified in the request.
        /// </summary>
        /// <short>Share a folder</short>
        /// <param name="folderId">Folder ID</param>
        /// <param name="share">Collection of sharing parameters</param>
        /// <param name="notify">Notifies users about the shared file</param>
        /// <param name="sharingMessage">Message to send when notifying about the shared file</param>
        /// <remarks>
        /// Each of the "share" parameters must contain two values: 'ShareTo' - ID of the user with whom we want to share the folder and 'Access' - access type which we want to grant to the user (Read, ReadWrite, etc). 
        /// </remarks>
        /// <category>Sharing</category>
        /// <returns>Shared folder information</returns>
        [Update("folder/{folderId}/share")]
        public IEnumerable<FileShareWrapper> SetFolderSecurityInfo(string folderId, IEnumerable<FileShareParams> share, bool notify, string sharingMessage)
        {
            if (share != null && share.Any())
            {
                var list = new Web.Files.Services.WCFService.ItemList<AceWrapper>(share.Select(x => x.ToAceObject()));
                var aceCollection = new AceCollection
                {
                    Entries = new Web.Files.Services.WCFService.ItemList<string> { "folder_" + folderId },
                    Aces = list,
                    Message = sharingMessage
                };
                _fileStorageService.SetAceObject(aceCollection, notify);
            }

            return GetFolderSecurityInfo(folderId);
        }

        /// <summary>
        /// Removes the sharing rights for the group with the ID specified in the request.
        /// </summary>
        /// <param name="folderIds">List of folder IDs</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <short>Remove group sharing rights</short>
        /// <category>Sharing</category>
        /// <returns>Shared file information</returns>
        [Delete("share")]
        public bool RemoveSecurityInfo(IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            _fileStorageService.RemoveAce(itemList);

            return true;
        }

        /// <summary>
        /// Returns an external link to the shared file with the ID specified in the request.
        /// </summary>
        /// <short>Get the shared link</short>
        /// <param name="fileId">File ID</param>
        /// <param name="share">Sharing rights</param>
        /// <category>Sharing</category>
        /// <returns>Shared file link</returns>
        [Update("{fileId}/sharedlink")]
        public string GenerateSharedLink(string fileId, FileShare share)
        {
            var file = GetFileInfo(fileId);

            var objectId = "file_" + file.Id;
            var sharedInfo = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { objectId }).Find(r => r.SubjectId == FileConstant.ShareLinkId);
            if (sharedInfo == null || sharedInfo.Share != share)
            {
                var list = new Web.Files.Services.WCFService.ItemList<AceWrapper>
                    {
                        new AceWrapper
                            {
                                SubjectId = FileConstant.ShareLinkId,
                                SubjectGroup = true,
                                Share = share
                            }
                    };
                var aceCollection = new AceCollection
                {
                    Entries = new Web.Files.Services.WCFService.ItemList<string> { objectId },
                    Aces = list
                };
                _fileStorageService.SetAceObject(aceCollection, false);
                sharedInfo = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { objectId }).Find(r => r.SubjectId == FileConstant.ShareLinkId);
            }

            return sharedInfo.Link;
        }

        /// <summary>
        /// Returns file properties of the specified file.
        /// </summary>
        /// <short>Get file properties</short>
        /// <param name="fileId">File ID</param>
        /// <category>Files</category>
        /// <returns>File properties</returns>
        [Read("{fileId}/properties")]
        public EntryProperties GetProperties(string fileId)
        {
            return _fileStorageService.GetFileProperties(fileId);
        }

        /// <summary>
        /// Saves file properties to the specified file.
        /// </summary>
        /// <short>Save file properties to a file</short>
        /// <param name="fileId">File ID</param>
        /// <param name="fileProperties">File properties</param>
        /// <category>Files</category>
        /// <returns>File properties</returns>
        [Update("{fileId}/properties")]
        public EntryProperties SetProperties(string fileId, EntryProperties fileProperties)
        {
            return _fileStorageService.SetFileProperties(fileId, fileProperties);
        }

        /// <summary>
        /// Saves file properties to the specified files.
        /// </summary>
        /// <short>Save file properties to files</short>
        /// <param name="filesId">IDs of files</param>
        /// <param name="createSubfolder">Creates a subfolder or not</param>
        /// <param name="fileProperties">File properties</param>
        /// <category>Files</category>
        /// <returns>List of file properties</returns>
        [Update("batch/properties")]
        public List<EntryProperties> SetProperties(string[] filesId, bool createSubfolder, EntryProperties fileProperties)
        {
            var result = new List<EntryProperties>();

            foreach (var fileId in filesId)
            {
                if (createSubfolder)
                {
                    var file = _fileStorageService.GetFile(fileId, -1).NotFoundIfNull("File not found");
                    fileProperties.FormFilling.CreateFolderTitle = Path.GetFileNameWithoutExtension(file.Title);
                }

                result.Add(_fileStorageService.SetFileProperties(fileId, fileProperties));
            }
            return result;
        }

        /// <summary>
        /// Returns a list of the available providers.
        /// </summary>
        /// <short>Get providers</short>
        /// <category>Third-party integration</category>
        /// <returns>List of provider keys</returns>
        /// <remarks>List of provider keys: DropboxV2, Box, WebDav, Yandex, OneDrive, SharePoint, GoogleDrive.</remarks>
        /// <returns>List of the available providers</returns>
        [Read("thirdparty/capabilities")]
        public List<List<string>> Capabilities()
        {
            var result = new List<List<string>>();

            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                || (!FilesSettings.EnableThirdParty
                    && !CoreContext.Configuration.Personal))
            {
                return result;
            }

            if (ThirdpartyConfiguration.SupportBoxInclusion)
            {
                result.Add(new List<string> { "Box", BoxLoginProvider.Instance.ClientID, BoxLoginProvider.Instance.RedirectUri });
            }
            if (ThirdpartyConfiguration.SupportDropboxInclusion)
            {
                result.Add(new List<string> { "DropboxV2", DropboxLoginProvider.Instance.ClientID, DropboxLoginProvider.Instance.RedirectUri });
            }
            if (ThirdpartyConfiguration.SupportGoogleDriveInclusion)
            {
                result.Add(new List<string> { "GoogleDrive", GoogleLoginProvider.Instance.ClientID, GoogleLoginProvider.Instance.RedirectUri });
            }
            if (ThirdpartyConfiguration.SupportOneDriveInclusion)
            {
                result.Add(new List<string> { "OneDrive", OneDriveLoginProvider.Instance.ClientID, OneDriveLoginProvider.Instance.RedirectUri });
            }
            if (ThirdpartyConfiguration.SupportSharePointInclusion)
            {
                result.Add(new List<string> { "SharePoint" });
            }
            if (ThirdpartyConfiguration.SupportkDriveInclusion)
            {
                result.Add(new List<string> { "kDrive" });
            }
            if (ThirdpartyConfiguration.SupportYandexInclusion)
            {
                result.Add(new List<string> { "Yandex" });
            }
            if (ThirdpartyConfiguration.SupportWebDavInclusion)
            {
                result.Add(new List<string> { "WebDav" });
            }

            //Obsolete BoxNet, DropBox, Google, SkyDrive,

            return result;
        }

        /// <summary>
        /// Saves the storage service account of the third-party file.
        /// </summary>
        /// <short>Save a third-party account</short>
        /// <param name="url">Connection URL for the share point</param>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <param name="token">Authentication token</param>
        /// <param name="isCorporate">Corporate account or not</param>
        /// <param name="customerTitle">Customer title</param>
        /// <param name="providerKey">Provider key</param>
        /// <param name="providerId">Provider ID</param>
        /// <category>Third-party integration</category>
        /// <returns>Folder contents</returns>
        /// <remarks>List of provider keys: DropboxV2, Box, WebDav, Yandex, OneDrive, SharePoint, GoogleDrive.</remarks>
        /// <exception cref="ArgumentException"></exception>
        [Create("thirdparty")]
        public FolderWrapper SaveThirdParty(
            String url,
            String login,
            String password,
            String token,
            bool isCorporate,
            String customerTitle,
            String providerKey,
            String providerId)
        {
            var thirdPartyParams = new ThirdPartyParams
            {
                AuthData = new AuthData(url, login, password, token),
                Corporate = isCorporate,
                CustomerTitle = customerTitle,
                ProviderId = providerId,
                ProviderKey = providerKey,
            };

            var folder = _fileStorageService.SaveThirdParty(thirdPartyParams);

            return new FolderWrapper(folder);
        }

        /// <summary>
        /// Returns a list of all the connected third-party services.
        /// </summary>
        /// <category>Third-party integration</category>
        /// <short>Get third-party services</short>
        /// <returns>Connected providers</returns>
        [Read("thirdparty")]
        public IEnumerable<ThirdPartyParams> GetThirdPartyAccounts()
        {
            return _fileStorageService.GetThirdParty();
        }

        /// <summary>
        /// Returns a list of the third-party services connected to the common section.
        /// </summary>
        /// <category>Third-party integration</category>
        /// <short>Get common third-party services</short>
        /// <returns>Common third-party folders</returns>
        [Read("thirdparty/common")]
        public IEnumerable<Folder> GetCommonThirdPartyFolders()
        {
            var parent = _fileStorageService.GetFolder(Global.FolderCommon.ToString());
            return EntryManager.GetThirpartyFolders(parent);
        }

        /// <summary>
        /// Removes the storage service account of the third-party file with the ID specified in the request.
        /// </summary>
        /// <param name="providerId">Provider ID. It is a part of the folder ID. Example: folder ID is "sbox-123", then provider ID is "123".</param>
        /// <short>Remove a third-party account</short>
        /// <category>Third-party integration</category>
        /// <returns>Folder ID</returns>
        ///<exception cref="ArgumentException"></exception>
        [Delete("thirdparty/{providerId:[0-9]+}")]
        public object DeleteThirdParty(int providerId)
        {
            return _fileStorageService.DeleteThirdParty(providerId.ToString(CultureInfo.InvariantCulture));

        }

        /// <summary>
        /// Searches for files and folders by the query specified in the request.
        /// </summary>
        /// <short>Search files and folders</short>
        /// <category>Operations</category>
        /// <param name="query">Query string</param>
        /// <returns>Files and folders</returns>
        [Read(@"@search/{query}")]
        public IEnumerable<FileEntryWrapper> Search(string query)
        {
            var searcher = new Web.Files.Configuration.SearchHandler();
            var files = searcher.SearchFiles(query).Select(r => (FileEntryWrapper)new FileWrapper(r));
            var folders = searcher.SearchFolders(query).Select(f => (FileEntryWrapper)new FolderWrapper(f));

            return files.Concat(folders);
        }

        /// <summary>
        /// Adds files and folders with the IDs specified in the request to the favorite list.
        /// </summary>
        /// <short>Add favorite files and folders</short>
        /// <category>Files</category>
        /// <param name="folderIds" visible="false">List of folder IDs</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <returns>Bool value: true if the operation is successful</returns>
        [Create("favorites")]
        public bool AddFavorites(IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.AddToFavorites(new FilesNS.ItemList<object>(folderIds), new FilesNS.ItemList<object>(fileIds));
            return true;
        }

        /// <summary>
        /// Removes files and folders with the IDs specified in the request from the favorite list.
        /// </summary>
        /// <short>Delete favorite files and folders</short>
        /// <category>Files</category>
        /// <param name="folderIds" visible="false">List of folder IDs</param>
        /// <param name="fileIds">List of file IDs</param>
        /// <returns>Bool value: true if the operation is successful</returns>
        [Delete("favorites")]
        public bool DeleteFavorites(IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.DeleteFavorites(new FilesNS.ItemList<object>(folderIds), new FilesNS.ItemList<object>(fileIds));
            return true;
        }

        /// <summary>
        /// Adds files with the IDs specified in the request to the template list.
        /// </summary>
        /// <short>Add template files</short>
        /// <category>Files</category>
        /// <param name="fileIds">List of file IDs</param>
        /// <returns>Bool value: true if the operation is successful</returns>
        [Create("templates")]
        public bool AddTemplates(IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.AddToTemplates(new FilesNS.ItemList<object>(fileIds));
            return true;
        }

        /// <summary>
        /// Removes files with the IDs specified in the request from the template list.
        /// </summary>
        /// <short>Delete template files</short>
        /// <category>Files</category>
        /// <param name="fileIds">List of file IDs</param>
        /// <returns>Bool value: true if the operation is successful</returns>
        [Delete("templates")]
        public bool DeleteTemplates(IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.DeleteTemplates(new FilesNS.ItemList<object>(fileIds));
            return true;
        }


        /// <summary>
        /// Stores files in the original formats when uploading and converting.
        /// </summary>
        /// <short>Store original formats</short>
        /// <param name="set">Sets the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        [Update(@"storeoriginal")]
        public bool StoreOriginal(bool set)
        {
            return _fileStorageService.StoreOriginal(set);
        }

        /// <summary>
        /// Hides the confirmation dialog.
        /// </summary>
        /// <short>Hide the confirmation dialog</short>
        /// <param name="save">Defines if it is for saving or not</param>
        /// <category>Settings</category>
        /// <visible>false</visible>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        [Update(@"hideconfirmconvert")]
        public bool HideConfirmConvert(bool save)
        {
            return _fileStorageService.HideConfirmConvert(save);
        }

        /// <summary>
        /// Updates a file version if a file with such a name already exists.
        /// </summary>
        /// <short>Update a file version if exists</short>
        /// <param name="set">Sets the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        [Update(@"updateifexist")]
        public bool UpdateIfExist(bool set)
        {
            return _fileStorageService.UpdateIfExist(set);
        }

        /// <summary>
        /// Displays the recent folder.
        /// </summary>
        /// <short>Display recent folder</short>
        /// <param name="set">Sets the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        [Update(@"displayRecent")]
        public bool DisplayRecent(bool set)
        {
            return _fileStorageService.DisplayRecent(set);
        }

        /// <summary>
        /// Displays the favorite folder.
        /// </summary>
        /// <short>Display favorite folder</short>
        /// <param name="set">Sets the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        [Update(@"settings/favorites")]
        public bool DisplayFavorite(bool set)
        {
            return _fileStorageService.DisplayFavorite(set);
        }

        /// <summary>
        /// Displays the template folder.
        /// </summary>
        /// <short>Display template folder</short>
        /// <param name="set">Sets the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        [Update(@"settings/templates")]
        public bool DisplayTemplates(bool set)
        {
            return _fileStorageService.DisplayTemplates(set);
        }

        /// <summary>
        /// Updates the auto cleanup setting.
        /// </summary>
        /// <short>Update the auto cleanup setting</short>
        /// <param name="set">Enables the auto cleanup or not</param>
        /// <param name="gap">A time interval when the auto cleanup will be performed (one week, two weeks, one month, two months, three months)</param>
        /// <category>Settings</category>
        /// <returns>The auto cleanup setting properties</returns>
        [Update(@"settings/autocleanup")]
        public AutoCleanUpData ChangeAutomaticallyCleanUp(bool set, DateToAutoCleanUp gap)
        {
            return _fileStorageService.ChangeAutomaticallyCleanUp(set, gap);
        }

        /// <summary>
        /// Returns the auto cleanup setting properties.
        /// </summary>
        /// <short>Get the auto cleanup setting properties</short>
        /// <category>Settings</category>
        /// <returns>The auto cleanup setting properties</returns>
        [Read(@"settings/autocleanup")]
        public AutoCleanUpData GetSettingsAutomaticallyCleanUp()
        {
            return _fileStorageService.GetSettingsAutomaticallyCleanUp();
        }

        /// <summary>
        /// Changes the default access rights in sharing settings
        /// </summary>
        /// <short>Change the default access rights</short>
        /// <param name="value">Default access rights</param>
        /// <category>Settings</category>
        /// <returns>Default access rights</returns>
        [Update(@"settings/dafaultaccessrights")]
        public List<FileShare> ChangeDafaultAccessRights(List<FileShare> value)
        {
            return _fileStorageService.ChangeDafaultAccessRights(value);
        }

        /// <summary>
        /// Changes the archive format for downloading from zip to tar.gz.
        /// </summary>
        /// <short>Change the archive format</short>
        /// <param name="set">Sets the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Archive</returns>
        [Update(@"settings/downloadtargz")]
        public ICompress ChangeDownloadZip(bool set)
        {
            return _fileStorageService.ChangeDownloadTarGz(set);
        }

        /// <summary>
        /// Checks the document service location.
        /// </summary>
        /// <short>Check the document service URL</short>
        /// <param name="docServiceUrl">Document editing service domain</param>
        /// <param name="docServiceUrlInternal">Document command service domain</param>
        /// <param name="docServiceUrlPortal">Community Server address</param>
        /// <category>Settings</category>
        /// <returns>Document service information</returns>
        [Update("docservice")]
        public IEnumerable<string> CheckDocServiceUrl(string docServiceUrl, string docServiceUrlInternal, string docServiceUrlPortal)
        {
            FilesLinkUtility.DocServiceUrl = docServiceUrl;
            FilesLinkUtility.DocServiceUrlInternal = docServiceUrlInternal;
            FilesLinkUtility.DocServicePortalUrl = docServiceUrlPortal;

            MessageService.Send(HttpContext.Current.Request, MessageAction.DocumentServiceLocationSetting);

            var https = new Regex(@"^https://", RegexOptions.IgnoreCase);
            var http = new Regex(@"^http://", RegexOptions.IgnoreCase);
            if (https.IsMatch(CommonLinkUtility.GetFullAbsolutePath("")) && http.IsMatch(FilesLinkUtility.DocServiceUrl))
            {
                throw new Exception("Mixed Active Content is not allowed. HTTPS address for Document Server is required.");
            }

            DocumentServiceConnector.CheckDocServiceUrl();

            return new[]
                {
                    FilesLinkUtility.DocServiceUrl,
                    FilesLinkUtility.DocServiceUrlInternal,
                    FilesLinkUtility.DocServicePortalUrl
                };
        }


        /// <summary>
        /// Returns the address of the connected editors.
        /// </summary>
        /// <short>Get the document service URL</short>
        /// <category>Settings</category>
        /// <param name="version" visible="false">Specifies version or not</param>
        /// <returns>Address</returns>
        [Read("docservice")]
        public object GetDocServiceUrl(bool version)
        {
            var url = CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.DocServiceApiUrl);
            if (!version)
            {
                return url;
            }

            var dsVersion = DocumentServiceConnector.GetVersion();

            return new
            {
                version = dsVersion,
                docServiceUrlApi = url,
            };
        }

        /// <summary>
        /// Creates thumbnails for the files with the IDs specified in the request.
        /// </summary>
        /// <short>Create thumbnails</short>
        /// <category>Files</category>
        /// <param name="fileIds">List of file IDs</param>
        /// <visible>false</visible>
        /// <returns>List of file IDs</returns>
        [Create("thumbnails")]
        public IEnumerable<String> CreateThumbnails(IEnumerable<String> fileIds)
        {
            try
            {
                using (var thumbnailBuilderServiceClient = new ThumbnailBuilderServiceClient())
                {
                    thumbnailBuilderServiceClient.BuildThumbnails(CoreContext.TenantManager.GetCurrentTenant().TenantId, fileIds);
                }
            }
            catch (Exception e)
            {
                Common.Logging.LogManager.GetLogger("ASC.Api.Documents").Error("CreateThumbnails", e);
            }
            return fileIds;
        }


        private FolderContentWrapper ToFolderContentWrapper(object folderId, Guid userIdOrGroupId, FilterType filterType, bool searchInContent, bool withSubfolders)
        {
            OrderBy orderBy = null;
            SortedByType sortBy;
            if (Enum.TryParse(_context.SortBy, true, out sortBy))
            {
                orderBy = new OrderBy(sortBy, !_context.SortDescending);
            }
            var startIndex = Convert.ToInt32(_context.StartIndex);
            return new FolderContentWrapper(_fileStorageService.GetFolderItems(folderId.ToString(),
                                                                               startIndex,
                                                                               Convert.ToInt32(_context.Count) - 1, //NOTE: in ApiContext +1
                                                                               filterType,
                                                                               filterType == FilterType.ByUser,
                                                                               userIdOrGroupId.ToString(),
                                                                               _context.FilterValue,
                                                                               searchInContent,
                                                                               withSubfolders,
                                                                               orderBy),
                                            startIndex);
        }

        #region wordpress

        /// <summary>
        /// Returns the WordPress plugin information.
        /// </summary>
        /// <short>Get the WordPress information</short>
        /// <category>WordPress</category>
        /// <returns>WordPress information</returns>
        /// <visible>false</visible>
        [Read("wordpress-info")]
        public object GetWordpressInfo()
        {
            var token = WordpressToken.GetToken();
            if (token != null)
            {
                var meInfo = WordpressHelper.GetWordpressMeInfo(token.AccessToken);
                var blogId = JObject.Parse(meInfo).Value<string>("token_site_id");
                var wordpressUserName = JObject.Parse(meInfo).Value<string>("username");

                var blogInfo = RequestHelper.PerformRequest(WordpressLoginProvider.WordpressSites + blogId, "", "GET", "");
                var jsonBlogInfo = JObject.Parse(blogInfo);
                jsonBlogInfo.Add("username", wordpressUserName);

                blogInfo = jsonBlogInfo.ToString();
                return new
                {
                    success = true,
                    data = blogInfo
                };
            }
            return new
            {
                success = false
            };
        }

        /// <summary>
        /// Deletes the WordPress plugin information.
        /// </summary>
        /// <short>Delete the WordPress information</short>
        /// <category>WordPress</category>
        /// <returns>Object with the bool value: true if the operation is successful</returns>
        /// <visible>false</visible>
        [Read("wordpress-delete")]
        public object DeleteWordpressInfo()
        {
            var token = WordpressToken.GetToken();
            if (token != null)
            {
                WordpressToken.DeleteToken(token);
                return new
                {
                    success = true
                };
            }
            return new
            {
                success = false
            };
        }

        /// <summary>
        /// Saves the user WordPress information at login.
        /// </summary>
        /// <short>Save the user WordPress information</short>
        /// <param name="code">Authorization code</param>
        /// <category>WordPress</category>
        /// <returns>User WordPress information</returns>
        /// <visible>false</visible>
        [Create("wordpress-save")]
        public object WordpressSave(string code)
        {
            if (code == "")
            {
                return new
                {
                    success = false
                };
            }
            try
            {
                var token = OAuth20TokenHelper.GetAccessToken<WordpressLoginProvider>(code);
                WordpressToken.SaveToken(token);
                var meInfo = WordpressHelper.GetWordpressMeInfo(token.AccessToken);
                var blogId = JObject.Parse(meInfo).Value<string>("token_site_id");

                var wordpressUserName = JObject.Parse(meInfo).Value<string>("username");

                var blogInfo = RequestHelper.PerformRequest(WordpressLoginProvider.WordpressSites + blogId, "", "GET", "");
                var jsonBlogInfo = JObject.Parse(blogInfo);
                jsonBlogInfo.Add("username", wordpressUserName);

                blogInfo = jsonBlogInfo.ToString();
                return new
                {
                    success = true,
                    data = blogInfo
                };
            }
            catch (Exception)
            {
                return new
                {
                    success = false
                };
            }
        }

        /// <summary>
        /// Creates a WordPress post with the parameters specified in the request.
        /// </summary>
        /// <short>Create a WordPress post</short>
        /// <param name="code">Authorization code</param>
        /// <param name="title">Post title</param>
        /// <param name="content">Post content</param>
        /// <param name="status">Operation status</param>
        /// <category>WordPress</category>
        /// <returns>WordPress post</returns>
        /// <visible>false</visible>
        [Create("wordpress")]
        public bool CreateWordpressPost(string code, string title, string content, int status)
        {
            try
            {
                var token = WordpressToken.GetToken();
                var meInfo = WordpressHelper.GetWordpressMeInfo(token.AccessToken);
                var parser = JObject.Parse(meInfo);
                if (parser == null) return false;
                var blogId = parser.Value<string>("token_site_id");

                if (blogId != null)
                {
                    var createPost = WordpressHelper.CreateWordpressPost(title, content, status, blogId, token);
                    return createPost;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region easybib

        /// <summary>
        /// Returns the EasyBib citation list.
        /// </summary>
        /// <short>Get the EasyBib citation list</short>
        /// <param name="source">Citation source: book (0), journal (1) or website (2)</param>
        /// <param name="data">Citation data</param>
        /// <category>EasyBib</category>
        /// <returns>EasyBib citation list</returns>
        /// <visible>false</visible>
        [Read("easybib-citation-list")]
        public object GetEasybibCitationList(int source, string data)
        {
            try
            {
                var citationList = EasyBibHelper.GetEasyBibCitationsList(source, data);
                return new
                {
                    success = true,
                    citations = citationList
                };
            }
            catch (Exception)
            {
                return new
                {
                    success = false
                };
            }

        }

        /// <summary>
        /// Returns the EasyBib styles.
        /// </summary>
        /// <short>Get the EasyBib styles</short>
        /// <category>EasyBib</category>
        /// <returns>List of EasyBib styles</returns>
        /// <visible>false</visible>
        [Read("easybib-styles")]
        public object GetEasybibStyles()
        {
            try
            {
                var data = EasyBibHelper.GetEasyBibStyles();
                return new
                {
                    success = true,
                    styles = data
                };
            }
            catch (Exception)
            {
                return new
                {
                    success = false
                };
            }
        }

        /// <summary>
        /// Returns the EasyBib citation book.
        /// </summary>
        /// <short>Get the EasyBib citation</short>
        /// <param name="citationData">Citation data</param>
        /// <category>EasyBib</category>
        /// <returns>EasyBib citation</returns>
        /// <visible>false</visible>
        [Create("easybib-citation")]
        public object EasyBibCitationBook(string citationData)
        {
            try
            {
                var citat = EasyBibHelper.GetEasyBibCitation(citationData);
                if (citat != null)
                {
                    return new
                    {
                        success = true,
                        citation = citat
                    };
                }
                else
                {
                    return new
                    {
                        success = false
                    };
                }

            }
            catch (Exception)
            {
                return new
                {
                    success = false
                };
            }
        }

        #endregion

        /// <summary>
        /// Result of the file conversion operation.
        /// </summary>
        [DataContract(Name = "operation_result", Namespace = "")]
        public class ConversationResult
        {
            /// <summary>
            /// Operation ID.
            /// </summary>
            [DataMember(Name = "id")]
            public string Id { get; set; }

            /// <summary>
            /// Operation type.
            /// </summary>
            [DataMember(Name = "operation")]
            public FileOperationType OperationType { get; set; }

            /// <summary>
            /// Operation progress.
            /// </summary>
            [DataMember(Name = "progress")]
            public int Progress { get; set; }

            /// <summary>
            /// Source files for operation.
            /// </summary>
            [DataMember(Name = "source")]
            public string Source { get; set; }

            /// <summary>
            /// Result file of operation.
            /// </summary>
            [DataMember(Name = "result")]
            public FileWrapper File { get; set; }

            /// <summary>
            /// Error during conversion.
            /// </summary>
            [DataMember(Name = "error")]
            public string Error { get; set; }

            /// <summary>
            /// Is operation processed.
            /// </summary>
            [DataMember(Name = "processed")]
            public string Processed { get; set; }
        }
    }
}