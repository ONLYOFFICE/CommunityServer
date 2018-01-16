/*
 *
 * (c) Copyright Ascensio System Limited 2010-2017
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
using System.Configuration;
using System.Text.RegularExpressions;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;

namespace ASC.Web.Studio.Core.SMS
{
    internal static class SmsSender
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (SmsSender));
        private static readonly TimeSpan TrustInterval = TimeSpan.FromMinutes(5);
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
            if (!StudioSmsNotificationSettings.IsVisibleSettings || !StudioSmsNotificationSettings.Enable || !SmsProviderManager.Enabled())
            {
                throw new MethodAccessException();
            }

            int count;

            int.TryParse(PhoneCache.Get<string>(number), out count);

            if (count >= 5)
            {
                throw new Exception(Resource.SmsTooMuchError);
            }
            PhoneCache.Insert(number, ++count, DateTime.UtcNow.Add(TrustInterval));

            if ("log".Equals(ConfigurationManager.AppSettings["core.notify.postman"], StringComparison.InvariantCultureIgnoreCase))
            {
                Log.InfoFormat("Tenant {0} send sms to phoneNumber {1} Message: {2}", TenantProvider.CurrentTenantID, number, message);
                return;
            }

            number = new Regex("[^\\d+]").Replace(number, string.Empty);
            if (SmsProviderManager.SendMessage(number, message))
            {
                CoreContext.TenantManager.SetTenantQuotaRow(new TenantQuotaRow { Tenant = TenantProvider.CurrentTenantID, Path = "/sms", Counter = 1 }, true);
            }
        }
    }
}