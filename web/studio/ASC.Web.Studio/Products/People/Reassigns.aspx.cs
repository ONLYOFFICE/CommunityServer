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
using ASC.Web.People.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;

namespace ASC.Web.People
{
    public partial class Reassigns : MainPage
    {
        protected UserInfo UserInfo { get; private set; }

        protected string PageTitle { get; private set; }

        protected string HelpLink { get; private set; }

        protected string ProfileLink { get; private set; }

        protected bool RemoveData { get; private set; }

        protected bool DeleteProfile { get; private set; }

        protected bool IsAdmin()
        {
            return WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var username = Request["user"];

            if (!IsAdmin() || string.IsNullOrEmpty(username))
            {
                Response.Redirect("~/Products/People/", true);
            }

            UserInfo = CoreContext.UserManager.GetUserByUserName(Request["user"]);

            if(UserInfo.Status != EmployeeStatus.Terminated)
            {
                Response.Redirect("~/Products/People/", true);
            }

            RemoveData = string.Equals(Request["remove"], bool.TrueString, StringComparison.InvariantCultureIgnoreCase);

            DeleteProfile = string.Equals(Request["delete"], bool.TrueString, StringComparison.InvariantCultureIgnoreCase);

            PageTitle = UserInfo.DisplayUserName(false) + " - " + (RemoveData ? PeopleResource.RemovingData : PeopleResource.ReassignmentData);

            Title = HeaderStringHelper.GetPageTitle(PageTitle);

            PageTitle = HttpUtility.HtmlEncode(PageTitle);

            HelpLink = CommonLinkUtility.GetHelpLink();

            ProfileLink = CommonLinkUtility.GetUserProfile(UserInfo.ID);

            Page.RegisterInlineScript(string.Format("ASC.People.Reassigns.init(\"{0}\", {1});", UserInfo.ID, RemoveData.ToString().ToLowerInvariant()));
        }
    }
}