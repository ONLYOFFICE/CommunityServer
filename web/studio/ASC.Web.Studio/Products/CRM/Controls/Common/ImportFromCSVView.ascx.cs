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

using ASC.Core;
using ASC.CRM.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ASC.Web.CRM.Controls.Common
{
    public partial class ImportFromCSVView : BaseUserControl
    {
        #region Property

        public static String Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Common/ImportFromCSVView.ascx"); }
        }

        public EntityType EntityType;

        protected String ImportFromCSVStepOneHeaderLabel;
        protected String ImportFromCSVStepTwoHeaderLabel;

        protected String ImportFromCSVStepOneDescriptionLabel;
        protected String ImportFromCSVStepTwoDescriptionLabel;

        protected String StartImportLabel;

        protected String ImportStartingPanelHeaderLabel;
        protected String ImportStartingPanelDescriptionLabel;
        protected String ImportStartingPanelButtonLabel;

        protected String GoToRedirectURL;

        protected String ImportImgSrc;

        #endregion

        #region Events

        protected void InitForContacts()
        {
            StartImportLabel = CRMContactResource.StartImport;

            ImportFromCSVStepOneHeaderLabel = CRMContactResource.ImportFromCSVStepOneHeader;
            ImportFromCSVStepTwoHeaderLabel = CRMContactResource.ImportFromCSVStepTwoHeader;

            ImportFromCSVStepOneDescriptionLabel = CRMContactResource.ImportFromCSVStepOneDescription;
            ImportFromCSVStepTwoDescriptionLabel = CRMContactResource.ImportFromCSVStepTwoDescription;


            ImportStartingPanelHeaderLabel = CRMContactResource.ImportStartingPanelHeader;
            ImportStartingPanelDescriptionLabel = CRMContactResource.ImportStartingPanelDescription;
            ImportStartingPanelButtonLabel = CRMContactResource.ImportStartingPanelButton;

            ImportImgSrc = WebImageSupplier.GetAbsoluteWebPath("import_contacts.png", ProductEntryPoint.ID);

            Page.RegisterClientScript(typeof(Masters.ClientScripts.ImportFromCSVViewDataContacts));
            RegisterClientScriptHelper.DataUserSelectorListView(Page, "_ImportContactsManager", null);
        }

        protected void InitForOpportunity()
        {
            StartImportLabel = CRMDealResource.StartImport;

            ImportFromCSVStepOneHeaderLabel = CRMDealResource.ImportFromCSVStepOneHeader;
            ImportFromCSVStepTwoHeaderLabel = CRMDealResource.ImportFromCSVStepTwoHeader;

            ImportFromCSVStepOneDescriptionLabel = CRMDealResource.ImportFromCSVStepOneDescription;
            ImportFromCSVStepTwoDescriptionLabel = CRMDealResource.ImportFromCSVStepTwoDescription;

            // 
            // ImportFromCSVStepTwoDescription

            ImportStartingPanelHeaderLabel = CRMDealResource.ImportStartingPanelHeader;
            ImportStartingPanelDescriptionLabel = CRMDealResource.ImportStartingPanelDescription;
            ImportStartingPanelButtonLabel = CRMDealResource.ImportStartingPanelButton;

            ImportImgSrc = WebImageSupplier.GetAbsoluteWebPath("import-opportunities.png", ProductEntryPoint.ID);

            Page.RegisterClientScript(typeof(Masters.ClientScripts.ImportFromCSVViewDataDeals));

            var privatePanel = (PrivatePanel)Page.LoadControl(PrivatePanel.Location);
            privatePanel.CheckBoxLabel = CRMDealResource.PrivatePanelCheckBoxLabel;
            privatePanel.IsPrivateItem = false;

            var usersWhoHasAccess = new List<string> {CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser")};
            privatePanel.UsersWhoHasAccess = usersWhoHasAccess;
            privatePanel.DisabledUsers = new List<Guid> {SecurityContext.CurrentAccount.ID};
            privatePanel.HideNotifyPanel = true;
            _phPrivatePanel.Controls.Add(privatePanel);
        }

        protected void InitForTask()
        {
            StartImportLabel = CRMTaskResource.StartImport;

            ImportFromCSVStepOneHeaderLabel = CRMTaskResource.ImportFromCSVStepOneHeader;
            ImportFromCSVStepTwoHeaderLabel = CRMTaskResource.ImportFromCSVStepTwoHeader;

            ImportFromCSVStepOneDescriptionLabel = CRMTaskResource.ImportFromCSVStepOneDescription;
            ImportFromCSVStepTwoDescriptionLabel = CRMTaskResource.ImportFromCSVStepTwoDescription;

            ImportStartingPanelHeaderLabel = CRMTaskResource.ImportStartingPanelHeader;
            ImportStartingPanelDescriptionLabel = CRMTaskResource.ImportStartingPanelDescription;
            ImportStartingPanelButtonLabel = CRMTaskResource.ImportStartingPanelButton;

            ImportImgSrc = WebImageSupplier.GetAbsoluteWebPath("import-tasks.png", ProductEntryPoint.ID);

            Page.RegisterClientScript(typeof(Masters.ClientScripts.ImportFromCSVViewDataTasks));
        }

        protected void InitForCase()
        {
            StartImportLabel = CRMCasesResource.StartImport;

            ImportFromCSVStepOneHeaderLabel = CRMCasesResource.ImportFromCSVStepOneHeader;
            ImportFromCSVStepTwoHeaderLabel = CRMCasesResource.ImportFromCSVStepTwoHeader;

            ImportFromCSVStepOneDescriptionLabel = CRMCasesResource.ImportFromCSVStepOneDescription;
            ImportFromCSVStepTwoDescriptionLabel = CRMCasesResource.ImportFromCSVStepTwoDescription;

            ImportStartingPanelHeaderLabel = CRMCasesResource.ImportStartingPanelHeader;
            ImportStartingPanelDescriptionLabel = CRMCasesResource.ImportStartingPanelDescription;
            ImportStartingPanelButtonLabel = CRMCasesResource.ImportStartingPanelButton;

            ImportImgSrc = WebImageSupplier.GetAbsoluteWebPath("import-cases.png", ProductEntryPoint.ID);

            Page.RegisterClientScript(typeof(Masters.ClientScripts.ImportFromCSVViewDataCases));
            
            var privatePanel = (PrivatePanel)Page.LoadControl(PrivatePanel.Location);
            privatePanel.CheckBoxLabel = CRMCasesResource.PrivatePanelCheckBoxLabel;
            privatePanel.IsPrivateItem = false;

            var usersWhoHasAccess = new List<string> {CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser")};

            privatePanel.UsersWhoHasAccess = usersWhoHasAccess;
            privatePanel.DisabledUsers = new List<Guid> {SecurityContext.CurrentAccount.ID};
            privatePanel.HideNotifyPanel = true;
            _phPrivatePanel.Controls.Add(privatePanel);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            switch (EntityType)
            {
                case EntityType.Contact:
                    GoToRedirectURL = "default.aspx";
                    InitForContacts();
                    break;
                case EntityType.Opportunity:
                    GoToRedirectURL = "deals.aspx";
                    InitForOpportunity();
                    break;
                case EntityType.Task:
                    GoToRedirectURL = "tasks.aspx";
                    InitForTask();
                    break;
                case EntityType.Case:
                    GoToRedirectURL = "cases.aspx";
                    InitForCase();
                    break;
            }

            RegisterScript();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.ImportFromCSVView.init(""{0}"", ""{1}"");",
                            (int)EntityType,
                            EntityType.ToString().ToLower());

            Page.RegisterInlineScript(sb.ToString());
        }

        public EncodingInfo[] GetEncodings()
        {
            return Encoding.GetEncodings().OrderBy(x => x.DisplayName).ToArray();
        }

        #endregion
    }
}