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
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.HealthCheck.Models;
using ASC.HealthCheck.Resources;
using ASC.HealthCheck.Settings;
using ASC.Notify.Config;
using ASC.Notify.Messages;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ASC.HealthCheck.Classes
{
    public static class HealthCheckRunner
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HealthCheckRunner));
        private static readonly IServiceRepository serviceRepository = new ServiceRepository();
        private static volatile bool alreadyNotifyAboutSmallDiskSpace;
        public static readonly ConcurrentDictionary<string, Timer> TimerDictionary = new ConcurrentDictionary<string, Timer>();
        public static IDictionary<string, INotifySender> NotifySenders = NotifyServiceCfg.Senders;

        private const string FakeTenantAlias = "fake-tenant-alias";
        public const string FakeUserId = "83A07DA6-EEA5-4DA3-B325-C2F4BCBA7DF2";
        public const int OneMb = 1048576;
        public const int OneGb = OneMb * 1024;
        public const string OneHundreedMb = "104857600";

        public static IServiceRepository ServiceRepository { get { return serviceRepository; } }

        public static void Run()
        {
            try
            {
                log.Debug("--Run HealthCheck--");

                if (GetFakeTenant() == 0)
                {
                    log.ErrorFormat("Error! Could not create tenant alias = {0}", FakeTenantAlias);
                    return;
                }

                GetFakeUserId();

               var mainLoopPeriod = HealthCheckCfgSectionHandler.Instance.MainLoopPeriod;

                foreach (var service in HealthCheckCfgSectionHandler.Instance.ServiceNames)
                {
                    log.DebugFormat("Add service {0} for checking.", service);
                    serviceRepository.Add(service);
                    TimerDictionary.TryAdd(service.ToString(), new Timer(IdleTimeout, service, TimeSpan.Zero, mainLoopPeriod));
                }

                IDriveSpaceChecker spaceChecker = new DriveSpaceChecker();
                TimerDictionary.TryAdd(spaceChecker.DriveName, new Timer(IdleTimeoutDriveSpace, spaceChecker.DriveName, TimeSpan.Zero, mainLoopPeriod));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on HealthCheckRunner.Run. {0} {1}", ex, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
        }

        public static void Stop()
        {
            try
            {
                log.Debug("--Stop HealthCheck--");
                foreach (var keyValue in TimerDictionary)
                {
                    keyValue.Value.Dispose();
                }
                TimerDictionary.Clear();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on HealthCheckRunner.Stop. {0} {1} {2}",
                    ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
        }

        public static bool ServerStatusIsOk()
        {
            foreach (var serviceObject in ServiceRepository.GetServicesSnapshot())
            {
                try
                {
                    if (serviceObject.Message != string.Empty)
                    {
                        log.DebugFormat("ServerStatusIsOk: serviceObject is not OK, ServiceName = {0}, Status = {1}",
                            serviceObject.ServiceName, serviceObject.Status);
                        return false; // "Error";
                    }
                }
                catch (InvalidOperationException)
                {
                    // serviceObject not found
                    log.DebugFormat("ServerStatusIsOk: serviceObject not found, ServiceName = {0}", serviceObject.ServiceName);
                }
            }
            log.Debug("ServerStatusIsOk: all is OK!");
            return true; // "Works";
        }

        private static int GetFakeTenant()
        {
            var fakeTenant = CoreContext.TenantManager.GetTenant(FakeTenantAlias);

            /*
var tenants = CoreContext.TenantManager.GetTenants();

if (fakeTenant != null)
{
    tenants = tenants.Where(t => t.TenantId != fakeTenant.TenantId).ToList();
}

if (tenants.Count() <= 0 || !CoreContext.TenantManager.GetTenantQuota(tenants.First().TenantId).HealthCheck)
{
    log.Debug("Service wasn't started. There is no correct license for HealthCheck.");
    return;
}*/

            if (fakeTenant == null)
            {
                fakeTenant = new Tenant
                {
                    TenantAlias = FakeTenantAlias
                };
                fakeTenant = CoreContext.TenantManager.SaveTenant(fakeTenant);
            }

            if (fakeTenant != null)
            {
                var healthcheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();
                healthcheckSettings.FakeTenantId = fakeTenant.TenantId;
                HealthCheckSettingsAccessor.SaveHealthCheckSettings(healthcheckSettings);

                CoreContext.TenantManager.SetCurrentTenant(fakeTenant.TenantId);

                return fakeTenant.TenantId;
            }

            return 0;
        }

        private static void GetFakeUserId()
        {
            var userGuid = new Guid(FakeUserId);
            var userInfo = CoreContext.UserManager.GetUsers(userGuid);
            if (userInfo == Constants.LostUser)
            {
                userInfo = new UserInfo
                {
                    ID = userGuid,
                    UserName = "fake-user",
                    Email = "fake-user@faketenantalias.com",
                    FirstName = "fake",
                    LastName = "user",
                    ActivationStatus = EmployeeActivationStatus.Activated
                };
                try
                {
                    SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);
                    CoreContext.UserManager.SaveUserInfo(userInfo);
                }
                finally
                {
                    SecurityContext.Logout();
                }
            }


            /*
            var ascAuthKey = SecurityContext.AuthenticateMe(userGuid);

            if (!MailBoxExists(tenant, userInfo, ascAuthKey))
            {
                CreateMailBox(tenant, userInfo, ascAuthKey);
            }
            */
        }

        private static void IdleTimeoutDriveSpace(object e)
        {
            try
            {
                var healthCheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();

                /*var tenants = CoreContext.TenantManager.GetTenants().Where(t => t.TenantId != healthCheckSettings.FakeTenantId).ToList();
                if (tenants.Count() <= 0 || !CoreContext.TenantManager.GetTenantQuota(tenants.First().TenantId).HealthCheck)
                {
                    log.Debug("Service wasn't started. There is no correct license for HealthCheck.");
                    return;
                }*/
                log.Debug("Begin IdleTimeoutDriveSpace");

                CoreContext.TenantManager.SetCurrentTenant(healthCheckSettings.FakeTenantId);
                Thread.CurrentThread.CurrentCulture = CoreContext.TenantManager.GetCurrentTenant().GetCulture();

                IDriveSpaceChecker spaceChecker = new DriveSpaceChecker();
                spaceChecker.GetTotalAndFreeSpace();
                var freeSpace = spaceChecker.TotalFreeSpace;
                if (freeSpace == -1)
                {
                    log.Error("Not found drive name.");
                    return;
                }
                var space = freeSpace / OneMb + " Mb";
                var driveSpaceThreashold = HealthCheckCfgSectionHandler.Instance.DriveSpaceThreashold;
                if (freeSpace >= driveSpaceThreashold)
                {
                    log.DebugFormat("It is enough free disk space, {0}.", space);
                    alreadyNotifyAboutSmallDiskSpace = false;
                    return;
                }

                // Send sms and e-mail about small free disk space
                log.WarnFormat("It isn't enough free disk space, {0}.", space);
                if (alreadyNotifyAboutSmallDiskSpace) return;

                if (!healthCheckSettings.SendNotify || (!healthCheckSettings.SendEmail && !healthCheckSettings.SendSms))
                {
                    log.Debug("Notification isn't sent. Notifications disabled.");
                    return;
                }

                alreadyNotifyAboutSmallDiskSpace = true;

                if (healthCheckSettings.SendEmail)
                {
                    log.Debug("Send email notification");
                    SendEmail("Onlyoffice:" + HealthCheckResource.SmallDriveSpace + ": " + space,
                        HealthCheckResource.SmallDriveSpace + ": " + space, healthCheckSettings);
                }
                else
                {
                    log.Debug("Email isn't sent. Email notification disabled.");
                }

                if (healthCheckSettings.SendSms)
                {
                    log.Debug("Send SMS notification");
                    SendSms(HealthCheckResource.SmallDriveSpace + ": " + space, healthCheckSettings);
                }
                else
                {
                    log.Debug("SMS isn't sent. SMS notifications disabled.");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on IdleTimeoutDriveSpace. {0} {1} {2}",
                    ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
            finally
            {
                log.Debug("End IdleTimeoutDriveSpace");
            }
        }

        private static void IdleTimeout(object e)
        {
            var healthCheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();
            /*
            var tenants = CoreContext.TenantManager.GetTenants().Where(t => t.TenantId != healthCheckSettings.FakeTenantId).ToList();
            if (tenants.Count() <= 0 || !CoreContext.TenantManager.GetTenantQuota(tenants.First().TenantId).HealthCheck)
            {
                log.Debug("Service wasn't started. There is no correct license for HealthCheck.");
                return;
            }*/
            log.Debug("Begin IdleTimeout");

            CoreContext.TenantManager.SetCurrentTenant(healthCheckSettings.FakeTenantId);
            Thread.CurrentThread.CurrentCulture = CoreContext.TenantManager.GetCurrentTenant().GetCulture();

            try
            {
                var serviceName = (ServiceEnum)e;

                ServiceStatus serviceStatus;
                using (var xplatServiceController = XplatServiceController.GetXplatServiceController(serviceName))
                {
                    serviceStatus = xplatServiceController.GetServiceStatus();
                }

                switch (serviceStatus)
                {
                    case ServiceStatus.StartPending:
                        // wait till started
                        break;
                    case ServiceStatus.Running:
                        var result = serviceRepository.GetService(serviceName).Check(healthCheckSettings.FakeTenantId);

                        if (result != null)
                        {
                            serviceRepository.SetStates(serviceName,
                                (result == string.Empty
                                    ? ServiceStatus.Running
                                    : ServiceStatus.NotFound).GetStringStatus(), result);
                            //if (result != string.Empty)
                            //{
                            //    HasAttempt(serviceName);
                            //}
                            //else
                            //{
                            //    serviceRepository.DropAttempt(serviceName);
                            //}
                        }
                        break;
                    case ServiceStatus.NotFound:
                        serviceRepository.SetStates(serviceName, ServiceStatus.NotFound.GetStringStatus(), ServiceStatus.NotFound.GetMessageStatus());
                        //HasAttempt(serviceName);
                        break;
                    default:
                        serviceRepository.SetStates(serviceName, ServiceStatus.Stopped.GetStringStatus(), ServiceStatus.Stopped.GetMessageStatus());
                        //HasAttempt(serviceName);
                        break;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on IdleTimeout. {0} {1}", ex.ToString(),
                    ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
            finally
            {
                log.Debug("End IdleTimeout");
            }
        }

        private static void HasAttempt(ServiceEnum serviceName)
        {
            log.DebugFormat("HasAttempt: service = {0}", serviceName);
            if (serviceRepository.ShouldRestart(serviceName))
            {
                log.DebugFormat("HasAttempt: ShouldRestart is true, service = {0}", serviceName);
                var healthCheckServiceManager = new HealthCheckServiceManager(ServiceRepository);
                healthCheckServiceManager.StopService(serviceName);
                Thread.Sleep(5);
                healthCheckServiceManager.StartService(serviceName);
                return;
            }
            if (!serviceRepository.HasAtempt(serviceName))
            {
                // Send sms and e-mail about service/site/zone
                var service = ServiceRepository.GetService(serviceName);
                log.ErrorFormat("There are some problem with serviceName = {0}, status = {1}, Message = {2}.",
                    serviceName, service.Status, service.Message);

                var healthCheckSettings = HealthCheckSettingsAccessor.GetHealthCheckSettings();

                if (healthCheckSettings.SendNotify)
                {
                    if (healthCheckSettings.SendEmail)
                    {
                        log.Debug("Send email notification.");
                        SendEmail(String.Format("Onlyoffice {0} problem.", serviceName),
                            String.Format(HealthCheckResource.ServiceProblem, serviceName, service.Status, service.Message),
                            healthCheckSettings);
                    }
                    else
                    {
                        log.Debug("Email isn't sent. Email notification disabled.");
                    }
                    if (healthCheckSettings.SendSms)
                    {
                        log.Debug("Send SMS notification.");
                        SendSms(String.Format(HealthCheckResource.ServiceProblem,
                            serviceName, service.Status, service.Message), healthCheckSettings);
                    }
                    else
                    {
                        log.Debug("SMS isn't sent. SMS notification disabled.");
                    }
                }
                else
                {
                    log.Debug("Notification isn't sent. Notification disabled.");
                }
            }
        }

        /*
        private static bool MailBoxExists(Tenant tenant, UserInfo userInfo, string ascAuthKey)
        {
            string portalUrl = ConfigurationManager.AppSettings["portalUrl"] ?? "http://localhost/";
            var client = new RestClient(portalUrl);
            var request = new RestRequest("/api/2.0/mail/accounts/" + HttpUtility.UrlEncode(userInfo.Email), Method.GET);
            request.AddCookie("asc_auth_key", ascAuthKey);
            var response = client.Execute(request);
            var jObject = JObject.Parse(response.Content);
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                if (jObject["error"]["type"].ToString() == "System.NullReferenceException")
                {
                    log.DebugFormat("MailBox account {0} does not exist", userInfo.Email);
                    return false;
                }
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Can't get mailBox using API. Response code = " + response.StatusCode, response.ErrorException);
            }
            return true;
        }
        
        private static void CreateMailBox(Tenant tenant, UserInfo userInfo, string ascAuthKey)
        {
            string portalUrl = ConfigurationManager.AppSettings["portalUrl"] ?? "http://localhost/";
            var client = new RestClient(portalUrl);
            var request = new RestRequest();
            request.AddCookie("asc_auth_key", ascAuthKey);
        }
        */

        private static void SendEmail(string subject, string message, HealthCheckSettings healthCheckSettings)
        {
            if (healthCheckSettings.Emails != null && healthCheckSettings.Emails.Count != 0)
            {
                log.DebugFormat("SendEmail: subject = {0}, message = {1}", subject, message);
                var sender = NotifySenders["email.sender"];
                sender.Send(new NotifyMessage
                {
                    Content = message,
                    Subject = subject,
                    CreationDate = DateTime.UtcNow,
                    Sender = Core.Configuration.Constants.NotifyEMailSenderSysName,
                    To = string.Join("|", healthCheckSettings.Emails),
                    From = MailAddressUtils.Create(CoreContext.Configuration.SmtpSettings.SenderAddress,
                        CoreContext.Configuration.SmtpSettings.SenderDisplayName).ToString()
                });
            }
            else
            {
                log.Debug("SendEmail: healthCheckSettings.Emails is empty, there are no recipients");
            }
        }

        private static void SendSms(string message, HealthCheckSettings healthCheckSettings)
        {
            if (healthCheckSettings.PhoneNumbers != null && healthCheckSettings.PhoneNumbers.Count != 0)
            {
                log.DebugFormat("SendSms: message = {0}", message);
                ISmsSender smsSender = new SmsSender();

                foreach (var t in healthCheckSettings.PhoneNumbers)
                {
                    smsSender.SendSMS(t, message, healthCheckSettings);
                }
            }
            else
            {
                log.Debug("SendSms: healthCheckSettings.PhoneNumbers is empty, there are no recipients");
            }
        }
    }
}