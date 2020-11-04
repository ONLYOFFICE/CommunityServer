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
using System.Web;
using ASC.CRM.Core;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.Classes;
using System.Text;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Configuration;

namespace ASC.Web.CRM.Controls.Cases
{
    public partial class CasesDetailsView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Cases/CasesDetailsView.ascx"); }
        }

        public ASC.CRM.Core.Entities.Cases TargetCase { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            ExecFullCardView();
            ExecPeopleInCaseView();
            ExecFilesView();
            RegisterScript();
        }

        #endregion

        #region Methods

        public void ExecPeopleInCaseView()
        {
            RegisterClientScriptHelper.DataListContactTab(Page, TargetCase.ID, EntityType.Case);
        }

        protected void ExecFullCardView()
        {
            var dealFullCardControl = (CasesFullCardView)LoadControl(CasesFullCardView.Location);
            dealFullCardControl.TargetCase = TargetCase;
            _phProfileView.Controls.Add(dealFullCardControl);
        }

        public void ExecFilesView()
        {
            var filesViewControl = (Studio.UserControls.Common.Attachments.Attachments)LoadControl(Studio.UserControls.Common.Attachments.Attachments.Location);
            filesViewControl.EntityType = "case";
            filesViewControl.ModuleName = "crm";
            _phFilesView.Controls.Add(filesViewControl);
        }

        private void RegisterScript()
        {
            Page.RegisterInlineScript(@"ASC.CRM.CasesDetailsView.init();");
        }

        #endregion
    }
}