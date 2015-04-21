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


using System;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;
using IO = System.IO;

namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class EditFile : BaseUserControl
    {
        public string FileName
        {
            get
            {
                return ViewState["FileName"] == null ? string.Empty : ViewState["FileName"].ToString();
            }
            set { ViewState["FileName"] = value; }
        }

        private File _fileInfo;

        protected File CurrentFile
        {
            get
            {
                if (_fileInfo == null)
                {
                    if (string.IsNullOrEmpty(FileName))
                        return null;

                    _fileInfo = Wiki.GetFile(FileName);
                }
                return _fileInfo;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack) return;
            if (CurrentFile != null && !string.IsNullOrEmpty(CurrentFile.FileName))
            {
                RisePublishVersionInfo(CurrentFile);
            }
        }

        protected string GetUploadFileName()
        {
            return CurrentFile == null ? string.Empty : CurrentFile.UploadFileName;
        }

        protected string GetFileLink()
        {
            var file = Wiki.GetFile(FileName);
            if (file == null)
            {
                RisePageEmptyEvent();
                return string.Empty; // "nonefile.png";
            }

            var ext = file.FileLocation.Split('.')[file.FileLocation.Split('.').Length - 1];
            if (!string.IsNullOrEmpty(ext) && !WikiFileHandler.ImageExtentions.Contains(ext.ToLower()))
            {
                return string.Format(@"<a class=""linkMedium"" href=""{0}"" title=""{1}"">{2}</a>",
                                     ResolveUrl(string.Format(ImageHandlerUrlFormat, FileName)),
                                     file.FileName,
                                     Resources.WikiUCResource.wikiFileDownloadCaption);
            }

            return string.Format(@"<img src=""{0}"" style=""max-width:300px; max-height:200px"" />",
                                 ResolveUrl(string.Format(ImageHandlerUrlFormat, FileName)));
        }

        private static string GetFileLocation(string fileName)
        {
            var letter = (byte) fileName[0];

            var secondFolder = letter.ToString("x");
            var firstFolder = secondFolder.Substring(0, 1);

            var fileLocation = IO.Path.Combine(firstFolder, secondFolder);
            fileLocation = IO.Path.Combine(fileLocation, EncodeSafeName(fileName)); //TODO: encode nameprep here

            return fileLocation;
        }

        private static string EncodeSafeName(string fileName)
        {
            return fileName;
        }

        public static void DeleteTempContent(string fileName, string configLocation, WikiSection section, int tenantId, HttpContext context)
        {
            var storage = StorageFactory.GetStorage(tenantId.ToString(), section.DataStorage.ModuleName);
            storage.Delete(section.DataStorage.TempDomain, fileName);
        }

        public static void DeleteContent(string fileName, string configLocation, WikiSection section, int tenantId, HttpContext context)
        {
            var storage = StorageFactory.GetStorage(tenantId.ToString(), section.DataStorage.ModuleName);
            storage.Delete(section.DataStorage.DefaultDomain, fileName);
        }

        public static SaveResult MoveContentFromTemp(Guid userId, string fromFileName, string toFileName, string configLocation, WikiSection section, int tenantId, HttpContext context, string rootFile, out string _fileName)
        {
            var storage = StorageFactory.GetStorage(tenantId.ToString(), section.DataStorage.ModuleName);

            var fileName = toFileName;
            var fileLocation = GetFileLocation(fileName);
            var file = new File
                           {
                               FileName = fileName,
                               UploadFileName = fileName,
                               UserID = userId,
                               FileLocation = fileLocation,
                               FileSize = (int) storage.GetFileSize(section.DataStorage.TempDomain, fromFileName),
                           };

            var wiki = new WikiEngine();
            wiki.SaveFile(file);

            storage.Move(section.DataStorage.TempDomain, fromFileName, section.DataStorage.DefaultDomain, fileLocation);
            _fileName = file.FileName;

            return SaveResult.Ok;
        }

        public static SaveResult DirectFileSave(Guid userId, FileUpload fuFile, string rootFile, WikiSection section, string configLocation, int tenantId, HttpContext context)
        {
            if (!fuFile.HasFile)
                return SaveResult.FileEmpty;

            var wikiEngine = new WikiEngine();
            File file = null;

            try
            {
                file = wikiEngine.CreateOrUpdateFile(new File {FileName = fuFile.FileName, FileSize = fuFile.FileBytes.Length});
                FileContentSave(file.FileLocation, fuFile.FileBytes, section, configLocation, tenantId, context);
            }
            catch (TenantQuotaException)
            {
                if (file != null)
                    wikiEngine.RemoveFile(file.FileName);
                return SaveResult.FileSizeExceeded;
            }

            return SaveResult.Ok;
        }

        private static void FileContentSave(string location, byte[] fileContent, WikiSection section, string configLocation, int tenantId, HttpContext context)
        {
            var storage = StorageFactory.GetStorage(tenantId.ToString(), section.DataStorage.ModuleName);
            FileContentSave(storage, location, fileContent, section);
        }

        private static void FileContentSave(string location, byte[] fileContent, WikiSection section, int tenantId)
        {
            var storage = StorageFactory.GetStorage(tenantId.ToString(), section.DataStorage.ModuleName);
            FileContentSave(storage, location, fileContent, section);
        }

        private static void FileContentSave(IDataStore storage, string location, byte[] fileContent, WikiSection section)
        {
            using (var ms = new IO.MemoryStream(fileContent))
            {
                storage.Save(section.DataStorage.DefaultDomain, location, ms);
            }
        }

        public SaveResult Save(Guid userId)
        {
            string fileName;
            return Save(userId, out fileName);
        }

        public SaveResult Save(Guid userId, out string fileName)
        {
            fileName = string.Empty;
            if (!fuFile.HasFile)
                return SaveResult.FileEmpty;

            var file = CurrentFile ?? new File {FileName = fuFile.FileName, UploadFileName = fuFile.FileName};

            file.FileSize = fuFile.FileBytes.Length;

            file = Wiki.CreateOrUpdateFile(file);

            try
            {
                FileContentSave(file.FileLocation, fuFile.FileBytes, WikiSection.Section, TenantId);
            }
            catch (TenantQuotaException)
            {
                Wiki.RemoveFile(file.FileName);
                return SaveResult.FileSizeExceeded;
            }

            _fileInfo = file;

            RisePublishVersionInfo(file);
            fileName = file.FileName;

            return SaveResult.Ok;
        }
    }
}