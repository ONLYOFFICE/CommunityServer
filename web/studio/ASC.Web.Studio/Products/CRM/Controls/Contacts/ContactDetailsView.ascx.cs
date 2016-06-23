/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


#region Import

using System;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Thrdparty.Configuration;
using ASC.Web.Core;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.SocialMedia;

#endregion

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
                var dealsCount = Global.DaoFactory.GetDealDao().GetDealsCount();
                var casesCount = Global.DaoFactory.GetCasesDao().GetCasesCount();

                return dealsCount + casesCount > 0;
            }
        }

        protected bool MobileVer = false;

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            MobileVer = MobileDetector.IsMobile;

            ExecFullCardView();
            ExecTasksView();
            ExecFilesView();
            ExecDealsView();

            if (TargetContact is Company)
                ExecPeopleContainerView();

            Page.RegisterStyle("~/products/projects/app_themes/default/css/allprojects.less");
            Page.RegisterBodyScripts("~/products/projects/js/base.js");
            Page.RegisterBodyScripts("~/products/projects/js/projects.js");
            RegisterScript();
        }

        #endregion

        #region Methods

        protected void ExecDealsView()
        {
            Page.RegisterClientScript(typeof(Masters.ClientScripts.ExchangeRateViewData));
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
                    ASC.CRM.ContactDetailsView.init({0},{1},{2},{3},{4});
                    {5}",

                TargetContact.ID,
                (TargetContact is Company).ToString().ToLower(),
                WebItemSecurity.IsAvailableForUser(WebItemManager.ProjectsProductID.ToString(), SecurityContext.CurrentAccount.ID).ToString().ToLower(),
                (!string.IsNullOrEmpty(KeyStorage.Get(SocialMediaConstants.ConfigKeyTwitterDefaultAccessToken))).ToString().ToLower(),
                (int)TargetContact.ShareType,
                ShowEventLinkToPanel ? "" : "jq('#eventLinkToPanel').hide();"
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}