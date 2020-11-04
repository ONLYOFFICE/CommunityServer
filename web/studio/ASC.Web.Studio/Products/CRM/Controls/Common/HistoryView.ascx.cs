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
            Page.RegisterClientScript(new Masters.ClientScripts.HistoryViewData());

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
                    var contact = DaoFactory.ContactDao.GetByID(TargetContactID);
                    if (contact.ShareType == ShareType.None)
                    {
                        users = CRMSecurity.GetAccessSubjectGuidsTo(contact);
                    }
                    break;
                case EntityType.Opportunity:
                    var deal = DaoFactory.DealDao.GetByID(TargetEntityID);
                    if (CRMSecurity.IsPrivate(deal))
                    {
                        users = CRMSecurity.GetAccessSubjectGuidsTo(deal);
                    }
                    break;
                case EntityType.Case:
                    var caseItem = DaoFactory.CasesDao.GetByID(TargetEntityID);
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
            Page.RegisterBodyScripts("~/UserControls/Common/ckeditor/ckeditor-connector.js");
            Page.RegisterInlineScript("ckeditorConnector.load(function () { ASC.CRM.HistoryView.historyCKEditor = jq('#historyCKEditor').ckeditor(function() { ASC.CRM.HistoryView.bindCKEditorEvents(); },{ toolbar: 'CrmHistory', height: '150'}).editor;});");
        }

        #endregion
    }
}