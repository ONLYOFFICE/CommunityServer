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


using ASC.Core.Notify;
using ASC.Core.Notify.Senders;
using ASC.Core.Tenants;
using ASC.Notify.Engine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Constants = ASC.Core.Configuration.Constants;
using NotifyContext = ASC.Notify.Context;

namespace ASC.Core
{
    public static class WorkContext
    {
        private static readonly object syncRoot = new object();
        private static bool notifyStarted;
        private static NotifyContext notifyContext;
        private static bool? ismono;
        private static string monoversion;


        public static NotifyContext NotifyContext
        {
            get
            {
                NotifyStartUp();
                return notifyContext;
            }
        }

        public static string[] DefaultClientSenders
        {
            get { return new[] { Constants.NotifyEMailSenderSysName, }; }
        }

        public static bool IsMono
        {
            get
            {
                if (ismono.HasValue)
                {
                    return ismono.Value;
                }

                var monoRuntime = Type.GetType("Mono.Runtime");
                ismono = monoRuntime != null;
                if (monoRuntime != null)
                {
                    var dispalayName = monoRuntime.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    if (dispalayName != null)
                    {
                        monoversion = dispalayName.Invoke(null, null) as string;
                    }
                }
                return ismono.Value;
            }
        }

        public static string MonoVersion
        {
            get
            {
                return IsMono ? monoversion : null;
            }
        }


        private static void NotifyStartUp()
        {
            if (notifyStarted) return;
            lock (syncRoot)
            {
                if (notifyStarted) return;

                notifyContext = new NotifyContext();

                INotifySender jabberSender = new NotifyServiceSender();
                INotifySender emailSender = new NotifyServiceSender();
                INotifySender telegramSender = new TelegramSender();

                var postman = ConfigurationManagerExtension.AppSettings["core.notify.postman"];

                if ("ases".Equals(postman, StringComparison.InvariantCultureIgnoreCase) || "smtp".Equals(postman, StringComparison.InvariantCultureIgnoreCase))
                {
                    jabberSender = new JabberSender();

                    var properties = new Dictionary<string, string>();
                    properties["useCoreSettings"] = "true";
                    if ("ases".Equals(postman, StringComparison.InvariantCultureIgnoreCase))
                    {
                        emailSender = new AWSSender();
                        properties["accessKey"] = ConfigurationManagerExtension.AppSettings["ses.accessKey"];
                        properties["secretKey"] = ConfigurationManagerExtension.AppSettings["ses.secretKey"];
                        properties["refreshTimeout"] = ConfigurationManagerExtension.AppSettings["ses.refreshTimeout"];
                    }
                    else
                    {
                        emailSender = new SmtpSender();
                    }
                    emailSender.Init(properties);
                }

                notifyContext.NotifyService.RegisterSender(Constants.NotifyEMailSenderSysName, new EmailSenderSink(emailSender));
                notifyContext.NotifyService.RegisterSender(Constants.NotifyMessengerSenderSysName, new JabberSenderSink(jabberSender));
                notifyContext.NotifyService.RegisterSender(Constants.NotifyTelegramSenderSysName, new TelegramSenderSink(telegramSender));

                notifyContext.NotifyEngine.BeforeTransferRequest += NotifyEngine_BeforeTransferRequest;
                notifyContext.NotifyEngine.AfterTransferRequest += NotifyEngine_AfterTransferRequest;
                notifyStarted = true;
            }
        }

        private static void NotifyEngine_BeforeTransferRequest(NotifyEngine sender, NotifyRequest request)
        {
            request.Properties.Add("Tenant", CoreContext.TenantManager.GetCurrentTenant(false));
        }

        private static void NotifyEngine_AfterTransferRequest(NotifyEngine sender, NotifyRequest request)
        {
            var tenant = (request.Properties.Contains("Tenant") ? request.Properties["Tenant"] : null) as Tenant;
            if (tenant != null)
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);
            }
        }
    }
}