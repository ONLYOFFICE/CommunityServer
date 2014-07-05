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

#region Import

using System;
using System.Linq;
using System.Web;
using System.Text;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Resources;
using ASC.Web.Core.Mobile;

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

        protected bool MobileVer = false;

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            MobileVer = MobileDetector.IsMobile;

            TargetCompanyIfPerson = TargetContact is Person && ((Person)TargetContact).CompanyID != 0 ?
                Global.DaoFactory.GetContactDao().GetByID(((Person)TargetContact).CompanyID) :
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
            var primaryEmail = Global.DaoFactory.GetContactInfoDao().GetList(TargetContact.ID, ContactInfoType.Email, null, true).FirstOrDefault();
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
                additionalContactsCount = Global.DaoFactory.GetContactDao().GetMembersCount(TargetContact.ID);
            }
            else if (TargetCompanyIfPerson != null && CRMSecurity.CanAccessTo(TargetCompanyIfPerson))
            {
                additionalContactsCount = 1;
            }

            sb.AppendFormat(@"
                    ASC.CRM.ContactFullCardView.init({0},{1},{2},{3},{4},""{5}"", {6});",
                TargetContact.ID,
                (TargetContact is Company).ToString().ToLower(),
                TargetContact is Person ? ((Person)TargetContact).CompanyID : 0,
                Global.TenantSettings.ChangeContactStatusGroupAuto != null ? Global.TenantSettings.ChangeContactStatusGroupAuto.ToString().ToLower() : "null",
                Global.TenantSettings.AddTagToContactGroupAuto != null ? Global.TenantSettings.AddTagToContactGroupAuto.ToString().ToLower() : "null",
                Studio.Core.FileSizeComment.GetFileImageSizeNote(CRMContactResource.ContactPhotoInfo, true),
                additionalContactsCount
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}