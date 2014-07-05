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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Users;

namespace ASC.Web.Studio.UserControls.Common.HelpTour
{
    public partial class HelpTour : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/HelpTour/HelpTour.ascx"; }
        }

        private Guid currentModule;
        private int stepHelp;
        private List<HelpTourItem> items;
        private bool isAdmin;
        private bool isVisitor;

        protected void Page_Load(object sender, EventArgs e)
        {
            currentModule = Utility.CommonLinkUtility.GetProductID();
            stepHelp = UserHelpTourHelper.GetStep(currentModule);

            if(stepHelp == -1 || currentModule.Equals(Guid.Empty)) return;

            AjaxPro.Utility.RegisterTypeForAjax(typeof(UserHelpTourHelper));

            items = new List<HelpTourItem>();
            isAdmin = WebItemSecurity.IsProductAdministrator(currentModule, SecurityContext.CurrentAccount.ID);
            isVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();

            if (currentModule == WebItemManager.CommunityProductID)
            {
                InitCommunityItems();
            }
            if (currentModule == WebItemManager.CRMProductID)
            {
                InitCrmItems();
            }
            if (currentModule == WebItemManager.PeopleProductID)
            {
                InitPeopleItems();
            }
            if (currentModule == WebItemManager.ProjectsProductID)
            {
                InitProjectsItems();
            }
            if (currentModule == WebItemManager.DocumentsProductID)
            {
                InitDocumentsItems();
            }

            RegisterScript();
        }

        private void InitCommunityItems()
        {
            if (isAdmin)
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuCreateNewButton",
                    HeaderText = "ASC.Community.Resources.HelpTitleAddNew",
                    BodyText = "ASC.Community.Resources.HelpContentAddNew",
                    PositionTop = 10,
                    PositionLeft = 20
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuAccessRights",
                    HeaderText = "ASC.Community.Resources.HelpTitleSettings",
                    BodyText = "ASC.Community.Resources.HelpContentSettings",
                    PositionTop = -22,
                    PositionLeft = 70
                });
            }
            else if (isVisitor)
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = ".menu-list .menu-item:first-child",
                    HeaderText = "ASC.Community.Resources.HelpTitleNavigateRead",
                    BodyText = "ASC.Community.Resources.HelpContentNavigateRead",
                    PositionTop = 15,
                    PositionLeft = 180
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = ".product-menu",
                    HeaderText = "ASC.Community.Resources.HelpTitleSwitchModules",
                    BodyText = "ASC.Community.Resources.HelpContentSwitchModules",
                    PositionTop = 5,
                    PositionLeft = 30
                });
            }
            else
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuCreateNewButton",
                    HeaderText = "ASC.Community.Resources.HelpTitleAddNew",
                    BodyText = "ASC.Community.Resources.HelpContentAddNew",
                    PositionTop = 10,
                    PositionLeft = 30,
                    InvertDown = true
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = ".menu-list .menu-item:first-child",
                    HeaderText = "ASC.Community.Resources.HelpTitleNavigateRead",
                    BodyText = "ASC.Community.Resources.HelpContentNavigateRead",
                    PositionTop = 15,
                    PositionLeft = 180
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = ".product-menu",
                    HeaderText = "ASC.Community.Resources.HelpTitleSwitchModules",
                    BodyText = "ASC.Community.Resources.HelpContentSwitchModules",
                    PositionTop = 5,
                    PositionLeft = 30
                });
            }
        }

        private void InitCrmItems()
        {
            if (isAdmin)
            {
                items.Add(new HelpTourItem
                    {
                        JsSelector = "#menuCreateNewButton",
                        HeaderText = "ASC.CRM.Resources.CRMJSResource.HelpTitleCreateNew",
                        BodyText = "ASC.CRM.Resources.CRMJSResource.HelpContentCreateNew",
                        PositionTop = 10,
                        PositionLeft = 20
                    });
                items.Add(new HelpTourItem
                    {
                        JsSelector = "#menuOtherActionsButton",
                        HeaderText = "ASC.CRM.Resources.CRMJSResource.HelpTitleAddContacts",
                        BodyText = "ASC.CRM.Resources.CRMJSResource.HelpContentAddContacts",
                        PositionTop = 10,
                        PositionLeft = 10
                    });
                items.Add(new HelpTourItem
                    {
                        JsSelector = "#menuCreateWebsite",
                        HeaderText = "ASC.CRM.Resources.CRMJSResource.HelpTitleCreateWebsite",
                        BodyText = "ASC.CRM.Resources.CRMJSResource.HelpContentCreateWebsite",
                        PositionTop = -22,
                        PositionLeft = 70
                    });
                items.Add(new HelpTourItem
                    {
                        JsSelector = "#menuAccessRights",
                        HeaderText = "ASC.CRM.Resources.CRMJSResource.HelpTitleAccessRights",
                        BodyText = "ASC.CRM.Resources.CRMJSResource.HelpContentAccessRights",
                        PositionTop = -22,
                        PositionLeft = 70
                    });
                items.Add(new HelpTourItem
                    {
                        JsSelector = ".product-menu",
                        HeaderText = "ASC.CRM.Resources.CRMJSResource.HelpTitleSwitchModules",
                        BodyText = "ASC.CRM.Resources.CRMJSResource.HelpContentSwitchModulesAdmin",
                        PositionTop = 10,
                        PositionLeft = 30
                    });
            }
            else
            {
                items.Add(new HelpTourItem
                    {
                        JsSelector = "#menuCreateNewButton",
                        HeaderText = "ASC.CRM.Resources.CRMJSResource.HelpTitleCreateNew",
                        BodyText = "ASC.CRM.Resources.CRMJSResource.HelpContentCreateNew",
                        PositionTop = 10,
                        PositionLeft = 30,
                        InvertDown = true
                    });
                items.Add(new HelpTourItem
                    {
                        JsSelector = "#menuOtherActionsButton",
                        HeaderText = "ASC.CRM.Resources.CRMJSResource.HelpTitleAddContacts",
                        BodyText = "ASC.CRM.Resources.CRMJSResource.HelpContentAddContacts",
                        PositionTop = 10,
                        PositionLeft = 30
                    });
                items.Add(new HelpTourItem
                    {
                        JsSelector = ".product-menu",
                        HeaderText = "ASC.CRM.Resources.CRMJSResource.HelpTitleSwitchModules",
                        BodyText = "ASC.CRM.Resources.CRMJSResource.HelpContentSwitchModules",
                        PositionTop = 5,
                        PositionLeft = 30
                    });
            }
        }

        private void InitPeopleItems()
        {
            if (isAdmin)
            {
                items.Add(new HelpTourItem
                    {
                        JsSelector = "#menuCreateNewButton",
                        HeaderText = "ASC.People.Resources.PeopleJSResource.HelpTitleAddNew",
                        BodyText = "ASC.People.Resources.PeopleJSResource.HelpContentAddNew",
                        PositionTop = 10,
                        PositionLeft = 20
                    });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuOtherActionsButton",
                    HeaderText = "ASC.People.Resources.PeopleJSResource.HelpTitleAddMore",
                    BodyText = "ASC.People.Resources.PeopleJSResource.HelpContentAddMore",
                    PositionTop = 10,
                    PositionLeft = 10
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "div[id^=peopleMenu]:first",
                    HeaderText = "ASC.People.Resources.PeopleJSResource.HelpTitleEditProfile",
                    BodyText = "ASC.People.Resources.PeopleJSResource.HelpContentEditProfile",
                    PositionTop = -22,
                    PositionLeft = 70
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuAccessRights",
                    HeaderText = "ASC.People.Resources.PeopleJSResource.HelpTitleAccessRights",
                    BodyText = "ASC.People.Resources.PeopleJSResource.HelpContentAccessRights",
                    PositionTop = -22,
                    PositionLeft = 70
                });
            }
            else
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = "div[id^=peopleMenu]:first",
                    HeaderText = "ASC.People.Resources.PeopleJSResource.HelpTitleEditProfile",
                    BodyText = "ASC.People.Resources.PeopleJSResource.HelpContentEditProfile",
                    PositionTop = -22,
                    PositionLeft = 70
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "tr[id^=user] td a.link.bold:first",
                    HeaderText = "ASC.People.Resources.PeopleJSResource.HelpTitleViewUser",
                    BodyText = "ASC.People.Resources.PeopleJSResource.HelpContentViewUser",
                    PositionTop = -27,
                    PositionLeft = 10
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = ".product-menu",
                    HeaderText = "ASC.People.Resources.PeopleJSResource.HelpTitleSwitchModules",
                    BodyText = "ASC.People.Resources.PeopleJSResource.HelpContentSwitchModules",
                    PositionTop = 5,
                    PositionLeft = 30
                });
            }
        }

        private void InitProjectsItems()
        {
            if (isAdmin)
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuCreateNewButton",
                    HeaderText = "ASC.Projects.Resources.ProjectsJSResource.HelpTitleStartNewProject",
                    BodyText = "ASC.Projects.Resources.ProjectsJSResource.HelpContentStartNewProject",
                    PositionTop = 10,
                    PositionLeft = 20
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuImport",
                    HeaderText = "ASC.Projects.Resources.ProjectsJSResource.HelpTitleImportProjects",
                    BodyText = "ASC.Projects.Resources.ProjectsJSResource.HelpContentImportProjects",
                    PositionTop = -22,
                    PositionLeft = 0
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuAccessRights",
                    HeaderText = "ASC.Projects.Resources.ProjectsJSResource.HelpTitleManageAccessRights",
                    BodyText = "ASC.Projects.Resources.ProjectsJSResource.HelpContentManageAccessRights",
                    PositionTop = -23,
                    PositionLeft = 18
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = ".product-menu",
                    HeaderText = "ASC.Projects.Resources.ProjectsJSResource.HelpTitleSwitchModules",
                    BodyText = "ASC.Projects.Resources.ProjectsJSResource.HelpContentSwitchModulesAd",
                    PositionTop = 10,
                    PositionLeft = 30
                });
            }
            else
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuMyProjects",
                    HeaderText = "ASC.Projects.Resources.ProjectsJSResource.HelpTitleViewProjects",
                    BodyText = "ASC.Projects.Resources.ProjectsJSResource.HelpContentViewProjects",
                    PositionTop = 10,
                    PositionLeft = 10,
                    InvertDown = true
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuFollowedProjects",
                    HeaderText = "ASC.Projects.Resources.ProjectsJSResource.HelpTitleFollowProjects",
                    BodyText = "ASC.Projects.Resources.ProjectsJSResource.HelpContentFollowProjects",
                    PositionTop = 5,
                    PositionLeft = 30,
                    InvertDown = true
                });

                items.Add(new HelpTourItem
                    {
                        JsSelector = "#ProjectsAdvansedFilter",
                        HeaderText = "ASC.Projects.Resources.ProjectsJSResource.HelpTitleUseFilter",
                        BodyText = "ASC.Projects.Resources.ProjectsJSResource.HelpContentUseFilter",
                        PositionTop = 5,
                        PositionLeft = 70
                    });

                items.Add(new HelpTourItem
                {
                    JsSelector = ".product-menu",
                    HeaderText = "ASC.Projects.Resources.ProjectsJSResource.HelpTitleSwitchModules",
                    BodyText = "ASC.Projects.Resources.ProjectsJSResource.HelpContentSwitchModules",
                    PositionTop = 10,
                    PositionLeft = 30
                });
            }
        }

        private void InitDocumentsItems()
        {
            if (isAdmin)
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuCreateNewButton",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleCreateFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentCreateFiles",
                    PositionTop = 10,
                    PositionLeft = 20
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#buttonUpload",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleUploadFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentUploadFiles",
                    PositionTop = 10,
                    PositionLeft = 10,
                    InvertDown = true
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#buttonThirdparty",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleConnectAccount",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentConnectAccount",
                    PositionTop = 10,
                    PositionLeft = 10
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "li.file-row[data-id^=file]:first div.name",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleViewFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentViewFiles",
                    PositionTop = -22,
                    PositionLeft = 70
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "div.share-button:first",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleShareFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentShareFiles",
                    PositionTop = -22,
                    PositionLeft = 30
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = ".product-menu",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleSwitchModules",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentSwitchModulesAd",
                    PositionTop = 10,
                    PositionLeft = 30
                });
            }
            else if (isVisitor)
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = "#buttonUpload",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleUploadFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentUploadFiles",
                    PositionTop = 10,
                    PositionLeft = 10,
                    InvertDown = true
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "li.file-row[data-id^=file]:first div.name",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleDownloadFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentDownloadFiles",
                    PositionTop = -22,
                    PositionLeft = 70
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = ".product-menu",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleSwitchModules",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentSwitchModules",
                    PositionTop = 10,
                    PositionLeft = 30
                });
            }
            else
            {
                items.Add(new HelpTourItem
                {
                    JsSelector = "#menuCreateNewButton",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleCreateFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentCreateFiles",
                    PositionTop = 10,
                    PositionLeft = 30,
                    InvertDown = true
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#buttonUpload",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleUploadFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentUploadFiles",
                    PositionTop = 10,
                    PositionLeft = 10,
                    InvertDown = true
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "#buttonThirdparty",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleConnectAccount",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentConnectAccount",
                    PositionTop = 10,
                    PositionLeft = 10
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "li.file-row[data-id^=file]:first div.name",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleViewFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentViewFiles",
                    PositionTop = -22,
                    PositionLeft = 70
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = "div.share-button:first",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleShareFiles",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentShareFiles",
                    PositionTop = -22,
                    PositionLeft = 30
                });
                items.Add(new HelpTourItem
                {
                    JsSelector = ".product-menu",
                    HeaderText = "ASC.Files.FilesJSResources.HelpTitleSwitchModules",
                    BodyText = "ASC.Files.FilesJSResources.HelpContentSwitchModules",
                    PositionTop = 10,
                    PositionLeft = 30
                });
            }
        }

        protected string GetItemsJsonString()
        {
            var sb = new StringBuilder();

            sb.Append("[");

            foreach (var item in items)
            {
                sb.AppendFormat(@"{{selector:""{0}"",header:{1},body:{2},top:{3},left:{4},posDown:{5}}},",
                item.JsSelector,
                item.HeaderText,
                item.BodyText,
                item.PositionTop,
                item.PositionLeft,
                item.InvertDown.ToString().ToLower());
            }

            sb.Append("]");

            return sb.ToString();
        }

        private void RegisterScript()
        {
            Page.RegisterBodyScripts(ResolveUrl("~/js/asc/plugins/jquery.sets.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/helptour/js/helptour.js"));
            
            var sb = new StringBuilder();

            sb.AppendFormat(@"                        
                        HelpTourSettings.Module = ""{0}"";
                        HelpTourSettings.Items = {1};
                        HelpTourSettings.Step = {2};",
                currentModule,
                GetItemsJsonString(),
                stepHelp
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        public class HelpTourItem
        {
            public string JsSelector { get; set; }
            public string HeaderText { get; set; }
            public string BodyText { get; set; }
            public int PositionTop { get; set; }
            public int PositionLeft { get; set; }
            public bool InvertDown { get; set; }
        }
    }
}