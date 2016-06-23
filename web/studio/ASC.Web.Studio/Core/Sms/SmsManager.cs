/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using Resources;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ASC.Web.Studio.Core.SMS
{
    public static class SmsManager
    {
        private static readonly ICache CodeCache = AscCache.Default;

        public static string GetPhoneValueDigits(string mobilePhone)
        {
            var reg = new Regex(@"[^\d]");
            mobilePhone = reg.Replace(mobilePhone ?? "", String.Empty).Trim();
            return mobilePhone.Substring(0, Math.Min(64, mobilePhone.Length));
        }

        public static string BuildPhoneNoise(string mobilePhone)
        {
            if (String.IsNullOrEmpty(mobilePhone))
                return String.Empty;

            mobilePhone = GetPhoneValueDigits(mobilePhone);

            const int startLen = 4;
            const int endLen = 4;
            if (mobilePhone.Length < startLen + endLen)
                return mobilePhone;

            var sb = new StringBuilder();
            sb.Append("+");
            sb.Append(mobilePhone.Substring(0, startLen));
            for (var i = startLen; i < mobilePhone.Length - endLen; i++)
            {
                sb.Append("*");
            }
            sb.Append(mobilePhone.Substring(mobilePhone.Length - endLen));
            return sb.ToString();
        }

        public static string SaveMobilePhone(UserInfo user, string mobilePhone)
        {
            mobilePhone = GetPhoneValueDigits(mobilePhone);

            if (user == null || Equals(user, Constants.LostUser)) throw new Exception(Resource.ErrorUserNotFound);
            if (string.IsNullOrEmpty(mobilePhone)) throw new Exception(Resource.ActivateMobilePhoneEmptyPhoneNumber);
            if (!string.IsNullOrEmpty(user.MobilePhone) && user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.Activated) throw new Exception(Resource.MobilePhoneMustErase);

            user.MobilePhone = mobilePhone;
            user.MobilePhoneActivationStatus = MobilePhoneActivationStatus.NotActivated;
            if (SecurityContext.IsAuthenticated)
            {
                CoreContext.UserManager.SaveUserInfo(user);
            }
            else
            {
                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    CoreContext.UserManager.SaveUserInfo(user);
                }
                finally
                {
                    SecurityContext.Logout();
                }
            }

            if (StudioSmsNotificationSettings.Enable)
            {
                PutAuthCode(user, false);
            }

            return mobilePhone;
        }

        public static void PutAuthCode(UserInfo user, bool again)
        {
            if (user == null || Equals(user, Constants.LostUser)) throw new Exception(Resource.ErrorUserNotFound);
            var mobilePhone = GetPhoneValueDigits(user.MobilePhone);

            if (SmsKeyStorage.ExistsKey(mobilePhone) && !again) return;

            var key = SmsKeyStorage.GenerateKey(mobilePhone);
            SmsSender.SendSMS(mobilePhone, string.Format(Resource.SmsAuthenticationMessageToUser, key));
        }

        public static void ValidateSmsCode(UserInfo user, string code)
        {
            if (!StudioSmsNotificationSettings.IsVisibleSettings
                || !StudioSmsNotificationSettings.Enable)
            {
                return;
            }

            if (user == null || Equals(user, Constants.LostUser)) throw new Exception(Resource.ErrorUserNotFound);

            code = (code ?? "").Trim();

            if (string.IsNullOrEmpty(code)) throw new Exception(Resource.ActivateMobilePhoneEmptyCode);

            int counter = 0;

            int.TryParse(CodeCache.Get<String>("loginsec/" + user.ID), out counter);

            if (++counter % 5 == 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
            CodeCache.Insert("loginsec/" + user.ID, counter, DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));

            if (!SmsKeyStorage.ValidateKey(user.MobilePhone, code))
                throw new ArgumentException(Resource.SmsAuthenticationMessageError);

            if (!SecurityContext.IsAuthenticated)
            {
                var cookiesKey = SecurityContext.AuthenticateMe(user.ID);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
            }

            if (user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated)
            {
                user.MobilePhoneActivationStatus = MobilePhoneActivationStatus.Activated;
                CoreContext.UserManager.SaveUserInfo(user);
            }
        }
    }
}