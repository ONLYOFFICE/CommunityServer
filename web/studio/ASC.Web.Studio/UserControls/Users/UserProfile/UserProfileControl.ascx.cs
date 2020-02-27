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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using Resources;

using LdapMapping = ASC.ActiveDirectory.Base.Settings.LdapSettings.MappingFields; 

namespace ASC.Web.Studio.UserControls.Users
{
    public partial class UserProfileControl : UserControl
    {
        protected class RoleUser
        {
            public string Class { get; set; }
            public string Title { get; set; }
        }

        public ProfileHelper UserProfileHelper { get; set; }

        protected UserInfo UserInfo { get; set; }

        protected bool ShowSocialLogins { get; set; }

        protected bool ShowPrimaryMobile;

        protected bool ShowTfaAppSettings { get; set; }

        protected string BirthDayText { get; set; }

        protected AllowedActions Actions { get; set; }

        protected List<LdapMapping> LdapFields { get; set; }

        protected bool IsAdmin { get; set; }

        protected bool IsVisitor { get; set; }

        protected int HappyBirthday { get; set; }

        protected RoleUser Role { get; set; }

        public string MainImgUrl
        {
            get { return UserPhotoManager.GetMaxPhotoURL(UserInfo.ID); }
        }

        protected bool ShowUserLocation
        {
            get { return UserInfo != null && !string.IsNullOrEmpty(UserInfo.Location) && !string.IsNullOrEmpty(UserInfo.Location.Trim()) && !"null".Equals(UserInfo.Location.Trim(), StringComparison.InvariantCultureIgnoreCase); }
        }

        protected string JoinAffilliateLink
        {
            get { return WebConfigurationManager.AppSettings["web.affiliates.link"]; }
        }

        protected bool IsPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        protected List<GroupInfo> Groups { get; set; }

        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/UserProfileControl.ascx"; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserProfileHelper == null)
            {
                UserProfileHelper = new ProfileHelper(SecurityContext.CurrentAccount.ID.ToString());
            }
            UserInfo = UserProfileHelper.UserInfo;
            ShowSocialLogins = UserInfo.IsMe();

            IsAdmin = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin() ||
                      WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);

            IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();

            if (!IsAdmin && (UserInfo.Status != EmployeeStatus.Active))
            {
                Response.Redirect(CommonLinkUtility.GetFullAbsolutePath("~/products/people/"), true);
            }

            Role = GetRole();

            Actions = new AllowedActions(UserInfo);

            LdapFields = ASC.ActiveDirectory.Base.Settings.LdapSettings.GetImportedFields;

            HappyBirthday = CheckHappyBirthday();

            ContactPhones.DataSource = UserProfileHelper.Phones;
            ContactPhones.DataBind();

            ContactEmails.DataSource = UserProfileHelper.Emails;
            ContactEmails.DataBind();

            ContactMessengers.DataSource = UserProfileHelper.Messengers;
            ContactMessengers.DataBind();

            ContactSoccontacts.DataSource = UserProfileHelper.Contacts;
            ContactSoccontacts.DataBind();

            _deleteProfileContainer.Options.IsPopup = true;

            Page.RegisterStyle("~/UserControls/Users/UserProfile/css/userprofilecontrol_style.less")
                .RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/UserControls/Users/UserProfile/js/userprofilecontrol.js"));

            if (Actions.AllowEdit)
            {
                _editControlsHolder.Controls.Add(LoadControl(PwdTool.Location));
            }
            if (Actions.AllowEdit || (UserInfo.IsOwner() && IsAdmin))
            {
                var control = (UserEmailChange)LoadControl(UserEmailChange.Location);
                control.UserInfo = UserInfo;
                control.RegisterStylesAndScripts = false;
                userEmailChange.Controls.Add(control);
            }

            if (ShowSocialLogins && AccountLinkControl.IsNotEmpty)
            {
                var accountLink = (AccountLinkControl)LoadControl(AccountLinkControl.Location);
                accountLink.ClientCallback = "loginCallback";
                accountLink.SettingsView = true;
                _accountPlaceholder.Controls.Add(accountLink);
            }

            var emailControl = (UserEmailControl)LoadControl(UserEmailControl.Location);
            emailControl.User = UserInfo;
            emailControl.Viewer = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            _phEmailControlsHolder.Controls.Add(emailControl);

            var photoControl = (LoadPhotoControl)LoadControl(LoadPhotoControl.Location);
            photoControl.User = UserInfo;
            loadPhotoWindow.Controls.Add(photoControl);

            if (UserInfo.IsMe() && SetupInfo.EnabledCultures.Count > 1)
            {
                _phLanguage.Controls.Add(LoadControl(UserLanguage.Location));
            }

            if ((UserInfo.IsLDAP() && !String.IsNullOrEmpty(UserInfo.MobilePhone))
                || !String.IsNullOrEmpty(UserInfo.MobilePhone)
                || UserInfo.IsMe())
            {
                ShowPrimaryMobile = true;
                if (Actions.AllowEdit && (!UserInfo.IsLDAP() || UserInfo.IsLDAP() && !LdapFields.Contains(LdapMapping.MobilePhoneAttribute)))
                {
                    var changeMobile = (ChangeMobileNumber)LoadControl(ChangeMobileNumber.Location);
                    changeMobile.User = UserInfo;
                    ChangeMobileHolder.Controls.Add(changeMobile);
                }
            }

            if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.Enable && TfaAppUserSettings.EnableForUser(UserInfo.ID) && (UserInfo.IsMe() || IsAdmin))
            {
                ShowTfaAppSettings = true;

                if (UserInfo.IsMe() || IsAdmin)
                {
                    var resetApp = (ResetAppDialog)LoadControl(ResetAppDialog.Location);
                    resetApp.User = UserInfo;
                    _backupCodesPlaceholder.Controls.Add(resetApp);
                }
                if (UserInfo.IsMe())
                {
                    var showBackup = (ShowBackupCodesDialog)LoadControl(ShowBackupCodesDialog.Location);
                    showBackup.User = UserInfo;
                    _backupCodesPlaceholder.Controls.Add(showBackup);
                }
            }

            if (UserInfo.BirthDate.HasValue)
            {
                switch (HappyBirthday)
                {
                    case 0:
                        BirthDayText = Resource.DrnToday;
                        break;
                    case 1:
                        BirthDayText = Resource.DrnTomorrow;
                        break;
                    case 2:
                        BirthDayText = Resource.In + " " + DateTimeExtension.Yet(2);
                        break;
                    case 3:
                        BirthDayText = Resource.In + " " + DateTimeExtension.Yet(3);
                        break;
                    default:
                        BirthDayText = String.Empty;
                        break;
                }
            }

            if (UserInfo.Status != EmployeeStatus.Terminated)
            {
                Groups = CoreContext.UserManager.GetUserGroups(UserInfo.ID).ToList();
            }
        }

        private RoleUser GetRole()
        {
            if (UserInfo.IsOwner())
            {
                return new RoleUser
                {
                    Class = "owner",
                    Title = Resource.Owner
                };
            }
            if (UserInfo.IsAdmin() || UserInfo.GetListAdminModules().Any())
            {
                return new RoleUser
                    {
                        Class = "admin",
                        Title = Resource.Administrator
                    };
            }
            if (UserInfo.IsVisitor())
            {
                return new RoleUser
                    {
                        Class = "guest",
                        Title = CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode()
                    };
            }
            return null;
        }

        private int CheckHappyBirthday()
        {
            if (!UserInfo.BirthDate.HasValue) return -1;

            var birthday = UserInfo.BirthDate.Value;

            var today = TenantUtil.DateTimeNow();
            var checkYear = (DateTime.IsLeapYear(today.Year) || birthday.Month == 2 && birthday.Day == 29)
                           ? 2000 : 2001;

            var checkToday = new DateTime(checkYear, today.Month, today.Day);
            var checkBirthday = new DateTime(checkYear, birthday.Month, birthday.Day);

            var days = checkBirthday.DayOfYear - checkToday.DayOfYear;

            if (days < 0) days += 365;
            return days;
        }
    }

    public class AllowedActions
    {
        public bool AllowEdit { get; private set; }
        public bool AllowAddOrDelete { get; private set; }

        public AllowedActions(UserInfo userInfo)
        {
            var isOwner = userInfo.IsOwner();
            var isMe = userInfo.IsMe();
            AllowAddOrDelete = SecurityContext.CheckPermissions(Constants.Action_AddRemoveUser) && (!isOwner || isMe);
            AllowEdit = SecurityContext.CheckPermissions(new UserSecurityProvider(userInfo.ID), Constants.Action_EditUser) && (!isOwner || isMe);
        }
    }
}