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
using System;
using System.Web;


#endregion

namespace ASC.Web.CRM.Controls.Deals
{
    public partial class DealDetailsView : BaseUserControl
    {
        #region Property

        public Deal TargetDeal { get; set; }

        public static String Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Deals/DealDetailsView.ascx"); }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            ExecFullCardView();
            ExecPeopleView();
            ExecFilesView();
            RegisterScript();
        }

        #endregion

        #region Methods

        protected void ExecFullCardView()
        {
            var dealFullCardControl = (DealFullCardView)LoadControl(DealFullCardView.Location);
            dealFullCardControl.TargetDeal = TargetDeal;

            _phProfileView.Controls.Add(dealFullCardControl);
        }

        private void ExecPeopleView()
        {
            RegisterClientScriptHelper.DataListContactTab(Page, TargetDeal.ID, EntityType.Opportunity);
        }

        private void ExecFilesView()
        {
            var filesViewControl = (Studio.UserControls.Common.Attachments.Attachments)LoadControl(Studio.UserControls.Common.Attachments.Attachments.Location);
            filesViewControl.EntityType = "opportunity";
            filesViewControl.ModuleName = "crm";
            _phFilesView.Controls.Add(filesViewControl);

            //var mainContent = (MainContent) LoadControl(MainContent.Location);
            //mainContent.FolderIDCurrentRoot = FilesIntegration.RegisterBunch("crm", "opportunity", TargetDeal.ID.ToString());
            //mainContent.TitlePage = "crm";
            //_phFilesView.Controls.Add(mainContent);

            //if (FileUtility.ExtsImagePreviewed.Count != 0)
            //    _phFilesView.Controls.Add(LoadControl(FileViewer.Location));

            //if (!Core.Mobile.MobileDetector.IsMobile)
            //    _phFilesView.Controls.Add(LoadControl(ChunkUploadDialog.Location));

        }

        private void RegisterScript()
        {
            Page.RegisterInlineScript(@"ASC.CRM.DealDetailsView.init();");
        }

        #endregion
    }
}