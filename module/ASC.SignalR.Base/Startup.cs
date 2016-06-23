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


using ASC.Core;
using ASC.SignalR.Base;
using ASC.SignalR.Base.Hubs.Chat;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Configuration;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Startup))]
namespace ASC.SignalR.Base
{
    public class Startup
    {
        private static CancellationTokenSource cancellationTokenSource;
        private static ServiceHost host;
        private static IDisposable signalrHost;
        private static readonly ILog log = LogManager.GetLogger(typeof(Startup));
        private static readonly string url = ConfigurationManager.AppSettings["web.hub"] ?? "http://localhost:9899/";


        public static void StartService()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(_ =>
            {
                try
                {
                    log.DebugFormat("RunSignalrService: start");
                    host = new ServiceHost(new SignalrService());
                    host.Open();
                    log.DebugFormat("RunSignalrService: host.Open");
                    signalrHost = WebApp.Start<Startup>(url);
                    log.DebugFormat("SignalRServer running on {0}", url);
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }, TaskCreationOptions.LongRunning, cancellationTokenSource.Token);
        }

        public static void StopService()
        {
            try
            {
                log.DebugFormat("StopService: cancellationTokenSource.Cancel");
                cancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception on cancellationTokenSource.Cancel {0}", ex);
            }

            if (host != null)
            {
                log.DebugFormat("StopService: host.Close");
                host.Close();
                host = null;
            }
            if (signalrHost != null)
            {
                signalrHost.Dispose();
                log.DebugFormat("StopService: signalrHost.Dispose");
                signalrHost = null;
            }
        }

        public void Configuration(IAppBuilder app)
        {
            object httpListener;

            if (WorkContext.IsMono &&
                app.Properties.TryGetValue(typeof(HttpListener).FullName, out httpListener) &&
                httpListener is HttpListener)
            {
                // HttpListener should not return exceptions that occur
                // when sending the response to the client
                ((HttpListener)httpListener).IgnoreWriteExceptions = true;
            }

            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new CustomUserIdProvider());
            GlobalHost.Configuration.TransportConnectTimeout = TimeSpan.FromSeconds(15);

            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    EnableDetailedErrors = true
                };
                map.RunSignalR(hubConfiguration);
            });
        }
    }
}