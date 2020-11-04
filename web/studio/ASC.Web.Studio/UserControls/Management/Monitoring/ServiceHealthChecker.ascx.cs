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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.UI;
using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Monitoring, Location, SortOrder = 50)]
    [AjaxNamespace("MonitoringAjax")]
    public partial class ServiceHealthChecker : UserControl
    {
        public const string Location = "~/UserControls/Management/Monitoring/ServiceHealthChecker.ascx";

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/Monitoring/js/servicehealthchecker.js")
                .RegisterStyle("~/UserControls/Management/Monitoring/css/monitoring.less");

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        protected string[] GetServiceNames()
        {
            return (ConfigurationManagerExtension.AppSettings["monitoring.service-names"] ?? "").Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        }

        [AjaxMethod]
        public List<object> GetServiceStatusList()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            return GetServiceNames().Select(serviceName => ToAjaxResponse(serviceName, GetServiceController(serviceName))).ToList();
        }

        [AjaxMethod]
        public object GetServiceStatus(string serviceName)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            return ToAjaxResponse(serviceName, GetServiceController(serviceName));
        }

        [AjaxMethod]
        public object StartService(string serviceName)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            var sc = GetServiceController(serviceName);
            if (GetServiceStatus(sc) == ServiceStatus.Stopped)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.StartPending, TimeSpan.FromSeconds(10));
            }
            return ToAjaxResponse(serviceName, sc);
        }

        [AjaxMethod]
        public object StopService(string serviceName)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            var sc = GetServiceController(serviceName);
            if (GetServiceStatus(sc) == ServiceStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.StopPending, TimeSpan.FromSeconds(10));
            }
            return ToAjaxResponse(serviceName, sc);
        }

        [AjaxMethod]
        public void ClearCache()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            //reset bundler cache key
            var config = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);

            var keyElement = config.AppSettings.Settings["web.client.cache.resetkey"];
            if (keyElement == null) return;

            keyElement.Value = DateTime.UtcNow.ToString("yyyyMMddhhmmss");

            config.Save(ConfigurationSaveMode.Modified);
        }

        private static ServiceController GetServiceController(string serviceName)
        {
            return ServiceController.GetServices(Environment.MachineName).FirstOrDefault(sc => string.Equals(sc.ServiceName, serviceName, StringComparison.InvariantCultureIgnoreCase));
        }

        private static ServiceStatus GetServiceStatus(ServiceController sc)
        {
            if (sc == null)
                return ServiceStatus.NotFound;

            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    return ServiceStatus.Running;

                case ServiceControllerStatus.StartPending: case ServiceControllerStatus.ContinuePending:
                    return ServiceStatus.StartPending;

                case ServiceControllerStatus.Paused: case ServiceControllerStatus.Stopped:
                    return ServiceStatus.Stopped;

                case ServiceControllerStatus.PausePending: case ServiceControllerStatus.StopPending:
                    return ServiceStatus.StopPending;

                default:
                    return ServiceStatus.NotFound;
            }
        }

        protected static string GetServiceId(string serviceName)
        {
            return serviceName.Replace(" ", "");
        }

        protected static string GetServiceStatusDescription(ServiceStatus status)
        {
            return MonitoringResource.ResourceManager.GetString("ServiceStatus" + status) ?? status.ToString();
        }

        private static object ToAjaxResponse(string serviceName, ServiceController sc)
        {
            var status = GetServiceStatus(sc);
            return new {id = GetServiceId(serviceName), name = serviceName, status = status, statusDescription = GetServiceStatusDescription(status)};
        }

        protected enum ServiceStatus
        {
            NotFound,
            Running,
            Stopped,
            StartPending,
            StopPending
        }
    }
}