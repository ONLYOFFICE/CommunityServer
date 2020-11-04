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

            Page.RegisterClientScript(new Masters.ClientScripts.ImportFromCSVViewDataContacts());
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

            Page.RegisterClientScript(new Masters.ClientScripts.ImportFromCSVViewDataDeals());

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

            Page.RegisterClientScript(new Masters.ClientScripts.ImportFromCSVViewDataTasks());
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

            Page.RegisterClientScript(new Masters.ClientScripts.ImportFromCSVViewDataCases());
            
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
                    GoToRedirectURL = "Default.aspx";
                    InitForContacts();
                    break;
                case EntityType.Opportunity:
                    GoToRedirectURL = "Deals.aspx";
                    InitForOpportunity();
                    break;
                case EntityType.Task:
                    GoToRedirectURL = "Tasks.aspx";
                    InitForTask();
                    break;
                case EntityType.Case:
                    GoToRedirectURL = "Cases.aspx";
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