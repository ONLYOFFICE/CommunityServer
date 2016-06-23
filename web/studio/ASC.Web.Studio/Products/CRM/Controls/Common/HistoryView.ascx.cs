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

using ASC.Core;
using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        #endregion

        #region Events

        private void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterClientScript(typeof(Masters.ClientScripts.HistoryViewData));

            _phfileUploader.Controls.Add(LoadControl(FileUploader.Location));

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
            Page.RegisterBodyScripts("~/usercontrols/common/ckeditor/ckeditor-connector.js");
            Page.RegisterInlineScript("ckeditorConnector.onReady(function () { ASC.CRM.HistoryView.historyCKEditor = jq('#historyCKEditor').ckeditor(function() { ASC.CRM.HistoryView.bindCKEditorEvents(); },{ toolbar: 'CrmHistory', height: '150'}).editor;});");
        }

        #endregion
    }
}