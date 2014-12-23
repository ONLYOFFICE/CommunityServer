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
using System.Web;
using System.Web.UI;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Files.Controls;
using ASC.Web.Studio.Controls.Common;
using Resources;

//using Resources;

namespace ASC.Web.Mail.Controls
{
    public partial class DocumentsPopup : UserControl
    {
        public string PopupName { get; set; }
        public static string Location { get { return "~/addons/mail/controls/documentspopup/DocumentsPopup.ascx"; } }
        public string LoaderImgSrc { get { return WebImageSupplier.GetAbsoluteWebPath("loader_32.gif"); } }
        public string AttachFilesButtonText { get { return UserControlsCommonResource.AttachFiles; } }
        public string CancelButtonText { get { return UserControlsCommonResource.CancelButton; } }

        public DocumentsPopup()
        {
            PopupName = UserControlsCommonResource.AttachFromDocuments;
        }

        private void InitScripts()
        {
            Page.RegisterStyleControl(ResolveUrl("~/addons/mail/controls/documentspopup/css/documentsPopup.less"));
            Page.RegisterBodyScripts(ResolveUrl("~/addons/mail/controls/documentspopup/js/documentsPopup.js"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _documentUploader.Options.IsPopup = true;
            InitScripts();
            var empty_participant_screen_control = new EmptyScreenControl
            {
                ImgSrc = VirtualPathUtility.ToAbsolute("~/addons/mail/controls/documentspopup/css/images/project-documents.png"),
                Header = "",
                HeaderDescribe = UserControlsCommonResource.EmptyDocsHeaderDescription,
                Describe = ""
            };
            _phEmptyDocView.Controls.Add(empty_participant_screen_control);

            var tree = (Tree)LoadControl(Tree.Location);
            tree.ID = "documentSelectorTree";
            tree.WithoutTrash = false;
            TreeHolder.Controls.Add(tree);
        }
    }
}