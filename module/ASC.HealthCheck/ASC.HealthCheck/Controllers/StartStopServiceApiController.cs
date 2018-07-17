/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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