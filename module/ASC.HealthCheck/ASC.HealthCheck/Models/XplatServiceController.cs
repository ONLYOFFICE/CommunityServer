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


using System;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using ASC.Core;
using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Resources;
using log4net;

namespace ASC.HealthCheck.Models
{
    public interface IXplatServiceController: IDisposable
    {
        ServiceStatus GetServiceStatus();
        string StartService();
        string StopService();
    }

    public class XplatServiceController : IXplatServiceController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(XplatServiceController));
        private static readonly TimeSpan waitStartStopServicePeriod = TimeSpan.FromSeconds(40);
        private ServiceController serviceController;

        public static IXplatServiceController GetXplatServiceController(ServiceEnum serviceName)
        {
            return WorkContext.IsMono
                ? (IXplatServiceController)new XplatMonoServiceController(serviceName)
                : new XplatServiceController(serviceName);
        }
        public XplatServiceController(ServiceEnum serviceName)
        {
            var services = ServiceController.GetServices(Environment.MachineName);
            serviceController = services.FirstOrDefault(sc => serviceName.ToString().Equals(sc.ServiceName, StringComparison.InvariantCultureIgnoreCase));
        }

        public ServiceStatus GetServiceStatus()
        {
            if (serviceController == null)
            {
                log.Debug("GetServiceStatus, ServiceController is NULL");
                return ServiceStatus.NotFound;
            }
            log.DebugFormat("GetServiceStatus, serviceName = {0}", serviceController.ServiceName);
            switch (serviceController.Status)
            {
                case ServiceControllerStatus.Running:
                    return ServiceStatus.Running;

                case ServiceControllerStatus.StartPending:
                case ServiceControllerStatus.ContinuePending:
                    return ServiceStatus.StartPending;

                case ServiceControllerStatus.Paused:
                case ServiceControllerStatus.Stopped:
                    return ServiceStatus.Stopped;

                case ServiceControllerStatus.PausePending:
                case ServiceControllerStatus.StopPending:
                    return ServiceStatus.StopPending;

                default:
                    return ServiceStatus.NotFound;
            }
        }

        public string StartService()
        {
            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.StartPending, waitStartStopServicePeriod);
            return string.Empty;
        }

        public string StopService()
        {
            serviceController.Stop();
            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, waitStartStopServicePeriod);
            return string.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (serviceController != null)
                {
                    serviceController.Dispose();
                    serviceController = null;
                }
            }
        }

        ~XplatServiceController()
        {
            Dispose(false);
        }
    }

    public class XplatMonoServiceController : IXplatServiceController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(XplatMonoServiceController));
        private readonly string serviceNameString;

        public XplatMonoServiceController(ServiceEnum serviceName)
        {
            serviceNameString = Regex.Replace(serviceName.ToString(), @"\A[A-Z]", m => m.ToString().ToLower());
        }

        public XplatMonoServiceController(string serviceNameString)
        {
            this.serviceNameString = serviceNameString;
        }

        public ServiceStatus GetServiceStatus()
        {
            try
            {
                using (var proc = new ShellExe().RunBinFile("ps", "auxf"))
                {
                    var output = proc.StandardOutput.ReadToEnd();
                    var error = proc.StandardError.ReadToEnd();
                    log.DebugFormat("GetMonoServiceStatus, processName = {0}", serviceNameString);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        log.ErrorFormat("GetMonoServiceStatus. Unexpected error: {0}, serviceStatus = {1}", error, ServiceStatus.NotFound);
                        return ServiceStatus.NotFound;
                    }
                    if (output.Contains(string.Format("-l:/tmp/{0}", serviceNameString)))
                    {
                        log.DebugFormat("GetMonoServiceStatus, serviceName = {0}, serviceStatus = {1}", serviceNameString, ServiceStatus.Running);
                        return ServiceStatus.Running;
                    }
                    log.DebugFormat("GetMonoServiceStatus, serviceName: {0}, serviceStatus = {1}", serviceNameString, ServiceStatus.Stopped);
                    return ServiceStatus.Stopped;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetMonoServiceStatus. Unexpected error. ProcessName = {0}, serviceStatus = {1}, ex = {2}", serviceNameString, ServiceStatus.NotFound, ex.ToString());
                return ServiceStatus.NotFound;
            }
        }

        public string StartService()
        {
            return Run("start", HealthCheckResource.ServiceStartError);
        }

        public string StopService()
        {
            return Run("stop", HealthCheckResource.ServiceStopError);
        }

        private string Run(string command, string errorResult)
        {
            try
            {
                using (var proc = new ShellExe().RunBinFile("sudo", string.Format("service {0} {1}", serviceNameString, command)))
                {
                    var output = proc.StandardOutput.ReadToEnd();
                    var error = proc.StandardError.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        log.ErrorFormat("{0}MonoService. Unexpected error: serviceName = {1}, error = {2}, output: {3}", command, serviceNameString, error, output);
                        return errorResult;
                    }
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("{0}MonoService. Unexpected error. ProcessName = {1}, ex = {2}", command, serviceNameString, ex.ToString());
                return errorResult;
            }
        }

        public void Dispose()
        {
        }
    }
}