/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Controls.Common;
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

        public string MenuNewDocument { get; set; }

        public string MenuUploadFile { get; set; }

        public string ModuleName { get; set; }

        public string EntityType { get; set; }

        public bool CanAddFile { get; set; }

        public bool EmptyScreenVisible { get; set; }

        protected string HelpLink { get; set; }

        protected string ExtsWebPreviewed = string.Join(", ", FileUtility.ExtsWebPreviewed.ToArray());
        protected string ExtsWebEdited = string.Join(", ", FileUtility.ExtsWebEdited.ToArray());

        protected static bool EnableAsUploaded
        {
            get { return FileUtility.ExtsMustConvert.Any() && !string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl); }
        }

        public Attachments()
        {
            EmptyScreenVisible = true;
            MenuNewDocument = UserControlsCommonResource.NewFile;
            MenuUploadFile = UserControlsCommonResource.UploadFile;

            EntityType = "";
            ModuleName = "";
            CanAddFile = true;
        }

        private void InitScripts()
        {
            Page.RegisterStyle("~/UserControls/Common/Attachments/css/attachments.less")
                .RegisterBodyScripts("~/UserControls/Common/Attachments/js/attachments.js");
        }

        private void CreateEmptyPanel()
        {
            var buttons = "<a id='uploadFirstFile' class='baseLinkAction'>" + MenuUploadFile + "</a><br/>" +
                          "<a id='createFirstDocument' class='baseLinkAction'>" + MenuNewDocument + "</a>" +
                          "<span class='sort-down-black newDocComb'></span>";

            var emptyParticipantScreenControl = new EmptyScreenControl
                {
                    ImgSrc = VirtualPathUtility.ToAbsolute("~/UserControls/Common/Attachments/images/documents-logo.png"),
                    Header = UserControlsCommonResource.EmptyListDocumentsHead,
                    Describe = String.Format(FileUtility.ExtsWebEdited.Any() ? UserControlsCommonResource.EmptyListDocumentsDescr.HtmlEncode() : UserControlsCommonResource.EmptyListDocumentsDescrPoor.HtmlEncode(),
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

        private void InitViewers()
        {
            MediaViewersPlaceHolder.Controls.Add(LoadControl(Common.MediaPlayer.Location));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();
            InitViewers();

            if (EmptyScreenVisible)
                CreateEmptyPanel();

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}