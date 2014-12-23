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
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Users;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using Resources;
using AjaxPro;

namespace ASC.Web.Studio.UserControls.Users
{
    public class AllowedActions
    {
        public bool AllowEdit { get; private set; }
        public bool AllowAddOrDelete { get; private set; }

        public AllowedActions(UserInfo userInfo)
        {
            var isOwner = userInfo.IsOwner();
            var isMe = userInfo.IsMe();
            AllowAddOrDelete = SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser) && (!isOwner || isMe);
            AllowEdit = SecurityContext.CheckPermissions(new UserSecurityProvider(userInfo.ID), ASC.Core.Users.Constants.Action_EditUser) && (!isOwner || isMe);
        }
    }

    [AjaxNamespace("AjaxPro.UserProfileControl")]
    public partial class UserProfileControl : UserControl
    {
        #region SavePhotoThumbnails

        public class SavePhotoThumbnails : IThumbnailsData
        {
            #region User ID

            private Guid userID;

            public Guid UserID
            {
                get
                {
                    if (Guid.Empty.Equals(userID))
                    {
                        userID = SecurityContext.CurrentAccount.ID;
                    }
                    return userID;
                }
                set { userID = value; }
            }

            #endregion

            public Bitmap MainImgBitmap
            {
                get { return UserPhotoManager.GetPhotoBitmap(UserID); }
            }

            public string MainImgUrl
            {
                get { return UserPhotoManager.GetPhotoAbsoluteWebPath(UserID); }
            }

            public List<ThumbnailItem> ThumbnailList
            {
                get
                {
                    var tmp = new List<ThumbnailItem>
                        {
                            new ThumbnailItem
                                {
                                    id = UserPhotoManager.BigFotoSize.ToString(),
                                    size = UserPhotoManager.BigFotoSize,
                                    imgUrl = UserPhotoManager.GetBigPhotoURL(UserID)
                                },
                            new ThumbnailItem
                                {
                                    id = UserPhotoManager.BigFotoSize.ToString(),
                                    size = UserPhotoManager.MediumFotoSize,
                                    imgUrl = UserPhotoManager.GetMediumPhotoURL(UserID)
                                },
                            new ThumbnailItem
                                {
                                    id = UserPhotoManager.BigFotoSize.ToString(),
                                    size = UserPhotoManager.SmallFotoSize,
                                    imgUrl = UserPhotoManager.GetSmallPhotoURL(UserID)
                                }
                        };
                    return tmp;
                }
            }

            public void Save(List<ThumbnailItem> bitmaps)
            {
                foreach (var item in bitmaps)
                    UserPhotoManager.SaveThumbnail(UserID, item.bitmap, MainImgBitmap.RawFormat);
            }
        }

        #endregion

        public ProfileHelper UserProfileHelper { get; set; }

        protected UserInfo UserInfo { get; set; }

        protected bool ShowSocialLogins { get; set; }

        protected bool ShowPrimaryMobile;

        protected string BirthDayText { get; set; }

        protected AllowedActions Actions { get; set; }

        protected bool IsAdmin { get; set; }
        protected bool IsVisitor { get; set; }

        protected int HappyBirthday { get; set; }

        protected class RoleUser
        {
            public string Class { get; set; }
            public string Title { get; set; }
        }

        protected RoleUser Role
        {
            get
            {
                var userRole = new RoleUser();
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


        public string MainImgUrl
        {
            get { return UserPhotoManager.GetPhotoAbsoluteWebPath(UserInfo.ID); }
        }

        protected bool UserHasAvatar
        {
            get { return !MainImgUrl.Contains("default/images/"); }
        }

        protected bool ShowUserLocation
        {
            get { return UserInfo != null && !string.IsNullOrEmpty(UserInfo.Location) && !string.IsNullOrEmpty(UserInfo.Location.Trim()) && !"null".Equals(UserInfo.Location.Trim(), StringComparison.InvariantCultureIgnoreCase); }
        }

        protected string JoinAffilliateLink
        {
            get { return WebConfigurationManager.AppSettings["web.affiliates.link"]; }
        }

        protected bool isPersonal
        {
            get { return CoreContext.Configuration.Personal; }
        }

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

            IsAdmin = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
            IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();

            if (!IsAdmin && (UserInfo.Status != EmployeeStatus.Active))
            {
                Response.Redirect(CommonLinkUtility.GetFullAbsolutePath("~/products/people/"), true);
            }

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Actions = new AllowedActions(UserInfo);

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

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/users/userprofile/css/userprofilecontrol_style.less"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/users/userprofile/js/userprofilecontrol.js"));

            if (Actions.AllowEdit)
            {
                _editControlsHolder.Controls.Add(LoadControl(PwdTool.Location));
            }
            if (Actions.AllowEdit || (UserInfo.IsOwner() && IsAdmin))
            {
                var control = (UserEmailChange)LoadControl(UserEmailChange.Location);
                control.UserInfo = UserInfo;
                userEmailChange.Controls.Add(control);
            }

            if (!MobileDetector.IsMobile)
            {
                var thumbnailEditorControl = (ThumbnailEditor)LoadControl(ThumbnailEditor.Location);
                thumbnailEditorControl.Title = Resource.TitleThumbnailPhoto;
                thumbnailEditorControl.BehaviorID = "UserPhotoThumbnail";
                thumbnailEditorControl.JcropMinSize = UserPhotoManager.SmallFotoSize;
                thumbnailEditorControl.JcropAspectRatio = 1;
                thumbnailEditorControl.SaveFunctionType = typeof(SavePhotoThumbnails);
                _editControlsHolder.Controls.Add(thumbnailEditorControl);
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
            loadPhotoWindow.Controls.Add(photoControl);

            if (UserInfo.IsMe())
            {
                _phLanguage.Controls.Add(LoadControl(UserLanguage.Location));
            }

            if (StudioSmsNotificationSettings.IsVisibleSettings && (Actions.AllowEdit && !String.IsNullOrEmpty(UserInfo.MobilePhone) || UserInfo.IsMe()))
            {
                ShowPrimaryMobile = true;
                var changeMobile = (ChangeMobileNumber)LoadControl(ChangeMobileNumber.Location);
                changeMobile.User = UserInfo;
                ChangeMobileHolder.Controls.Add(changeMobile);
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
                List<GroupInfo> Groups = CoreContext.UserManager.GetUserGroups(UserInfo.ID).ToList();
                if (!Groups.Any())
                {
                    DepartmentsRepeater.Visible = false;
                }
                else {
                   Groups.Sort((group1, group2) => String.Compare(group1.Name, group2.Name, StringComparison.Ordinal));
                }

                DepartmentsRepeater.DataSource = Groups;
                DepartmentsRepeater.DataBind();

            }
        }

        private int CheckHappyBirthday()
        {
            if (!UserInfo.BirthDate.HasValue) return -1;

            var now = TenantUtil.DateTimeNow();
            var birthday = UserInfo.BirthDate;
            var today = new DateTime(now.Year, now.Month, now.Day);

            var daysInMonth = DateTime.DaysInMonth(today.Year, birthday.Value.Month);
            if (daysInMonth < birthday.Value.Day)
            {
                return -1;
            }

            var fest = new DateTime(today.Year, birthday.Value.Month, birthday.Value.Day);

            if ((fest - today).Days < 0)
            {
                fest = new DateTime(today.Year + 1, birthday.Value.Month, birthday.Value.Day);
            }
            return (fest - today).Days;
        }

        #region Ajax

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SendInstructionsToDelete()
        {
            try
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                StudioNotifyService.Instance.SendMsgProfileDeletion(user.Email);
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentDeleteInstructions);

                return new {Status = 1, Message = String.Format(Resource.SuccessfullySentNotificationDeleteUserInfoMessage, "<b>" + user.Email + "</b>")};
            }
            catch(Exception e)
            {
                return new {Status = 0, e.Message};
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse JoinToAffiliateProgram()
        {
            var resp = new AjaxResponse();
            try
            {
                resp.rs1 = "1";
                resp.rs2 = AffiliateHelper.Join();
            }
            catch(Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }
            return resp;
        }

        #endregion
    }
}