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
using System.Web;
using System.Collections.Generic;
using System.Linq;
using ASC.CRM.Core;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Classes;
using ASC.Core;
using System.Text;
using ASC.Core.Tenants;
using ASC.Web.CRM.Core.Enums;

#endregion

namespace ASC.Web.CRM.Controls.Common
{
    public partial class HistoryView : BaseUserControl
    {
        #region Property

        public static String Location { get { return PathProvider.GetFileStaticRelativePath("Common/HistoryView.ascx"); } }

        public int TargetEntityID { get; set; }

        public EntityType TargetEntityType { get; set; }

        public int TargetContactID { get; set; }

        protected bool MobileVer = false;

        #endregion

        #region Events

        private void Page_Load(object sender, EventArgs e)
        {
            MobileVer = MobileDetector.IsMobile;

            Page.RegisterClientScript(typeof(Masters.ClientScripts.HistoryViewData));

            var userSelector = (Studio.UserControls.Users.UserSelector) LoadControl(Studio.UserControls.Users.UserSelector.Location);
            userSelector.BehaviorID = "UserSelector";

            if (!MobileVer)
            {
                _phfileUploader.Controls.Add(LoadControl(FileUploader.Location));
            }

            initUserSelectorListView();
            RegisterScript();
        }

        #endregion

        #region Methods

        private void initUserSelectorListView(){

            List<Guid> users = null;

            switch (TargetEntityType)
            {
                case EntityType.Contact:
                    var contact = Global.DaoFactory.GetContactDao().GetByID(TargetContactID);
                    if (contact.ShareType == ShareType.None)
                    {
                        users = CRMSecurity.GetAccessSubjectGuidsTo(contact);
                    }
                    break;
                case EntityType.Opportunity:
                    var deal = Global.DaoFactory.GetDealDao().GetByID(TargetEntityID);
                    if (CRMSecurity.IsPrivate(deal))
                    {
                        users = CRMSecurity.GetAccessSubjectGuidsTo(deal);
                    }
                    break;
                case EntityType.Case:
                    var caseItem = Global.DaoFactory.GetCasesDao().GetByID(TargetEntityID);
                    if (CRMSecurity.IsPrivate(caseItem))
                    {
                        users = CRMSecurity.GetAccessSubjectGuidsTo(caseItem);
                    }
                    break;
            }


            //init userSelectorListView
            if (users == null)
            {
                RegisterClientScriptHelper.DataHistoryView(Page, null);
            }
            else
            {
                users = users.Where(g => g != SecurityContext.CurrentAccount.ID).ToList();
                RegisterClientScriptHelper.DataHistoryView(Page, users);
            }
        }

        private void RegisterScript()
        {
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/common/ckeditor/ckeditor.js"));
            Page.RegisterInlineScript("CKEDITOR.replace('historyCKEditor', { toolbar: 'CrmHistory', height: '150'});");
        }

        #endregion
    }
}