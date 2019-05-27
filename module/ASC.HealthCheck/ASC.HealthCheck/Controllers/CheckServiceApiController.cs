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