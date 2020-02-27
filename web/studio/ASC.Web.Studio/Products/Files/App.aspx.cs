/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.HttpHandlers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Studio;
using Global = ASC.Web.Files.Classes.Global;

namespace ASC.Web.Files
{
    public partial class App : MainPage
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "app.aspx"; }
        }

        private string FileId
        {
            get { return Request[FilesLinkUtility.FileId]; }
        }

        protected bool IsConvert
        {
            get { return !string.IsNullOrEmpty(FileId); }
        }

        private string FolderId
        {
            get { return Request[FilesLinkUtility.FolderId]; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!IsConvert && string.IsNullOrEmpty(FolderId))
            {
                Response.Redirect(PathProvider.StartURL
                                  + "#error/" +
                                  HttpUtility.UrlEncode(FilesCommonResource.ErrorMassage_FileNotFound));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                if (IsConvert)
                {
                    if (ConvertCheck.Checked)
                    {
                        FilesSettings.ConvertNotify = !ConvertCheck.Checked;
                    }

                    Response.Redirect(ThirdPartyAppHandler.HandlerPath
                                      + "?" + FilesLinkUtility.Action + "=convert"
                                      + "&" + FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(FileId)
                                      + "&" + ThirdPartySelector.AppAttr + "=" + GoogleDriveApp.AppAttr,
                                      true);
                    return;
                }

                var fileName = string.IsNullOrEmpty(InputName.Text) ? FilesJSResource.TitleNewFileText : InputName.Text;
                fileName = fileName.Substring(0, Math.Min(fileName.Length, Global.MaxTitle - 5));
                fileName = Global.ReplaceInvalidCharsAndTruncate(fileName);

                var fileType = FileType.Document;
                if (!string.IsNullOrEmpty(Request[ButtonCreateSpreadsheet.UniqueID]))
                {
                    fileType = FileType.Spreadsheet;
                }
                else if (!string.IsNullOrEmpty(Request[ButtonCreatePresentation.UniqueID]))
                {
                    fileType = FileType.Presentation;
                }
                var ext = FileUtility.InternalExtension[fileType];
                fileName += ext;

                Response.Redirect(ThirdPartyAppHandler.HandlerPath
                                  + "?" + FilesLinkUtility.Action + "=create"
                                  + "&" + FilesLinkUtility.FolderId + "=" + HttpUtility.UrlEncode(FolderId)
                                  + "&" + FilesLinkUtility.FileTitle + "=" + HttpUtility.UrlEncode(fileName)
                                  + "&" + ThirdPartySelector.AppAttr + "=" + GoogleDriveApp.AppAttr,
                                  true);
                return;
            }

            Master.Master.DisabledSidePanel = true;
            Master.Master.TopStudioPanel.DisableProductNavigation = true;
            Master.Master.TopStudioPanel.DisableUserInfo = true;
            Master.Master.TopStudioPanel.DisableSearch = true;
            Master.Master.TopStudioPanel.DisableSettings = true;
            Master.Master.TopStudioPanel.DisableTariff = true;

            Page.RegisterStyle(PathProvider.GetFileStaticRelativePath, "app.css");
            Page.RegisterInlineScript(@"
jq("".files-app-create"").trackEvent(""files_app"", ""action-click"", ""create"");
jq("".files-app-convert"").trackEvent(""files_app"", ""action-click"", ""convert"");
");

            if (IsConvert)
            {
                ButtonConvert.Text = FilesCommonResource.AppButtonConvert;
                ConvertCheck.Text = FilesCommonResource.AppConvertCopyHide;
            }
            else
            {
                ButtonCreateDocument.Text = FilesUCResource.ButtonCreateDocument2;
                ButtonCreateSpreadsheet.Text = FilesUCResource.ButtonCreateSpreadsheet2;
                ButtonCreatePresentation.Text = FilesUCResource.ButtonCreatePresentation2;
                InputName.MaxLength = Global.MaxTitle;
                InputName.Attributes.Add("placeholder", FilesJSResource.TitleNewFileText);
            }
        }
    }
}