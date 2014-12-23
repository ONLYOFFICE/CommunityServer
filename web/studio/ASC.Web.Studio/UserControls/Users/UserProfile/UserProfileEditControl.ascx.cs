/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Text;
using ASC.Core;
using ASC.Core.Users;
using ASC.Core.Tenants;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using Resources;
using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls.Users.UserProfile
{
    public partial class UserProfileEditControl : UserControl
    {
        #region Properies

        public ProfileHelper ProfileHelper { get; private set; }

        protected UserInfo UserInfo { get; set; }

        protected bool IsVisitor
        {
            get
            {
                if (UserInfo.IsMe() && !IsPageEditProfileFlag)
                {
                    return Request["type"] == "guest";
                }

                return UserInfo.IsVisitor();
            }
        }

        protected class RoleUser
        {
            public string Class { get; set; }
            public string Title { get; set; }
        }
        protected RoleUser Role
        {
            get
            {
                RoleUser userRole = new RoleUser();
                if ((UserInfo.IsAdmin() || UserInfo.GetListAdminModules().Any()) && !UserInfo.IsOwner())
                {
                    userRole.Class = "admin";
                    userRole.Title = Resource.Administrator;
                }
                if (UserInfo.IsVisitor())
                {
                    userRole.Class = "guest";
                    userRole.Title = CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode();
                }
                if (UserInfo.IsOwner())
                {
                    userRole.Class = "owner";
                    userRole.Title = Resource.Owner;
                }
                return userRole;
            }
        }

        protected string Phone { get; set; }
        protected string ProfileGender { get; set; }
        protected List<MyContact> SocContacts { get; set; }
        protected List<MyContact> OtherContacts { get; set; }
        protected GroupInfo[] Departments { get; set; }
        protected bool CanAddUser { get; set; }
        protected bool CanEditType { get; private set; }

        protected bool IsPageEditProfileFlag { get; private set; }

        protected bool IsAdmin()
        {
            return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
        }

        private static bool CanEdit()
        {
            var curUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            return curUser.IsAdmin() || curUser.IsOwner();
        }
        protected bool isPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/UserProfileEditControl.ascx"; }
        }

        #endregion

        #region events

        protected void Page_Load(object sender, EventArgs e)
        {
            IsPageEditProfileFlag = (Request["action"] == "edit");

            ProfileHelper = new ProfileHelper(Request["user"]);
            UserInfo = ProfileHelper.UserInfo;

            if ((IsPageEditProfileFlag && !(UserInfo.IsMe() || CanEdit())) || (!IsPageEditProfileFlag && !IsAdmin()))
            {
                Response.Redirect("~/products/people/", true);
            }

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/users/userprofile/js/userprofileeditcontrol.js"));
            Page.RegisterStyleControl(ResolveUrl("~/usercontrols/users/userprofile/css/profileeditcontrol_style.less"));

            CanAddUser = TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers;

            CanEditType = SecurityContext.CheckPermissions(Constants.Action_AddRemoveUser) &&
                          (!(UserInfo.IsAdmin() || IsModuleAdmin()) || !IsPageEditProfileFlag);

            if (IsPageEditProfileFlag)
            {
                Phone = UserInfo.MobilePhone.HtmlEncode();
                ProfileGender = UserInfo.Sex.HasValue ? UserInfo.Sex.Value ? "1" : "0" : "-1";
                Departments = CoreContext.UserManager.GetUserGroups(UserInfo.ID);
                SocContacts = ProfileHelper.Contacts;
                OtherContacts = new List<MyContact>();
                OtherContacts.AddRange(ProfileHelper.Emails);
                OtherContacts.AddRange(ProfileHelper.Messengers);
                OtherContacts.AddRange(ProfileHelper.Phones);
                var deps = Departments.ToList();

                var script =
                    String.Format(
                        @"<script type='text/javascript'>
                                    var departmentsList = {0};
                                    var socContacts = {1};
                                    var otherContacts = {2};
                                    var userId= {3};
                                  
                </script>",
                        JsonConvert.SerializeObject(deps.ConvertAll(item => new
                            {
                                id = item.ID,
                                title = item.Name.HtmlEncode()
                            })),
                        JsonConvert.SerializeObject(SocContacts),
                        JsonConvert.SerializeObject(OtherContacts),
                        JsonConvert.SerializeObject(UserInfo.ID));
                Page.ClientScript.RegisterStartupScript(GetType(), Guid.NewGuid().ToString(), script);
            }

            var photoControl = (LoadPhotoControl)LoadControl(LoadPhotoControl.Location);
            loadPhotoWindow.Controls.Add(photoControl);

            Page.Title = HeaderStringHelper.GetPageTitle(GetTitle());
        }

        #endregion

        #region Methods

        public bool IsModuleAdmin()
        {
            return UserInfo.GetListAdminModules().Any();
        }

        public string GetTitle()
        {
            return IsPageEditProfileFlag
                       ? UserInfo.DisplayUserName(true) + " - " + Resource.EditUserDialogTitle
                       : Resource.CreateNewProfile;
        }

        public string GetFirstName()
        {
            return IsPageEditProfileFlag ? UserInfo.FirstName.HtmlEncode() : String.Empty;
        }

        public string GetLastName()
        {
            return IsPageEditProfileFlag ? UserInfo.LastName.HtmlEncode() : String.Empty;
        }

        public string GetPosition()
        {
            return IsPageEditProfileFlag ? UserInfo.Title.HtmlEncode() : String.Empty;
        }

        public string GetEmail()
        {
            return IsPageEditProfileFlag ? UserInfo.Email.HtmlEncode() : String.Empty;
        }

        public string GetPlace()
        {
            return IsPageEditProfileFlag ? UserInfo.Location.HtmlEncode() : String.Empty;
        }

        public string GetComment()
        {
            return IsPageEditProfileFlag ? UserInfo.Notes.HtmlEncode() : String.Empty;
        }

        public string GetTextButton()
        {
            return IsPageEditProfileFlag ? Resource.SaveButton : Resource.AddButton;
        }

        public string GetPhotoPath()
        {
            return IsPageEditProfileFlag ? UserPhotoManager.GetPhotoAbsoluteWebPath(UserInfo.ID) : UserPhotoManager.GetDefaultPhotoAbsoluteWebPath();
        }

        public string GetWorkFromDate()
        {
            return IsPageEditProfileFlag
                       ? UserInfo.WorkFromDate.HasValue ? UserInfo.WorkFromDate.Value.ToShortDateString() : String.Empty
                       : TenantUtil.DateTimeNow().ToString(DateTimeExtension.DateFormatPattern);
        }

        public string GetBirthDate()
        {
            return IsPageEditProfileFlag
                       ? UserInfo.BirthDate.HasValue ? UserInfo.BirthDate.Value.ToShortDateString() : String.Empty
                       : String.Empty;
        }


        private static IEnumerable<GroupInfo> GetChildDepartments(GroupInfo dep)
        {
            return Enumerable.Empty<GroupInfo>();
        }

        #endregion
    }
}