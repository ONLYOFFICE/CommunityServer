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
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Core.Tenants;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using Resources;
using Newtonsoft.Json;
using ASC.Web.Core.Utility;

using LdapMapping = ASC.ActiveDirectory.Base.Settings.LdapSettings.MappingFields;

namespace ASC.Web.Studio.UserControls.Users.UserProfile
{
    public partial class UserProfileEditControl : UserControl
    {
        protected class RoleUser
        {
            public string Class { get; set; }
            public string Title { get; set; }
        }

        #region Protected Fields

        protected bool IsPageEditProfileFlag;
        protected bool CurrentUserIsPeopleAdmin;
        protected bool CurrentUserIsMailAdmin;
        protected bool IsPersonal;
        protected bool IsTrial;
        protected string HelpLink;
        protected List<LdapMapping> LdapFields;

        protected UserInfo Profile;

        protected bool ProfileIsMe;
        protected bool ProfileIsOwner;
        protected bool ProfileIsAdmin;
        protected bool ProfileIsAnyModuleAdmin;
        protected bool ProfileIsVisitor;
        protected bool ProfileIsLdap;
        protected bool ProfileIsSso;
        protected bool ProfileHasAvatar;
        protected string ProfilePath;

        protected RoleUser ProfileRole;

        protected string FirstName;
        protected string LastName;
        protected string Email;
        protected string Phone;
        protected string Position;
        protected string Place;
        protected string Login;
        protected string Comment;
        protected string ProfileGender;
        protected string PhotoPath = UserPhotoManager.GetDefaultPhotoAbsoluteWebPath();
        protected string WorkFromDate = TenantUtil.DateTimeNow().ToString(DateTimeExtension.DateFormatPattern);
        protected string BirthDate;

        protected GroupInfo[] Departments;
        protected List<MyContact> SocContacts;
        protected List<MyContact> OtherContacts;

        protected string UserTypeSelectorClass;
        protected string UserTypeSelectorGuestItemClass;
        protected string UserTypeSelectorUserItemClass;

        protected int UserPasswordMinLength;
        protected bool UserPasswordUpperCase;
        protected bool UserPasswordDigits;
        protected bool UserPasswordSpecSymbols;

        protected string PageTitle = Resource.CreateNewProfile;
        protected string ButtonText = Resource.AddButton;

        #endregion

        #region Properies

        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/UserProfileEditControl.ascx"; }
        }

        #endregion

        #region events

        protected void Page_Load(object sender, EventArgs e)
        {
            IsPageEditProfileFlag = (Request["action"] == "edit");
            CurrentUserIsPeopleAdmin = WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);
            CurrentUserIsMailAdmin = WebItemSecurity.IsProductAdministrator(WebItemManager.MailProductID, SecurityContext.CurrentAccount.ID);
            IsPersonal = CoreContext.Configuration.Personal;
            IsTrial = TenantExtra.GetTenantQuota().Trial;
            HelpLink = CommonLinkUtility.GetHelpLink();
            LdapFields = ActiveDirectory.Base.Settings.LdapSettings.GetImportedFields;

            var profileHelper = new ProfileHelper(Request["user"]);

            InitProfile(profileHelper);

            CheckPermission();

            RegisterBodyScript();

            InitUserTypeSelector();

            InitPasswordSettings();

            if (IsPageEditProfileFlag)
            {
                PageTitle = Profile.DisplayUserName(false) + " - " + Resource.EditUserDialogTitle;
                ButtonText = Resource.SaveButton;

                InitProfileFields(profileHelper);

                RegisterStartupScript();
            }

            var photoControl = (LoadPhotoControl)LoadControl(LoadPhotoControl.Location);
            photoControl.User = IsPageEditProfileFlag ? Profile : null;
            loadPhotoWindow.Controls.Add(photoControl);

            Page.Title = HeaderStringHelper.GetPageTitle(PageTitle);
        }

        #endregion

        #region Private Methods

        private void InitProfile(ProfileHelper profileHelper)
        {
            Profile = profileHelper.UserInfo;

            ProfileIsMe = Profile.IsMe();
            ProfileIsOwner = Profile.IsOwner();
            ProfileIsAdmin = Profile.IsAdmin();

            ProfileIsAnyModuleAdmin = Profile.GetListAdminModules().Any();

            ProfileIsVisitor = Profile.IsVisitor();
            ProfileIsLdap = Profile.IsLDAP();
            ProfileIsSso = Profile.IsSSO();

            ProfileHasAvatar = Profile.HasAvatar();

            ProfilePath = CommonLinkUtility.GetUserProfile(Profile.ID);

            ProfileRole = GetRole();
        }

        private RoleUser GetRole()
        {
            if (ProfileIsOwner)
            {
                return new RoleUser
                {
                    Class = "owner",
                    Title = Resource.Owner
                };
            }
            if (ProfileIsAdmin || ProfileIsAnyModuleAdmin)
            {
                return new RoleUser
                {
                    Class = "admin",
                    Title = Resource.Administrator
                };
            }
            if (ProfileIsVisitor)
            {
                return new RoleUser
                {
                    Class = "guest",
                    Title = CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode()
                };
            }
            return null;
        }

        private void CheckPermission()
        {
            if (IsPageEditProfileFlag)
            {
                if (CurrentUserIsPeopleAdmin)
                {
                    if (ProfileIsOwner && !ProfileIsMe)
                        Response.Redirect("~/products/people/", true);
                }
                else
                {
                    if (!ProfileIsMe)
                        Response.Redirect("~/products/people/", true);
                }
            }
            else
            {
                if (!CurrentUserIsPeopleAdmin)
                    Response.Redirect("~/products/people/", true);
            }
        }

        private void RegisterBodyScript()
        {
            Page.RegisterBodyScripts("~/js/third-party/xregexp.js", "~/UserControls/Users/UserProfile/js/userprofileeditcontrol.js")
                .RegisterStyle("~/UserControls/Users/UserProfile/css/profileeditcontrol_style.less");
        }

        private void InitUserTypeSelector()
        {
            var canAddUser = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;

            var canEditType = SecurityContext.CheckPermissions(Constants.Action_AddRemoveUser) &&
                              (!(ProfileIsAdmin || ProfileIsAnyModuleAdmin) || !IsPageEditProfileFlag);

            var isVisitorType = (ProfileIsMe && !IsPageEditProfileFlag) ? Request["type"] == "guest" : ProfileIsVisitor;

            if (canAddUser)
            {
                if (isVisitorType && !canEditType)
                {
                    UserTypeSelectorClass = "disabled";
                    UserTypeSelectorGuestItemClass = "active";
                    UserTypeSelectorUserItemClass = "disabled";
                }
                else
                {
                    if (canEditType)
                    {
                        UserTypeSelectorClass = "";
                        UserTypeSelectorGuestItemClass = isVisitorType ? "active" : "";
                        UserTypeSelectorUserItemClass = isVisitorType ? "" : "active";
                    }
                    else
                    {
                        UserTypeSelectorClass = "disabled";
                        UserTypeSelectorGuestItemClass = "disabled";
                        UserTypeSelectorUserItemClass = "active";
                    }
                }
            }
            else
            {
                if (isVisitorType || !IsPageEditProfileFlag)
                {
                    UserTypeSelectorClass = canEditType ? "" : "disabled";
                    UserTypeSelectorGuestItemClass = "active";
                    UserTypeSelectorUserItemClass = "disabled";
                }
                else
                {
                    if (canEditType)
                    {
                        UserTypeSelectorClass = "";
                        UserTypeSelectorGuestItemClass = isVisitorType ? "active" : "";
                        UserTypeSelectorUserItemClass = isVisitorType ? "" : "active";
                    }
                    else
                    {
                        UserTypeSelectorClass = "disabled";
                        UserTypeSelectorGuestItemClass = "disabled";
                        UserTypeSelectorUserItemClass = "active";
                    }
                }
            }
        }

        private void InitPasswordSettings()
        {
            var passwordSettings = PasswordSettings.Load();

            UserPasswordMinLength = passwordSettings.MinLength;
            UserPasswordDigits = passwordSettings.Digits;
            UserPasswordSpecSymbols = passwordSettings.SpecSymbols;
            UserPasswordUpperCase = passwordSettings.UpperCase;
        }

        private void InitProfileFields(ProfileHelper profileHelper)
        {
            FirstName = Profile.FirstName.HtmlEncode();
            LastName = Profile.LastName.HtmlEncode();
            Email = Profile.Email.HtmlEncode();
            Phone = Profile.MobilePhone.HtmlEncode();
            Position = Profile.Title.HtmlEncode();
            Place = Profile.Location.HtmlEncode();
            Login = Profile.UserName.HtmlEncode();
            Comment = Profile.Notes.HtmlEncode();
            ProfileGender = Profile.Sex.HasValue ? Profile.Sex.Value ? "1" : "0" : "-1";
            PhotoPath = UserPhotoManager.GetMaxPhotoURL(Profile.ID);
            WorkFromDate = Profile.WorkFromDate.HasValue ? Profile.WorkFromDate.Value.ToShortDateString() : "";
            BirthDate = Profile.BirthDate.HasValue ? Profile.BirthDate.Value.ToShortDateString() : "";
            Departments = CoreContext.UserManager.GetUserGroups(Profile.ID);
            
            SocContacts = profileHelper.Contacts;

            OtherContacts = new List<MyContact>();
            OtherContacts.AddRange(profileHelper.Emails);
            OtherContacts.AddRange(profileHelper.Messengers);
            OtherContacts.AddRange(profileHelper.Phones);
        }

        private void RegisterStartupScript()
        {
            var script =
                    String.Format(
                        @"<script type='text/javascript'>
                                    var departmentsList = {0},
                                        socContacts = {1},
                                        otherContacts = {2},
                                        userId = {3},
                                        userSex = {4};
                        </script>",
                        JsonConvert.SerializeObject(Departments.Select(item => new
                        {
                            id = item.ID,
                            title = item.Name.HtmlEncode()
                        })),
                        JsonConvert.SerializeObject(SocContacts),
                        JsonConvert.SerializeObject(OtherContacts),
                        JsonConvert.SerializeObject(Profile.ID),
                        JsonConvert.SerializeObject(Profile.Sex));

            Page.ClientScript.RegisterStartupScript(GetType(), Guid.NewGuid().ToString(), script);
        }

        #endregion

        #region Protected Methods

        protected bool IsLdapField(LdapMapping field)
        {
            return ProfileIsLdap && LdapFields.Contains(field);
        }

        #endregion
    }
}