/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
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
            Master.Master.DisabledHelpTour = true;
            Master.Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;

            var accessRights = (AccessRights)LoadControl(AccessRights.Location);
            accessRights.IsPopup = false;
            CommonContainerHolder.Controls.Add(accessRights);

            InitScript();
        }

        private void InitScript()
        {
            Page.RegisterStyleControl(FilesLinkUtility.FilesBaseAbsolutePath + "controls/accessrights/accessrights.css");
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/third-party/zeroclipboard.js"));
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("common.js"));
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("templatemanager.js"));
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("servicemanager.js"));
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("ui.js"));

            var fileId = Request[FilesLinkUtility.FileId];
            File file;
            using (var fileDao = Classes.Global.DaoFactory.GetFileDao())
            {
                file = fileDao.GetFile(fileId);
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