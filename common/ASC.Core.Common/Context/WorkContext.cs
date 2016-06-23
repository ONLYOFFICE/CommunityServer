/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

                var postman = ConfigurationManager.AppSettings["core.notify.postman"];

                if ("ases".Equals(postman, StringComparison.InvariantCultureIgnoreCase) || "smtp".Equals(postman, StringComparison.InvariantCultureIgnoreCase))
                {
                    jabberSender = new JabberSender();

                    var properties = new Dictionary<string, string>();
                    properties["useCoreSettings"] = "true";
                    if ("ases".Equals(postman, StringComparison.InvariantCultureIgnoreCase))
                    {
                        emailSender = new AWSSender();
                        properties["accessKey"] = ConfigurationManager.AppSettings["ses.accessKey"];
                        properties["secretKey"] = ConfigurationManager.AppSettings["ses.secretKey"];
                        properties["refreshTimeout"] = ConfigurationManager.AppSettings["ses.refreshTimeout"];
                    }
                    else
                    {
                        emailSender = new SmtpSender();
                    }
                    emailSender.Init(properties);
                }

                notifyContext.NotifyService.RegisterSender(Constants.NotifyEMailSenderSysName, new EmailSenderSink(emailSender));
                notifyContext.NotifyService.RegisterSender(Constants.NotifyMessengerSenderSysName, new JabberSenderSink(jabberSender));

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