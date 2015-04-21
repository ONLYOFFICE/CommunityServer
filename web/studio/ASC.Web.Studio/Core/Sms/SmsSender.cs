/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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