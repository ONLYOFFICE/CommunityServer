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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Studio.Core;
using ASC.Core.Users;
using ASC.Core;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class ActivateEmailPanel : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/ActivateEmailPanel.ascx"; }
        }
        
        protected UserInfo CurrentUser
        {
            get { return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(EmailOperationService));

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("if (jq('div.mainPageLayout table.mainPageTable').hasClass('with-mainPageTableSidePanel'))jq('.info-box.excl').removeClass('display-none');");

            Page.RegisterInlineScript(stringBuilder.ToString());
        }
    }
}