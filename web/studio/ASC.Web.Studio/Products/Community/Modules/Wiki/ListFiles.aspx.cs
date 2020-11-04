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
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Community.Product;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.Core.Files;
using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Core;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Web.UserControls.Wiki.Resources;
using ASC.Web.UserControls.Wiki.UC;
using WikiResource = ASC.Web.UserControls.Wiki.Resources.WikiResource;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Community.Wiki
{
    public partial class ListFiles : WikiBasePage
    {
        protected bool hasFilesToDelete = false;

        protected bool HasFiles { get; set; }

        protected bool CanUpload { get; set; }

        protected bool CanDeleteTheFile(IWikiObjectOwner owner)
        {
            return CommunitySecurity.CheckPermissions(new WikiObjectsSecurityObject(owner), Community.Wiki.Common.Constants.Action_RemoveFile);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            UploadFileContainer.Options.IsPopup = true;
            UploadFileContainer.Options.OnCancelButtonClick = "javascript:HideUploadFileBox();";

            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            CanUpload = CommunitySecurity.CheckPermissions(Community.Wiki.Common.Constants.Action_UploadFile) && !currentUser.IsVisitor();

            if (IsPostBack) return;

            cmdFileUpload.Text = WikiResource.cmdUpload;
            cmdFileUploadCancel.Text = WikiResource.cmdCancel;
            BindRepeater();
        }

        protected void cmdFileUpload_Click(object sender, EventArgs e)
        {
            if (fuFile.HasFile)
            {
                var result = EditFile.DirectFileSave(SecurityContext.CurrentAccount.ID, fuFile, MapPath("~"), WikiSection.Section, ConfigLocation, TenantId, HttpContext.Current);

                switch (result)
                {
                    case SaveResult.Ok:
                        Response.Redirect(Request.GetUrlRewriter().ToString(), true);
                        return;
                    case SaveResult.FileEmpty:
                        WikiMaster.PrintInfoMessage(WikiResource.msgFileEmpty, InfoType.Alert);
                        break;
                    case SaveResult.FileSizeExceeded:
                        WikiMaster.PrintInfoMessage(FileSizeComment.GetFileSizeExceptionString(FileUploader.MaxUploadSize), InfoType.Alert);
                        break;
                }
            }

            BindRepeater();
        }

        protected string GetMaxFileUploadString()
        {
            var result = string.Empty;
            var lMaxFileSize = FileUploader.MaxUploadSize;
            if (lMaxFileSize == 0)
                return result;

            result = GetFileLengthToString(lMaxFileSize);
            result = string.Format(WikiUCResource.wikiMaxUploadSizeFormat, result);

            return result;
        }

        private void BindRepeater()
        {
            var resultToShow = Wiki.GetFiles();
            UpdateHasFilesToDelete(resultToShow);

            HasFiles = resultToShow.Count > 0;

            if (HasFiles)
            {
                rptFilesList.DataSource = resultToShow;
                rptFilesList.DataBind();
            }
            else
            {
                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("wikilogo150.png", WikiManager.ModuleId),
                        Header = WikiResource.EmptyScreenWikiFilesCaption,
                        Describe = WikiResource.EmptyScreenWikiFilesText,
                    };

                if (CanUpload)
                    emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='javascript:ShowUploadFileBox();'>{0}</a>", WikiResource.menu_AddNewFile);

                EmptyContent.Controls.Add(emptyScreenControl);
            }
        }

        private void UpdateHasFilesToDelete(IEnumerable<File> files)
        {
            var lsitToDelete = from file in files
                               where CommunitySecurity.CheckPermissions(new WikiObjectsSecurityObject(file), Community.Wiki.Common.Constants.Action_RemoveFile)
                               select file;

            hasFilesToDelete = false;
            if (lsitToDelete != null && lsitToDelete.Count() > 0)
            {
                hasFilesToDelete = true;
            }
        }

        protected string GetFileName(File file)
        {
            return file.FileName;
        }

        protected string GetFileViewLink(File file)
        {
            var ext = file.FileLocation.Split('.')[file.FileLocation.Split('.').Length - 1];
            if (!string.IsNullOrEmpty(ext) && !WikiFileHandler.ImageExtentions.Contains(ext.ToLower()))
            {
                return this.ResolveUrlLC(string.Format(WikiSection.Section.ImageHangler.UrlFormat, HttpUtility.UrlEncode(file.FileName)));
            }

            return this.ResolveUrlLC(string.Format(WikiSection.Section.ImageHangler.UrlFormat, HttpUtility.UrlEncode(file.FileName), TenantId));
        }

        protected string GetFileViewLinkPopUp(File file)
        {
            return string.Format(@"javascript:popitup('{0}'); return false;", GetFileViewLink(file));
        }

        protected string GetAuthor(File file)
        {
            return CoreContext.UserManager.GetUsers(file.UserID).RenderCustomProfileLink("", "linkMedium");
        }

        protected string GetDate(File file)
        {
            return string.Format("{0} {1}", file.Date.ToString("t"), file.Date.ToString("d"));
        }

        protected string GetFileTypeClass(File file)
        {
            var fileType = FileUtility.GetFileTypeByFileName(file.FileLocation);

            switch (fileType)
            {
                case FileType.Archive:
                    return "ftArchive";
                case FileType.Video:
                    return "ftVideo";
                case FileType.Audio:
                    return "ftAudio";
                case FileType.Image:
                    return "ftImage";
                case FileType.Spreadsheet:
                    return "ftSpreadsheet";
                case FileType.Presentation:
                    return "ftPresentation";
                case FileType.Document:
                    return "ftDocument";

                default:
                    return "ftUnknown";
            }
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            var fileName = (sender as LinkButton).CommandName;
            if (string.IsNullOrEmpty(fileName)) return;
            var file = Wiki.GetFile(fileName);
            Wiki.RemoveFile(fileName);
            if (file != null && !string.IsNullOrEmpty(file.FileLocation))
            {
                try
                {
                    EditFile.DeleteContent(file.FileLocation, ConfigLocation, PageWikiSection, TenantId, HttpContext.Current);
                }
                catch
                {
                }
            }
            BindRepeater();
        }
    }
}