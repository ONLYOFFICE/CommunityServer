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
using System.Collections.Generic;
using System.Linq;

namespace ASC.HealthCheck.Models
{
    public class ServiceRepository : IServiceRepository
    {
        private const int RestartAttempt = 3;
        private const int AttemptsLimit = 4;
        private static readonly HashSet<Service> serviceSet = new HashSet<Service>();
        private static readonly object syncRoot = new object();

        public void Add(ServiceEnum serviceName)
        {
            lock (syncRoot)
            {
                if (serviceSet.All(s => s.ServiceName != serviceName))
                {
                    serviceSet.Add(Service.CreateNewService(serviceName));
                }
            }
        }

        public void Remove(ServiceEnum serviceName)
        {
            lock (syncRoot)
            {
                var service = serviceSet.FirstOrDefault(s => s.ServiceName == serviceName);
                if (service != null)
                {
                    serviceSet.Remove(service);
                }
            }
        }

        public bool HasAtempt(ServiceEnum serviceName)
        {
            lock (syncRoot)
            {
                var service = serviceSet.FirstOrDefault(s => s.ServiceName == serviceName);
                if (service == null) return false;

                return !(service.Attempt == AttemptsLimit || (service.Attempt + 1)%1000 == 0);
            }
        }

        public void SetStates(ServiceEnum serviceName, string status, string message)
        {
            if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("status");
            if (message == null) throw new ArgumentNullException("message");

            lock (syncRoot)
            {
                var service = serviceSet.FirstOrDefault(s => s.ServiceName == serviceName);
                if (service != null)
                {
                    service.Status = status;
                    service.Message = message;
                }
            }
        }

        public Service GetService(ServiceEnum serviceName)
        {
            lock (syncRoot)
            {
                return serviceSet.FirstOrDefault(s => s.ServiceName == serviceName);
            }
        }

        // if service throws error then show error
        // else try to access service from xplatServiceController
        // and get real state of service
        public List<object> GetServices()
        {
            var list = new List<object>();
            lock (syncRoot)
            {
                foreach (var service in serviceSet)
                {
                    var code = 0;
                    if (service.Status == ServiceStatus.NotFound.GetStringStatus())
                    {
                        code = 0;
                    }
                    else
                    {
                        ServiceStatus status;
                        using (var xplatServiceController = XplatServiceController.GetXplatServiceController(service.ServiceName))
                        {
                            status = xplatServiceController.GetServiceStatus();
                        }

                        service.Status = status.GetStringStatus();
                        service.Message = status.GetMessageStatus();
                        switch (status)
                        {
                            case ServiceStatus.NotFound:
                                code = 0;
                                break;
                            case ServiceStatus.Stopped:
                                code = 1;
                                break;
                            case ServiceStatus.Running:
                                code = 2;
                                break;
                            case ServiceStatus.StartPending:
                                code = 3;
                                break;
                            case ServiceStatus.StopPending:
                                code = 4;
                                break;
                        }
                    }
                    list.Add(new
                    {
                        serviceName = service.ServiceName.ToString(),
                        status = service.Status,
                        message = service.Message == string.Empty ? ServiceStatus.Running.GetMessageStatus() : service.Message,
                        code,
                        title = service.Title
                    });
                }
                return list;
            }
        }

        public List<Service> GetServicesSnapshot()
        {
            return serviceSet.ToList();
        }

        public void DropAttempt(ServiceEnum serviceName)
        {
            lock (syncRoot)
            {
                var service = serviceSet.FirstOrDefault(s => s.ServiceName == serviceName);
                if (service != null)
                {
                    service.Attempt = 0;
                }
            }
        }

        public bool ShouldRestart(ServiceEnum serviceName)
        {
            lock (syncRoot)
            {
                var service = serviceSet.FirstOrDefault(s => s.ServiceName == serviceName);
                if (service != null)
                {
                    service.Attempt++;
                    return service.Attempt == RestartAttempt;
                }
                return false;
            }
        }
    }
}