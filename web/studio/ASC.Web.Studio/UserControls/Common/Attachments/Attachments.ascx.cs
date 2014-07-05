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
using System.Web.UI;
using ASC.Web.Core.Files;
using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Common.Attachments
{
    public partial class Attachments : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/Attachments/Attachments.ascx"; }
        }

        public bool PortalDocUploaderVisible { get; set; }

        public string MenuNewDocument { get; set; }

        public string MenuUploadFile { get; set; }

        public string MenuProjectDocuments { get; set; }

        public string ModuleName { get; set; }

        public string EntityType { get; set; }

        public bool CanAddFile { get; set; }

        public int ProjectId { get; set; }

        public bool EmptyScreenVisible { get; set; }

        protected string ExtsWebPreviewed = string.Join(", ", FileUtility.ExtsWebPreviewed.ToArray());
        protected string ExtsWebEdited = string.Join(", ", FileUtility.ExtsWebEdited.ToArray());

        public Attachments()
        {
            PortalDocUploaderVisible = true;
            EmptyScreenVisible = true;
            MenuNewDocument = UserControlsCommonResource.NewFile;
            MenuUploadFile = UserControlsCommonResource.UploadFile;
            MenuProjectDocuments = UserControlsCommonResource.AttachOfProjectDocuments;

            EntityType = "";
            ModuleName = "";
            CanAddFile = true;
        }

        private void InitScripts()
        {
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/common/attachments/css/attachments.less"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/attachments/js/attachments.js"));
        }

        private void CreateEmptyPanel()
        {
            var buttons = "<a id='uploadFirstFile' class='baseLinkAction'>" + MenuUploadFile + "</a><br/>" +
                          "<a id='createFirstDocument' class='baseLinkAction'>" + MenuNewDocument + "</a>" +
                          "<span class='sort-down-black newDocComb'></span>";
            if (ModuleName != "crm")
            {
                buttons += "<br/><a id='attachProjDocuments' class='baseLinkAction'>" + MenuProjectDocuments + "</a>";
            }

            var emptyParticipantScreenControl = new EmptyScreenControl
                {
                    ImgSrc = VirtualPathUtility.ToAbsolute("~/UserControls/Common/Attachments/Images/documents-logo.png"),
                    Header = UserControlsCommonResource.EmptyListDocumentsHead,
                    Describe =
                        MobileDetector.IsMobile
                            ? UserControlsCommonResource.EmptyListDocumentsDescrMobile
                            : String.Format(UserControlsCommonResource.EmptyListDocumentsDescr,
                                            //create
                                            "<span class='hintCreate baseLinkAction' >", "</span>",
                                            //upload
                                            "<span class='hintUpload baseLinkAction' >", "</span>",
                                            //open
                                            "<span class='hintOpen baseLinkAction' >", "</span>",
                                            //edit
                                            "<span class='hintEdit baseLinkAction' >", "</span>"),
                    ButtonHTML = buttons
                };
            _phEmptyDocView.Controls.Add(emptyParticipantScreenControl);
        }

        private void InitProjectDocumentsPopup()
        {
            var projectDocumentsPopup = (ProjectDocumentsPopup.ProjectDocumentsPopup)LoadControl(ProjectDocumentsPopup.ProjectDocumentsPopup.Location);
            projectDocumentsPopup.ProjectId = ProjectId;
            _phDocUploader.Controls.Add(projectDocumentsPopup);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _hintPopup.Options.IsPopup = true;
            InitScripts();

            if (EmptyScreenVisible)
                CreateEmptyPanel();

            if (!TenantExtra.GetTenantQuota().DocsEdition)
            {
                TariffDocsEditionPlaceHolder.Controls.Add(LoadControl(TariffLimitExceed.Location));
            }

            if (ModuleName != "crm")
            {
                var projId = Request["prjID"];
                if (!String.IsNullOrEmpty(projId))
                {
                    ProjectId = Convert.ToInt32(projId);
                    InitProjectDocumentsPopup();
                }
                else
                {
                    ProjectId = 0;
                }
            }
            else
            {
                PortalDocUploaderVisible = false;
            }
        }
    }
}