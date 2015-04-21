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
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Controls
{
    public partial class Tree : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("Tree/Tree.ascx"); }
        }

        public object FolderIDCurrentRoot { get; set; }
        public string AdditionalCssClass { get; set; }
        public bool WithoutTrash { get; set; }
        public bool WithNew { get; set; }
        public bool WithoutBaseScripts { get; set; }

        protected bool IsVisitor;

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterClientScript(typeof(Masters.ClientScripts.FilesConstantsResources));

            if (!WithoutBaseScripts)
            {
                Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("common.js"));
                Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("xsltmanager.js"));
                Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("templatemanager.js"));
                Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("servicemanager.js"));

                Page.RegisterStyleControl(FilesLinkUtility.FilesBaseAbsolutePath + "controls/thirdparty/thirdparty.css");
            }
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/products/files/controls/tree/tree.js"));

            Page.RegisterStyleControl(FilesLinkUtility.FilesBaseAbsolutePath + "controls/tree/tree.css");
            Page.RegisterStyleControl(FilesLinkUtility.FilesBaseAbsolutePath + "app_themes/<theme_folder>/leftmenu.less", true);

            if (Global.IsOutsider)
                WithoutTrash = true;

            IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();
        }
    }
}