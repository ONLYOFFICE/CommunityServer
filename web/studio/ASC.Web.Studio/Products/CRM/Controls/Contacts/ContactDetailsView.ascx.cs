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
using System.Text;
using System.Web;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core;
using ASC.Web.Core.Mobile;
using ASC.Web.CRM.Classes;
using ASC.Web.Studio.Utility;

namespace ASC.Web.CRM.Controls.Contacts
{
    public partial class ContactDetailsView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Contacts/ContactDetailsView.ascx"); }
        }

        public Contact TargetContact { get; set; }

        protected bool ShowEventLinkToPanel
        {
            get
            {
                var dealsCount = DaoFactory.DealDao.GetDealsCount();
                var casesCount = DaoFactory.CasesDao.GetCasesCount();

                return dealsCount + casesCount > 0;
            }
        }

        protected bool MobileVer = false;

        protected string HelpLink { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            MobileVer = MobileDetector.IsMobile;
            HelpLink = CommonLinkUtility.GetHelpLink();

            ExecFullCardView();
            ExecTasksView();
            ExecFilesView();
            ExecDealsView();

            if (TargetContact is Company)
                ExecPeopleContainerView();

            Page.RegisterStyle("~/Products/Projects/App_Themes/default/css/allprojects.less")
                .RegisterBodyScripts("~/Products/Projects/js/base.js")
                .RegisterBodyScripts("~/Products/Projects/js/projects.js");
            RegisterScript();
        }

        #endregion

        #region Methods

        protected void ExecDealsView()
        {
            Page.RegisterClientScript(new Masters.ClientScripts.ExchangeRateViewData());
        }

        protected void ExecPeopleContainerView()
        {
            RegisterClientScriptHelper.DataListContactTab(Page, TargetContact.ID, EntityType.Company);
        }

        protected void ExecFullCardView()
        {
            var contactFullCardControl = (ContactFullCardView)LoadControl(ContactFullCardView.Location);
            contactFullCardControl.TargetContact = TargetContact;

            _phProfileView.Controls.Add(contactFullCardControl);
        }

        protected void ExecFilesView()
        {
            var filesViewControl = (Studio.UserControls.Common.Attachments.Attachments)LoadControl(Studio.UserControls.Common.Attachments.Attachments.Location);
            filesViewControl.EntityType = "contact";
            filesViewControl.ModuleName = "crm";
            _phFilesView.Controls.Add(filesViewControl);
        }

        protected void ExecTasksView()
        {
            RegisterClientScriptHelper.DataContactDetailsViewForTaskAction(Page, TargetContact);
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"
                    ASC.CRM.ContactDetailsView.init({0},{1},{2},{3},{4},{5});
                    {6}",

                TargetContact.ID,
                (TargetContact is Company).ToString().ToLower(),
                WebItemSecurity.IsAvailableForMe(WebItemManager.ProjectsProductID).ToString().ToLower(),
                (!string.IsNullOrEmpty(TwitterLoginProvider.TwitterDefaultAccessToken)).ToString().ToLower(),
                (int)TargetContact.ShareType,
                CRMSecurity.CanEdit(TargetContact).ToString().ToLower(),
                ShowEventLinkToPanel ? "" : "jq('#eventLinkToPanel').hide();"
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}