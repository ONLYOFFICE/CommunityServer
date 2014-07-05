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
using System.Text;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Import;

namespace ASC.Web.Files.Controls
{
    public partial class ThirdParty : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("ThirdParty/ThirdParty.ascx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(ResolveUrl("~/products/files/Controls/ThirdParty/thirdparty.js"));

            ThirdPartyEditorTemp.Options.IsPopup = true;
            ThirdPartyDeleteTmp.Options.IsPopup = true;
            ThirdPartyNewAccountTmp.Options.IsPopup = true;

            EmptyScreenThirdParty.Controls.Add(ThirdPartyEmptyScreen("emptyThirdPartyContainer"));
        }

        public static EmptyScreenControl ThirdPartyEmptyScreen(string controlId)
        {
            const string buttonString = "<span class=\"empty-container-add-account add-account-button {1}\" data-provider=\"{1}\"><a class=\"baseLinkAction\">{0}</a></span>";
            var buttonAddBoxNet = string.Format(buttonString, FilesUCResource.ButtonAddBoxNet, "BoxNet");
            var buttonAddDropBox = string.Format(buttonString, FilesUCResource.ButtonAddDropBox, "DropBox");
            var buttonAddGoogle = string.Format(buttonString, FilesUCResource.ButtonAddGoogle, "Google");
            var buttonAddGoogleDrive = string.Format(buttonString, FilesUCResource.ButtonAddGoogle, "GoogleDrive");
            var buttonAddSkyDrive = string.Format(buttonString, FilesUCResource.ButtonAddSkyDrive, "SkyDrive");
            var buttonAddSharePoint = string.Format(buttonString, FilesUCResource.ButtonAddSharePoint, "SharePoint");

            var buttons = new StringBuilder();

            if (ImportConfiguration.SupportBoxNetInclusion)
                buttons.Append(buttonAddBoxNet);

            if (ImportConfiguration.SupportDropboxInclusion)
                buttons.Append(buttonAddDropBox);

            if (ImportConfiguration.SupportGoogleInclusion)
                buttons.Append(buttonAddGoogle);

            if (ImportConfiguration.SupportGoogleDriveInclusion)
                buttons.Append(buttonAddGoogleDrive);

            if (ImportConfiguration.SupportSkyDriveInclusion)
                buttons.Append(buttonAddSkyDrive);

            if (ImportConfiguration.SupportSharePointInclusion)
                buttons.Append(buttonAddSharePoint);

            var descrMy = string.Format("");
            return new EmptyScreenControl
                {
                    ID = controlId,
                    ImgSrc = PathProvider.GetImagePath("empty_screen_my.png"),
                    Header = FilesUCResource.ThirdPartyConnectAccounts,
                    HeaderDescribe = FilesUCResource.EmptyScreenThirdPartyDscr,
                    Describe = descrMy,
                    ButtonHTML = buttons.ToString()
                };
        }
    }
}