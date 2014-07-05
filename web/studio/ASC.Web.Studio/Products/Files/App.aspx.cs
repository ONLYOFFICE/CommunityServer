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
                if (ConvertCheck.Checked)
                {
                    FilesSettings.ConvertNotify = !ConvertCheck.Checked;
                }

                if (IsConvert)
                {
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
                FileType fileType;
                if (!Enum.TryParse(Request["fileType"], true, out fileType))
                {
                    fileType = FileType.Document;
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

            Master.Master.DisabledHelpTour = true;
            Master.Master.DisabledSidePanel = true;

            Master.Master.TopStudioPanel.DisableProductNavigation = true;
            Master.Master.TopStudioPanel.DisableUserInfo = true;
            Master.Master.TopStudioPanel.DisableSearch = true;
            Master.Master.TopStudioPanel.DisableSettings = true;
            Master.Master.TopStudioPanel.DisableVideo = true;
            Master.Master.TopStudioPanel.DisableTariff = true;

            Page.RegisterStyleControl(PathProvider.GetFileStaticRelativePath("app.css"));

            if (IsConvert)
            {
                ButtonConvert.Text = FilesCommonResource.AppButtonConvert;
                ConvertCheck.Text = FilesCommonResource.AppConvertCopyHide;
            }
            else
            {
                ButtonCreate.Text = FilesCommonResource.AppButtonCreate;
                InputName.MaxLength = Global.MaxTitle;
                InputName.Attributes.Add("placeholder", FilesJSResource.TitleNewFileText);
            }
        }
    }
}