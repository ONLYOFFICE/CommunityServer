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


using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using System;
using System.Web.UI;

namespace ASC.Web.Files.Controls
{
    public partial class ThirdParty : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("ThirdParty/ThirdParty.ascx"); }
        }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            ThirdPartyEditorTemp.Options.IsPopup = true;
            ThirdPartyDeleteTmp.Options.IsPopup = true;
            ThirdPartyNewAccountTmp.Options.IsPopup = true;
            ThirPartyConfirmMoveTmp.Options.IsPopup = true;
            thirdpartyToDocuSignDialog.Options.IsPopup = true;
            thirdpartyToDocuSignHelperDialog.Options.IsPopup = true;

            EmptyScreenThirdParty.Controls.Add(new EmptyScreenControl
                {
                    ID = "emptyThirdPartyContainer",
                    ImgSrc = PathProvider.GetImagePath("empty_screen_my.png"),
                    Header = FilesUCResource.ThirdPartyConnectAccounts,
                    HeaderDescribe = FilesUCResource.EmptyScreenThirdPartyDscr,
                    ButtonHTML = string.Format(@"<div class=""account-connect""><a class=""link dotline plus"">{0}</a></div>", FilesUCResource.AddAccount)
                });

            var docuSignSelectorContainer = (Tree)LoadControl(Tree.Location);
            docuSignSelectorContainer.ID = "docuSignFolderSelector";
            docuSignSelectorContainer.WithoutTrash = true;
            docuSignSelectorContainer.WithoutAdditionalFolder = true;
            DocuSignFolderSelectorHolder.Controls.Add(docuSignSelectorContainer);

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}