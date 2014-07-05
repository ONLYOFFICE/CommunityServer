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

            CanUpload = CommunitySecurity.CheckPermissions(Community.Wiki.Common.Constants.Action_UploadFile) && !MobileDetector.IsMobile;

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
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("WikiLogo150.png", WikiManager.ModuleId),
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
            return CoreContext.UserManager.GetUsers(file.UserID).RenderCustomProfileLink(CommunityProduct.ID, "", "linkMedium");
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