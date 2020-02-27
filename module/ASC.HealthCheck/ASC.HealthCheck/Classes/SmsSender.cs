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


using ASC.HealthCheck.Settings;
using log4net;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using RestSharp.Extensions;


namespace ASC.HealthCheck.Classes
{
    public class SmsSender : ISmsSender
    {
        private readonly ILog log;
        private const string GetMethod = "GET";
        private const string PostMethod = "POST";
        private readonly string SMSUSAregex;
        private readonly string SMSCISregex;

        public SmsSender()
        {
            log = LogManager.GetLogger(typeof(SmsSender));
            SMSUSAregex = HealthCheckCfgSectionHandler.Instance.SMSUSAregex;
            SMSCISregex = HealthCheckCfgSectionHandler.Instance.SMSCISregex;
        }

        public void SendSMS(string number, string message, HealthCheckSettings healthCheckSettings)
        {
            if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("number");
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("message");
            if (healthCheckSettings == null) throw new ArgumentNullException("healthCheckSettings");

            log.DebugFormat("SendSMS: number = {0}, message = {1}", number, message);
            if ("log".Equals(ConfigurationManager.AppSettings["core.notify.postman"], StringComparison.InvariantCultureIgnoreCase))
            {
                log.InfoFormat("Send notify sms to phoneNumber: {0}\n Message: {1}", number, message);
                return;
            }

            number = new Regex("[^\\d+]").Replace(number, string.Empty);

            var url = healthCheckSettings.SmsOperatorUrlClickatel;
            var method = PostMethod;

            if (!string.IsNullOrEmpty(SMSUSAregex) &&
                Regex.IsMatch(number, SMSUSAregex))
            {
                url = healthCheckSettings.SmsOperatorUrlClickatelUSA;
            }

            if (!string.IsNullOrEmpty(SMSCISregex) &&
                Regex.IsMatch(number, SMSCISregex))
            {
                url = healthCheckSettings.SmsOperatorUrlSmsc;
                method = GetMethod;
            }

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url.Replace("{phone}", number).Replace("{text}", message.UrlEncode()));
                request.Method = method;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 1000;

                using (var stream = request.GetResponse().GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    log.InfoFormat("SMS was sent to {0}, service returned: {1}", number, result);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendSMS: Failed to send sms message. {0} {1}",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return;
            }
        }
    }
}