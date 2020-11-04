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
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Notify;
using Resources;

namespace ASC.Web.Studio.Core.Users
{
    /// <summary>
    /// Web studio user manager helper
    /// </summary>
    public sealed class UserManagerWrapper
    {
        private static bool TestUniqueUserName(string uniqueName)
        {
            if (String.IsNullOrEmpty(uniqueName))
                return false;
            return Equals(CoreContext.UserManager.GetUserByUserName(uniqueName), Constants.LostUser);
        }

        private static string MakeUniqueName(UserInfo userInfo)
        {
            if (string.IsNullOrEmpty(userInfo.Email))
                throw new ArgumentException(Resource.ErrorEmailEmpty, "userInfo");

            var uniqueName = new MailAddress(userInfo.Email).User;
            var startUniqueName = uniqueName;
            var i = 0;
            while (!TestUniqueUserName(uniqueName))
            {
                uniqueName = string.Format("{0}{1}", startUniqueName, (++i).ToString(CultureInfo.InvariantCulture));
            }
            return uniqueName;
        }

        public static bool CheckUniqueEmail(Guid userId, string email)
        {
            var foundUser = CoreContext.UserManager.GetUserByEmail(email);
            return Equals(foundUser, Constants.LostUser) || foundUser.ID == userId;
        }

        public static UserInfo AddUser(UserInfo userInfo, string passwordHash, bool afterInvite = false, bool notify = true, bool isVisitor = false, bool fromInviteLink = false, bool makeUniqueName = true)
        {
            if (userInfo == null) throw new ArgumentNullException("userInfo");

            if (!UserFormatter.IsValidUserName(userInfo.FirstName, userInfo.LastName))
                throw new Exception(Resource.ErrorIncorrectUserName);

            if (!CheckUniqueEmail(userInfo.ID, userInfo.Email))
                throw new Exception(CustomNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists"));
            if (makeUniqueName)
            {
                userInfo.UserName = MakeUniqueName(userInfo);
            }
            if (!userInfo.WorkFromDate.HasValue)
            {
                userInfo.WorkFromDate = TenantUtil.DateTimeNow();
            }

            if (!CoreContext.Configuration.Personal && !fromInviteLink)
            {
                userInfo.ActivationStatus = !afterInvite ? EmployeeActivationStatus.Pending : EmployeeActivationStatus.Activated;
            }

            var newUserInfo = CoreContext.UserManager.SaveUserInfo(userInfo, isVisitor);
            SecurityContext.SetUserPasswordHash(newUserInfo.ID, passwordHash);

            if (CoreContext.Configuration.Personal)
            {
                StudioNotifyService.Instance.SendUserWelcomePersonal(newUserInfo);
                return newUserInfo;
            }

            if ((newUserInfo.Status & EmployeeStatus.Active) == EmployeeStatus.Active && notify)
            {
                //NOTE: Notify user only if it's active
                if (afterInvite)
                {
                    if (isVisitor)
                    {
                        StudioNotifyService.Instance.GuestInfoAddedAfterInvite(newUserInfo);
                    }
                    else
                    {
                        StudioNotifyService.Instance.UserInfoAddedAfterInvite(newUserInfo);
                    }

                    if (fromInviteLink)
                    {
                        StudioNotifyService.Instance.SendEmailActivationInstructions(newUserInfo, newUserInfo.Email);
                    }
                }
                else
                {
                    //Send user invite
                    if (isVisitor)
                    {
                        StudioNotifyService.Instance.GuestInfoActivation(newUserInfo);
                    }
                    else
                    {
                        StudioNotifyService.Instance.UserInfoActivation(newUserInfo);
                    }

                }
            }

            if (isVisitor)
            {
                CoreContext.UserManager.AddUserIntoGroup(newUserInfo.ID, Constants.GroupVisitor.ID);
            }

            return newUserInfo;
        }

        #region Password

        public static void CheckPasswordPolicy(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
                throw new Exception(Resource.ErrorPasswordEmpty);

            var passwordSettingsObj = PasswordSettings.Load();

            if (!PasswordSettings.CheckPasswordRegex(passwordSettingsObj, password))
                throw new Exception(GenerateErrorMessage(passwordSettingsObj));
        }

        public static string SendUserPassword(string email)
        {
            email = (email ?? "").Trim();
            if (!email.TestEmailRegex()) throw new ArgumentNullException("email", Resource.ErrorNotCorrectEmail);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var settings = IPRestrictionsSettings.Load();
            if (settings.Enable && !IPSecurity.IPSecurity.Verify(tenant))
            {
                throw new Exception(Resource.ErrorAccessRestricted);
            }

            var userInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (!CoreContext.UserManager.UserExists(userInfo.ID) || string.IsNullOrEmpty(userInfo.Email))
            {
                return String.Format(Resource.ErrorUserNotFoundByEmail, email);
            }
            if (userInfo.Status == EmployeeStatus.Terminated)
            {
                return Resource.ErrorDisabledProfile;
            }
            if (userInfo.IsLDAP())
            {
                return Resource.CouldNotRecoverPasswordForLdapUser;
            }
            if (userInfo.IsSSO())
            {
                return Resource.CouldNotRecoverPasswordForSsoUser;
            }

            StudioNotifyService.Instance.UserPasswordChange(userInfo);

            var displayUserName = userInfo.DisplayUserName(false);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentPasswordChangeInstructions, displayUserName);

            return null;
        }

        public static string GeneratePassword()
        {
            return Guid.NewGuid().ToString();
        }

        internal static string GenerateErrorMessage(PasswordSettings passwordSettings)
        {
            var error = new StringBuilder();

            error.AppendFormat("{0} ", Resource.ErrorPasswordMessage);
            error.AppendFormat(Resource.ErrorPasswordLength, passwordSettings.MinLength, PasswordSettings.MaxLength);
            if (passwordSettings.UpperCase)
                error.AppendFormat(", {0}", Resource.ErrorPasswordNoUpperCase);
            if (passwordSettings.Digits)
                error.AppendFormat(", {0}", Resource.ErrorPasswordNoDigits);
            if (passwordSettings.SpecSymbols)
                error.AppendFormat(", {0}", Resource.ErrorPasswordNoSpecialSymbols);

            return error.ToString();
        }

        public static string GetPasswordHelpMessage()
        {
            var info = new StringBuilder();
            var passwordSettings = PasswordSettings.Load();
            info.AppendFormat("{0} ", Resource.ErrorPasswordMessageStart);
            info.AppendFormat(Resource.ErrorPasswordLength, passwordSettings.MinLength, PasswordSettings.MaxLength);
            if (passwordSettings.UpperCase)
                info.AppendFormat(", {0}", Resource.ErrorPasswordNoUpperCase);
            if (passwordSettings.Digits)
                info.AppendFormat(", {0}", Resource.ErrorPasswordNoDigits);
            if (passwordSettings.SpecSymbols)
                info.AppendFormat(", {0}", Resource.ErrorPasswordNoSpecialSymbols);

            return info.ToString();
        }

        #endregion

        public static bool ValidateEmail(string email)
        {
            const string pattern = @"^(([^<>()[\]\\.,;:\s@\""]+"
                                   + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                                   + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
            return new Regex(pattern, options).IsMatch(email);
        }
    }
}