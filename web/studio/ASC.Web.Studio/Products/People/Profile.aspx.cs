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
using System.Text;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Studio;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.UserControls.Users.TipsSettings;
using ASC.Web.Studio.Utility;

namespace ASC.Web.People
{
    public partial class Profile : MainPage
    {
        public ProfileHelper ProfileHelper;

        protected bool IsAdmin()
        {
            return WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ProfileHelper = new ProfileHelper(Request["user"]);

            Title = HeaderStringHelper.GetPageTitle(ProfileHelper.UserInfo.DisplayUserName(false));

            var control = (UserProfileControl)LoadControl(UserProfileControl.Location);
            control.UserProfileHelper = ProfileHelper;

            CommonContainerHolder.Controls.Add(control);

            var actions = (UserProfileActions)LoadControl(UserProfileActions.Location);
            actions.ProfileHelper = ProfileHelper;
            actionsHolder.Controls.Add(actions);

            if (ProfileHelper.UserInfo.IsMe())
            {
                InitSubscriptionView();
                InitTipsSettingsView();

                var script = new StringBuilder();
                script.Append("jq('#switcherSubscriptionButton').one('click',");
                script.Append("function() {");
                script.Append("if (!jq('#subscriptionBlockContainer').hasClass('subsLoaded') &&");
                script.Append("typeof (window.CommonSubscriptionManager) != 'undefined' &&");
                script.Append("typeof (window.CommonSubscriptionManager.LoadSubscriptions) === 'function') {");
                script.Append("window.CommonSubscriptionManager.LoadSubscriptions();");
                script.Append("jq('#subscriptionBlockContainer').addClass('subsLoaded');");
                script.Append("}});");

                Page.RegisterInlineScript(script.ToString());
            }
        }

        private void InitSubscriptionView()
        {
            _phSubscriptionView.Controls.Add(LoadControl(UserSubscriptions.Location));
        }

        private void InitTipsSettingsView()
        {
            if (!string.IsNullOrEmpty(Studio.Core.SetupInfo.TipsAddress))
                _phTipsSettingsView.Controls.Add(LoadControl(TipsSettings.Location));
        }
    }
}