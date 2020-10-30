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