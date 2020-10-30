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


using ASC.HealthCheck.Models;
using ASC.HealthCheck.Resources;
using log4net;
using System;

namespace ASC.HealthCheck.Classes
{
    public class HealthCheckServiceManager
    {
        private readonly IServiceRepository serviceRepository;
        private readonly ILog log = LogManager.GetLogger(typeof(HealthCheckServiceManager));

        public HealthCheckServiceManager(IServiceRepository repository)
        {
            serviceRepository = repository;
        }

        public ServiceEnum? GetServiceName(string service)
        {
            log.DebugFormat("GetServiceName: service = {0}", service);
            if (service == "minichat")
            {
                service = "signalr";
            }
            ServiceEnum result;
            if (Enum.TryParse(service, out result) && serviceRepository.GetService(result) != null)
            {
                return result;
            }
            return null;
        }

        public string StartService(ServiceEnum serviceName)
        {
            string status;
            try
            {
                log.DebugFormat("StartService: serviceName = {0}", serviceName);
                using (var xplatServiceController = XplatServiceController.GetXplatServiceController(serviceName))
                {
                    var serviceStatus = xplatServiceController.GetServiceStatus();
                    switch (serviceStatus)
                    {
                        case ServiceStatus.Stopped:
                            status = xplatServiceController.StartService();
                            if (status == String.Empty)
                            {
                                serviceRepository.SetStates(serviceName, ServiceStatus.StartPending.GetStringStatus(), string.Empty);
                                log.DebugFormat("Service run on the local machine: {0}.", serviceName);
                            }
                            break;
                        case ServiceStatus.Running:
                        case ServiceStatus.StopPending:
                        case ServiceStatus.StartPending:
                            log.ErrorFormat("Service still not stopped on the current machine: {0}, {1}.", serviceName, serviceStatus);
                            status = ServiceStatus.StopPending.GetStringStatus();
                            break;
                        default:
                            log.ErrorFormat("Service not found on the current machine: {0}.", serviceName);
                            status = ServiceStatus.NotFound.GetStringStatus();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                status = HealthCheckResource.ServiceStartError;
                log.ErrorFormat("Error on StartService. {0} {1} {2}", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
            return status;
        }

        public string StopService(ServiceEnum serviceName)
        {
            string status;
            try
            {
                log.DebugFormat("StopService: serviceName = {0}", serviceName);
                using (var xplatServiceController = XplatServiceController.GetXplatServiceController(serviceName))
                {
                    var serviceStatus = xplatServiceController.GetServiceStatus();

                    switch (serviceStatus)
                    {
                        case ServiceStatus.Running:
                            status = xplatServiceController.StopService();
                            if (status != string.Empty)
                            {
                                serviceRepository.SetStates(serviceName, ServiceStatus.Stopped.GetStringStatus(), string.Empty);
                                log.DebugFormat("Service stopped on the local machine: serviceName = {0}, serviceStatus = {1}", serviceName, serviceStatus);
                            }
                            break;
                        case ServiceStatus.Stopped:
                        case ServiceStatus.StopPending:
                        case ServiceStatus.StartPending:
                            log.ErrorFormat("Service still not started on the current machine: serviceName = {0}, serviceStatus = {1}", serviceName, serviceStatus);
                            status = HealthCheckResource.ServiceStillNotStarted;
                            break;
                        default:
                            log.ErrorFormat("Service not found on the current machine: serviceName = {0}, serviceStatus = {1}", serviceName, serviceStatus);
                            status = ServiceStatus.NotFound.GetStringStatus();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                status = HealthCheckResource.ServiceStopError;
                log.ErrorFormat("Error on StopService. {0} {1} {2}",
                    ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
            return status;
        }

        public ServiceStatus GetStatus(ServiceEnum serviceName)
        {
            log.DebugFormat("GetStatus: serviceName = {0}", serviceName);
            try
            {
                using (var xplatServiceController = XplatServiceController.GetXplatServiceController(serviceName))
                {
                    return xplatServiceController.GetServiceStatus();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on GetStatus. {0} {1} {2}", ex.ToString(), ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ServiceStatus.NotFound;
            }
        }
    }
}
