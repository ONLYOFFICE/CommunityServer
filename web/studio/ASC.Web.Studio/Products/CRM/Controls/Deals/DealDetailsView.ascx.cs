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
using System.Web;
using ASC.Web.CRM.Configuration;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility.Skins;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using System.Text;

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

        protected bool MobileVer = false;

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            MobileVer = MobileDetector.IsMobile;

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