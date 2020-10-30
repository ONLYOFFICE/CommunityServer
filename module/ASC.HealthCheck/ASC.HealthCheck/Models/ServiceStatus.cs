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


using ASC.HealthCheck.Resources;

namespace ASC.HealthCheck.Models
{
    public enum ServiceStatus
    {
        NotFound,
        Running,
        Stopped,
        StartPending,
        StopPending
    }

    public static class ServiceStatusExtention
    {
        public static string GetStringStatus(this ServiceStatus serviceEnum)
        {
            switch (serviceEnum)
            {
                case ServiceStatus.Running:
                    return HealthCheckResource.StatusServiceWork;
                case ServiceStatus.Stopped:
                    return HealthCheckResource.StatusServiceStoped;
                case ServiceStatus.StartPending:
                    return HealthCheckResource.StatusServiceStartWorking;
                case ServiceStatus.StopPending:
                    return HealthCheckResource.ServiceStillNotStoped;
                case ServiceStatus.NotFound:
                    return HealthCheckResource.ServiceNotFound;
                default:
                    return "";
            }
        }
        public static string GetMessageStatus(this ServiceStatus serviceEnum)
        {
            switch (serviceEnum)
            {
                case ServiceStatus.Running:
                    return HealthCheckResource.ServiceWorks;
                case ServiceStatus.Stopped:
                    return HealthCheckResource.ServiceStop;
                case ServiceStatus.StartPending:
                    return HealthCheckResource.ServiceStillNotStarted;
                case ServiceStatus.StopPending:
                    return HealthCheckResource.ServiceStillNotStoped;
                case ServiceStatus.NotFound:
                    return HealthCheckResource.StatusServiceError;
                default:
                    return "";
            }
        }
    }
}