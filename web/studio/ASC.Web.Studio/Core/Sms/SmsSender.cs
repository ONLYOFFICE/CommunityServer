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

using ASC.Core;
using ASC.Core.Caching;
using ASC.Thrdparty.Configuration;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace ASC.Web.Studio.Core.SMS
{
    static class SmsSender
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SmsSender));
        private static readonly TimeSpan trustInterval = TimeSpan.FromMinutes(5);
        private static readonly ICache PhoneCache = AscCache.Default;


        public static void SendSMS(string number, string message)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number");
            }
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }
            if (!StudioSmsNotificationSettings.IsVisibleSettings || !StudioSmsNotificationSettings.Enable)
            {
                throw new MethodAccessException();
            }
            if (StudioSmsNotificationSettings.LeftSms <= 0)
            {
                throw new Exception(Resource.SmsNotPaidError);
            }

            var count = Convert.ToInt32(PhoneCache.Get(number) ?? 0);
            if (count >= 5)
            {
                throw new Exception(Resource.SmsTooMuchError);
            }
            PhoneCache.Insert(number, ++count, DateTime.UtcNow.Add(trustInterval));

            if ("log".Equals(ConfigurationManager.AppSettings["core.notify.postman"], StringComparison.InvariantCultureIgnoreCase))
            {
                log.InfoFormat("Tenant {0} send sms to phoneNumber: {1}", TenantProvider.CurrentTenantID, number);
                log.InfoFormat("Message: {0}", message);
                return;
            }

            var left = StudioSmsNotificationSettings.LeftSms - 1;
            var leftMessage = string.Format(Resource.SmsAuthenticationMessageLeft, left);
            message += "\n" + leftMessage;

            number = new Regex("[^\\d+]").Replace(number, string.Empty);
            var url = KeyStorage.Get("smsOperatorUrl_clickatel");
            var method = "POST";

            if (!string.IsNullOrEmpty(KeyStorage.Get("sms.USAregex")) && Regex.IsMatch(number, KeyStorage.Get("sms.USAregex")))
            {
                url = KeyStorage.Get("smsOperatorUrl_clickatelUSA");
            }

            if (!string.IsNullOrEmpty(KeyStorage.Get("sms.CISregex")) && Regex.IsMatch(number, KeyStorage.Get("sms.CISregex")))
            {
                url = KeyStorage.Get("smsOperatorUrl_smsc");
                method = "GET";
            }

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url.Replace("{phone}", number).Replace("{text}", HttpUtility.UrlEncode(message)));
                request.Method = method;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 1000;

                using (var stream = request.GetResponse().GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    StudioSmsNotificationSettings.IncrementSentSms();
                    log.InfoFormat("SMS was sent to {0}, service returned: {1}", number, result);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to send sms message", ex);
                return;
            }

            if (left == 0 || left == TenantExtra.GetTenantQuota().ActiveUsers)
            {
                StudioNotifyService.Instance.SendToAdminSmsCount(left);
            }
        }
    }
}