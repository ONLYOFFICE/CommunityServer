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