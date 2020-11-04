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
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.UserControls.Users.TipsSettings;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;

using Resources;

namespace ASC.Web.Studio
{
    public partial class MyStaff : MainPage
    {
        protected bool EditProfileFlag { get; private set; }

        protected string PageTitle { get; private set; }

        protected bool IsPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        protected bool IsAdmin()
        {
            return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
        }

        protected ProfileHelper Helper;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOutsider())
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/js/uploader/ajaxupload.js")
                .RegisterBodyScripts("~/js/asc/core/my.js");

            Master.DisabledSidePanel = true;

            Helper = new ProfileHelper(Request["user"]);

            if (Request.Params["action"] == "edit")
            {
                InitEditControl();
                EditProfileFlag = true;
                PageTitle = Helper.UserInfo.DisplayUserName(false) + " - " + Resource.EditUserDialogTitle;
                Title = HeaderStringHelper.GetPageTitle(PageTitle);
            }
            else
            {
                InitProfileControl();
                InitTipsSettingsView();
                EditProfileFlag = false;
                PageTitle = Helper.UserInfo.DisplayUserName(false);
                Title = HeaderStringHelper.GetPageTitle(Resource.MyProfile);
            }
        }

        private void InitProfileControl()
        {
            var actions = (UserProfileActions)LoadControl(UserProfileActions.Location);
            actions.ProfileHelper = Helper;
            actionsHolder.Controls.Add(actions);

            var userProfileControl = (UserProfileControl)LoadControl(UserProfileControl.Location);
            userProfileControl.UserProfileHelper = Helper;
            _contentHolderForProfile.Controls.Add(userProfileControl);

            if (!IsPersonal)
            {
                _phSubscriptionView.Controls.Add(LoadControl(UserSubscriptions.Location));
            }
        }

        private void InitEditControl()
        {
            _contentHolderForEditForm.Controls.Add(LoadControl(UserProfileEditControl.Location));
        }

        private void InitTipsSettingsView()
        {
            if (!string.IsNullOrEmpty(Core.SetupInfo.TipsAddress))
                _phTipsSettingsView.Controls.Add(LoadControl(TipsSettings.Location));
        }
    }
}