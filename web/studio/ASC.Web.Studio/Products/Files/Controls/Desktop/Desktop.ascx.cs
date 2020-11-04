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
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Files.Controls
{
    public partial class Desktop : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("Desktop/Desktop.ascx"); }
        }

        public AdditionalWhiteLabelSettings Setting;

        protected string AuthLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle(FilesLinkUtility.FilesBaseAbsolutePath + "Controls/Desktop/desktop.css");
            Page.RegisterBodyScripts("~/Products/Files/Controls/Desktop/desktop.js");

            desktopWelcomeDialog.Options.IsPopup = true;
            Setting = AdditionalWhiteLabelSettings.Instance;

            AuthLink = CommonLinkUtility.GetConfirmationUrlRelative(TenantProvider.CurrentTenantID, CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email, ConfirmType.Auth);
        }
    }
}