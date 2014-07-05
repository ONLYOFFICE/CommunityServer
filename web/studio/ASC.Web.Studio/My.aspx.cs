/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio
{
    public partial class MyStaff : MainPage
    {
        protected string UserName { get; private set; }

        public bool EditProfileFlag { get; private set; }

        private ProfileHelper _helper;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (CoreContext.Configuration.Personal)
            {
                Context.Response.Redirect(FilesLinkUtility.FilesBaseAbsolutePath);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();
            Page.RegisterBodyScripts(ResolveUrl("~/js/uploader/ajaxupload.js"));

            Master.DisabledSidePanel = true;

            _helper = new ProfileHelper(Request["user"]);
            UserName = _helper.UserInfo.DisplayUserName(true);

            if (Request.Params["action"] == "edit")
            {
                InitEditControl();
                EditProfileFlag = true;
            }
            else
            {
                InitProfileControl();
                EditProfileFlag = false;
            }

            Title = HeaderStringHelper.GetPageTitle(Resource.MyProfile);
        }

        private void InitScripts()
        {
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
    }
}