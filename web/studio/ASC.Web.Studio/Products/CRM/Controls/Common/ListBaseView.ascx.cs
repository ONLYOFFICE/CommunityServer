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
using System.Collections.Generic;
using System.Web;
using ASC.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.EmptyScreens;

namespace ASC.Web.CRM.Controls.Common
{
    public partial class ListBaseView : BaseUserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Common/ListBaseView.ascx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitPage();

            var hasactivity = DaoFactory.ContactDao.HasActivity();
            if (!hasactivity)
            {
                RenderDashboardEmptyScreen();
            }
        }


        private void InitPage()
        {
            Page.RegisterClientScript(new Masters.ClientScripts.ListViewData());
            Page.RegisterClientScript(new Masters.ClientScripts.ExchangeRateViewData());

            var privatePanel = (PrivatePanel)LoadControl(PrivatePanel.Location);
            var usersWhoHasAccess = new List<string> {CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser")};
            privatePanel.UsersWhoHasAccess = usersWhoHasAccess;
            privatePanel.DisabledUsers = new List<Guid> {SecurityContext.CurrentAccount.ID};
            privatePanel.HideNotifyPanel = true;
            _phPrivatePanel.Controls.Add(privatePanel);
        }


        protected void RenderDashboardEmptyScreen()
        {
            var dashboardEmptyScreen = (CRMDashboardEmptyScreen)Page.LoadControl(CRMDashboardEmptyScreen.Location);
            _phDashboardEmptyScreen.Controls.Add(dashboardEmptyScreen);
        }
    }
}