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
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Sms;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.Core.SMS
{
    public static class SmsManager
    {
        public static string SaveMobilePhone(UserInfo user, string mobilePhone)
        {
            mobilePhone = SmsSender.GetPhoneValueDigits(mobilePhone);

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

            if (!StudioSmsNotificationSettings.IsVisibleSettings || !StudioSmsNotificationSettings.Enable) throw new MethodAccessException();

            var mobilePhone = SmsSender.GetPhoneValueDigits(user.MobilePhone);

            if (SmsKeyStorage.ExistsKey(mobilePhone) && !again) return;

            string key;
            if (!SmsKeyStorage.GenerateKey(mobilePhone, out key)) throw new Exception(Resource.SmsTooMuchError);
            if (SmsSender.SendSMS(mobilePhone, string.Format(Resource.SmsAuthenticationMessageToUser, key)))
            {
                CoreContext.TenantManager.SetTenantQuotaRow(new TenantQuotaRow { Tenant = TenantProvider.CurrentTenantID, Path = "/sms", Counter = 1 }, true);
            }
        }

        public static void ValidateSmsCode(UserInfo user, string code)
        {
            if (!StudioSmsNotificationSettings.IsVisibleSettings
                || !StudioSmsNotificationSettings.Enable)
            {
                return;
            }

            if (user == null || Equals(user, Constants.LostUser)) throw new Exception(Resource.ErrorUserNotFound);

            var valid = SmsKeyStorage.ValidateKey(user.MobilePhone, code);
            switch (valid)
            {
                case SmsKeyStorage.Result.Empty:
                    throw new Exception(Resource.ActivateMobilePhoneEmptyCode);
                case SmsKeyStorage.Result.TooMuch:
                    throw new Authorize.BruteForceCredentialException(Resource.SmsTooMuchError);
                case SmsKeyStorage.Result.Timeout:
                    throw new TimeoutException(Resource.SmsAuthenticationTimeout);
                case SmsKeyStorage.Result.Invalide:
                    throw new ArgumentException(Resource.SmsAuthenticationMessageError);
            }
            if (valid != SmsKeyStorage.Result.Ok) throw new Exception("Error: " + valid);

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