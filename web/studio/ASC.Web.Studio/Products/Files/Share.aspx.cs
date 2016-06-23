/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Net;
using System.Text;
using System.Web;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Resources;
using ASC.Web.Studio;

namespace ASC.Web.Files
{
    public partial class Share : MainPage
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "share.aspx"; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (string.IsNullOrEmpty(Request[FilesLinkUtility.FileId]))
            {
                Response.Redirect(PathProvider.StartURL
                                  + "#error/" +
                                  HttpUtility.UrlEncode(FilesCommonResource.ErrorMassage_FileNotFound));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;

            var accessRights = (AccessRights) LoadControl(AccessRights.Location);
            accessRights.IsPopup = false;
            CommonContainerHolder.Controls.Add(accessRights);

            InitScript();
        }

        private void InitScript()
        {
            Page.RegisterStyle(FilesLinkUtility.FilesBaseAbsolutePath + "controls/accessrights/accessrights.css");
            Page.RegisterBodyScripts("~/js/third-party/zeroclipboard.js");

            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath, 
                "common.js", 
                "templatemanager.js",
                "servicemanager.js", 
                "ui.js");

            var fileId = Request[FilesLinkUtility.FileId];
            File file;
            using (var fileDao = Classes.Global.DaoFactory.GetFileDao())
            {
                file = fileDao.GetFile(fileId);
            }

            if (file == null)
            {
                Response.StatusCode = (int) HttpStatusCode.NotFound;
                return;
            }
            if (!Classes.Global.GetFilesSecurity().CanRead(file))
            {
                Response.StatusCode = (int) HttpStatusCode.Forbidden;
                return;
            }

            var script = new StringBuilder();
            script.AppendFormat("ASC.Files.Share.getSharedInfo(\"file_{0}\", \"{1}\", true, {2} === true);",
                                file.ID,
                                file.Title,
                                (file.RootFolderType == FolderType.COMMON).ToString().ToLower());
            Page.RegisterInlineScript(script.ToString());
        }
    }
}