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


using ASC.Core.Billing;
using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Models;
using ASC.HealthCheck.Resources;
using log4net;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace ASC.HealthCheck.Controllers
{
    public class CheckServiceApiController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CheckServiceApiController));
        public IServiceRepository ServiceRepository { get; set; }

        public ResultHelper ResultHelper { get { return new ResultHelper(Configuration.Formatters.JsonFormatter); } }

        public CheckServiceApiController()
        {
            ServiceRepository = HealthCheckRunner.ServiceRepository;
        }

        [HttpGet]
        public HttpResponseMessage GetState()
        {
            try
            {
                log.Debug("GetState: Get state of host");

                return HealthCheckRunner.ServerStatusIsOk()
                    ? ResultHelper.Success(HealthCheckResource.HostWorks)
                    : ResultHelper.Error(HealthCheckResource.HostError);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on GetState: {0} {1}", ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.ServerError();
            }
        }

        [HttpGet]
        public HttpResponseMessage GetStates()
        {
            try
            {
                log.Debug("GetStates: Get states of services");
                return ResultHelper.GetContent(
                    new
                    {
                        code = 1,
                        services = ServiceRepository.GetServices()
                    });
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on GetStates: {0} {1}", ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.ServiceError);
            }
        }

        [HttpGet]
        public HttpResponseMessage RefreshLicense()
        {
            try
            {
                log.Debug("ChangeLicenseStatus");
                LicenseReader.RefreshLicense();
                if (!ServiceRepository.GetServicesSnapshot().Any())
                {
                    HealthCheckRunner.Run();
                }

                return ResultHelper.Success();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on RefreshLicense: {0} {1}", ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.ServerError();
            }
        }
    }
}