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
using ASC.Web.Files.Helpers;
using ASC.Web.Files.HttpHandlers;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using FileShare = ASC.Files.Core.Security.FileShare;
using FilesNS = ASC.Web.Files.Services.WCFService;
using MimeMapping = ASC.Common.Web.MimeMapping;
using SortedByType = ASC.Files.Core.SortedByType;

namespace ASC.Api.Documents
{
    /// <summary>
    /// Provides access to documents
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
        /// Returns the detailed list of files and folders located in the current user My section
        /// </summary>
        /// <short>Section My</short>
        /// <category>Folders</category>
        /// <returns>My folder contents</returns>
        [Read("@my")]
        public FolderContentWrapper GetMyFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderMy, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the current user Projects section
        /// </summary>
        /// <short>Section Projects</short>
        /// <category>Folders</category>
        /// <returns>Projects folder contents</returns>
        [Read("@projects")]
        public FolderContentWrapper GetProjectsFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderProjects, userIdOrGroupId, filterType);
        }


        /// <summary>
        /// Returns the detailed list of files and folders located in the Common section
        /// </summary>
        /// <short>Section Common</short>
        /// <category>Folders</category>
        /// <returns>Common folder contents</returns>
        [Read("@common")]
        public FolderContentWrapper GetCommonFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderCommon, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the Shared with Me section
        /// </summary>
        /// <short>Section Shared</short>
        /// <category>Folders</category>
        /// <returns>Shared folder contents</returns>
        [Read("@share")]
        public FolderContentWrapper GetShareFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderShare, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of recent files
        /// </summary>
        /// <short>Section Recent</short>
        /// <category>Folders</category>
        /// <returns>Recent contents</returns>
        [Read("@recent")]
        public FolderContentWrapper GetRecentFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderRecent, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of favorites files
        /// </summary>
        /// <short>Section Favorite</short>
        /// <category>Folders</category>
        /// <returns>Favorites contents</returns>
        [Read("@favorites")]
        public FolderContentWrapper GetFavoritesFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderFavorites, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of templates files
        /// </summary>
        /// <short>Section Template</short>
        /// <category>Folders</category>
        /// <returns>Templates contents</returns>
        [Read("@templates")]
        public FolderContentWrapper GetTemplatesFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderTemplates, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the Recycle Bin
        /// </summary>
        /// <short>Section Trash</short>
        /// <category>Folders</category>
        /// <returns>Trash folder contents</returns>
        [Read("@trash")]
        public FolderContentWrapper GetTrashFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderTrash, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the folder with the ID specified in the request
        /// </summary>
        /// <short>
        /// Folder by ID
        /// </short>
        /// <category>Folders</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true" remark="Allowed values: None (0), FilesOnly (1), FoldersOnly (2), DocumentsOnly (3), PresentationsOnly (4), SpreadsheetsOnly (5) or ImagesOnly (7)">Filter type</param>
        /// <returns>Folder contents</returns>
        [Read("{folderId}")]
        public FolderContentWrapper GetFolder(String folderId, Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(folderId, userIdOrGroupId, filterType).NotFoundIfNull();

        }

        /// <summary>
        /// Uploads the file specified with single file upload or standart multipart/form-data method to My section
        /// </summary>
        /// <short>Upload to My</short>
        /// <category>Uploads</category>
        /// <remarks>
        /// <![CDATA[
        ///  Upload can be done in 2 different ways:
        ///  <ol>
        /// <li>Single file upload. You should set Content-Type &amp; Content-Disposition header to specify filename and content type, and send file in request body</li>
        /// <li>Using standart multipart/form-data method</li>
        /// </ol>]]>
        /// </remarks>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="contentType" visible="false">Content-Type Header</param>
        /// <param name="contentDisposition" visible="false">Content-Disposition Header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <returns>Uploaded file</returns>
        [Create("@my/upload")]
        public object UploadFileToMy(Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<HttpPostedFileBase> files)
        {
            return UploadFile(Global.FolderMy.ToString(), file, contentType, contentDisposition, files, false, null);
        }

        /// <summary>
        /// Uploads the file specified with single file upload or standart multipart/form-data method to Common section
        /// </summary>
        /// <short>Upload to Common</short>
        /// <category>Uploads</category>
        /// <remarks>
        /// <![CDATA[
        ///  Upload can be done in 2 different ways:
        ///  <ol>
        /// <li>Single file upload. You should set Content-Type &amp; Content-Disposition header to specify filename and content type, and send file in request body</li>
        /// <li>Using standart multipart/form-data method</li>
        /// </ol>]]>
        /// </remarks>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="contentType" visible="false">Content-Type Header</param>
        /// <param name="contentDisposition" visible="false">Content-Disposition Header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <returns>Uploaded file</returns>
        [Create("@common/upload")]
        public object UploadFileToCommon(Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<HttpPostedFileBase> files)
        {
            return UploadFile(Global.FolderCommon.ToString(), file, contentType, contentDisposition, files, false, null);
        }


        /// <summary>
        /// Uploads the file specified with single file upload or standart multipart/form-data method to the selected folder
        /// </summary>
        /// <short>Upload file</short>
        /// <category>Uploads</category>
        /// <remarks>
        /// <![CDATA[
        ///  Upload can be done in 2 different ways:
        ///  <ol>
        /// <li>Single file upload. You should set Content-Type &amp; Content-Disposition header to specify filename and content type, and send file in request body</li>
        /// <li>Using standart multipart/form-data method</li>
        /// </ol>]]>
        /// </remarks>
        /// <param name="folderId">Folder ID to upload to</param>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="contentType" visible="false">Content-Type Header</param>
        /// <param name="contentDisposition" visible="false">Content-Disposition Header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <param name="createNewIfExist" visible="false">Create New If Exist</param>
        /// <param name="storeOriginalFileFlag" visible="false">If True, upload documents in original formats as well</param>
        /// <param name="keepConvertStatus" visible="false">Keep status conversation after finishing</param>
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
        /// Uploads the file specified with single file upload to Common section
        /// </summary>
        /// <short>Insert to My</short>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="title">Name of file which has to be uploaded</param>
        /// <param name="createNewIfExist" visible="false">Create New If Exist</param>
        /// <param name="keepConvertStatus" visible="false">Keep status conversation after finishing</param>
        /// <category>Uploads</category>
        /// <returns></returns>
        [Create("@my/insert")]
        public FileWrapper InsertFileToMy(Stream file, string title, bool? createNewIfExist, bool keepConvertStatus = false)
        {
            return InsertFile(Global.FolderMy.ToString(), file, title, createNewIfExist, keepConvertStatus);
        }

        /// <summary>
        /// Uploads the file specified with single file upload to Common section
        /// </summary>
        /// <short>Insert to Common</short>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="title">Name of file which has to be uploaded</param>
        /// <param name="createNewIfExist" visible="false">Create New If Exist</param>
        /// <param name="keepConvertStatus" visible="false">Keep status conversation after finishing</param>
        /// <category>Uploads</category>
        /// <returns></returns>
        [Create("@common/insert")]
        public FileWrapper InsertFileToCommon(Stream file, string title, bool? createNewIfExist, bool keepConvertStatus = false)
        {
            return InsertFile(Global.FolderCommon.ToString(), file, title, createNewIfExist, keepConvertStatus);
        }

        /// <summary>
        /// Uploads the file specified with single file upload
        /// </summary>
        /// <param name="folderId">Folder ID to upload to</param>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="title">Name of file which has to be uploaded</param>
        /// <param name="createNewIfExist" visible="false">Create New If Exist</param>
        /// <param name="keepConvertStatus" visible="false">Keep status conversation after finishing</param>
        /// <category>Uploads</category>
        /// <returns></returns>
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
        /// Update file content
        /// </summary>
        /// <category>Files</category>
        /// <param name="file">Stream of file</param>
        /// <param name="fileId">File ID</param>
        /// <param name="encrypted" visible="false"></param>
        /// <param name="forcesave" visible="false"></param>
        [Update("{fileId}/update")]
        public FileWrapper UpdateFileStream(Stream file, string fileId, bool encrypted = false, bool forcesave = false)
        {
            try
            {
                var resultFile = _fileStorageService.UpdateFileStream(fileId, file, encrypted, forcesave);
                return new FileWrapper(resultFile);
            }
            catch (FileNotFoundException e)
            {
                throw new ItemNotFoundException("File not found", e);
            }
        }


        /// <summary>
        /// Save file 
        /// </summary>
        /// <short>Editing save</short>
        /// <param name="fileId">File ID</param>
        /// <param name="fileExtension"></param>
        /// <param name="downloadUri"></param>
        /// <param name="stream"></param>
        /// <param name="doc"></param>
        /// <param name="forcesave"></param>
        /// <category>Files</category>
        /// <returns></returns>
        [Update("file/{fileId}/saveediting")]
        public FileWrapper SaveEditing(String fileId, string fileExtension, string downloadUri, Stream stream, String doc, bool forcesave)
        {
            return new FileWrapper(_fileStorageService.SaveEditing(fileId, fileExtension, downloadUri, stream, doc, forcesave));
        }

        /// <summary>
        /// Lock file when editing
        /// </summary>
        /// <short>Editing start</short>
        /// <param name="fileId">File ID</param>
        /// <param name="editingAlone" visible="false"></param>
        /// <param name="doc" visible="false"></param>
        /// <category>Files</category>
        /// <returns>File key for Document Service</returns>
        [Create("file/{fileId}/startedit")]
        public string StartEdit(String fileId, bool editingAlone, String doc)
        {
            return _fileStorageService.StartEdit(fileId, editingAlone, doc);
        }

        /// <summary>
        /// Continue to lock file when editing
        /// </summary>
        /// <short>Editing track</short>
        /// <param name="fileId">File ID</param>
        /// <param name="tabId" visible="false"></param>
        /// <param name="docKeyForTrack" visible="false"></param>
        /// <param name="doc" visible="false"></param>
        /// <param name="isFinish">for unlock</param>
        /// <category>Files</category>
        /// <returns></returns>
        [Read("file/{fileId}/trackeditfile")]
        public KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String doc, bool isFinish)
        {
            return _fileStorageService.TrackEditFile(fileId, tabId, docKeyForTrack, doc, isFinish);
        }

        /// <summary>
        /// Get initialization configuration for open editor
        /// </summary>
        /// <short>Editing open</short>
        /// <param name="fileId">File ID</param>
        /// <param name="version">File version</param>
        /// <param name="doc" visible="false"></param>
        /// <category>Files</category>
        /// <returns>Configuration</returns>
        [Read("file/{fileId}/openedit")]
        public Configuration OpenEdit(String fileId, int version, String doc)
        {
            Configuration configuration;
            DocumentServiceHelper.GetParams(fileId, version, doc, true, true, true, out configuration);
            configuration.Type = Configuration.EditorType.External;

            configuration.Token = DocumentServiceHelper.GetSignature(configuration);
            return configuration;
        }


        /// <summary>
        /// Creates session to upload large files in multiple chunks.
        /// </summary>
        /// <short>Chunked upload</short>
        /// <category>Uploads</category>
        /// <param name="folderId">Id of the folder in which file will be uploaded</param>
        /// <param name="fileName">Name of file which has to be uploaded</param>
        /// <param name="fileSize">Length in bytes of file which has to be uploaded</param>
        /// <param name="relativePath">Relative folder from folderId</param>
        /// <param name="encrypted" visible="false"></param>
        /// <remarks>
        /// <![CDATA[
        /// Each chunk can have different length but its important what length is multiple of <b>512</b> and greater or equal than <b>5 mb</b>. Last chunk can have any size.
        /// After initial request respond with status 200 OK you must obtain value of 'location' field from the response. Send all your chunks to that location.
        /// Each chunk must be sent in strict order in which chunks appears in file.
        /// After receiving each chunk if no errors occured server will respond with current information about upload session.
        /// When number of uploaded bytes equal to the number of bytes you send in initial request server will respond with 201 Created and will send you info about uploaded file.
        /// ]]>
        /// </remarks>
        /// <returns>
        /// <![CDATA[
        /// Information about created session. Which includes:
        /// <ul>
        /// <li><b>id:</b> unique id of this upload session</li>
        /// <li><b>created:</b> UTC time when session was created</li>
        /// <li><b>expired:</b> UTC time when session will be expired if no chunks will be sent until that time</li>
        /// <li><b>location:</b> URL to which you must send your next chunk</li>
        /// <li><b>bytes_uploaded:</b> If exists contains number of bytes uploaded for specific upload id</li>
        /// <li><b>bytes_total:</b> Number of bytes which has to be uploaded</li>
        /// </ul>
        /// ]]>
        /// </returns>
        [Create("{folderId}/upload/create_session")]
        public object CreateUploadSession(string folderId, string fileName, long fileSize, string relativePath, bool encrypted)
        {
            var file = FileUploader.VerifyChunkedUpload(folderId, fileName, fileSize, FilesSettings.UpdateIfExist, relativePath);

            if (FilesLinkUtility.IsLocalFileUploader)
            {
                var session = FileUploader.InitiateUpload(file.FolderID.ToString(), (file.ID ?? "").ToString(), file.Title, file.ContentLength, encrypted);

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
        /// Creates a text (.txt) file in My section with the title and contents sent in the request
        /// </summary>
        /// <short>Create txt in My</short>
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
        /// Creates a text (.txt) file in Common Documents section with the title and contents sent in the request
        /// </summary>
        /// <short>Create txt in Common</short>
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
        /// Creates a text (.txt) file in the selected folder with the title and contents sent in the request
        /// </summary>
        /// <short>Create txt</short>
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
        /// Creates an html (.html) file in the selected folder with the title and contents sent in the request
        /// </summary>
        /// <short>Create html</short>
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
        /// Creates an html (.html) file in My section with the title and contents sent in the request
        /// </summary>
        /// <short>Create html in My</short>
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
        /// Creates an html (.html) file in Common section with the title and contents sent in the request
        /// </summary>
        /// <short>Create html in Common</short>
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
        /// Creates a new folder with the title sent in the request. The ID of a parent folder can be also specified.
        /// </summary>
        /// <short>
        /// Create folder
        /// </short>
        /// <category>Folders</category>
        /// <param name="folderId">Parent folder ID</param>
        /// <param name="title">Title of new folder</param>
        /// <returns>New folder contents</returns>
        [Create("folder/{folderId}")]
        public FolderWrapper CreateFolder(string folderId, string title)
        {
            var folder = _fileStorageService.CreateNewFolder(folderId, title);
            return new FolderWrapper(folder);
        }

        /// <summary>
        /// Creates a new file in the My section with the title sent in the request
        /// </summary>
        /// <short>Create file in My</short>
        /// <category>Files</category>
        /// <param name="title" remark="Allowed values: the file must have one of the following extensions: DOCX, XLSX, PPTX">File title</param>
        /// <remarks>In case the extension for the file title differs from DOCX/XLSX/PPTX and belongs to one of the known text, spreadsheet or presentation formats, it will be changed to DOCX/XLSX/PPTX accordingly. If the file extension is not set or is unknown, the DOCX extension will be added to the file title.</remarks>
        /// <returns>New file info</returns>
        [Create("@my/file")]
        public FileWrapper CreateFile(string title)
        {
            return CreateFile(Global.FolderMy.ToString(), title, null);
        }

        /// <summary>
        /// Creates a new file in the specified folder with the title sent in the request
        /// </summary>
        /// <short>Create file</short>
        /// <category>Files</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="title" remark="Allowed values: the file must have one of the following extensions: DOCX, XLSX, PPTX">File title</param>
        /// <param name="templateId">File ID for using as template</param>
        /// <remarks>In case the extension for the file title differs from DOCX/XLSX/PPTX and belongs to one of the known text, spreadsheet or presentation formats, it will be changed to DOCX/XLSX/PPTX accordingly. If the file extension is not set or is unknown, the DOCX extension will be added to the file title.</remarks>
        /// <returns>New file info</returns>
        [Create("{folderId}/file")]
        public FileWrapper CreateFile(string folderId, string title, string templateId)
        {
            var file = _fileStorageService.CreateNewFile(folderId, title, templateId);
            return new FileWrapper(file);
        }

        /// <summary>
        /// Renames the selected folder to the new title specified in the request
        /// </summary>
        /// <short>
        /// Rename folder
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
        /// Returns a detailed information about the folder with the ID specified in the request
        /// </summary>
        /// <short>Folder information</short>
        /// <category>Folders</category>
        /// <returns>Folder info</returns>
        [Read("folder/{folderId}")]
        public FolderWrapper GetFolderInfo(string folderId)
        {
            var folder = _fileStorageService.GetFolder(folderId).NotFoundIfNull("Folder not found");

            return new FolderWrapper(folder);
        }

        /// <summary>
        /// Returns parent folders
        /// </summary>
        /// <short>Folder path</short>
        /// <param name="folderId"></param>
        /// <category>Folders</category>
        /// <returns>Parent folders</returns>
        [Read("folder/{folderId}/path")]
        public IEnumerable<FolderWrapper> GetFolderPath(string folderId)
        {
            return EntryManager.GetBreadCrumbs(folderId).Select(f => new FolderWrapper(f)).ToSmartList();
        }

        /// <summary>
        /// Returns a detailed information about the file with the ID specified in the request
        /// </summary>
        /// <short>File information</short>
        /// <category>Files</category>
        /// <returns>File info</returns>
        [Read("file/{fileId}")]
        public FileWrapper GetFileInfo(string fileId, int version = -1)
        {
            var file = _fileStorageService.GetFile(fileId, version).NotFoundIfNull("File not found");
            return new FileWrapper(file);
        }

        /// <summary>
        ///     Updates the information of the selected file with the parameters specified in the request
        /// </summary>
        /// <short>Update file info</short>
        /// <category>Files</category>
        /// <param name="fileId">File ID</param>
        /// <param name="title">New title</param>
        /// <param name="lastVersion">File last version number</param>
        /// <returns>File info</returns>
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
        /// Deletes the file with the ID specified in the request
        /// </summary>
        /// <short>Delete file</short>
        /// <category>Operations</category>
        /// <param name="fileId">File ID</param>
        /// <param name="deleteAfter">Delete after finished</param>
        /// <param name="immediately">Don't move to the Recycle Bin</param>
        /// <returns>Operation result</returns>
        [Delete("file/{fileId}")]
        public IEnumerable<FileOperationWraper> DeleteFile(String fileId, bool deleteAfter, bool immediately)
        {
            return DeleteBatchItems(null, new[] { fileId }, deleteAfter, immediately);
        }

        /// <summary>
        ///  Start conversion operation
        /// </summary>
        /// <short>Convert start</short>
        /// <category>Operations</category>
        /// <param name="fileId"></param>
        /// <returns>Operation result</returns>
        [Update("file/{fileId}/checkconversion")]
        public IEnumerable<ConversationResult> StartConversion(String fileId)
        {
            return CheckConversion(fileId, true);
        }

        /// <summary>
        ///  Check conversion status
        /// </summary>
        /// <short>Convert status</short>
        /// <category>Operations</category>
        /// <param name="fileId"></param>
        /// <param name="start"></param>
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
        /// Deletes the folder with the ID specified in the request
        /// </summary>
        /// <short>Delete folder</short>
        /// <category>Operations</category>
        /// <param name="folderId">Folder ID</param>
        /// <param name="deleteAfter">Delete after finished</param>
        /// <param name="immediately">Don't move to the Recycle Bin</param>
        /// <returns>Operation result</returns>
        [Delete("folder/{folderId}")]
        public IEnumerable<FileOperationWraper> DeleteFolder(String folderId, bool deleteAfter, bool immediately)
        {
            return DeleteBatchItems(new[] { folderId }, null, deleteAfter, immediately);
        }

        /// <summary>
        /// Checking for conflicts
        /// </summary>
        /// <category>Operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileIds">File ID list</param>
        /// <returns>Conflicts file ids</returns>
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
        ///   Moves all the selected files and folders to the folder with the ID specified in the request
        /// </summary>
        /// <short>Move to folder</short>
        /// <category>Operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileIds">File ID list</param>
        /// <param name="conflictResolveType">Overwriting behavior: skip(0), overwrite(1) or duplicate(2)</param>
        /// <param name="deleteAfter">Delete after finished</param>
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
        ///   Copies all the selected files and folders to the folder with the ID specified in the request
        /// </summary>
        /// <short>Copy to folder</short>
        /// <category>Operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileIds">File ID list</param>
        /// <param name="conflictResolveType">Overwriting behavior: skip(0), overwrite(1) or duplicate(2)</param>
        /// <param name="deleteAfter">Delete after finished</param>
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
        ///   Marks all files and folders as read
        /// </summary>
        /// <short>Mark as read</short>
        /// <category>Operations</category>
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
        ///  Finishes all the active Operations
        /// </summary>
        /// <short>Finish all</short>
        /// <category>Operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/terminate")]
        public IEnumerable<FileOperationWraper> TerminateTasks()
        {
            return _fileStorageService.TerminateTasks().Select(o => new FileOperationWraper(o));
        }


        /// <summary>
        ///  Returns the list of all active Operations
        /// </summary>
        /// <short>Operations list</short>
        /// <category>Operations</category>
        /// <returns>Operation result</returns>
        [Read("fileops")]
        public IEnumerable<FileOperationWraper> GetOperationStatuses()
        {
            return _fileStorageService.GetTasksStatuses().Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Start downlaod process of files and folders with ID
        /// </summary>
        /// <short>Finish Operations</short>
        /// <param name="fileConvertIds" visible="false">File ID list for download with convert to format</param>
        /// <param name="fileIds">File ID list</param>
        /// <param name="folderIds">Folder ID list</param>
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
        ///   Deletes the files and folders with the IDs specified in the request
        /// </summary>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileIds">File ID list</param>
        /// <param name="deleteAfter">Delete after finished</param>
        /// <param name="immediately">Don't move to the Recycle Bin</param>
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
        ///   Deletes all files and folders from the recycle bin
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
        /// Returns the detailed information about all the available file versions with the ID specified in the request
        /// </summary>
        /// <short>File versions</short>
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
        /// Change version history
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <param name="version">Version of history</param>
        /// <param name="continueVersion">Mark as version or revision</param>
        /// <category>Files</category>
        /// <returns></returns>
        [Update("file/{fileId}/history")]
        public IEnumerable<FileWrapper> ChangeHistory(string fileId, int version, bool continueVersion)
        {
            var history = _fileStorageService.CompleteVersion(fileId, version, continueVersion).Value;
            return history.Select(x => new FileWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Returns the detailed information about shared file with the ID specified in the request
        /// </summary>
        /// <short>File sharing</short>
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
        /// Returns the detailed information about shared folder with the ID specified in the request
        /// </summary>
        /// <short>Folder sharing</short>
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
        /// Sets sharing settings for the file with the ID specified in the request
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <param name="share">Collection of sharing rights</param>
        /// <param name="notify">Should notify people</param>
        /// <param name="sharingMessage">Sharing message to send when notifying</param>
        /// <short>Share file</short>
        /// <category>Sharing</category>
        /// <remarks>
        /// Each of the FileShareParams must contain two parameters: 'ShareTo' - ID of the user with whom we want to share and 'Access' - access type which we want to grant to the user (Read, ReadWrite, etc) 
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
        /// Sets sharing settings for the folder with the ID specified in the request
        /// </summary>
        /// <short>Share folder</short>
        /// <param name="folderId">Folder ID</param>
        /// <param name="share">Collection of sharing rights</param>
        /// <param name="notify">Should notify people</param>
        /// <param name="sharingMessage">Sharing message to send when notifying</param>
        /// <remarks>
        /// Each of the FileShareParams must contain two parameters: 'ShareTo' - ID of the user with whom we want to share and 'Access' - access type which we want to grant to the user (Read, ReadWrite, etc) 
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
        ///   Removes sharing rights for the group with the ID specified in the request
        /// </summary>
        /// <param name="folderIds">Folders ID</param>
        /// <param name="fileIds">Files ID</param>
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
        ///   Returns the external link to the shared file with the ID specified in the request
        /// </summary>
        /// <short>Shared link</short>
        /// <param name="fileId">File ID</param>
        /// <param name="share">Access right</param>
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
        ///   Get a list of available providers
        /// </summary>
        /// <category>Third-Party Integration</category>
        /// <returns>List of provider key</returns>
        /// <remarks>List of provider key: DropboxV2, Box, WebDav, Yandex, OneDrive, SharePoint, GoogleDrive</remarks>
        /// <returns></returns>
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
        ///   Saves the third party file storage service account
        /// </summary>
        /// <short>Save third party account</short>
        /// <param name="url">Connection url for SharePoint</param>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <param name="token">Authentication token</param>
        /// <param name="isCorporate"></param>
        /// <param name="customerTitle">Title</param>
        /// <param name="providerKey">Provider Key</param>
        /// <param name="providerId">Provider ID</param>
        /// <category>Third-Party Integration</category>
        /// <returns>Folder contents</returns>
        /// <remarks>List of provider key: DropboxV2, Box, WebDav, Yandex, OneDrive, SharePoint, GoogleDrive</remarks>
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
        ///    Returns the list of all connected third party services
        /// </summary>
        /// <category>Third-Party Integration</category>
        /// <short>Third party list</short>
        /// <returns>Connected providers</returns>
        [Read("thirdparty")]
        public IEnumerable<ThirdPartyParams> GetThirdPartyAccounts()
        {
            return _fileStorageService.GetThirdParty();
        }

        /// <summary>
        ///    Returns the list of third party services connected in the Common section
        /// </summary>
        /// <category>Third-Party Integration</category>
        /// <short>Third party folder</short>
        /// <returns>Connected providers folder</returns>
        [Read("thirdparty/common")]
        public IEnumerable<Folder> GetCommonThirdPartyFolders()
        {
            var parent = _fileStorageService.GetFolder(Global.FolderCommon.ToString());
            return EntryManager.GetThirpartyFolders(parent);
        }

        /// <summary>
        ///   Removes the third party file storage service account with the ID specified in the request
        /// </summary>
        /// <param name="providerId">Provider ID. Provider id is part of folder id.
        /// Example, folder id is "sbox-123", then provider id is "123"
        /// </param>
        /// <short>Remove third party account</short>
        /// <category>Third-Party Integration</category>
        /// <returns>Folder id</returns>
        ///<exception cref="ArgumentException"></exception>
        [Delete("thirdparty/{providerId:[0-9]+}")]
        public object DeleteThirdParty(int providerId)
        {
            return _fileStorageService.DeleteThirdParty(providerId.ToString(CultureInfo.InvariantCulture));

        }

        /// <summary>
        /// Search files
        /// </summary>
        /// <param name="query">Queary string</param>
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
        /// Adding files to favorite list
        /// </summary>
        /// <short>Favorite add</short>
        /// <category>Files</category>
        /// <param name="folderIds" visible="false"></param>
        /// <param name="fileIds">File IDs</param>
        /// <returns></returns>
        [Create("favorites")]
        public bool AddFavorites(IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.AddToFavorites(new FilesNS.ItemList<string>(folderIds), new FilesNS.ItemList<string>(fileIds));
            return true;
        }

        /// <summary>
        /// Removing files from favorite list
        /// </summary>
        /// <short>Favorite delete</short>
        /// <category>Files</category>
        /// <param name="folderIds" visible="false"></param>
        /// <param name="fileIds">File IDs</param>
        /// <returns></returns>
        [Delete("favorites")]
        public bool DeleteFavorites(IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.DeleteFavorites(new FilesNS.ItemList<string>(folderIds), new FilesNS.ItemList<string>(fileIds));
            return true;
        }

        /// <summary>
        /// Adding files to template list
        /// </summary>
        /// <short>Template add</short>
        /// <category>Files</category>
        /// <param name="fileIds">File IDs</param>
        /// <returns></returns>
        [Create("templates")]
        public bool AddTemplates(IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.AddToTemplates(new FilesNS.ItemList<string>(fileIds));
            return true;
        }

        /// <summary>
        /// Removing files from template list
        /// </summary>
        /// <short>Template delete</short>
        /// <category>Files</category>
        /// <param name="fileIds">File IDs</param>
        /// <returns></returns>
        [Delete("templates")]
        public bool DeleteTemplates(IEnumerable<String> fileIds)
        {
            var list = _fileStorageService.DeleteTemplates(new FilesNS.ItemList<string>(fileIds));
            return true;
        }


        /// <summary>
        /// Store file in original formats when upload and convert
        /// </summary>
        /// <param name="set"></param>
        /// <category>Settings</category>
        /// <returns></returns>
        [Update(@"storeoriginal")]
        public bool StoreOriginal(bool set)
        {
            return _fileStorageService.StoreOriginal(set);
        }

        /// <summary>
        /// Do not show the confirmation dialog
        /// </summary>
        /// <param name="save"></param>
        /// <category>Settings</category>
        /// <visible>false</visible>
        /// <returns></returns>
        [Update(@"hideconfirmconvert")]
        public bool HideConfirmConvert(bool save)
        {
            return _fileStorageService.HideConfirmConvert(save);
        }

        /// <summary>
        /// Update the file version if the same name is exist
        /// </summary>
        /// <param name="set"></param>
        /// <category>Settings</category>
        /// <returns></returns>
        [Update(@"updateifexist")]
        public bool UpdateIfExist(bool set)
        {
            return _fileStorageService.UpdateIfExist(set);
        }

        /// <summary>
        /// Display recent folder
        /// </summary>
        /// <param name="set"></param>
        /// <category>Settings</category>
        /// <returns></returns>
        [Update(@"displayRecent")]
        public bool DisplayRecent(bool set)
        {
            return _fileStorageService.DisplayRecent(set);
        }

        /// <summary>
        /// Display favorite folder
        /// </summary>
        /// <param name="set"></param>
        /// <category>Settings</category>
        /// <returns></returns>
        [Update(@"settings/favorites")]
        public bool DisplayFavorite(bool set)
        {
            return _fileStorageService.DisplayFavorite(set);
        }

        /// <summary>
        /// Display template folder
        /// </summary>
        /// <param name="set"></param>
        /// <category>Settings</category>
        /// <returns></returns>
        [Update(@"settings/templates")]
        public bool DisplayTemplates(bool set)
        {
            return _fileStorageService.DisplayTemplates(set);
        }

        /// <summary>
        ///  Checking document service location
        /// </summary>
        /// <param name="docServiceUrl">Document editing service Domain</param>
        /// <param name="docServiceUrlInternal">Document command service Domain</param>
        /// <param name="docServiceUrlPortal">Community Server Address</param>
        /// <category>Settings</category>
        /// <returns></returns>
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
        /// Get the address of connected editors
        /// </summary>
        /// <category>Settings</category>
        /// <param name="version" visible="false"></param>
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


        private FolderContentWrapper ToFolderContentWrapper(object folderId, Guid userIdOrGroupId, FilterType filterType)
        {
            SortedByType sortBy;
            if (!Enum.TryParse(_context.SortBy, true, out sortBy))
                sortBy = SortedByType.AZ;
            var startIndex = Convert.ToInt32(_context.StartIndex);
            return new FolderContentWrapper(_fileStorageService.GetFolderItems(folderId.ToString(),
                                                                               startIndex,
                                                                               Convert.ToInt32(_context.Count) - 1, //NOTE: in ApiContext +1
                                                                               filterType,
                                                                               filterType == FilterType.ByUser,
                                                                               userIdOrGroupId.ToString(),
                                                                               _context.FilterValue,
                                                                               false,
                                                                               false,
                                                                               new OrderBy(sortBy, !_context.SortDescending)),
                                            startIndex);
        }

        #region wordpress

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
        /// Result of file conversation operation.
        /// </summary>
        [DataContract(Name = "operation_result", Namespace = "")]
        public class ConversationResult
        {
            /// <summary>
            /// Operation Id.
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
            /// Error during conversation.
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