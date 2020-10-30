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


using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Notify.Senders;
using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Resources;
using ASC.HealthCheck.Settings;
using ASC.Notify.Messages;
using log4net;
using System;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;

namespace ASC.HealthCheck.Controllers
{
    public class NotifiersApiController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(NotifiersApiController));
        public ResultHelper ResultHelper { get { return new ResultHelper(Configuration.Formatters.JsonFormatter); } }

        [HttpGet]
        public HttpResponseMessage GetNotifiers()
        {
            try
            {
                log.Debug("GetNotifiers");

                var healthcheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();

                return ResultHelper.GetContent(new { code = 1, numbers = healthcheckSettings.PhoneNumbers, emails = healthcheckSettings.Emails});
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error! {0}", ex.ToString());
                return ResultHelper.Error(HealthCheckResource.GetNotifiersError);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("email");
            }
            try
            {
                log.DebugFormat("AddEmail email = {0}", email);

                var healthcheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();

                email = email.ToLower();
                if (IsValidEmail(email))
                {
                    if (healthcheckSettings.Emails.Contains(email))
                    {
                        log.DebugFormat("emails already contains this email address! email = {0}", email);
                        return ResultHelper.Error(HealthCheckResource.AlreadyContainsEmailAddress);
                    }
                    healthcheckSettings.Emails.Add(email);
                    HealthCheckSettingsAccessor.SaveHealthCheckSettings(healthcheckSettings);

                    log.DebugFormat("Add Email Success! email = {0}", email);
                    return ResultHelper.Success(HealthCheckResource.AddEmailSuccess);
                }

                log.DebugFormat("Wrong Email Address! email = {0}", email);
                return ResultHelper.Error(HealthCheckResource.WrongEmailAddress);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on AddEmail. email = {0} {1} {2}", email,
                  ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.AddEmailError);
            }
        }

        [HttpPost]
        public HttpResponseMessage RemoveEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("email");
            }
            try
            {
                log.DebugFormat("RemoveEmail email = {0}", email);

                var healthcheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();

                email = email.ToLower();
                if (!healthcheckSettings.Emails.Contains(email))
                {
                    log.DebugFormat("No such email address email = {0}", email);
                    return ResultHelper.Error(HealthCheckResource.NoSuchEmailAddress);
                }
                healthcheckSettings.Emails.Remove(email);
                HealthCheckSettingsAccessor.SaveHealthCheckSettings(healthcheckSettings);

                log.DebugFormat("Remove Email Success! email = {0}", email);
                return ResultHelper.Success(HealthCheckResource.RemoveEmailSuccess);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on RemoveEmail. email = {0} {1} {2}", email,
                  ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.RemoveEmailError);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException("phone");
            }
            try
            {
                log.DebugFormat("AddPhone phone = {0}", phone);

                var healthcheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();

                phone = phone.ToLower();
                if (IsValidPhone(phone))
                {
                    if (healthcheckSettings.PhoneNumbers.Contains(phone))
                    {
                        log.DebugFormat("PhoneNumbers already contains this phone! phone = {0}", phone);
                        return ResultHelper.Error(HealthCheckResource.AlreadyContainsPhone);
                    }
                    healthcheckSettings.PhoneNumbers.Add(phone);
                    HealthCheckSettingsAccessor.SaveHealthCheckSettings(healthcheckSettings);

                    log.DebugFormat("Add Phone Success! phone = {0}", phone);
                    return ResultHelper.Success(HealthCheckResource.AddPhoneSuccess);
                }

                log.DebugFormat("Wrong Phone Number! phone = {0}", phone);
                return ResultHelper.Error(HealthCheckResource.WrongPhone);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on AddPhone. phone = {0} {1} {2}", phone,
                  ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.AddPhoneError);
            }
        }

        [HttpPost]
        public HttpResponseMessage RemovePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException("phone");
            }
            try
            {
                log.DebugFormat("RemovePhone phone = {0}", phone);

                var healthcheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();

                phone = phone.ToLower();
                if (!healthcheckSettings.PhoneNumbers.Contains(phone))
                {
                    log.DebugFormat("No such phone number, phone = {0}", phone);
                    return ResultHelper.Error(HealthCheckResource.NoSuchPnone);
                }
                healthcheckSettings.PhoneNumbers.Remove(phone);
                HealthCheckSettingsAccessor.SaveHealthCheckSettings(healthcheckSettings);

                log.DebugFormat("Remove Phone Success! phone = {0}", phone);
                return ResultHelper.Success(HealthCheckResource.RemovePhoneSuccess);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on RemovePhone. phone = {0} {1} {2}", phone,
                  ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.RemovePhoneError);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetNotifySettings()
        {
            try
            {
                log.Debug("GetNotifySettings");
                var healthcheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();
                var sendEmailSms = 0;
                if (!healthcheckSettings.SendEmail && healthcheckSettings.SendSms)
                {
                    sendEmailSms = 1;
                }
                else if (healthcheckSettings.SendEmail && healthcheckSettings.SendSms)
                {
                    sendEmailSms = 2;
                }
                log.DebugFormat("Get Notify Settings Success! sendNotify = {0} sendNotify = {1}",
                    healthcheckSettings.SendNotify, sendEmailSms);
                return ResultHelper.GetContent(new { code = 1, sendNotify = healthcheckSettings.SendNotify, sendEmailSms = sendEmailSms });
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on GetNotifySettings. {0} {1}",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.GetNotifySettingsError);
            }
        }

        [HttpPost]
        public HttpResponseMessage SetNotifySettings(bool sendNotify, int sendEmailSms)
        {
            try
            {
                log.DebugFormat("SetNotifySettings sendNotify = {0}, sendEmailSms = {1}", sendNotify, sendEmailSms);
                var healthcheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();
                healthcheckSettings.SendNotify = sendNotify;
                switch (sendEmailSms)
                {
                    case 0:
                        healthcheckSettings.SendEmail = true;
                        healthcheckSettings.SendSms = false;
                        break;
                    case 1:
                        healthcheckSettings.SendEmail = false;
                        healthcheckSettings.SendSms = true;
                        break;
                    case 2:
                        healthcheckSettings.SendEmail = true;
                        healthcheckSettings.SendSms = true;
                        break;
                    default:
                        log.ErrorFormat("Error on SetNotifySettings. Wrong sendEmailSms = {0}", sendEmailSms);
                        return ResultHelper.Error(HealthCheckResource.SetNotifySettingsError);
                }
                HealthCheckSettingsAccessor.SaveHealthCheckSettings(healthcheckSettings);

                log.DebugFormat("Set Notify Settings Success! sendNotify = {0} sendNotify = {1}", sendNotify, sendEmailSms);
                return ResultHelper.Success();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on SetNotifySettings. {0} {1}",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.SetNotifySettingsError);
            }
        }

        [HttpPost]
        public HttpResponseMessage SendNotification(string name, string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("subject");
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("message");

            try
            {
                log.DebugFormat("SendNotification name = {0}, email = {1}", name, email);
                if (IsValidEmail(email))
                {
                    var sender = HealthCheckRunner.NotifySenders["email.sender"];
                    var result = sender.Send(new NotifyMessage
                    {
                        Content = message,
                        Subject = subject,
                        CreationDate = DateTime.UtcNow,
                        Sender = name,
                        From = MailAddressUtils.Create(CoreContext.Configuration.SmtpSettings.SenderAddress,
                            CoreContext.Configuration.SmtpSettings.SenderDisplayName).ToString(),//email,
                        To = HealthCheckCfgSectionHandler.Instance.SupportEmails
                    });
                    if (result == NoticeSendResult.OK)
                    {
                        log.ErrorFormat("Successful Send Notification! email = {0}", email);
                        return ResultHelper.Success(HealthCheckResource.SuccessfulSendNotification);
                    }

                    log.ErrorFormat("Error Send Notification. email = {0}  result = {1}", email, result);
                    return ResultHelper.Error(HealthCheckResource.ErrorSendNotification);
                }

                log.ErrorFormat("Wrong Email Address! email = {0}", email);
                return ResultHelper.Error(HealthCheckResource.WrongEmailAddress);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on SendNotification. email = {0} {1} {2}", email,
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.SendNotificationError);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            var charArray = phone.ToCharArray();
            foreach (char c in charArray)
            {
                if (!char.IsNumber(c) && c != '-' && c != '+' && c != '(' && c != ')' && c != ' ')
                {
                    return false;
                }
            }
            return true;
        }
    }
}