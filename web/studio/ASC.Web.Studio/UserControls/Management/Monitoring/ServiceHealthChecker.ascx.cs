/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/monitoring/js/servicehealthchecker.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/monitoring/css/monitoring.less"));
        }

        protected string[] GetServiceNames()
        {
            return (ConfigurationManager.AppSettings["monitoring.service-names"] ?? "").Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
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