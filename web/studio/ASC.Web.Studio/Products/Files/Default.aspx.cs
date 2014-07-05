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
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Import;
using ASC.Web.Files.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Users;

namespace ASC.Web.Files
{
    public partial class _Default : MainPage
    {
        protected bool AddCustomScript;

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadScripts();

            LoadControls();
        }

        private void LoadScripts()
        {
            Page.RegisterStyleControl(LoadControl(VirtualPathUtility.ToAbsolute("~/products/files/masters/styles.ascx")));
            Page.RegisterBodyScripts(LoadControl(VirtualPathUtility.ToAbsolute("~/products/files/masters/FilesScripts.ascx")));
        }

        private void LoadControls()
        {
            CommonSideHolder.Controls.Add(LoadControl(MainMenu.Location));

            var mainContent = (MainContent)LoadControl(MainContent.Location);
            mainContent.TitlePage = FilesCommonResource.TitlePage;
            CommonContainerHolder.Controls.Add(mainContent);

            CommonContainerHolder.Controls.Add(LoadControl(AccessRights.Location));

            if (CoreContext.Configuration.Personal)
            {
                UserLanguageHolder.Controls.Add(LoadControl(UserLanguage.Location));

                if (PersonalSettings.IsNewUser)
                {
                    PersonalSettings.IsNewUser = false;
                    AddCustomScript = SetupInfo.CustomScripts.Length != 0;
                }
            }

            if (ImportConfiguration.SupportInclusion
                && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                && (Classes.Global.IsAdministrator
                    || CoreContext.Configuration.Personal
                    || FilesSettings.EnableThirdParty))
            {
                SettingPanelHolder.Controls.Add(LoadControl(ThirdParty.Location));
            }
        }
    }
}