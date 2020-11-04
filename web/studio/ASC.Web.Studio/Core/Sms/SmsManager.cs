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