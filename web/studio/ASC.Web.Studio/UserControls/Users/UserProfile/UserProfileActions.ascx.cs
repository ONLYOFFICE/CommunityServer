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
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using System.Web;
using Resources;

using LdapMapping = ASC.ActiveDirectory.Base.Settings.LdapSettings.MappingFields;

namespace ASC.Web.Studio.UserControls.Users
{
    public partial class UserProfileActions : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/UserProfileActions.ascx"; }
        }

        public ProfileHelper ProfileHelper { get; set; }
        protected List<LdapMapping> LdapFields { get; set; }

        protected AllowedActions Actions;
        protected string ProfileEditLink;
        protected string ReassignDataLink;
        protected bool MyStaff;
        protected bool HasActions;
        protected bool IsAdmin;
        protected string SubscribeBtnText;

        protected void Page_Load(object sender, EventArgs e)
        {
            Actions = new AllowedActions(ProfileHelper.UserInfo);
            MyStaff = ProfileHelper.UserInfo.IsMe();
            HasActions = Actions.AllowEdit || Actions.AllowAddOrDelete;

            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            IsAdmin = currentUser.IsAdmin();

            SubscribeBtnText =
                StudioNotifyHelper.IsSubscribedToNotify(currentUser, Core.Notify.Actions.PeriodicNotify)
                    ? ResourceJS.TipsAndTricksUnsubscribeBtn
                    : ResourceJS.TipsAndTricksSubscribeBtn;

            if (HasActions && Actions.AllowAddOrDelete)
            {
                _phConfirmationDeleteUser.Controls.Add(LoadControl(ConfirmationDeleteUser.Location));
            }

            ProfileEditLink =
                Page is MyStaff
                    ? "/My.aspx?action=edit"
                    : "ProfileAction.aspx?action=edit&user=" + HttpUtility.UrlEncode(ProfileHelper.UserInfo.UserName);

            ReassignDataLink = "Reassigns.aspx?user=" + HttpUtility.UrlEncode(ProfileHelper.UserInfo.UserName);

            LdapFields = ASC.ActiveDirectory.Base.Settings.LdapSettings.GetImportedFields;

            if (Request.Params["email_change"] == "success")
            {
                Page.RegisterInlineScript(string.Format("toastr.success(\"{0}\");", Resource.ChangeEmailSuccess));
            }
        }
    }
}