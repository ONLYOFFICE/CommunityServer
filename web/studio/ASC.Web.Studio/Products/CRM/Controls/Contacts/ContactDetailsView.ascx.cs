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

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/products/projects/app_themes/default/css/allprojects.css"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/products/projects/js/projects.js"));
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