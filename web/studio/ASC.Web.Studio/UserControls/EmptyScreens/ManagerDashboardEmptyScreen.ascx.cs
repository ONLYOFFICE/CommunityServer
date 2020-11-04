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
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;

namespace ASC.Web.Studio.UserControls.EmptyScreens
{
    public partial class ManagerDashboardEmptyScreen : UserControl
    {
        public static string Location
        {
            get { return VirtualPathUtility.ToAbsolute("~/UserControls/EmptyScreens/ManagerDashboardEmptyScreen.ascx"); }
        }

        public static string CurrentUserName
        {
            get { return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).DisplayUserName(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected bool IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();

        protected bool ProductDisabled(Guid productId)
        {
            var product = WebItemManager.Instance[productId];
            return product == null || product.IsDisabled(SecurityContext.CurrentAccount.ID);
        }
    }
}