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


#region Import

using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Resources;
using System;
using System.Linq;
using System.Text;
using System.Web;


#endregion

namespace ASC.Web.CRM.Controls.Contacts
{
    public partial class ContactFullCardView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Contacts/ContactFullCardView.ascx"); }
        }

        public Contact TargetContact { get; set; }

        public Contact TargetCompanyIfPerson { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            TargetCompanyIfPerson = TargetContact is Person && ((Person)TargetContact).CompanyID != 0 ?
                DaoFactory.ContactDao.GetByID(((Person)TargetContact).CompanyID) :
                null;

            RegisterClientScriptHelper.DataContactFullCardView(Page, TargetContact);

            ExecHistoryView();
            RegisterScript();
        }

        #endregion

        #region Methods

        protected void ExecHistoryView()
        {
            var historyViewControl = (HistoryView)LoadControl(HistoryView.Location);

            historyViewControl.TargetContactID = TargetContact.ID;
            historyViewControl.TargetEntityID = 0;
            historyViewControl.TargetEntityType = EntityType.Contact;

            _phHistoryView.Controls.Add(historyViewControl);
        }

        protected string GetMailingHistoryUrl()
        {
            var primaryEmail = DaoFactory.ContactInfoDao.GetList(TargetContact.ID, ContactInfoType.Email, null, true).FirstOrDefault();
            if (primaryEmail == null || string.IsNullOrEmpty(primaryEmail.Data))
            {
                return string.Empty;
            }

            var virtualPath = string.Format("addons/mail/#inbox/from={0}", HttpUtility.UrlEncode(primaryEmail.Data));
            return Studio.Utility.CommonLinkUtility.GetFullAbsolutePath(virtualPath);
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();


            var additionalContactsCount = 0;
            if (TargetContact is Company)
            {
                var members = DaoFactory.ContactDao.GetMembersIDsAndShareType(TargetContact.ID);
                foreach (var m in members) {
                    if (CRMSecurity.CanAccessTo(m.Key, EntityType.Person, m.Value, 0)) {
                        additionalContactsCount++;
                    }
                }
            }
            else if (TargetCompanyIfPerson != null && CRMSecurity.CanAccessTo(TargetCompanyIfPerson))
            {
                additionalContactsCount = 1;
            }

            sb.AppendFormat(@"
                    ASC.CRM.ContactFullCardView.init({0},{1},{2},{3},{4},{5},""{6}"",{7},{8});",
                TargetContact.ID,
                (TargetContact is Company).ToString().ToLower(),
                TargetContact is Person ? ((Person)TargetContact).CompanyID : 0,
                Global.TenantSettings.ChangeContactStatusGroupAuto != null ? Global.TenantSettings.ChangeContactStatusGroupAuto.ToString().ToLower() : "null",
                Global.TenantSettings.AddTagToContactGroupAuto != null ? Global.TenantSettings.AddTagToContactGroupAuto.ToString().ToLower() : "null",
                Global.TenantSettings.WriteMailToHistoryAuto.ToString().ToLower(),
                Studio.Core.FileSizeComment.GetFileImageSizeNote(CRMContactResource.ContactPhotoInfo, true),
                additionalContactsCount,
                CRMSecurity.CanEdit(TargetContact).ToString().ToLower()
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}