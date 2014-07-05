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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Api.Utils;
using ASC.Common.Data;
using ASC.Common.Web;
using ASC.CRM.Core;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Files.Utils;
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
        public FolderContentWrapper GetMyFolder()
        {
            return ToFolderContentWrapper(Global.FolderMy);
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
        public FolderContentWrapper GetProjectsFolder()
        {
            return ToFolderContentWrapper(Global.FolderProjects);
        }


        /// <summary>
        /// Returns the detailed list of files and folders located in the 'Common Documents' section
        /// </summary>
        /// <short>
        /// Common folder
        /// </short>
        /// <returns>Common folder contents</returns>
        [Read("@common")]
        public FolderContentWrapper GetCommonFolder()
        {
            return ToFolderContentWrapper(Global.FolderCommon);
        }

        /// <summary>
        /// Returns the detailed list of files and folders located in the 'Shared with Me' section
        /// </summary>
        /// <short>
        /// Shared folder
        /// </short>
        /// <returns>Shared folder contents</returns>
        [Read("@share")]
        public FolderContentWrapper GetShareFolder()
        {
            return ToFolderContentWrapper(Global.FolderShare);
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
        public FolderContentWrapper GetTrashFolder()
        {
            return ToFolderContentWrapper(Global.FolderTrash);
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
                    return SaveFile(folderid, postedFile.InputStream, postedFile.FileName, createNewIfExist);
                }
                //For case with multiple files
                return files.Select(postedFile => SaveFile(folderid, postedFile.InputStream, postedFile.FileName, createNewIfExist)).ToList();
            }
            if (file != null)
            {
                var fileName = "file" + MimeMapping.GetExtention(contentType.MediaType);
                if (contentDisposition != null)
                {
                    fileName = contentDisposition.FileName;
                }

                return SaveFile(folderid, file, fileName, createNewIfExist);
            }
            throw new InvalidOperationException("No input files");
        }

        private static FileWrapper SaveFile(string folderid, Stream file, string fileName, bool createNewIfExist)
        {
            try
            {
                var resultFile = FileUploader.Exec(folderid, fileName, file.Length, file, createNewIfExist);
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
        /// Creates session to upload large files in multiple chunks.
        /// </summary>
        /// <short>Chunked upload</short>
        /// <category>Uploads</category>
        /// <param name="folderId">Id of the folder in which file will be uploaded</param>
        /// <param name="fileName">Name of file which has to be uploaded</param>
        /// <param name="fileSize">Length in bytes of file which has to be uploaded</param>
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
        public string CreateUploadSession(string folderId, string fileName, long fileSize)
        {
            var file = FileUploader.VerifyChunkedUpload(folderId, fileName, fileSize, FilesSettings.UpdateIfExist);

            var createSessionUrl = FilesLinkUtility.GetInitiateUploadSessionUrl(file.FolderID, file.ID, file.Title, file.ContentLength);
            var request = (HttpWebRequest)WebRequest.Create(createSessionUrl);
            request.Method = "POST";
            request.ContentLength = 0;

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
            var contentType = "text/plain";
            var extension = ".txt";
            if (!string.IsNullOrEmpty(content))
            {
                if (Regex.IsMatch(content, @"<([^\s>]*)(\s[^<]*)>"))
                {
                    contentType = "text/html";
                    extension = ".html";
                }
            }
            CreateFile(folderid, title, content, contentType, extension);
            return ToFolderContentWrapper(folderid);
        }

        private static void CreateFile(string folderid, string title, string content, string contentType, string extension)
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
            CreateFile(folderid, title, content, "text/html", ".html");
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
        [Create("{folderid}")]
        public FolderContentWrapper CreateFolder(string folderid, string title)
        {
            var folder = _fileStorageService.CreateNewFolder(title, folderid);
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
        [Update("{folderid}")]
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
        /// Returns a detailed information about the file with the ID specified in the request
        /// </summary>
        /// <short>File information</short>
        /// <category>File information</category>
        /// <returns>File info</returns>
        [Read("file/{fileid}")]
        public FileWrapper GetFileInfo(string fileid)
        {
            var file = _fileStorageService.GetFile(fileid, -1).NotFoundIfNull("File not found");

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
        public IEnumerable<FileOperationResult> DeleteFile(String fileid)
        {
            var result = _fileStorageService.DeleteItems(new Web.Files.Services.WCFService.ItemList<string> { "file_" + fileid.ToString(CultureInfo.InvariantCulture) });
            return result.ToSmartList();
        }

        /// <summary>
        /// Deletes the folder with the ID specified in the request
        /// </summary>
        /// <short>Delete folder</short>
        /// <category>Files</category>
        /// <param name="folderid">Folder ID</param>
        /// <returns>Operation result</returns>
        [Delete("folder/{folderid}")]
        public IEnumerable<FileOperationResult> DeleteFolder(String folderid)
        {
            var result = _fileStorageService.DeleteItems(new Web.Files.Services.WCFService.ItemList<string> { "folder_" + folderid.ToString(CultureInfo.InvariantCulture) });

            return result.ToSmartList();
        }

        /// <summary>
        ///   Moves all the selected files and folders to the folder with the ID specified in the request
        /// </summary>
        /// <short>Move to folder</short>
        /// <category>File operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileids">File ID list</param>
        /// <param name="overwrite">Overwriting behavior: overwrite or skip</param>
        /// <returns>Operation result</returns>
        [Update("fileops/move")]
        public IEnumerable<FileOperationResult> MoveBatchItems(String destFolderId, IEnumerable<String> folderIds, IEnumerable<String> fileids, bool overwrite)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange(folderIds.Select(x => "folder_" + x));
            itemList.AddRange(fileids.Select(x => "file_" + x));

            return _fileStorageService.MoveOrCopyItems(itemList, destFolderId, overwrite.ToString(CultureInfo.InvariantCulture), false.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///   Copies all the selected files and folders to the folder with the ID specified in the request
        /// </summary>
        /// <short>Copy to folder</short>
        /// <category>File operations</category>
        /// <param name="destFolderId">Destination folder ID</param>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileids">File ID list</param>
        /// <param name="overwrite">Overwriting behavior: overwrite or skip</param>
        /// <returns>Operation result</returns>
        [Update("fileops/copy")]
        public IEnumerable<FileOperationResult> CopyBatchItems(String destFolderId, IEnumerable<String> folderIds, IEnumerable<String> fileids, bool overwrite)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange(folderIds.Select(x => "folder_" + x));
            itemList.AddRange(fileids.Select(x => "file_" + x));

            return _fileStorageService.MoveOrCopyItems(itemList, destFolderId, overwrite.ToString(CultureInfo.InvariantCulture), true.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///   Deletes all files and folders from the recycle bin
        /// </summary>
        /// <short>Clear recycle bin</short>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/emptytrash")]
        public IEnumerable<FileOperationResult> EmptyTrash()
        {
            return _fileStorageService.EmptyTrash();
        }

        /// <summary>
        ///   Marks all files and folders as read
        /// </summary>
        /// <short>Mark as read</short>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/markasread")]
        public IEnumerable<FileOperationResult> MarkAsRead(IEnumerable<String> folderIds, IEnumerable<String> fileids)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange(folderIds.Select(x => "folder_" + x));
            itemList.AddRange(fileids.Select(x => "file_" + x));

            return _fileStorageService.MarkAsRead(itemList);
        }

        /// <summary>
        ///  Finishes all the active file operations
        /// </summary>
        /// <short>Finish all</short>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/terminate")]
        public IEnumerable<FileOperationResult> TerminateTasks()
        {
            return _fileStorageService.TerminateTasks(false);
        }

        /// <summary>
        ///  Gets the files and folders for importing from Box.com, Google Drive or Zoho Docs
        /// </summary>
        /// <short>Get third party files</short>
        /// <param name="source" remark="Allowed values: boxnet, google, zoho">Source name</param>
        /// <param name="login" optional="true" remark="Necessary for Zoho Docs account authorization">Login</param>
        /// <param name="password" optional="true" remark="Necessary for Zoho Docs account authorization">Password</param>
        /// <param name="token" optional="true"  remark="Necessary for box.com, Google Drive account authorization (OAuth)">Authorization token</param>
        /// <category>Files</category>
        /// <returns>Data for importing</returns>
        [Read("settings/import/{source:(boxnet|google|zoho)}/data")]
        public IEnumerable<DataToImport> GetImportData(
            String source,
            String login,
            String password,
            String token)
        {
            return _fileStorageService.GetImportDocs(source, new AuthData(login, password, token));
        }

        /// <summary>
        ///   Imports data from Box.com, Google Drive or Zoho Docs
        /// </summary>
        /// <short>Import from third party</short>
        /// <param name="source" remark="Allowed values: boxnet, google, zoho">Source name</param>
        /// <param name="login" optional="true" remark="Necessary for Zoho Docs account authorization">Login</param>
        /// <param name="password" optional="true" remark="Necessary for Zoho Docs account authorization">Password</param>
        /// <param name="token" optional="true"  remark="Necessary for box.com, Google Drive account authorization (OAuth)">Authorization token</param>
        /// <param name="folderId">Folder ID form import</param>
        /// <param name="ignoreCoincidenceFiles">Overwriting behavior: overwrite or ignore</param>
        /// <param name="dataToImport">Data for importing</param>
        /// <category>Files</category>
        /// <returns>Operation result</returns>
        [Update("settings/import/{source:(boxnet|google|zoho)}/data")]
        public IEnumerable<FileOperationResult> ExecImportData(
            String source,
            String login,
            String password,
            String token,
            String folderId,
            bool ignoreCoincidenceFiles,
            IEnumerable<DataToImport> dataToImport)
        {
            return _fileStorageService.ExecImportDocs(
                login,
                password,
                token,
                source,
                folderId,
                ignoreCoincidenceFiles,
                dataToImport.ToList()
                );
        }

        /// <summary>
        ///  Finishes importing of the data
        /// </summary>
        /// <short>Finish importing</short>
        /// <category>Files</category>
        /// <returns>Operation result</returns>
        [Update("settings/import/terminate")]
        public IEnumerable<FileOperationResult> ImportDataTerminate()
        {
            return _fileStorageService.TerminateTasks(true);
        }


        /// <summary>
        ///  Returns the list of all active file operations
        /// </summary>
        /// <short>Get file operations list</short>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Read("fileops")]
        public IEnumerable<FileOperationResult> GetOperationStatuses()
        {
            return _fileStorageService.GetTasksStatuses();
        }

        /// <summary>
        
        /// </summary>
        /// <short>Finish file operations</short>
        /// <param name="folderIds">Folder ID list</param>
        /// <param name="fileIds">File ID list</param>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/bulkdownload")]
        public IEnumerable<FileOperationResult> BulkDownload(
            IEnumerable<ItemKeyValuePair<String, String>> fileIds,
            IEnumerable<String> folderIds)
        {
            var itemList = new Web.Files.Services.WCFService.ItemDictionary<String, String>();

            foreach (ItemKeyValuePair<String, String> fileid in fileIds)
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
        /// <param name="fileids">File ID list</param>
        /// <short>Delete files and folders</short>
        /// <category>File operations</category>
        /// <returns>Operation result</returns>
        [Update("fileops/delete")]
        public IEnumerable<FileOperationResult> DeleteBatchItems(IEnumerable<String> folderIds, IEnumerable<String> fileids)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String>();

            itemList.AddRange(folderIds.Select(x => "folder_" + x));
            itemList.AddRange(fileids.Select(x => "file_" + x));

            return _fileStorageService.DeleteItems(itemList);
        }

        /// <summary>
        /// Returns the detailed information about all the available file versions with the ID specified in the request
        /// </summary>
        /// <short>File versions</short>
        /// <category>File information</category>
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
        ///   Removes sharing rights from the file with the ID specified in the request
        /// </summary>
        /// <param name="shareTo">Groups or users ID list</param>
        /// <param name="fileid">File ID</param>
        /// <short>Remove file sharing rights</short>
        /// <category>Sharing</category>
        /// <returns>Shared file information</returns>
        [Delete("file/{fileid}/share")]
        public IEnumerable<FileShareWrapper> RemoveFileSecurityInfo(String fileid, IEnumerable<Guid> shareTo)
        {
            //TODO: shareTo

            var itemList = new Web.Files.Services.WCFService.ItemList<String> { "file_" + fileid };

            _fileStorageService.RemoveAce(itemList);

            return GetFileSecurityInfo(fileid);
        }


        /// <summary>
        ///   Removes sharing rights for the group with the ID specified in the request
        /// </summary>
        /// <param name="shareTo">Groups or users ID list</param>
        /// <param name="folderid">Group ID</param>
        /// <short>Remove group sharing rights</short>
        /// <category>Sharing</category>
        /// <returns>Shared file information</returns>
        [Delete("folder/{folderid}/share")]
        public IEnumerable<FileShareWrapper> RemoveFolderSecurityInfo(String folderid, IEnumerable<Guid> shareTo)
        {
            var itemList = new Web.Files.Services.WCFService.ItemList<String> { "folder_" + folderid };

            _fileStorageService.RemoveAce(itemList);

            return GetFolderSecurityInfo(folderid);
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
        /// <param name="providerid">Provider ID</param>
        /// <category>Third-Party Integration</category>
        /// <returns>Folder contents</returns>
        /// <remarks>
        /// 
        /// 
        /// </remarks>
        ///<exception cref="ArgumentException"></exception>
        [Create("thirdparty")]
        public FolderContentWrapper SaveThirdParty(
            String url,
            String login,
            String password,
            String token,
            bool isCorporate,
            String customerTitle,
            String providerid)
        {
            var thirdPartyParams = new ThirdPartyParams
                {
                    AuthData = new AuthData(url, login, password, token),
                    Corporate = isCorporate,
                    CustomerTitle = customerTitle,
                    ProviderId = providerid
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


        private FolderContentWrapper ToFolderContentWrapper(object folderId, Guid userIdOrGroupId, FilterType filterType)
        {
            return new FolderContentWrapper(
                _fileStorageService.GetFolderItems(folderId.ToString(),
                                                   _context.StartIndex.ToString(CultureInfo.InvariantCulture),
                                                   _context.Count.ToString(CultureInfo.InvariantCulture),
                                                   ((int)filterType).ToString(CultureInfo.InvariantCulture),
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