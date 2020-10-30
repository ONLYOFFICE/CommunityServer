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