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
