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