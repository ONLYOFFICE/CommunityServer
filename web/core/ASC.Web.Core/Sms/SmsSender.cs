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
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Web.Core.Sms
{
    public static class SmsSender
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");


        public static bool SendSMS(string number, string message)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number");
            }
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }
            if (!SmsProviderManager.Enabled())
            {
                throw new MethodAccessException();
            }

            if ("log".Equals(ConfigurationManagerExtension.AppSettings["core.notify.postman"], StringComparison.InvariantCultureIgnoreCase))
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                var tenantId = tenant == null ? Tenant.DEFAULT_TENANT : tenant.TenantId;

                Log.InfoFormat("Tenant {0} send sms to phoneNumber {1} Message: {2}", tenantId, number, message);
                return false;
            }

            number = new Regex("[^\\d+]").Replace(number, string.Empty);
            return SmsProviderManager.SendMessage(number, message);
        }

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
    }
}