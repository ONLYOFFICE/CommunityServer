/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Settings;
using log4net;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Configuration;
using System.Net.Http.Headers;
using System.Web.Http;
using TMResourceData;

namespace ASC.HealthCheck.Infrastructure
{
    public class Startup
    {
        private static IDisposable webApiHost;
        private static readonly ILog log = LogManager.GetLogger(typeof(Startup));

        public static void StartService()
        {
            if (ConfigurationManager.AppSettings["resources.from-db"] == "true")
            {
                DBResourceManager.PatchAssemblies();
            }
            var url = HealthCheckCfgSectionHandler.Instance.Url;

            log.DebugFormat("StartSevice: RunHealthCheckService. url: {0}", url);
            webApiHost = WebApp.Start<Startup>(url);

            /* Hack for mono because first requests can failed.
             * https://bugzilla.xamarin.com/show_bug.cgi?format=multiple&id=17965
             */
            var warmingUpper = new WarmingUpper();
            warmingUpper.WarmingUp(url);

            HealthCheckRunner.Run();
        }

        public static void StopService()
        {
            try
            {
                HealthCheckRunner.Stop();
                if (webApiHost != null)
                {
                    webApiHost.Dispose();
                    log.DebugFormat("StopService: HealthCheckService");
                    webApiHost = null;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while stopping the service", ex);
            }
        }

        public void Configuration(IAppBuilder app)
        {
            var webApiConfiguration = ConfigureWebApi();
            app.UseWebApi(webApiConfiguration);
        }

        private HttpConfiguration ConfigureWebApi()
        {
            var config = new HttpConfiguration { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            // EnableCors does not work with Mono
            // use AddAccessControlAllowOriginHeaderFilter instead
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            config.Routes.MapHttpRoute(
                name: "WorksApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: string.Empty,
                defaults: new { controller = "defaultApi" }
            );

            config.Filters.Add(new HealthCheckAuthorizationFilter());
            config.Filters.Add(new AddAccessControlAllowOriginHeaderFilter());

            return config;
        }
    }
}
