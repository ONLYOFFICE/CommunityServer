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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Controls;

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Api.Utils;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Users;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Files.Core;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Specific.AuthorizationApi;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Core.Compress;
using ASC.Web.Files.Core.Entries;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.HttpHandlers;
using ASC.Web.Files.Resources;
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
    /// <name>files</name>
    public class DocumentsApi : Interfaces.IApiEntryPoint
    {
        private static readonly ICache _cache = AscCache.Memory;
        private readonly ApiContext _context;
        private readonly IFileStorageService _fileStorageService;

        /// <summary>
        /// Api name entry
        /// </summary>
        public string Name
        {
            get { return "files"; }
        }

        /// <summary>
        /// Constructor
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
        /// <short>Get filtered sections</short>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <param type="System.Boolean, System" name="withoutTrash">Specifies whether to return sections with or without trash folder</param>
        /// <param type="System.Boolean, System" name="withoutAdditionalFolder">Specifies whether to return sections with or without additional folders</param>
        /// <category>Folders</category>
        /// <returns>Contents of the sections</returns>
        /// <path>api/2.0/files/@root</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("@root")]
        public IEnumerable<FolderContentWrapper> GetRootFolders(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders, bool withoutTrash, bool withoutAdditionalFolder)
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

            return result.Select(r => ToFolderContentWrapper(r, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders));
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "My documents" section.
        /// </summary>
        /// <short>Get the "My documents" section</short>
        /// <category>Folders</category>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">The "My documents" section contents</returns>
        /// <path>api/2.0/files/@my</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@my")]
        public FolderContentWrapper GetMyFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderMy, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "In projects" section.
        /// </summary>
        /// <short>Get the "In projects" section</short>
        /// <category>Folders</category>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">The "In projects" section contents</returns>
        /// <path>api/2.0/files/@projects</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@projects")]
        public FolderContentWrapper GetProjectsFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderProjects, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders);
        }


        /// <summary>
        /// Returns the detailed list of files and folders located in the "Common" section.
        /// </summary>
        /// <short>Get the "Common" section</short>
        /// <category>Folders</category>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">The "Common" section contents</returns>
        /// <path>api/2.0/files/@common</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@common")]
        public FolderContentWrapper GetCommonFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderCommon, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "Shared with me" section.
        /// </summary>
        /// <short>Get the "Shared with me" section</short>
        /// <category>Folders</category>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">The "Shared with me" section contents</returns>
        /// <path>api/2.0/files/@share</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@share")]
        public FolderContentWrapper GetShareFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderShare, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files located in the "Recent" section.
        /// </summary>
        /// <short>Get the "Recent" section</short>
        /// <category>Folders</category>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">The "Recent" section contents</returns>
        /// <path>api/2.0/files/@recent</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@recent")]
        public FolderContentWrapper GetRecentFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderRecent, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "Favorites" section.
        /// </summary>
        /// <short>Get the "Favorites" section</short>
        /// <category>Folders</category>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">The "Favorites" section contents</returns>
        /// <path>api/2.0/files/@favorites</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@favorites")]
        public FolderContentWrapper GetFavoritesFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderFavorites, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files located in the "Templates" section.
        /// </summary>
        /// <short>Get the "Templates" section</short>
        /// <category>Folders</category>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">The "Templates" section contents</returns>
        /// <path>api/2.0/files/@templates</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@templates")]
        public FolderContentWrapper GetTemplatesFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderTemplates, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the "Trash" section.
        /// </summary>
        /// <short>Get the "Trash" section</short>
        /// <category>Folders</category>
        /// <param type="System.Guid, System" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" name="filterType" optional="true">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">The "Trash" section contents</returns>
        /// <path>api/2.0/files/@trash</path>
        /// <httpMethod>GET</httpMethod>
        [Read("@trash")]
        public FolderContentWrapper GetTrashFolder(Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(Global.FolderTrash, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the folder with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a folder by ID
        /// </short>
        /// <category>Folders</category>
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <param type="System.Guid, System" method="url" name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param type="ASC.Files.Core.FilterType, ASC.Files.Core" method="url" name="filterType" optional="true" remark="Allowed values: None (0), FilesOnly (1), FoldersOnly (2), DocumentsOnly (3), PresentationsOnly (4), SpreadsheetsOnly (5) or ImagesOnly (7)">Filter type</param>
        /// <param type="System.Boolean, System" name="searchInContent">Specifies whether to search within the section contents or not</param>
        /// <param type="System.String, System" name="extension">File extension by which files will be searched for if the FilterType.ByExtension parameter is passed</param>
        /// <param type="System.Boolean, System" name="withSubfolders">Specifies whether to return sections with or without subfolders</param>
        /// <returns type="ASC.Api.Documents.FolderContentWrapper, ASC.Api.Documents">Folder contents</returns>
        /// <path>api/2.0/files/{folderId}</path>
        /// <httpMethod>GET</httpMethod>
        [Read("{folderId}")]
        public FolderContentWrapper GetFolder(String folderId, Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            return ToFolderContentWrapper(folderId, userIdOrGroupId, filterType, searchInContent, extension, withSubfolders).NotFoundIfNull();

        }

        /// <summary>
        /// Uploads a file specified in the request to the "My documents" section by single file uploading or standart multipart/form-data method.
        /// </summary>
        /// <short>Upload a file to the "My documents" section</short>
        /// <category>Folders</category>
        /// <param type="System.IO.Stream, System.IO" name="file" visible="false">Request input stream</param>
        /// <param type="System.Net.Mime.ContentType, System.Net.Mime" name="contentType" visible="false">Content-Type header</param>
        /// <param type="System.Net.Mime.ContentDisposition, System.Net.Mime" name="contentDisposition" visible="false">Content-Disposition header</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Web.HttpPostedFileBase}, System.Collections.Generic" name="files" visible="false">List of files when specified as multipart/form-data</param>
        /// <remarks>
        /// <![CDATA[
        ///  You can upload files in two different ways:
        ///  <ol>
        /// <li>Using single file upload. You should set the Content-Type and Content-Disposition headers to specify a file name and content type, and send the file to the request body.</li>
        /// <li>Using standart multipart/form-data method.</li>
        /// </ol>]]>
        /// </remarks>
        /// <returns>Uploaded file(s)</returns>
        /// <path>api/2.0/files/@my/upload</path>
        /// <httpMethod>POST</httpMethod>
        [Create("@my/upload")]
        public object UploadFileToMy(Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<HttpPostedFileBase> files)
        {
            return UploadFile(Global.FolderMy.ToString(), file, contentType, contentDisposition, files, false, null);
        }

        /// <summary>
        /// Uploads a file specified in the request to the "Common" section by single file uploading or standart multipart/form-data method.
        /// </summary>
        /// <short>Upload a file to the "Common" section</short>
        /// <category>Folders</category>
        /// <param type="System.IO.Stream, System.IO" name="file" visible="false">Request input stream</param>
        /// <param type="System.Net.Mime.ContentType, System.Net.Mime" name="contentType" visible="false">Content-Type header</param>
        /// <param type="System.Net.Mime.ContentDisposition, System.Net.Mime" name="contentDisposition" visible="false">Content-Disposition header</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Web.HttpPostedFileBase}, System.Collections.Generic" name="files" visible="false">List of files when specified as multipart/form-data</param>
        /// <remarks>
        /// <![CDATA[
        ///  You can upload files in two different ways:
        ///  <ol>
        /// <li>Using single file upload. You should set the Content-Type and Content-Disposition headers to specify a file name and content type, and send the file to the request body.</li>
        /// <li>Using standart multipart/form-data method.</li>
        /// </ol>]]>
        /// </remarks>
        /// <returns>Uploaded file(s)</returns>
        /// <path>api/2.0/files/@common/upload</path>
        /// <httpMethod>POST</httpMethod>
        [Create("@common/upload")]
        public object UploadFileToCommon(Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<HttpPostedFileBase> files)
        {
            return UploadFile(Global.FolderCommon.ToString(), file, contentType, contentDisposition, files, false, null);
        }


        /// <summary>
        /// Uploads a file specified in the request to the selected folder by single file uploading or standart multipart/form-data method.
        /// </summary>
        /// <short>Upload a file</short>
        /// <category>Folders</category>
        /// <remarks>
        /// <![CDATA[
        ///  You can upload files in two different ways:
        ///  <ol>
        /// <li>Using single file upload. You should set the Content-Type and Content-Disposition headers to specify a file name and content type, and send the file to the request body.</li>
        /// <li>Using standart multipart/form-data method.</li>
        /// </ol>]]>
        /// </remarks>
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <param type="System.IO.Stream, System.IO" name="file" visible="false">Request input stream</param>
        /// <param type="System.Net.Mime.ContentType, System.Net.Mime" name="contentType" visible="false">Content-Type header</param>
        /// <param type="System.Net.Mime.ContentDisposition, System.Net.Mime" name="contentDisposition" visible="false">Content-Disposition header</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Web.HttpPostedFileBase}, System.Collections.Generic" name="files" visible="false">List of files when specified as multipart/form-data</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="createNewIfExist" visible="false">Specifies whether to create a new file if it already exists or not</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="storeOriginalFileFlag" visible="false">Specifies whether to upload documents in the original formats as well or not</param>
        /// <param type="System.Boolean, System" name="keepConvertStatus" visible="false">Specifies whether to keep the file converting status or not</param>
        /// <returns>Uploaded file(s)</returns>
        /// <path>api/2.0/files/{folderId}/upload</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <short>Insert a file to the "My documents" section</short>
        /// <param type="System.IO.Stream, System.IO" name="file" visible="false">Request input stream</param>
        /// <param type="System.String, System" name="title">File name</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="createNewIfExist" visible="false">Specifies whether to create a new file if it already exists or not</param>
        /// <param type="System.Boolean, System" name="keepConvertStatus" visible="false">Specifies whether to keep the file converting status or not</param>
        /// <category>Folders</category>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">Inserted file</returns>
        /// <path>api/2.0/files/@my/insert</path>
        /// <httpMethod>POST</httpMethod>
        [Create("@my/insert")]
        public FileWrapper InsertFileToMy(Stream file, string title, bool? createNewIfExist, bool keepConvertStatus = false)
        {
            return InsertFile(Global.FolderMy.ToString(), file, title, createNewIfExist, keepConvertStatus);
        }

        /// <summary>
        /// Inserts a file specified in the request to the "Common" section by single file uploading.
        /// </summary>
        /// <short>Insert a file to the "Common" section</short>
        /// <param type="System.IO.Stream, System.IO" name="file" visible="false">Request input stream</param>
        /// <param type="System.String, System" name="title">File name</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="createNewIfExist" visible="false">Specifies whether to create a new file if it already exists or not</param>
        /// <param type="System.Boolean, System" name="keepConvertStatus" visible="false">Specifies whether to keep the file converting status or not</param>
        /// <category>Folders</category>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">Inserted file</returns>
        /// <path>api/2.0/files/@common/insert</path>
        /// <httpMethod>POST</httpMethod>
        [Create("@common/insert")]
        public FileWrapper InsertFileToCommon(Stream file, string title, bool? createNewIfExist, bool keepConvertStatus = false)
        {
            return InsertFile(Global.FolderCommon.ToString(), file, title, createNewIfExist, keepConvertStatus);
        }

        /// <summary>
        /// Inserts a file specified in the request to the selected folder by single file uploading.
        /// </summary>
        /// <short>Insert a file</short>
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <param type="System.IO.Stream, System.IO" name="file" visible="false">Request input stream</param>
        /// <param type="System.String, System" name="title">File name</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="createNewIfExist" visible="false">Specifies whether to create a new file if it already exists or not</param>
        /// <param type="System.Boolean, System" name="keepConvertStatus" visible="false">Specifies whether to keep the file converting status or not</param>
        /// <category>Folders</category>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">Inserted file</returns>
        /// <path>api/2.0/files/{folderId}/insert</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <param type="System.IO.Stream, System.IO" name="file">Request input stream</param>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.String, System" name="fileExtension">File extension</param>
        /// <param type="System.Boolean, System" name="encrypted" visible="false">Specifies whether to encrypt a file or not</param>
        /// <param type="System.Boolean, System" name="forcesave" visible="false">Specifies whether to force save a file or not</param>
        /// <path>api/2.0/files/{fileId}/update</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">Updated file</returns>
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
        /// Saves edits to a file with the ID specified in the request.
        /// </summary>
        /// <short>Save file edits</short>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.String, System" name="fileExtension">File extension</param>
        /// <param type="System.String, System" name="downloadUri">URI to download a file</param>
        /// <param type="System.IO.Stream, System.IO" name="stream">Request file stream</param>
        /// <param type="System.String, System" name="doc">Shared token</param>
        /// <param type="System.Boolean, System" name="forcesave" visible="false">Specifies whether to force save a file or not</param>
        /// <category>Files</category>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">Saved file</returns>
        /// <path>api/2.0/files/file/{fileId}/saveediting</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("file/{fileId}/saveediting")]
        public FileWrapper SaveEditing(String fileId, string fileExtension, string downloadUri, Stream stream, String doc, bool forcesave)
        {
            return new FileWrapper(_fileStorageService.SaveEditing(fileId, fileExtension, downloadUri, stream, doc, forcesave));
        }

        /// <summary>
        /// Informs about opening a file with the ID specified in the request for editing, locking it from being deleted or moved (this method is called by the mobile editors).
        /// </summary>
        /// <short>Start file editing</short>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.Boolean, System" name="editingAlone" visible="false">Specifies whether to share a file with other users for editing or not</param>
        /// <param type="System.String, System" name="doc" visible="false">Shared token</param>
        /// <category>Files</category>
        /// <returns>File key for Document Service</returns>
        /// <path>api/2.0/files/file/{fileId}/startedit</path>
        /// <httpMethod>POST</httpMethod>
        [Create("file/{fileId}/startedit")]
        public string StartEdit(String fileId, bool editingAlone, String doc)
        {
            return _fileStorageService.StartEdit(fileId, editingAlone, doc);
        }

        /// <summary>
        /// Tracks file changes when editing.
        /// </summary>
        /// <short>Track file editing</short>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.Guid, System" name="tabId" visible="false">Tab ID</param>
        /// <param type="System.String, System" name="docKeyForTrack" visible="false">Document key for tracking</param>
        /// <param type="System.String, System" name="doc" visible="false">Shared token</param>
        /// <param type="System.Boolean, System" method="url" name="isFinish">Specifies whether to finish file tracking or not</param>
        /// <category>Files</category>
        /// <returns>File changes</returns>
        /// <path>api/2.0/files/file/{fileId}/trackeditfile</path>
        /// <httpMethod>GET</httpMethod>
        [Read("file/{fileId}/trackeditfile")]
        public KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String doc, bool isFinish)
        {
            return _fileStorageService.TrackEditFile(fileId, tabId, docKeyForTrack, doc, isFinish);
        }

        /// <summary>
        /// Returns the initialization configuration of a file to open it in the editor.
        /// </summary>
        /// <short>Open a file</short>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.Int32, System" method="url" name="version">File version</param>
        /// <param type="System.String, System" method="url" name="doc" visible="false">Shared token</param>
        /// <category>Files</category>
        /// <returns type="ASC.Web.Files.Services.DocumentService.Configuration, ASC.Web.Files">Configuration</returns>
        /// <path>api/2.0/files/file/{fileId}/openedit</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <httpMethod>GET</httpMethod>
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
        /// <category>Operations</category>
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <param type="System.String, System" name="fileName">File name</param>
        /// <param type="System.Int64, System" name="fileSize">File length in bytes</param>
        /// <param type="System.String, System" name="relativePath">Relative path to the folder</param>
        /// <param type="System.Boolean, System" name="encrypted" visible="false">Specifies whether to encrypt a file or not</param>
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
        /// <path>api/2.0/files/{folderId}/upload/create_session</path>
        /// <httpMethod>POST</httpMethod>
        /// <requiresAuthorization>false</requiresAuthorization>
        [Create("{folderId}/upload/create_session", false)] // NOTE: This method doesn't require auth!!!
        public object CreateUploadSession(string folderId, string fileName, long fileSize, string relativePath, bool encrypted)
        {
            string link = null;

            if (!SecurityContext.IsAuthenticated)
            {
                if (Web.Files.Utils.FileShareLink.TryGetCurrentLinkId(out var linkId))
                {
                    link = linkId.ToString();
                }
                else
                {
                    throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException);
                }
            }

            var file = FileUploader.VerifyChunkedUpload(folderId, fileName, fileSize, FilesSettings.UpdateIfExist, relativePath);

            return CreateUploadSession(file, encrypted, link);
        }

        /// <summary>
        /// Creates a session to edit the existing file with multiple chunks (needed for WebDAV).
        /// </summary>
        /// <short>Create the editing session</short>
        /// <category>Files</category>
        /// <param type="System.Object, System" name="fileId">File ID</param>
        /// <param type="System.Int64, System" name="fileSize">File size in bytes</param>
        /// <returns>Upload session</returns>
        /// <path>api/2.0/files/file/{fileId}/edit_session</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("file/{fileId}/edit_session")]
        public object CreateEditSession(object fileId, long fileSize)
        {
            var file = FileUploader.VerifyChunkedUploadForEditing(fileId, fileSize);

            return CreateUploadSession(file, false, null, true);
        }

        private object CreateUploadSession(Files.Core.File file, bool encrypted, string linkId, bool keepVersion = false)
        {
            if (FilesLinkUtility.IsLocalFileUploader)
            {
                var session = FileUploader.InitiateUpload(file.FolderID.ToString(), (file.ID ?? "").ToString(), file.Title, file.ContentLength, encrypted, linkId, keepVersion);

                var response = ChunkedUploaderHandler.ToResponseObject(session, true);
                return new
                {
                    success = true,
                    data = response
                };
            }

            var createSessionUrl = FilesLinkUtility.GetInitiateUploadSessionUrl(file.FolderID, file.ID, file.Title, file.ContentLength, encrypted, linkId);
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

            if (!string.IsNullOrEmpty(linkId))
            {
                var cookies = CookiesManager.GetCookies(CookiesType.ShareLink, linkId);
                if (!string.IsNullOrEmpty(cookies))
                {
                    var name = CookiesManager.GetCookiesName(CookiesType.ShareLink, linkId);
                    request.Headers[name] = cookies;
                }
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
        /// <short>Create a txt file in the "My documents" section</short>
        /// <category>Files</category>
        /// <param type="System.String, System" name="title">File title</param>
        /// <param type="System.String, System" name="content">File contents</param>
        /// <returns>File contents</returns>
        /// <path>api/2.0/files/@my/text</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("@my/text")]
        public FileWrapper CreateTextFileInMy(string title, string content)
        {
            return CreateTextFile(Global.FolderMy.ToString(), title, content);
        }

        /// <summary>
        /// Creates a text (.txt) file in the "Common" section with the title and contents specified in the request.
        /// </summary>
        /// <short>Create a txt file in the "Common" section</short>
        /// <category>Files</category>
        /// <param type="System.String, System" name="title">File title</param>
        /// <param type="System.String, System" name="content">File contents</param>
        /// <returns>File contents</returns>
        /// <path>api/2.0/files/@common/text</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <param type="System.String, System" name="folderId">Folder ID</param>
        /// <param type="System.String, System" name="title">File title</param>
        /// <param type="System.String, System" name="content">File contents</param>
        /// <returns>File contents</returns>
        /// <path>api/2.0/files/{folderId}/text</path>
        /// <httpMethod>POST</httpMethod>
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
        /// Creates an HTML (.html) file in the selected folder with the title and contents specified in the request.
        /// </summary>
        /// <short>Create an HTML file</short>
        /// <category>Files</category>
        /// <param type="System.String, System" name="folderId">Folder ID</param>
        /// <param type="System.String, System" name="title">File title</param>
        /// <param type="System.String, System" name="content">File contents</param>
        /// <returns>File contents</returns>
        /// <path>api/2.0/files/{folderId}/html</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("{folderId}/html")]
        public FileWrapper CreateHtmlFile(string folderId, string title, string content)
        {
            if (title == null) throw new ArgumentNullException("title");
            return CreateFile(folderId, title, content, ".html");
        }

        /// <summary>
        /// Creates an HTML (.html) file in the "My documents" section with the title and contents specified in the request.
        /// </summary>
        /// <short>Create an HTML file in the "My documents" section</short>
        /// <category>Files</category>
        /// <param type="System.String, System" name="title">File title</param>
        /// <param type="System.String, System" name="content">File contents</param>
        /// <returns>File contents</returns>
        /// <path>api/2.0/files/@my/html</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("@my/html")]
        public FileWrapper CreateHtmlFileInMy(string title, string content)
        {
            return CreateHtmlFile(Global.FolderMy.ToString(), title, content);
        }


        /// <summary>
        /// Creates an HTML (.html) file in the "Common" section with the title and contents specified in the request.
        /// </summary>
        /// <short>Create an HTML file in the "Common" section</short>
        /// <category>Files</category>
        /// <param type="System.String, System" name="title">File title</param>
        /// <param type="System.String, System" name="content">File contents</param>
        /// <returns>File contents</returns>
        /// <path>api/2.0/files/@common/html</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <param type="System.String, System" method="url" name="folderId">Parent folder ID</param>
        /// <param type="System.String, System" name="title">Folder title</param>
        /// <returns type="ASC.Api.Documents.FolderWrapper, ASC.Api.Documents">New folder contents</returns>
        /// <path>api/2.0/files/folder/{folderId}</path>
        /// <httpMethod>POST</httpMethod>
        [Create("folder/{folderId}")]
        public FolderWrapper CreateFolder(string folderId, string title)
        {
            var folder = _fileStorageService.CreateNewFolder(folderId, title);
            return new FolderWrapper(folder);
        }


        /// <summary>
        /// Creates a new folder structure specified in the request in a folder with a specific ID.
        /// </summary>
        /// <short>
        /// Create a folder structure
        /// </short>
        /// <category>Folders</category>
        /// <param type="System.String, System" name="folderId">Parent folder ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="relativePaths">Relative paths to create a folder structure</param>
        /// <returns>Main folder contents</returns>
        /// <path>api/2.0/files/folders/{folderId}</path>
        /// <httpMethod>POST</httpMethod>
        [Create("folders/{folderId}", false)] // NOTE: This method doesn't require auth!!!
        public Folder CreateFolders(string folderId, IEnumerable<string> relativePaths)
        {
            var folder = _fileStorageService.CreateNewFolders(folderId, relativePaths);
            return folder;
        }


        /// <summary>
        /// Creates a new file in the "My documents" section with the title specified in the request.
        /// </summary>
        /// <short>Create a file in the "My documents" section</short>
        /// <category>Files</category>
        /// <param type="System.String, System" name="title" remark="Allowed values: the file must have one of the following extensions: DOCX, XLSX, PPTX">File title</param>
        /// <remarks>If a file extension is different from DOCX/XLSX/PPTX and refers to one of the known text, spreadsheet or presentation formats, it will be changed to DOCX/XLSX/PPTX accordingly. If the file extension is not specified or is unknown, the DOCX extension will be added to the file title.</remarks>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">New file information</returns>
        /// <path>api/2.0/files/@my/file</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <param type="System.String, System" name="title" remark="Allowed values: the file must have one of the following extensions: DOCX, XLSX, PPTX">File title</param>
        /// <param type="System.String, System" name="templateId">Template file ID</param>
        /// <param type="System.Boolean, System" name="enableExternalExt">Specifies whether to allow the creation of external extension files or not</param>
        /// <remarks>If a file extension is different from DOCX/XLSX/PPTX and refers to one of the known text, spreadsheet or presentation formats, it will be changed to DOCX/XLSX/PPTX accordingly. If the file extension is not specified or is unknown, the DOCX extension will be added to the file title.</remarks>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">New file information</returns>
        /// <path>api/2.0/files/{folderId}/file</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <param type="System.String, System" name="title">New folder title</param>
        /// <returns type="ASC.Api.Documents.FolderWrapper, ASC.Api.Documents">Folder contents</returns>
        /// <path>api/2.0/files/folder/{folderId}</path>
        /// <httpMethod>PUT</httpMethod>
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
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <category>Folders</category>
        /// <returns type="ASC.Api.Documents.FolderWrapper, ASC.Api.Documents">Folder information</returns>
        /// <path>api/2.0/files/folder/{folderId}</path>
        /// <httpMethod>GET</httpMethod>
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
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <category>Folders</category>
        /// <returns type="ASC.Api.Documents.FolderWrapper, ASC.Api.Documents">Folder path</returns>
        /// <path>api/2.0/files/folder/{folderId}/path</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("folder/{folderId}/path")]
        public IEnumerable<FolderWrapper> GetFolderPath(string folderId)
        {
            return EntryManager.GetBreadCrumbs(folderId).Select(f => new FolderWrapper(f)).ToSmartList();
        }

        /// <summary>
        /// Returns the detailed information about a file with the ID specified in the request.
        /// </summary>
        /// <short>Get the file information</short>
        /// <param type="System.String, System" name="fileId">File ID</param>
        /// <param type="System.Int32, System" name="version">File version</param>
        /// <category>Files</category>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">File information</returns>
        /// <path>api/2.0/files/file/{fileId}</path>
        /// <httpMethod>GET</httpMethod>
        [Read("file/{fileId}")]
        public FileWrapper GetFileInfo(string fileId, int version = -1)
        {
            var file = _fileStorageService.GetFile(fileId, version).NotFoundIfNull("File not found");
            return new FileWrapper(file);
        }

        /// <summary>
        /// Copies (and converts if possible) an existing file to the specified folder.
        /// </summary>
        /// <short>Copy a file</short>
        /// <category>Files</category>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.String, System" name="destFolderId">Destination folder ID</param>
        /// <param type="System.String, System" name="destTitle">Destination file title</param>
        /// <returns>Copied file</returns>
        /// <path>api/2.0/files/file/{fileId}/copyas</path>
        /// <httpMethod>POST</httpMethod>
        /// <requiresAuthorization>false</requiresAuthorization>
        [Create("file/{fileId}/copyas", false)] // NOTE: This method doesn't require auth!!!
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
        /// <param type="System.String, System" name="fileId">File ID</param>
        /// <param type="System.String, System" name="title">New file title</param>
        /// <param type="System.Int32, System" name="lastVersion">Number of the latest file version</param>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">File information</returns>
        /// <path>api/2.0/files/file/{fileId}</path>
        /// <httpMethod>PUT</httpMethod>
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
        /// <category>Files</category>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.Boolean, System" name="deleteAfter">Specifies whether to delete a file after the editing session is finished or not</param>
        /// <param type="System.Boolean, System" name="immediately">Specifies whether to move a file to the "Trash" folder or delete it immediately</param>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/file/{fileId}</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <collection>list</collection>
        [Delete("file/{fileId}")]
        public IEnumerable<FileOperationWraper> DeleteFile(String fileId, bool deleteAfter, bool immediately)
        {
            return DeleteBatchItems(null, new[] { fileId }, deleteAfter, immediately);
        }

        /// <summary>
        /// Starts a conversion operation of a file with the ID specified in the request.
        /// </summary>
        /// <short>Start file conversion</short>
        /// <category>Operations</category>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <returns type="ASC.Api.Documents.DocumentsApi.ConversationResult, ASC.Api.Documents">Operation result</returns>
        /// <path>api/2.0/files/file/{fileId}/checkconversion</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update("file/{fileId}/checkconversion")]
        public IEnumerable<ConversationResult> StartConversion(String fileId)
        {
            return CheckConversion(fileId, true);
        }

        /// <summary>
        /// Checks the conversion status of a file with the ID specified in the request.
        /// </summary>
        /// <short>Get conversion status</short>
        /// <category>Operations</category>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.Boolean, System" method="url" name="start">Specifies if a conversion operation is started or not</param>
        /// <returns type="ASC.Api.Documents.DocumentsApi.ConversationResult, ASC.Api.Documents">Operation result</returns>
        /// <path>api/2.0/files/file/{fileId}/checkconversion</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.String, System" name="fileId">File ID</param>
        /// <returns>File download link</returns>
        /// <path>api/2.0/files/file/{fileId}/presigneduri</path>
        /// <httpMethod>GET</httpMethod>
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
        /// <category>Folders</category>
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <param type="System.Boolean, System" name="deleteAfter">Specifies whether to delete a folder after the editing session is finished or not</param>
        /// <param type="System.Boolean, System" name="immediately">Specifies whether to move a folder to the "Trash" folder or delete it immediately</param>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/folder/{folderId}</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.String, System" method="url" name="destFolderId">Destination folder ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" method="url" name="folderIds">List of folder IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" method="url" name="fileIds">List of file IDs</param>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">IDs of files with conflicts</returns>
        /// <path>api/2.0/files/fileops/move</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("fileops/move")]
        public IEnumerable<FileWrapper> MoveOrCopyBatchCheck(String destFolderId, IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            var ids = _fileStorageService.MoveOrCopyFilesCheck(itemList, destFolderId).Keys.Select(id => "file_" + id);

            var entries = _fileStorageService.GetItems(new Web.Files.Services.WCFService.ItemList<string>(ids), FilterType.FilesOnly, false, "", "", null);
            return entries.Select(x => new FileWrapper((Files.Core.File)x)).ToSmartList();
        }

        /// <summary>
        /// Moves all the selected files and folders to the folder with the ID specified in the request.
        /// </summary>
        /// <short>Move to a folder</short>
        /// <category>Operations</category>
        /// <param type="System.String, System" name="destFolderId">Destination folder ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="folderIds">List of folder IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <param type="ASC.Web.Files.Services.WCFService.FileOperations.FileConflictResolveType, ASC.Web.Files.Services.WCFService.FileOperations" name="conflictResolveType">Overwriting behavior: skip (0), overwrite (1) or duplicate (2)</param>
        /// <param type="System.Boolean, System" name="deleteAfter">Specifies whether to delete a folder after the editing session is finished or not</param>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/fileops/move</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.String, System" name="destFolderId">Destination folder ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="folderIds">List of folder IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <param type="ASC.Web.Files.Services.WCFService.FileOperations.FileConflictResolveType, ASC.Web.Files.Services.WCFService.FileOperations" name="conflictResolveType">Overwriting behavior: skip (0), overwrite (1) or duplicate (2)</param>
        /// <param type="System.Boolean, System" name="deleteAfter">Specifies whether to delete a folder after the editing session is finished or not</param>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/fileops/copy</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="folderIds">List of folder IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/fileops/markasread</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// <short>Finish active operations</short>
        /// <category>Operations</category>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/fileops/terminate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update("fileops/terminate")]
        public IEnumerable<FileOperationWraper> TerminateTasks()
        {
            return _fileStorageService.TerminateTasks().Select(o => new FileOperationWraper(o));
        }


        /// <summary>
        ///  Returns a list of all the active operations.
        /// </summary>
        /// <short>Get active operations</short>
        /// <category>Operations</category>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/fileops</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("fileops")]
        public IEnumerable<FileOperationWraper> GetOperationStatuses()
        {
            return _fileStorageService.GetTasksStatuses().Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Starts the download process of files and folders with the IDs specified in the request.
        /// </summary>
        /// <short>Bulk download</short>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.String, System.String}}, System.Collections.Generic" name="fileConvertIds" visible="false">List of file IDs which will be converted</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="folderIds">List of folder IDs</param>
        /// <category>Operations</category>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/fileops/bulkdownload</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="folderIds">List of folder IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <param type="System.Boolean, System" name="deleteAfter">Specifies whether to delete a file after the editing session is finished or not</param>
        /// <param type="System.Boolean, System" name="immediately">Specifies whether to move a file to the "Trash" folder or delete it immediately</param>
        /// <short>Delete files and folders</short>
        /// <category>Operations</category>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/fileops/delete</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update("fileops/delete")]
        public IEnumerable<FileOperationWraper> DeleteBatchItems(IEnumerable<String> folderIds, IEnumerable<String> fileIds, bool deleteAfter, bool immediately)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            return _fileStorageService.DeleteItems("delete", itemList, false, deleteAfter, immediately).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Deletes all the files and folders from the "Trash" folder.
        /// </summary>
        /// <short>Empty the "Trash" folder</short>
        /// <category>Operations</category>
        /// <returns type="ASC.Api.Documents.FileOperationWraper, ASC.Api.Documents">List of file operations</returns>
        /// <path>api/2.0/files/fileops/emptytrash</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">File information</returns>
        /// <path>api/2.0/files/file/{fileId}/history</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("file/{fileId}/history")]
        public IEnumerable<FileWrapper> GetFileVersionInfo(string fileId)
        {
            var files = _fileStorageService.GetFileHistory(fileId);
            return files.Select(x => new FileWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Changes version history of a file with the ID specified in the request.
        /// </summary>
        /// <short>Change version history</short>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.Int32, System" name="version">History version</param>
        /// <param type="System.Boolean, System" name="continueVersion">Specifies whether to continue the current version and mark it as a revision or create a new one</param>
        /// <category>Files</category>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">File history</returns>
        /// <path>api/2.0/files/file/{fileId}/history</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <returns type="ASC.Api.Documents.FileShareWrapper, ASC.Api.Documents">Shared file information</returns>
        /// <path>api/2.0/files/file/{fileId}/share</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <category>Sharing</category>
        /// <returns type="ASC.Api.Documents.FileShareWrapper, ASC.Api.Documents">Shared folder information</returns>
        /// <path>api/2.0/files/folder/{folderId}/share</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("folder/{folderId}/share")]
        public IEnumerable<FileShareWrapper> GetFolderSecurityInfo(string folderId)
        {
            var fileShares = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { String.Format("folder_{0}", folderId) });
            return fileShares.Select(x => new FileShareWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Sets the sharing settings to a file with the ID specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Documents.FileShareParams}, System.Collections.Generic" file="ASC.Api.Documents" name="share">Collection of sharing parameters</param>
        /// <param type="System.Boolean, System" name="notify">Notifies users about the shared file or not</param>
        /// <param type="System.String, System" name="sharingMessage">Message to send when notifying about the shared file</param>
        /// <param type="ASC.Web.Files.Services.WCFService.AceAdvancedSettingsWrapper, ASC.Web.Files.Services.WCFService" name="advancedSettings">Advanced settings which prohibit printing, downloading, copying the file, and changing sharing settings</param>
        /// <short>Share a file</short>
        /// <category>Sharing</category>
        /// <remarks>
        /// Each of the sharing parameters must contain two values: "ShareTo" - ID of the user with whom we want to share a file, "Access" - access type which we want to give to the user (Read, ReadWrite, etc).
        /// </remarks>
        /// <returns type="ASC.Api.Documents.FileShareWrapper, ASC.Api.Documents">Shared file information</returns>
        /// <path>api/2.0/files/file/{fileId}/share</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update("file/{fileId}/share")]
        public IEnumerable<FileShareWrapper> SetFileSecurityInfo(string fileId, IEnumerable<FileShareParams> share, bool notify, string sharingMessage, AceAdvancedSettingsWrapper advancedSettings)
        {
            if (share != null && share.Any())
            {
                var list = new Web.Files.Services.WCFService.ItemList<AceWrapper>(share.Select(x => x.ToAceObject()));
                var aceCollection = new AceCollection
                {
                    Entries = new Web.Files.Services.WCFService.ItemList<string> { "file_" + fileId },
                    Aces = list,
                    Message = sharingMessage,
                    AdvancedSettings = advancedSettings
                };
                _fileStorageService.SetAceObject(aceCollection, notify);
            }
            return GetFileSecurityInfo(fileId);
        }

        /// <summary>
        /// Sets the sharing settings to a folder with the ID specified in the request.
        /// </summary>
        /// <short>Share a folder</short>
        /// <param type="System.String, System" method="url" name="folderId">Folder ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Documents.FileShareParams}, System.Collections.Generic" file="ASC.Api.Documents" name="share">Collection of sharing parameters</param>
        /// <param type="System.Boolean, System" name="notify">Notifies users about the shared folder or not</param>
        /// <param type="System.String, System" name="sharingMessage">Message to send when notifying about the shared folder</param>
        /// <remarks>
        /// Each of the sharing parameters must contain two values: "ShareTo" - ID of the user with whom we want to share a folder, "Access" - access type which we want to give to the user (Read, ReadWrite, etc). 
        /// </remarks>
        /// <category>Sharing</category>
        /// <returns type="ASC.Api.Documents.FileShareWrapper, ASC.Api.Documents">Shared folder information</returns>
        /// <path>api/2.0/files/folder/{folderId}/share</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// Removes the sharing rights for the group of folders and files with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="folderIds">List of folder IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <short>Remove sharing rights</short>
        /// <category>Sharing</category>
        /// <returns>Bool value: true if the operation is successful</returns>
        /// <path>api/2.0/files/share</path>
        /// <httpMethod>DELETE</httpMethod>
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
        /// <param type="System.String, System" method="url" name="fileId">File ID</param>
        /// <param type="ASC.Files.Core.Security.FileShare, ASC.Files.Core.Security" name="share">Sharing rights</param>
        /// <category>Sharing</category>
        /// <returns>Shared file link</returns>
        /// <path>api/2.0/files/{fileId}/sharedlink</path>
        /// <httpMethod>PUT</httpMethod>
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
        /// Sets a cookie after verifying the password for a password-protected external link and returns a link to the shared file.
        /// </summary>
        /// <short>Set a cookie for a password-protected external link</short>
        /// <param type="System.String, System" name="key">Link signature</param>
        /// <param type="System.String, System" name="passwordHash">Password hash</param>
        /// <param type="System.Boolean, System" name="isFolder">Specifies if a link is to the shared folder or not</param>
        /// <category>Sharing</category>
        /// <returns>Shared file link</returns>
        /// <path>api/2.0/files/sharedlink/password</path>
        /// <httpMethod>POST</httpMethod>
        /// <requiresAuthorization>false</requiresAuthorization>
        [Create("sharedlink/password", false)] // NOTE: This method doesn't require auth!!!
        public string ApplyShareLinkPassword(string key, string passwordHash, bool isFolder)
        {
            Tuple<Files.Core.File, Files.Core.Security.FileShareRecord> fileData = null;
            Tuple<Files.Core.Folder, Files.Core.Security.FileShareRecord> folderData = null;

            if (!isFolder)
            {
                fileData = _fileStorageService.ParseFileShareLinkKey(key);
            }
            else
            {
                folderData = _fileStorageService.ParseFolderShareLinkKey(key);
            }

            var record = !isFolder ? fileData.Item2 : folderData.Item2;

            if (fileData != null && Web.Files.Utils.FileShareLink.CheckCookieOrPasswordKey(record, null, out string _))
            {
                return Web.Files.Utils.FileShareLink.GetLink(fileData.Item1, true, record.Subject);
            }
            else if (folderData != null && Web.Files.Utils.FileShareLink.CheckCookieOrPasswordKey(record, null, out _))
            {
                return Web.Files.Utils.FileShareLink.GetLink(folderData.Item1, record.Subject);
            }

            var requestIp = MessageSettings.GetFullIPAddress(HttpContext.Current.Request);

            var bruteForceLoginManager = new BruteForceLoginManager(_cache, record.Subject.ToString(), requestIp);

            if (!bruteForceLoginManager.Increment(out bool _))
            {
                throw new Exception(Web.Files.Resources.FilesCommonResource.ErrorMassage_ShareLinkPasswordBruteForce);
            }

            if (PasswordHasher.GetClientPassword(record.Options.Password) != passwordHash)
            {
                throw new ArgumentException(Web.Files.Resources.FilesCommonResource.ErrorMassage_ShareLinkPassword);
            }

            Web.Files.Utils.FileShareLink.SetCookieKey(record);

            bruteForceLoginManager.Decrement();

            return !isFolder ? Web.Files.Utils.FileShareLink.GetLink(fileData.Item1, true, record.Subject) :
                Web.Files.Utils.FileShareLink.GetLink(folderData.Item1, record.Subject);
        }

        /// <summary>
        /// Returns a token after verifying the password or password hash for a password-protected external link.
        /// </summary>
        /// <short>Get a token for a password-protected external link</short>
        /// <param type="System.String, System" name="fileId">File ID</param>
        /// <param type="System.Guid, System" name="linkId">Link ID</param>
        /// <param type="System.String, System" name="password">Password</param>
        /// <param type="System.String, System" name="passwordHash">Password hash</param>
        /// <category>Sharing</category>
        /// <returns>Token for a password-protected external link</returns>
        /// <remarks>The token is used in the cookies with the 'sharelink[linkId]' name when calling API methods.</remarks>
        /// <path>api/2.0/files/{fileId}/sharedlink/{linkId}/password</path>
        /// <httpMethod>POST</httpMethod>
        [Create("{fileId}/sharedlink/{linkId}/password")]
        public AuthenticationTokenData GetTokenForSharedLink(string fileId, Guid linkId, string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(fileId) ||
                linkId == FileConstant.ShareLinkId || linkId == Guid.Empty ||
                (string.IsNullOrEmpty(password) && string.IsNullOrEmpty(passwordHash)))
            {
                throw new ArgumentException();
            }

            var record = _fileStorageService.GetFileShareLink(fileId, linkId).Item2;

            if (record.Options == null || string.IsNullOrEmpty(record.Options.Password))
            {
                return new AuthenticationTokenData();
            }

            var requestIp = MessageSettings.GetFullIPAddress(HttpContext.Current.Request);

            var bruteForceLoginManager = new BruteForceLoginManager(_cache, record.Subject.ToString(), requestIp);

            if (!bruteForceLoginManager.Increment(out bool _))
            {
                throw new Exception(Web.Files.Resources.FilesCommonResource.ErrorMassage_ShareLinkPasswordBruteForce);
            }

            if (string.IsNullOrEmpty(password))
            {
                var hash = PasswordHasher.GetClientPassword(record.Options.Password);
                if (hash != passwordHash)
                {
                    throw new ArgumentException(Web.Files.Resources.FilesCommonResource.ErrorMassage_ShareLinkPassword);
                }
            }
            else
            {
                if (record.Options.Password != password)
                {
                    throw new ArgumentException(Web.Files.Resources.FilesCommonResource.ErrorMassage_ShareLinkPassword);
                }
            }

            bruteForceLoginManager.Decrement();

            return new AuthenticationTokenData()
            {
                Token = record.Options.GetPasswordKey()
            };
        }

        /// <summary>
        /// Returns a new unsaved link object to the file with the ID specified in the request.
        /// </summary>
        /// <short>Get the shared link template</short>
        /// <param type="System.String, System" name="fileId">File ID</param>
        /// <param type="System.Boolean, System" name="isFolder">Specifies if a link is to the shared folder or not</param>
        /// <category>Sharing</category>
        /// <returns>Shared link template</returns>
        /// <path>api/2.0/files/{fileId}/sharedlink/template</path>
        /// <httpMethod>GET</httpMethod>
        [Read("{fileId}/sharedlink/template")]
        public AceWrapper GetShareLinkTemplate(string fileId, bool isFolder)
        {
            Files.Core.File file = null;
            Folder folder = null;
            
            if (!isFolder) file = _fileStorageService.GetFile(fileId, -1).NotFoundIfNull("File not found");
            else folder = _fileStorageService.GetFolder(fileId).NotFoundIfNull("File not found");

            var subject = Guid.NewGuid();

            var aceWrapper = new AceWrapper()
            {
                SubjectId = subject,
                SubjectGroup = true,
                SubjectName = Web.Files.Resources.FilesJSResource.TitleNewSharedLink,
                Link = !isFolder ? Web.Files.Utils.FileShareLink.GetLink(file, true, subject) : Web.Files.Utils.FileShareLink.GetLink(folder, subject),
                LinkSettings = new LinkSettingsWrapper(),
                EntryType = !isFolder ? FileEntryType.File : FileEntryType.Folder,
            };

            return aceWrapper;
        }

        /// <summary>
        /// Returns file properties of the specified file.
        /// </summary>
        /// <short>Get file properties</short>
        /// <param type="System.String, System" name="fileId">File ID</param>
        /// <category>Files</category>
        /// <returns>File properties</returns>
        /// <path>api/2.0/files/{fileId}/properties</path>
        /// <httpMethod>GET</httpMethod>
        [Read("{fileId}/properties")]
        public EntryProperties GetProperties(string fileId)
        {
            return _fileStorageService.GetFileProperties(fileId);
        }

        /// <summary>
        /// Saves file properties to the specified file.
        /// </summary>
        /// <short>Save file properties to a file</short>
        /// <param type="System.String, System" name="fileId">File ID</param>
        /// <param type="ASC.Files.Core.EntryProperties, ASC.Files.Core" name="fileProperties">File properties</param>
        /// <category>Files</category>
        /// <returns>File properties</returns>
        /// <path>api/2.0/files/{fileId}/properties</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("{fileId}/properties")]
        public EntryProperties SetProperties(string fileId, EntryProperties fileProperties)
        {
            return _fileStorageService.SetFileProperties(fileId, fileProperties);
        }

        /// <summary>
        /// Saves file properties to the specified files.
        /// </summary>
        /// <short>Save file properties to files</short>
        /// <param type="System.String[], System" name="filesId">IDs of files</param>
        /// <param type="System.Boolean, System" name="createSubfolder">Creates a subfolder or not</param>
        /// <param type="ASC.Files.Core.EntryProperties, ASC.Files.Core" name="fileProperties">File properties</param>
        /// <category>Files</category>
        /// <returns>List of file properties</returns>
        /// <path>api/2.0/files/batch/properties</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// <remarks>List of provider keys: DropboxV2, Box, WebDav, Yandex, OneDrive, SharePoint, GoogleDrive, kDrive.</remarks>
        /// <path>api/2.0/files/thirdparty/capabilities</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
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
        /// Saves the third-party storage service account. For WebDav, Yandex, kDrive and SharePoint, the login and password are used for authentication. For other providers, the authentication is performed using a token received via OAuth 2.0.
        /// </summary>
        /// <short>Save a third-party account</short>
        /// <param type="System.String, System" name="url">Connection URL for the sharepoint</param>
        /// <param type="System.String, System" name="login">Login</param>
        /// <param type="System.String, System" name="password">Password</param>
        /// <param type="System.String, System" name="token">Authentication token</param>
        /// <param type="System.Boolean, System" name="isCorporate">Specifies if this is a corporate account or not</param>
        /// <param type="System.String, System" name="customerTitle">Customer title</param>
        /// <param type="System.String, System" name="providerKey">Provider key</param>
        /// <param type="System.String, System" name="providerId">Provider ID</param>
        /// <category>Third-party integration</category>
        /// <returns type="ASC.Api.Documents.FolderWrapper, ASC.Api.Documents">Folder contents</returns>
        /// <remarks>List of provider keys: DropboxV2, Box, WebDav, Yandex, OneDrive, SharePoint, GoogleDrive, kDrive.</remarks>
        /// <exception cref="ArgumentException"></exception>
        /// <path>api/2.0/files/thirdparty</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <returns type="ASC.Web.Files.Services.WCFService.ThirdPartyParams, ASC.Web.Files">Connected providers</returns>
        /// <path>api/2.0/files/thirdparty</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("thirdparty")]
        public IEnumerable<ThirdPartyParams> GetThirdPartyAccounts()
        {
            return _fileStorageService.GetThirdParty();
        }

        /// <summary>
        /// Returns a list of the third-party services connected to the "Common" section.
        /// </summary>
        /// <category>Third-party integration</category>
        /// <short>Get common third-party services</short>
        /// <returns type="ASC.Files.Core.Folder, ASC.Web.Files">Common third-party folders</returns>
        /// <path>api/2.0/files/thirdparty/common</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("thirdparty/common")]
        public IEnumerable<Folder> GetCommonThirdPartyFolders()
        {
            var parent = _fileStorageService.GetFolder(Global.FolderCommon.ToString());
            return EntryManager.GetThirpartyFolders(parent);
        }

        /// <summary>
        /// Removes the third-party storage service account with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="providerId">Provider ID. It is a part of the folder ID. Example: folder ID is "sbox-123", then provider ID is "123".</param>
        /// <short>Remove a third-party account</short>
        /// <category>Third-party integration</category>
        /// <returns>Deleted third-party account</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<path>api/2.0/files/thirdparty/{providerId}</path>
        ///<httpMethod>DELETE</httpMethod>
        [Delete("thirdparty/{providerId:[0-9]+}")]
        public object DeleteThirdParty(int providerId)
        {
            return _fileStorageService.DeleteThirdParty(providerId.ToString(CultureInfo.InvariantCulture));

        }

        /// <summary>
        /// Searches for files and folders by the query specified in the request.
        /// </summary>
        /// <short>Search for files and folders</short>
        /// <category>Operations</category>
        /// <param type="System.String, System" method="url" name="query">Query string</param>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">Files and folders</returns>
        /// <path>api/2.0/files/@search/{query}</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
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
        /// <category>Operations</category>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="folderIds" visible="false">List of folder IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <returns>Bool value: true if the operation is successful</returns>
        /// <path>api/2.0/files/favorites</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <category>Operations</category>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="folderIds" visible="false">List of folder IDs</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <returns>Bool value: true if the operation is successful</returns>
        /// <path>api/2.0/files/favorites</path>
        /// <httpMethod>DELETE</httpMethod>
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
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <returns>Bool value: true if the operation is successful</returns>
        /// <path>api/2.0/files/templates</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <returns>Bool value: true if the operation is successful</returns>
        /// <path>api/2.0/files/templates</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("templates")]
        public bool DeleteTemplates(IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.DeleteTemplates(new FilesNS.ItemList<object>(fileIds));
            return true;
        }


        /// <summary>
        /// Stores files in the original formats as well when uploading and converting.
        /// </summary>
        /// <short>Store original formats</short>
        /// <param type="System.Boolean, System" name="set">Turns the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        /// <path>api/2.0/files/storeoriginal</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"storeoriginal")]
        public bool StoreOriginal(bool set)
        {
            return _fileStorageService.StoreOriginal(set);
        }

        /// <summary>
        /// Hides the confirmation dialog for saving the file copy in the original format when converting a file.
        /// </summary>
        /// <short>Hide the confirmation dialog when converting</short>
        /// <param type="System.Boolean, System" name="save">Specifies whether to save the file in the original format or not</param>
        /// <category>Settings</category>
        /// <visible>false</visible>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        /// <path>api/2.0/files/hideconfirmconvert</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"hideconfirmconvert")]
        public bool HideConfirmConvert(bool save)
        {
            return _fileStorageService.HideConfirmConvert(save);
        }

        /// <summary>
        /// Updates a file version if a file with such a name already exists.
        /// </summary>
        /// <short>Update a file version if it exists</short>
        /// <param type="System.Boolean, System" name="set">Turns the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        /// <path>api/2.0/files/updateifexist</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"updateifexist")]
        public bool UpdateIfExist(bool set)
        {
            return _fileStorageService.UpdateIfExist(set);
        }

        /// <summary>
        /// Displays the "Recent" folder.
        /// </summary>
        /// <short>Display the "Recent" folder</short>
        /// <param type="System.Boolean, System" name="set">Turns the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        /// <path>api/2.0/files/displayRecent</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"displayRecent")]
        public bool DisplayRecent(bool set)
        {
            return _fileStorageService.DisplayRecent(set);
        }

        /// <summary>
        /// Displays the "Favorites" folder.
        /// </summary>
        /// <short>Display the "Favorites" folder</short>
        /// <param type="System.Boolean, System" name="set">Turns the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        /// <path>api/2.0/files/settings/favorites</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"settings/favorites")]
        public bool DisplayFavorite(bool set)
        {
            return _fileStorageService.DisplayFavorite(set);
        }

        /// <summary>
        /// Displays the "Templates" folder.
        /// </summary>
        /// <short>Display the "Templates" folder</short>
        /// <param type="System.Boolean, System" name="set">Turns the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Bool value: true if the parameter is enabled</returns>
        /// <path>api/2.0/files/settings/templates</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"settings/templates")]
        public bool DisplayTemplates(bool set)
        {
            return _fileStorageService.DisplayTemplates(set);
        }

        /// <summary>
        /// Updates the trash bin auto-clearing setting.
        /// </summary>
        /// <short>Update the trash bin auto-clearing setting</short>
        /// <param type="System.Boolean, System" name="set">Enables the auto-clearing or not</param>
        /// <param type="ASC.Files.Core.DateToAutoCleanUp, ASC.Files.Core" name="gap">A time interval when the auto-clearing will be performed (one week, two weeks, one month, two months, three months)</param>
        /// <category>Settings</category>
        /// <returns>The auto-clearing setting properties</returns>
        /// <path>api/2.0/files/settings/autocleanup</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"settings/autocleanup")]
        public AutoCleanUpData ChangeAutomaticallyCleanUp(bool set, DateToAutoCleanUp gap)
        {
            return _fileStorageService.ChangeAutomaticallyCleanUp(set, gap);
        }

        /// <summary>
        /// Returns the auto-clearing setting properties.
        /// </summary>
        /// <short>Get the auto-clearing setting properties</short>
        /// <category>Settings</category>
        /// <returns>The auto-clearing setting properties</returns>
        /// <path>api/2.0/files/settings/autocleanup</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"settings/autocleanup")]
        public AutoCleanUpData GetSettingsAutomaticallyCleanUp()
        {
            return _fileStorageService.GetSettingsAutomaticallyCleanUp();
        }

        /// <summary>
        /// Changes the default access rights in the sharing settings.
        /// </summary>
        /// <short>Change the default access rights</short>
        /// <param type="System.Collections.Generic.List{ASC.Files.Core.Security.FileShare}, System.Collections.Generic" name="value">Default access rights</param>
        /// <category>Settings</category>
        /// <returns>Default access rights</returns>
        /// <path>api/2.0/files/settings/dafaultaccessrights</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"settings/dafaultaccessrights")]
        public List<FileShare> ChangeDafaultAccessRights(List<FileShare> value)
        {
            return _fileStorageService.ChangeDafaultAccessRights(value);
        }

        /// <summary>
        /// Changes the format of the downloaded archive from .zip to .tar.gz.
        /// </summary>
        /// <short>Change the archive format</short>
        /// <param type="System.Boolean, System" name="set">Turns the parameter on or off</param>
        /// <category>Settings</category>
        /// <returns>Archive</returns>
        /// <path>api/2.0/files/settings/downloadtargz</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"settings/downloadtargz")]
        public ICompress ChangeDownloadZip(bool set)
        {
            return _fileStorageService.ChangeDownloadTarGz(set);
        }

        /// <summary>
        /// Checks the document service location.
        /// </summary>
        /// <short>Check the document service URL</short>
        /// <param type="System.String, System" name="docServiceUrl">The address of Document Server</param>
        /// <param type="System.String, System" name="docServiceUrlInternal">The address of Document Server in the local private network</param>
        /// <param type="System.String, System" name="docServiceUrlPortal">The address of Community Server</param>
        /// <category>Settings</category>
        /// <returns>Document service information</returns>
        /// <path>api/2.0/files/docservice</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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
        /// <param type="System.Boolean, System" name="version" visible="false">Specifies the editor version or not</param>
        /// <returns>Address</returns>
        /// <path>api/2.0/files/docservice</path>
        /// <httpMethod>GET</httpMethod>
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
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="fileIds">List of file IDs</param>
        /// <visible>false</visible>
        /// <returns>List of file IDs</returns>
        /// <path>api/2.0/files/thumbnails</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        [Create("thumbnails", false)] // NOTE: This method doesn't require auth!!!
        public IEnumerable<String> CreateThumbnails(IEnumerable<String> fileIds)
        {
            try
            {
                var files = _fileStorageService.GetFilterReadFiles(fileIds);

                fileIds = files.Select(f => f.ID.ToString());

                if (!fileIds.Any())
                {
                    return fileIds;
                }

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


        private FolderContentWrapper ToFolderContentWrapper(object folderId, Guid userIdOrGroupId, FilterType filterType, bool searchInContent, string extension, bool withSubfolders)
        {
            if (folderId == null)
            {
                throw new ItemNotFoundException(Web.Files.Resources.FilesCommonResource.ErrorMassage_FolderNotFound);
            }

            var subjectGroup = false;
            var groupInfo = CoreContext.UserManager.GetGroupInfo(userIdOrGroupId);
            if(groupInfo.ID != Core.Users.Constants.LostGroupInfo.ID)
            {
                subjectGroup = true;
            }
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
                                                                               subjectGroup,
                                                                               userIdOrGroupId.ToString(),
                                                                               _context.FilterValue,
                                                                               searchInContent,
                                                                               extension,
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
        /// <path>api/2.0/files/wordpress-info</path>
        /// <httpMethod>GET</httpMethod>
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
        /// <path>api/2.0/files/wordpress-delete</path>
        /// <httpMethod>GET</httpMethod>
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
        /// Saves the user WordPress information when logging in.
        /// </summary>
        /// <short>Save the user WordPress information</short>
        /// <param type="System.String, System" name="code">Authorization code</param>
        /// <category>WordPress</category>
        /// <returns>User WordPress information</returns>
        /// <path>api/2.0/files/wordpress-save</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <param type="System.String, System" name="code">Authorization code</param>
        /// <param type="System.String, System" name="title">Post title</param>
        /// <param type="System.String, System" name="content">Post content</param>
        /// <param type="System.Int32, System" name="status">Operation status</param>
        /// <category>WordPress</category>
        /// <returns>Boolean value: true if the operation is successful</returns>
        /// <path>api/2.0/files/wordpress</path>
        /// <httpMethod>POST</httpMethod>
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
        /// <param type="System.Int32, System" name="source">Citation source: book (0), journal (1) or website (2)</param>
        /// <param type="System.String, System" name="data">Citation data</param>
        /// <category>EasyBib</category>
        /// <returns>EasyBib citation list</returns>
        /// <path>api/2.0/files/easybib-citation-list</path>
        /// <httpMethod>GET</httpMethod>
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
        /// <path>api/2.0/files/easybib-styles</path>
        /// <httpMethod>GET</httpMethod>
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
        /// <short>Get the EasyBib citation book</short>
        /// <param type="System.String, System" name="citationData">Citation data</param>
        /// <category>EasyBib</category>
        /// <returns>EasyBib citation</returns>
        /// <path>api/2.0/files/easybib-citation</path>
        /// <httpMethod>POST</httpMethod>
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
            /// <example name="id">d5490cba-a5e6-40db-acb2-94203dba12d6</example>
            [DataMember(Name = "id")]
            public string Id { get; set; }

            /// <summary>
            /// Operation type.
            /// </summary>
            /// <example type="int" name="operation">6</example>
            [DataMember(Name = "operation")]
            public FileOperationType OperationType { get; set; }

            /// <summary>
            /// Operation progress.
            /// </summary>
            /// <example type="int" name="progress">30</example>
            [DataMember(Name = "progress")]
            public int Progress { get; set; }

            /// <summary>
            /// Source files for operation.
            /// </summary>
            /// <example name="source">source</example>
            [DataMember(Name = "source")]
            public string Source { get; set; }

            /// <summary>
            /// Result file of operation.
            /// </summary>
            /// <type name="result">ASC.Api.Documents.FileWrapper, ASC.Api.Documents</type>
            [DataMember(Name = "result")]
            public FileWrapper File { get; set; }

            /// <summary>
            /// Error during conversion.
            /// </summary>
            /// <example name="error"></example>
            [DataMember(Name = "error")]
            public string Error { get; set; }

            /// <summary>
            /// Is operation processed.
            /// </summary>
            /// <example name="processed">1</example>
            [DataMember(Name = "processed")]
            public string Processed { get; set; }
        }
    }
}