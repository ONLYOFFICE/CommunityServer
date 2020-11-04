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

using System;
using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using System.Web;

#endregion

namespace ASC.Web.CRM.Controls.Cases
{
    public partial class CasesFullCardView : BaseUserControl
    {
        #region Property

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Cases/CasesFullCardView.ascx"); }
        }

        public ASC.CRM.Core.Entities.Cases TargetCase { get; set; }

        #endregion

        #region Events


        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterClientScriptHelper.DataCasesFullCardView(Page, TargetCase);
            ExecHistoryView();
            RegisterScript();
        }

        #endregion

        #region Methods

        public void ExecHistoryView()
        {
            var historyViewControl = (HistoryView) LoadControl(HistoryView.Location);

            historyViewControl.TargetEntityType = EntityType.Case;
            historyViewControl.TargetEntityID = TargetCase.ID;
            historyViewControl.TargetContactID = 0;

            _phHistoryView.Controls.Add(historyViewControl);
        }

        private void RegisterScript()
        {
            var script = @"ASC.CRM.CasesFullCardView.init();";

            Page.RegisterInlineScript(script);
        }

        #endregion
    }
}