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


using System.Net.Http;
using System.Web.Http;

using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Models;
using ASC.HealthCheck.Resources;

using log4net;

namespace ASC.HealthCheck.Controllers
{
    public class StartStopServiceApiController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(StartStopServiceApiController));
        public ResultHelper ResultHelper { get { return new ResultHelper(Configuration.Formatters.JsonFormatter); } }

        [HttpPost]
        public HttpResponseMessage StartService(string service)
        {
            if (string.IsNullOrWhiteSpace(service))
            {
                log.Error("Argument service is white space, null or empty");
                return ResultHelper.Error(HealthCheckResource.WrongParameter);
            }

            return Start(service);
        }

        [HttpPost]
        public HttpResponseMessage StopService(string service)
        {
            if (string.IsNullOrWhiteSpace(service))
            {
                log.Error("Argument service is white space, null or empty");
                return ResultHelper.WrongParameterError();
            }

            return Stop(service);
        }

        [HttpGet]
        public HttpResponseMessage GetStatus(string service)
        {
            if (string.IsNullOrWhiteSpace(service))
            {
                log.Error("Argument service is white space, null or empty");
                return ResultHelper.WrongParameterError();
            }

            return GetServiceStatus(service);
        }

        private HttpResponseMessage GetServiceStatus(string service)
        {
            var healthCheckServiceManager = new HealthCheckServiceManager(HealthCheckRunner.ServiceRepository);
            var serviceName = healthCheckServiceManager.GetServiceName(service);

            if (!serviceName.HasValue)
            {
                log.ErrorFormat("Wrong service {0}", service);
                return ResultHelper.WrongParameterError();
            }
            var serviceStatus = healthCheckServiceManager.GetStatus(serviceName.Value);
            var status = serviceStatus.GetStringStatus();
            string message;
            int code;

            switch (serviceStatus)
            {
                case ServiceStatus.StartPending:
                    message = HealthCheckResource.ServiceStartWorking;
                    code = 1;
                    break;
                case ServiceStatus.Running:
                    message = HealthCheckResource.ServiceRun;
                    code = 2;
                    break;
                case ServiceStatus.Stopped:
                    message = HealthCheckResource.ServiceStop;
                    code = 3;
                    break;
                default:
                    message = HealthCheckResource.NotExpectedState;
                    code = 0;
                    break;
            }

            HealthCheckRunner.ServiceRepository.SetStates(serviceName.Value, status,
                message != HealthCheckResource.ServiceRun ? HealthCheckResource.ServiceRun : string.Empty);

            return ResultHelper.GetContent(new {code, status, message});
        }

        private HttpResponseMessage Stop(string service)
        {
            var healthCheckServiceManager = new HealthCheckServiceManager(HealthCheckRunner.ServiceRepository);
            var serviceName = healthCheckServiceManager.GetServiceName(service);
            if (!serviceName.HasValue)
            {
                log.ErrorFormat("Wrong service {0}", service);
                return ResultHelper.WrongServiceNameError();
            }
            var message = healthCheckServiceManager.StopService(serviceName.Value);
            if (message == string.Empty)
            {
                log.DebugFormat("Stop service: {0}", serviceName);
                return ResultHelper.Success(ServiceStatus.Stopped.GetStringStatus(), ServiceStatus.Stopped.GetMessageStatus());
            }
            log.ErrorFormat("Not stop service: {0}, status = {1}", serviceName, message);
            return ResultHelper.StatusServerError(message);
        }

        private HttpResponseMessage Start(string service)
        {
            var healthCheckServiceManager = new HealthCheckServiceManager(HealthCheckRunner.ServiceRepository);
            var serviceName = healthCheckServiceManager.GetServiceName(service);
            if (!serviceName.HasValue)
            {
                log.ErrorFormat("Wrong service {0}", service);
                return ResultHelper.WrongServiceNameError();
            }
            var message = healthCheckServiceManager.StartService(serviceName.Value);
            if (message == string.Empty)
            {
                log.DebugFormat("Start service: {0}", serviceName);
                return ResultHelper.Success(HealthCheckResource.StatusServiceStartWorking, HealthCheckResource.ServiceStartWorking);
            }

            log.ErrorFormat("Not start service: {0}, status = {1}", serviceName, message);
            return ResultHelper.StatusServerError(message);
        }
    }
}