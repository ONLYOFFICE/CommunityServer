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