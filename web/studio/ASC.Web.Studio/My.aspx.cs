/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System;
using System.Text;
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
        protected string UserName { get; private set; }
        protected string UserEmail { get; private set; }

        public bool EditProfileFlag { get; private set; }

        protected bool isPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        protected bool IsAdmin()
        {
            return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
        }

        protected ProfileHelper _helper;

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

            _helper = new ProfileHelper(Request["user"]);
            UserName = _helper.UserInfo.DisplayUserName(true);
            UserEmail = _helper.UserInfo.Email;

            if (Request.Params["action"] == "edit")
            {
                InitEditControl();
                EditProfileFlag = true;
            }
            else
            {
                InitProfileControl();
                InitTipsSettingsView();
                EditProfileFlag = false;
            }

            Title = HeaderStringHelper.GetPageTitle(Resource.MyProfile);
        }

        private void InitProfileControl()
        {
            var actions = (UserProfileActions)LoadControl(UserProfileActions.Location);
            actions.ProfileHelper = _helper;
            actionsHolder.Controls.Add(actions);

            var userProfileControl = (UserProfileControl)LoadControl(UserProfileControl.Location);
            userProfileControl.UserProfileHelper = _helper;
            _contentHolderForProfile.Controls.Add(userProfileControl);

            _phSubscriptionView.Controls.Add(LoadControl(UserSubscriptions.Location));
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