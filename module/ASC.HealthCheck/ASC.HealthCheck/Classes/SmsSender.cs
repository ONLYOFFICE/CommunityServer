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