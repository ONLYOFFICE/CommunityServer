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
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "App.aspx"; }
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
            Master.Master.TopStudioPanel.DisableGift = true;

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