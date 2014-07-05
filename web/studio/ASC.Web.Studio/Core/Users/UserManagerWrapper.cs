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
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Users
{
    /// <summary>
    /// Web studio user manager helper
    /// </summary>
    public sealed class UserManagerWrapper
    {
        public static Guid AdminID
        {
            get { return new Guid("00000000-0000-0000-0000-000000000ace"); }
        }

        private static bool TestUniqueUserName(string uniqueName)
        {
            if (String.IsNullOrEmpty(uniqueName))
                return false;
            return Equals(CoreContext.UserManager.GetUserByUserName(uniqueName), ASC.Core.Users.Constants.LostUser);
        }

        private static string MakeUniqueName(UserInfo userInfo)
        {
            if (string.IsNullOrEmpty(userInfo.Email))
                throw new ArgumentException(Resources.Resource.ErrorEmailEmpty, "userInfo");

            var uniqueName = new MailAddress(userInfo.Email).User;
            var startUniqueName = uniqueName;
            var i = 0;
            while (!TestUniqueUserName(uniqueName))
            {
                uniqueName = string.Format("{0}{1}", startUniqueName, (++i).ToString(CultureInfo.InvariantCulture));
            }
            return uniqueName;
        }

        public static bool CheckUniqueEmail(Guid userID, string email)
        {
            var foundUser = CoreContext.UserManager.GetUserByEmail(email);
            return Equals(foundUser, ASC.Core.Users.Constants.LostUser) || foundUser.ID == userID;
        }

        #region Add User

        public static UserInfo AddUser(UserInfo userInfo, string password)
        {
            return AddUser(userInfo, password, false);
        }

        public static UserInfo AddUser(UserInfo userInfo, string password, bool afterInvite)
        {
            return AddUser(userInfo, password, afterInvite, true);
        }

        public static UserInfo AddUser(UserInfo userInfo, string password, bool afterInvite, bool notify)
        {
            return AddUser(userInfo, password, afterInvite, notify, false);
        }

        public static UserInfo AddUser(UserInfo userInfo, string password, bool afterInvite, bool notify, bool isVisitor)
        {
            return AddUser(userInfo, password, afterInvite, notify, isVisitor, false);
        }

        public static UserInfo AddUser(UserInfo userInfo, string password, bool afterInvite, bool notify, bool isVisitor, bool fromInviteLink)
        {
            if (userInfo == null) throw new ArgumentNullException("userInfo");

            CheckPasswordPolicy(password);

            if (!CheckUniqueEmail(userInfo.ID, userInfo.Email))
                throw new Exception(CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists"));

            userInfo.UserName = MakeUniqueName(userInfo);
            if (!userInfo.WorkFromDate.HasValue)
            {
                userInfo.WorkFromDate = TenantUtil.DateTimeNow();
            }

            if (!fromInviteLink)
            {
                userInfo.ActivationStatus = !afterInvite ? EmployeeActivationStatus.Pending : EmployeeActivationStatus.Activated;
            }

            var newUserInfo = CoreContext.UserManager.SaveUserInfo(userInfo);
            CoreContext.Authentication.SetUserPassword(newUserInfo.ID, password);

            if ((newUserInfo.Status & EmployeeStatus.Active) == EmployeeStatus.Active && notify)
            {
                //NOTE: Notify user only if it's active
                if (afterInvite)
                {
                    if (isVisitor)
                    {
                        StudioNotifyService.Instance.GuestInfoAddedAfterInvite(newUserInfo, password);
                    }
                    else
                    {
                        StudioNotifyService.Instance.UserInfoAddedAfterInvite(newUserInfo, password);
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
                CoreContext.UserManager.AddUserIntoGroup(newUserInfo.ID, ASC.Core.Users.Constants.GroupVisitor.ID);
            }

            return newUserInfo;
        }

        #endregion

        #region Password

        public static void SetUserPassword(Guid userID, string password)
        {
            CheckPasswordPolicy(password);

            SecurityContext.SetUserPassword(userID, password);
            StudioNotifyService.Instance.UserPasswordChanged(userID, password);
        }

        public static void CheckPasswordPolicy(string password)
        {
            if (String.IsNullOrEmpty(password))
                throw new Exception(Resources.Resource.ErrorPasswordEmpty);

            var passwordSettingsObj =
                SettingsManager.Instance.LoadSettings<StudioPasswordSettings>(TenantProvider.CurrentTenantID);

            if (!CheckPasswordRegex(passwordSettingsObj, password))
                throw new Exception(GenerateErrorMessage(passwordSettingsObj));
        }

        public static void SendUserPassword(string email)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException("email");

            var userInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (!CoreContext.UserManager.UserExists(userInfo.ID) || string.IsNullOrEmpty(userInfo.Email))
            {
                throw new Exception(String.Format(Resources.Resource.ErrorUserNotFoundByEmail, email));
            }
            if (userInfo.Status == EmployeeStatus.Terminated)
            {
                throw new Exception(Resources.Resource.ErrorDisabledProfile);
            }
            StudioNotifyService.Instance.UserPasswordChange(userInfo);
        }

        private const string Noise = "1234567890mnbasdflkjqwerpoiqweyuvcxnzhdkqpsdk@%&;";

        public static string GeneratePassword()
        {
            var ps = SettingsManager.Instance.LoadSettings<StudioPasswordSettings>(TenantProvider.CurrentTenantID);

            return String.Format("{0}{1}{2}{3}",
                                 GeneratePassword(ps.MinLength, ps.MinLength, Noise.Substring(0, Noise.Length - 4)),
                                 ps.Digits ? GeneratePassword(1, 1, Noise.Substring(0, 10)) : String.Empty,
                                 ps.UpperCase ? GeneratePassword(1, 1, Noise.Substring(10, 20).ToUpper()) : String.Empty,
                                 ps.SpecSymbols ? GeneratePassword(1, 1, Noise.Substring(Noise.Length - 4, 4).ToUpper()) : String.Empty);
        }

        private static int _counter;

        internal static string GeneratePassword(int minLength, int maxLength, string noise)
        {
            var rnd = new Random(Interlocked.Increment(ref _counter));
            var length = minLength + rnd.Next(maxLength - maxLength);

            var pwd = string.Empty;
            while (length-- > 0)
            {
                pwd += noise.Substring(rnd.Next(noise.Length - 1), 1);
            }
            return pwd;
        }

        internal static string GenerateErrorMessage(StudioPasswordSettings passwordSettings)
        {
            var error = new StringBuilder();

            error.AppendFormat("{0} ", Resources.Resource.ErrorPasswordMessage);
            error.AppendFormat(Resources.Resource.ErrorPasswordShort, passwordSettings.MinLength);
            if (passwordSettings.UpperCase)
                error.AppendFormat(", {0}", Resources.Resource.ErrorPasswordNoUpperCase);
            if (passwordSettings.Digits)
                error.AppendFormat(", {0}", Resources.Resource.ErrorPasswordNoDigits);
            if (passwordSettings.SpecSymbols)
                error.AppendFormat(", {0}", Resources.Resource.ErrorPasswordNoSpecialSymbols);

            return error.ToString();
        }

        public static string GetPasswordHelpMessage()
        {
            var info = new StringBuilder();
            var passwordSettings = SettingsManager.Instance.LoadSettings<StudioPasswordSettings>(TenantProvider.CurrentTenantID);
            info.AppendFormat("{0} ", Resources.Resource.ErrorPasswordMessageStart);
            info.AppendFormat(Resources.Resource.ErrorPasswordShort, passwordSettings.MinLength);
            if (passwordSettings.UpperCase)
                info.AppendFormat(", {0}", Resources.Resource.ErrorPasswordNoUpperCase);
            if (passwordSettings.Digits)
                info.AppendFormat(", {0}", Resources.Resource.ErrorPasswordNoDigits);
            if (passwordSettings.SpecSymbols)
                info.AppendFormat(", {0}", Resources.Resource.ErrorPasswordNoSpecialSymbols);

            return info.ToString();
        }

        internal static bool CheckPasswordRegex(StudioPasswordSettings passwordSettings, string password)
        {
            var pwdBuilder = new StringBuilder(@"^(?=.*\p{Ll}{0,})");

            if (passwordSettings.Digits)
                pwdBuilder.Append(@"(?=.*\d)");

            if (passwordSettings.UpperCase)
                pwdBuilder.Append(@"(?=.*\p{Lu})");

            if (passwordSettings.SpecSymbols)
                pwdBuilder.Append(@"(?=.*[\W])");

            pwdBuilder.Append(@".{");
            pwdBuilder.Append(passwordSettings.MinLength);
            pwdBuilder.Append(@",}$");

            return new Regex(pwdBuilder.ToString()).IsMatch(password);
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