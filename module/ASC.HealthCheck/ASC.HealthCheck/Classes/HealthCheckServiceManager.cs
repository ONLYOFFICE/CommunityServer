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
