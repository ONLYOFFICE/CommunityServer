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


using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Api.Utils;
using ASC.Common.Web;
using ASC.Core;
using ASC.CRM.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using ASC.Web.Files.HttpHandlers;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Files.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using FileShare = ASC.Files.Core.Security.FileShare;
using SortedByType = ASC.Files.Core.SortedByType;

namespace ASC.Api.Documents
{
    /// <summary>
    /// Provides acces to documents
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
            if (!FilesIntegration.IsRegisteredFileSecurityProvider("crm", "crm_common"))
            {
                FilesIntegration.RegisterFileSecurityProvider("crm", "crm_common", new FileSecurityProvider());
            }
        }



        /// <summary>
        /// Returns the detailed list of files and folders located in the current user 'My Documents' section
        /// </summary>
        /// <short>
        /// My folder
        /// </short>
        /// <category>Folders</category>
        /// <returns>My folder contents</returns>
        [Read("@my")]
        public FolderContentWrapper GetMyFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderMy, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the current user 'Projects Documents' section
        /// </summary>
        /// <short>
        /// Projects folder
        /// </short>
        /// <category>Folders</category>
        /// <returns>Projects folder contents</returns>
        [Read("@projects")]
        public FolderContentWrapper GetProjectsFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderProjects, userIdOrGroupId, filterType);
        }


        /// <summary>
        /// Returns the detailed list of files and folders located in the 'Common Documents' section
        /// </summary>
        /// <short>
        /// Common folder
        /// </short>
        /// <category>Folders</category>
        /// <returns>Common folder contents</returns>
        [Read("@common")]
        public FolderContentWrapper GetCommonFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderCommon, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the 'Shared with Me' section
        /// </summary>
        /// <short>
        /// Shared folder
        /// </short>
        /// <category>Folders</category>
        /// <returns>Shared folder contents</returns>
        [Read("@share")]
        public FolderContentWrapper GetShareFolder(Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(Global.FolderShare, userIdOrGroupId, filterType);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the 'Recycle Bin' section
        /// </summary>
        /// <short>
        /// Trash folder
        /// </short>
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
        /// <param name="folderid">Folder ID</param>
        /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
        /// <param name="filterType" optional="true" remark="Allowed values: None (0), FilesOnly (1), FoldersOnly (2), DocumentsOnly (3), PresentationsOnly (4), SpreadsheetsOnly (5) or ImagesOnly (7)">Filter type</param>
        /// <returns>Folder contents</returns>
        [Read("{folderid}")]
        public FolderContentWrapper GetFolder(String folderid, Guid userIdOrGroupId, FilterType filterType)
        {
            return ToFolderContentWrapper(folderid, userIdOrGroupId, filterType).NotFoundIfNull();

        }

        /// <summary>
        /// Uploads the file specified with single file upload or standart multipart/form-data method to 'My Documents' section
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
        public object UploadFileToMy(Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<System.Web.HttpPostedFileBase> files)
        {
            return UploadFile(Global.FolderMy.ToString(), file, contentType, contentDisposition, files, false, true);
        }

        /// <summary>
        /// Uploads the file specified with single file upload or standart multipart/form-data method to 'Common Documents' section
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
        public object UploadFileToCommon(Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<System.Web.HttpPostedFileBase> files)
        {
            return UploadFile(Global.FolderCommon.ToString(), file, contentType, contentDisposition, files, false, true);
        }


        /// <summary>
        /// Uploads the file specified with single file upload or standart multipart/form-data method to the selected folder
        /// </summary>
        /// <short>Upload to folder</short>
        /// <category>Uploads</category>
        /// <remarks>
        /// <![CDATA[
        ///  Upload can be done in 2 different ways:
        ///  <ol>
        /// <li>Single file upload. You should set Content-Type &amp; Content-Disposition header to specify filename and content type, and send file in request body</li>
        /// <li>Using standart multipart/form-data method</li>
        /// </ol>]]>
        /// </remarks>
        /// <param name="folderid">Folder ID to upload to</param>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="contentType" visible="false">Content-Type Header</param>
        /// <param name="contentDisposition" visible="false">Content-Disposition Header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <param name="createNewIfExist" visible="false">Create New If Exist</param>
        /// <param name="storeOriginalFileFlag" visible="false">If True, upload documents in original formats as well</param>
        /// <returns>Uploaded file</returns>
        [Create("{folderid}/upload")]
        public object UploadFile(string folderid, Stream file, ContentType contentType, ContentDisposition contentDisposition, IEnumerable<System.Web.HttpPostedFileBase> files, bool createNewIfExist, bool storeOriginalFileFlag)
        {
            FilesSettings.StoreOriginalFiles = storeOriginalFileFlag;

            if (files != null && files.Any())
            {
                if (files.Count() == 1)
                {
                    //Only one file. return it
                    var postedFile = files.First();
                    return InsertFile(folderid, postedFile.InputStream, postedFile.FileName, createNewIfExist);
                }
                //For case with multiple files
                return files.Select(postedFile => InsertFile(folderid, postedFile.InputStream, postedFile.FileName, createNewIfExist)).ToList();
            }
            if (file != null)
            {
                var fileName = "file" + MimeMapping.GetExtention(contentType.MediaType);
                if (contentDisposition != null)
                {
                    fileName = contentDisposition.FileName;
                }

                return InsertFile(folderid, file, fileName, createNewIfExist);
            }
            throw new InvalidOperationException("No input files");
        }

        /// <summary>
        /// Uploads the file specified with single file upload
        /// </summary>
        /// <param name="folderId">Folder ID to upload to</param>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="title">Name of file which has to be uploaded</param>
        /// <param name="createNewIfExist" visible="false">Create New If Exist</param>
        /// <category>Uploads</category>
        /// <returns></returns>
        [Create("{folderid}/insert")]
        public FileWrapper InsertFile(string folderId, Stream file, string title, bool createNewIfExist)
        {
            try
            {
                var resultFile = FileUploader.Exec(folderId, title, file.Length, file, createNewIfExist);
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
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="version"></param>
        /// <param name="tabId"></param>
        /// <param name="downloadUri"></param>
        /// <param name="asNew"></param>
        /// <param name="doc"></param>
        [Update("file/{fileid}/saveediting")]
        public FileWrapper SaveEditing(String fileId, int version, Guid tabId, string downloadUri, bool asNew, String doc)
        {
            return new FileWrapper(_fileStorageService.SaveEditing(fileId, version, tabId, downloadUri, asNew, doc));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileid"></param>
        /// <param name="docKeyForTrack"></param>
        /// <param name="asNew"></param>
        /// <param name="editingAlone"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        [Create("file/{fileid}/startedit")]
        public string StartEdit(String fileid, String docKeyForTrack, bool asNew, bool editingAlone, String doc)
        {
            return _fileStorageService.StartEdit(fileid, docKeyForTrack, asNew, editingAlone, doc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="tabId"></param>
        /// <param name="docKeyForTrack"></param>
        /// <param name="shareLinkKey"></param>
        /// <param name="isFinish"></param>
        /// <param name="fixedVersion"></param>
        /// <returns></returns>
        [Read("file/{fileid}/trackeditfile")]
        public KeyValuePair<bool, String> TrackEditFile(String fileId, Guid tabId, String docKeyForTrack, String shareLinkKey, bool isFinish, bool fixedVersion)
        {
            return _fileStorageService.TrackEditFile(fileId, tabId, docKeyForTrack, shareLinkKey, isFinish, fixedVersion);
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
        [Create("{folderid}/upload/create_session")]
        public string CreateUploadSession(string folderId, string fileName, long fileSize, string relativePath)
        {
            var file = FileUploader.VerifyChunkedUpload(folderId, fileName, fileSize, FilesSettings.UpdateIfExist, relativePath);

            //if "files.uploader.url" value="products/files/"
            if (CoreContext.Configuration.Standalone)
            {
                var session = FileUploader.InitiateUpload(file.FolderID.ToString(), (file.ID ?? "").ToString(), file.Title, file.ContentLength);

                var response = ChunkedUploaderHandler.ToResponseObject(session, true);
                return JsonConvert.SerializeObject(new
                    {
                        success = true,
                        data = JsonConvert.SerializeObject(response)
                    });
            }

            var createSessionUrl = FilesLinkUtility.GetInitiateUploadSessionUrl(file.FolderID, file.ID, file.Title, file.ContentLength);
            var request = (HttpWebRequest)WebRequest.Create(createSessionUrl);
            request.Method = "POST";
            request.ContentLength = 0;

            // hack for uploader.onlyoffice.com in api requests
            var rewriterHeader = _context.RequestContext.HttpContext.Request.Headers[System.Web.HttpRequestExtensions.UrlRewriterHeader];
            if (!string.IsNullOrEmpty(rewriterHeader))
            {
                request.Headers[System.Web.HttpRequestExtensions.UrlRewriterHeader] = rewriterHeader;
            }

            // hack. http://ubuntuforums.org/showthread.php?t=1841740
            if (WorkContext.IsMono)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            }

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                return new StreamReader(responseStream).ReadToEnd(); //result is json string
            }
        }

        /// <summary>
        /// Creates a text (.txt) file in 'My Documents' section with the title and contents sent in the request
        /// </summary>
        /// <short>Create txt in 'My'</short>
        /// <category>File Creation</category>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        [Create("@my/text")]
        public FolderContentWrapper CreateTextFileInMy(string title, string content)
        {
            return CreateTextFile(Global.FolderMy.ToString(), title, content);
        }

        /// <summary>
        /// Creates a text (.txt) file in 'Common Documents' section with the title and contents sent in the request
        /// </summary>
        /// <short>Create txt in 'Common'</short>
        /// <category>File Creation</category>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        [Create("@common/text")]
        public FolderContentWrapper CreateTextFileInCommon(string title, string content)
        {
            return CreateTextFile(Global.FolderCommon.ToString(), title, content);
        }

        /// <summary>
        /// Creates a text (.txt) file in the selected folder with the title and contents sent in the request
        /// </summary>
        /// <short>Create txt</short>
        /// <category>File Creation</category>
        /// <param name="folderid">Folder ID</param>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        [Create("{folderid}/text")]
        public FolderContentWrapper CreateTextFile(string folderid, string title, string content)
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
            CreateFile(folderid, title, content, extension);
            return ToFolderContentWrapper(folderid);
        }

        private static void CreateFile(string folderid, string title, string content, string extension)
        {
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                FileUploader.Exec(folderid,
                                  title.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? title : (title + extension),
                                  memStream.Length, memStream);
            }
        }

        /// <summary>
        /// Creates an html (.html) file in the selected folder with the title and contents sent in the request
        /// </summary>
        /// <short>Create html</short>
        /// <category>File Creation</category>
        /// <param name="folderid">Folder ID</param>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        [Create("{folderid}/html")]
        public FolderContentWrapper CreateHtmlFile(string folderid, string title, string content)
        {
            if (title == null) throw new ArgumentNullException("title");
            CreateFile(folderid, title, content, ".html");
            return ToFolderContentWrapper(folderid);
        }

        /// <summary>
        /// Creates an html (.html) file in 'My Documents' section with the title and contents sent in the request
        /// </summary>
        /// <short>Create html in 'My'</short>
        /// <category>File Creation</category>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>
        [Create("@my/html")]
        public FolderContentWrapper CreateHtmlFileInMy(string title, string content)
        {
            return CreateHtmlFile(Global.FolderMy.ToString(), title, content);
        }


        /// <summary>
        /// Creates an html (.html) file in 'Common Documents' section with the title and contents sent in the request
        /// </summary>
        /// <short>Create html in 'Common'</short>
        /// <category>File Creation</category>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>Folder contents</returns>        
        [Create("@common/html")]
        public FolderContentWrapper CreateHtmlFileInCommon(string title, string content)
        {
            return CreateHtmlFile(Global.FolderCommon.ToString(), title, content);
        }


        /// <summary>
        /// Creates a new folder with the title sent in the request. The ID of a parent folder can be also specified.
        /// </summary>
        /// <short>
        /// New folder
        /// </short>
        /// <category>Folders</category>
        /// <param name="folderid">Parent folder ID</param>
        /// <param name="title">Title of new folder</param>
        /// <returns>New folder contents</returns>
        [Create("folder/{folderid}")]
        public FolderContentWrapper CreateFolder(string folderid, string title)
        {
            var folder = _fileStorageService.CreateNewFolder(folderid, title);
            return ToFolderContentWrapper(folder.ID);
        }

        /// <summary>
        /// Creates a new file in the specified folder with the title sent in the request
        /// </summary>
        /// <short>Create file</short>
        /// <category>File Creation</category>
        /// <param name="folderid">Folder ID</param>
        /// <param name="title" remark="Allowed values: the file must have one of the following extensions: DOCX, XLSX, PPTX">File title</param>
        /// <remarks>In case the extension for the file title differs from DOCX/XLSX/PPTX and belongs to one of the known text, spreadsheet or presentation formats, it will be changed to DOCX/XLSX/PPTX accordingly. If the file extension is not set or is unknown, the DOCX extension will be added to the file title.</remarks>
        /// <returns>New file info</returns>
        [Create("{folderid}/file")]
        public FileWrapper CreateFile(string folderid, string title)
        {
            var file = _fileStorageService.CreateNewFile(folderid, title);
            return new FileWrapper(file);
        }

        /// <summary>
        /// Renames the selected folder to the new title specified in the request
        /// </summary>
        /// <short>
        /// Rename folder
        /// </short>
        /// <category>Folders</category>
        /// <param name="folderid">Folder ID</param>
        /// <param name="title">New title</param>
        /// <returns>Folder contents</returns>
        [Update("folder/{folderid}")]
        public FolderContentWrapper RenameFolder(string folderid, string title)
        {
            _fileStorageService.FolderRename(folderid, title);
            return ToFolderContentWrapper(folderid);
        }

        /// <summary>
        /// Returns a detailed information about the folder with the ID specified in the request
        /// </summary>
        /// <short>Folder information</short>
        /// <category>Folders</category>
        /// <returns>Folder info</returns>
        [Read("folder/{folderid}")]
        public FolderWrapper GetFolderInfo(string folderid)
        {
            var folder = _fileStorageService.GetFolder(folderid).NotFoundIfNull("Folder not found");

            return new FolderWrapper(folder);
        }

        /// <summary>
        /// Returns parent folders
        /// </summary>
        /// <param name="folderid"></param>
        /// <category>Folders</category>
        /// <returns>Parent folders</returns>
        [Read("folder/{folderid}/path")]
        public IEnumerable<FolderWrapper> GetFolderPath(string folderid)
        {
            return EntryManager.GetBreadCrumbs(folderid).Select(f => new FolderWrapper(f)).ToSmartList();
        }

        /// <summary>
        /// Returns a detailed information about the file with the ID specified in the request
        /// </summary>
        /// <short>File information</short>
        /// <category>Files</category>
        /// <returns>File info</returns>
        [Read("file/{fileid}")]
        public FileWrapper GetFileInfo(string fileid, int version = -1)
        {
            var file = _fileStorageService.GetFile(fileid, version).NotFoundIfNull("File not found");

            return new FileWrapper(file);
        }

        /// <summary>
        ///     Updates the information of the selected file with the parameters specified in the request
        /// </summary>
        /// <short>Update file info</short>
        /// <category>Files</category>
        /// <param name="fileid">File ID</param>
        /// <param name="title">New title</param>
        /// <param name="lastVersion">File last version number</param>
        /// <returns>File info</returns>
        [Update("file/{fileid}")]
        public FileWrapper UpdateFile(String fileid, String title, int lastVersion)
        {
            if (!String.IsNullOrEmpty(title))
                _fileStorageService.FileRename(fileid.ToString(CultureInfo.InvariantCulture), title);

            if (lastVersion > 0)
                _fileStorageService.UpdateToVersion(fileid.ToString(CultureInfo.InvariantCulture), lastVersion);

            return GetFileInfo(fileid);
        }

        /// <summary>
        /// Deletes the file with the ID specified in the request
        /// </summary>
        /// <short>Delete file</short>
        /// <category>Files</category>
        /// <param name="fileid">File ID</param>
        /// <returns>Operation result</returns>
        [Delete("file/{fileid}")]
        public IEnumerable<FileOperationWraper> DeleteFile(String fileid)
        {
            return DeleteBatchItems(null, new[] { fileid });
        }

        /// <summary>
        ///  Start conversion
        /// </summary>
        /// <short>Convert</short>
        /// <category>File operations</category>
        /// <param name="fileid"></param>
        /// <param name="start"></param>
        /// <returns>Operation result</returns>
        [Update("file/{fileid}/checkconversion")]
        public IEnumerable<FileOperationResult> StartConversion(String fileid, bool start = true)
        {
            var conversion = _fileStorageService.CheckConversion(new Web.Files.Services.WCFService.ItemList<Web.Files.Services.WCFService.ItemList<string>>
                {
                    new Web.Files.Services.WCFService.ItemList<string> {fileid, "0", start.ToString()}
                });
            conversion.ForEach(item =>
                {
                    if (string.IsNullOrEmpty(item.Result.ToString())) return;
                    var result = JObject.Parse(item.Result.ToString());
                    var file = result.Value<JObject>("file");
                    item.Result = GetFileInfo(file.Value<string>("id"), file.Value<int>("version"));
                });
            return conversion.ToSmartList();
        }

        /// <summary>
        ///  Check conversion status
        /// </summary>
        /// <short>Convert</short>
        /// <category>File operations</category>
        /// <param name="fileid"></param>
        /// <returns>Operation result</returns>
        [Read("file/{fileid}/checkconversion")]
        public IEnumerable<FileOperationResult> CheckConversion(String fileid)
        {
            return StartConversion(fileid, false);
        }

        /// <summary>
        /// Get presigned Uri
        /// </summary>
        /// <param name="fileid">File ID</param>
        /// <returns>Url</returns>
        [Read("file/{fileid}/presigned")]
        public string GetPresignedUri(String fileid)
        {
            var file = _fileStorageService.GetFile(fileid, -1).NotFoundIfNull("File not found");
            return PathProvider.GetFileStreamUrl(file);
        }

        /// <summary>
        /// Deletes the folder with the ID specified in the request
        /// </summary>
        /// <short>Delete folder</short>
        /// <category>Folders</category>
        /// <param name="folderid">Folder ID</param>
        /// <returns>Operation result</returns>
        [Delete("folder/{folderid}")]
        public IEnumerable<FileOperationWraper> DeleteFolder(String folderid)
        {
            return DeleteBatchItems(new[] { folderid }, null);
        }

        /// <summary>
        /// Checking for conflicts
        /// </summary>
        /// <category>File operations</category>
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

            var entries = _fileStorageService.GetItems(new Web.Files.Services.WCFService.ItemList<string>(ids), FilterType.FilesOnly, "", "");
            return entries.Select(x => new FileWrapper((Files.Core.File)x)).ToSmartList();
        }

        /// <summary>
        ///   Moves all the selected files and folders to the folder with the ID specified in the request
        /// </summary>
        /// <short>Move to folder</short>
        /// <category>File operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileIds">File ID list</param>
        /// <param name="conflictResolveType">Overwriting behavior: skip(0), overwrite(1) or duplicate(2)</param>
        /// <returns>Operation result</returns>
        [Update("fileops/move")]
        public IEnumerable<FileOperationWraper> MoveBatchItems(String destFolderId, IEnumerable<String> folderIds, IEnumerable<String> fileIds, FileConflictResolveType conflictResolveType)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            return _fileStorageService.MoveOrCopyItems(itemList, destFolderId, conflictResolveType, false).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        ///   Copies all the selected files and folders to the folder with the ID specified in the request
        /// </summary>
        /// <short>Copy to folder</short>
        /// <category>File operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileIds">File ID list</param>
        /// <param name="conflictResolveType">Overwriting behavior: skip(0), overwrite(1) or duplicate(2)</param>
        /// <returns>Operation result</returns>
        [Update("fileops/copy")]
        public IEnumerable<FileOperationWraper> CopyBatchItems(String destFolderId, IEnumerable<String> folderIds, IEnumerable<String> fileIds, FileConflictResolveType conflictResolveType)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            return _fileStorageService.MoveOrCopyItems(itemList, destFolderId, conflictResolveType, true).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        ///   Marks all files and folders as read
        /// </summary>
        /// <short>Mark as read</short>
        /// <category>File operations</category>
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
        ///  Finishes all the active file operations
        /// </summary>
        /// <short>Finish all</short>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/terminate")]
        public IEnumerable<FileOperationWraper> TerminateTasks()
        {
            return _fileStorageService.TerminateTasks().Select(o => new FileOperationWraper(o));
        }


        /// <summary>
        ///  Returns the list of all active file operations
        /// </summary>
        /// <short>Get file operations list</short>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Read("fileops")]
        public IEnumerable<FileOperationWraper> GetOperationStatuses()
        {
            return _fileStorageService.GetTasksStatuses().Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        /// Start downlaod process of files and folders with ID
        /// </summary>
        /// <short>Finish file operations</short>
        /// <param name="fileIds">File ID list</param>
        /// <param name="folderIds">Folder ID list</param>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/bulkdownload")]
        public IEnumerable<FileOperationResult> BulkDownload(
            IEnumerable<ItemKeyValuePair<String, String>> fileIds,
            IEnumerable<String> folderIds)
        {
            var itemList = new Dictionary<String, String>();

            foreach (var fileid in fileIds)
            {
                if (!itemList.ContainsKey(fileid.Key))
                    itemList.Add(fileid.Key, fileid.Value);
            }

            foreach (var folderId in folderIds)
            {
                if (!itemList.ContainsKey(folderId))
                    itemList.Add(folderId, String.Empty);
            }

            return _fileStorageService.BulkDownload(itemList);
        }

        /// <summary>
        ///   Deletes the files and folders with the IDs specified in the request
        /// </summary>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileIds">File ID list</param>
        /// <short>Delete files and folders</short>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/delete")]
        public IEnumerable<FileOperationWraper> DeleteBatchItems(IEnumerable<String> folderIds, IEnumerable<String> fileIds)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange((folderIds ?? new List<String>()).Select(x => "folder_" + x));
            itemList.AddRange((fileIds ?? new List<String>()).Select(x => "file_" + x));

            return _fileStorageService.DeleteItems("delete", itemList).Select(o => new FileOperationWraper(o));
        }

        /// <summary>
        ///   Deletes all files and folders from the recycle bin
        /// </summary>
        /// <short>Clear recycle bin</short>
        /// <category>File operations</category>
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
        /// <param name="fileid">File ID</param>
        /// <returns>File information</returns>
        [Read("file/{fileid}/history")]
        public IEnumerable<FileWrapper> GetFileVersionInfo(string fileid)
        {
            var files = _fileStorageService.GetFileHistory(fileid);
            return files.Select(x => new FileWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Returns the detailed information about shared file with the ID specified in the request
        /// </summary>
        /// <short>File sharing</short>
        /// <category>Sharing</category>
        /// <param name="fileid">File ID</param>
        /// <returns>Shared file information</returns>
        [Read("file/{fileid}/share")]
        public IEnumerable<FileShareWrapper> GetFileSecurityInfo(string fileid)
        {
            var fileShares = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { String.Format("file_{0}", fileid) });
            return fileShares.Select(x => new FileShareWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Returns the detailed information about shared folder with the ID specified in the request
        /// </summary>
        /// <short>Folder sharing</short>
        /// <param name="folderid">Folder ID</param>
        /// <category>Sharing</category>
        /// <returns>Shared folder information</returns>
        [Read("folder/{folderid}/share")]
        public IEnumerable<FileShareWrapper> GetFolderSecurityInfo(string folderid)
        {
            var fileShares = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { String.Format("folder_{0}", folderid) });
            return fileShares.Select(x => new FileShareWrapper(x)).ToSmartList();
        }

        /// <summary>
        /// Sets sharing settings for the file with the ID specified in the request
        /// </summary>
        /// <param name="fileid">File ID</param>
        /// <param name="share">Collection of sharing rights</param>
        /// <param name="notify">Should notify people</param>
        /// <param name="sharingMessage">Sharing message to send when notifying</param>
        /// <short>Share file</short>
        /// <category>Sharing</category>
        /// <remarks>
        /// Each of the FileShareParams must contain two parameters: 'ShareTo' - ID of the user with whom we want to share and 'Access' - access type which we want to grant to the user (Read, ReadWrite, etc) 
        /// </remarks>
        /// <returns>Shared file information</returns>
        [Update("file/{fileid}/share")]
        public IEnumerable<FileShareWrapper> SetFileSecurityInfo(string fileid, IEnumerable<FileShareParams> share, bool notify, string sharingMessage)
        {
            if (share != null && share.Any())
            {
                var list = new Web.Files.Services.WCFService.ItemList<AceWrapper>(share.Select(x => x.ToAceObject()));
                var aceCollection = new AceCollection
                    {
                        Entries = new Web.Files.Services.WCFService.ItemList<string> { "file_" + fileid },
                        Aces = list,
                        Message = sharingMessage
                    };
                _fileStorageService.SetAceObject(aceCollection, notify);
            }
            return GetFileSecurityInfo(fileid);
        }

        /// <summary>
        /// Sets sharing settings for the folder with the ID specified in the request
        /// </summary>
        /// <short>Share folder</short>
        /// <param name="folderid">Folder ID</param>
        /// <param name="share">Collection of sharing rights</param>
        /// <param name="notify">Should notify people</param>
        /// <param name="sharingMessage">Sharing message to send when notifying</param>
        /// <remarks>
        /// Each of the FileShareParams must contain two parameters: 'ShareTo' - ID of the user with whom we want to share and 'Access' - access type which we want to grant to the user (Read, ReadWrite, etc) 
        /// </remarks>
        /// <category>Sharing</category>
        /// <returns>Shared folder information</returns>
        [Update("folder/{folderid}/share")]
        public IEnumerable<FileShareWrapper> SetFolderSecurityInfo(string folderid, IEnumerable<FileShareParams> share, bool notify, string sharingMessage)
        {
            if (share != null && share.Any())
            {
                var list = new Web.Files.Services.WCFService.ItemList<AceWrapper>(share.Select(x => x.ToAceObject()));
                var aceCollection = new AceCollection
                    {
                        Entries = new Web.Files.Services.WCFService.ItemList<string> { "folder_" + folderid },
                        Aces = list,
                        Message = sharingMessage
                    };
                _fileStorageService.SetAceObject(aceCollection, notify);
            }

            return GetFolderSecurityInfo(folderid);
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
        /// <summary>
        ///   File external link
        /// </summary>
        /// <param name="fileid">File ID</param>
        /// <param name="share">Access right</param>
        /// <returns>Shared file link</returns>
        [Update("{fileid}/sharedlink")]
        public string GenerateSharedLink(string fileid, FileShare share)
        {
            var file = GetFileInfo(fileid);

            var objectid = "file_" + file.Id;
            var sharedInfo = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { objectid }).Find(r => r.SubjectId == FileConstant.ShareLinkId);
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
                        Entries = new Web.Files.Services.WCFService.ItemList<string> { objectid },
                        Aces = list
                    };
                _fileStorageService.SetAceObject(aceCollection, false);
                sharedInfo = _fileStorageService.GetSharedInfo(new Web.Files.Services.WCFService.ItemList<string> { objectid }).Find(r => r.SubjectId == FileConstant.ShareLinkId);
            }

            return sharedInfo.SubjectName;
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
        /// <param name="providerid">Provider ID</param>
        /// <category>Third-Party Integration</category>
        /// <returns>Folder contents</returns>
        /// <remarks> List of provider key: DropBox, BoxNet, WebDav, Google, Yandex, SkyDrive, SharePoint, GoogleDrive</remarks>
        /// <exception cref="ArgumentException"></exception>
        [Create("thirdparty")]
        public FolderContentWrapper SaveThirdParty(
            String url,
            String login,
            String password,
            String token,
            bool isCorporate,
            String customerTitle,
            String providerKey,
            String providerid)
        {
            var thirdPartyParams = new ThirdPartyParams
                {
                    AuthData = new AuthData(url, login, password, token),
                    Corporate = isCorporate,
                    CustomerTitle = customerTitle,
                    ProviderId = providerid,
                    ProviderKey = providerKey,
                };

            var folder = _fileStorageService.SaveThirdParty(thirdPartyParams);

            return ToFolderContentWrapper(folder.ID);
        }

        /// <summary>
        ///    Returns the list of all connected third party services
        /// </summary>
        /// <category>Third-Party Integration</category>
        /// <short>Get third party list</short>
        /// <returns>Connected providers</returns>
        [Read("thirdparty")]
        public IEnumerable<ThirdPartyParams> GetThirdPartyAccounts()
        {
            return _fileStorageService.GetThirdParty();
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
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Read(@"@search/{query}")]
        public IEnumerable<FileEntryWrapper> Search(string query)
        {
            var searcher = new Web.Files.Configuration.SearchHandler();
            var files = searcher.SearchFiles(query).Select(r => (FileEntryWrapper)new FileWrapper(r));
            var folders = searcher.SearchFolders(query).Select(f => (FileEntryWrapper)new FolderWrapper(f));

            return files.Concat(folders);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        [Update(@"storeoriginal")]
        public bool StoreOriginal(bool set)
        {
            return _fileStorageService.StoreOriginal(set);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        [Update(@"updateifexist")]
        public bool UpdateIfExist(bool set)
        {
            return _fileStorageService.StoreOriginal(set);
        }


        private FolderContentWrapper ToFolderContentWrapper(object folderId, Guid userIdOrGroupId, FilterType filterType)
        {
            return new FolderContentWrapper(_fileStorageService.GetFolderItems(folderId.ToString(),
                                                                               Convert.ToInt32(_context.StartIndex),
                                                                               Convert.ToInt32(_context.Count) - 1, //NOTE: in ApiContext +1
                                                                               filterType,
                                                                               new OrderBy(SortedByType.AZ, true),
                                                                               userIdOrGroupId.ToString(),
                                                                               _context.FilterValue));
        }

        private FolderContentWrapper ToFolderContentWrapper(object folderId)
        {
            return ToFolderContentWrapper(folderId, Guid.Empty, FilterType.None);
        }
    }
}