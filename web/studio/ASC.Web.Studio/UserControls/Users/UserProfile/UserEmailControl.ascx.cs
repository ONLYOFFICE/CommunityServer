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
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Studio.Core;

namespace ASC.Web.Studio.UserControls.Users.UserProfile
{
    public partial class UserEmailControl : System.Web.UI.UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/UserEmailControl.ascx"; }
        }

        /// <summary>
        /// The user represented by the control
        /// </summary>
        public UserInfo User { get; set; }

        /// <summary>
        /// The user who is viewing the page
        /// </summary>
        public UserInfo Viewer { get; set; }

        protected bool IsAdmin { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof (EmailOperationService));

            IsAdmin = Viewer.IsAdmin() || WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, Viewer.ID);
        }

        protected string RenderMailLinkAttribute()
        {
            return CoreContext.Configuration.Personal || WebItemManager.Instance[WebItemManager.MailProductID].IsDisabled()
                       ? "href=\"mailto:" + User.Email.HtmlEncode() + "\""
                       : "target=\"_blank\" href=\"" + VirtualPathUtility.ToAbsolute("~/addons/mail/#composeto/email=" + HttpUtility.UrlEncode(User.Email)) + "\"";
        }
    }
}