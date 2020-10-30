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


using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Resources;
using log4net;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASC.HealthCheck.Controllers
{
    public class DownloadLogsApiController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(DownloadLogsApiController));
        public ResultHelper ResultHelper { get { return new ResultHelper(Configuration.Formatters.JsonFormatter); } }

        [HttpGet]
        public HttpResponseMessage DownloadLogs(string startDate, string endDate)
        {
            log.Debug("DownloadLogs");
            if (string.IsNullOrWhiteSpace(startDate))
            {
                throw new ArgumentException("startDate");
            }
            if (string.IsNullOrWhiteSpace(endDate))
            {
                throw new ArgumentException("endDate");
            }
            DateTime startDateStr;
            DateTime endDateStr;
            
            if (string.IsNullOrWhiteSpace(startDate))
            {
                log.Error("Argument startDate is white space, null or empty");
                return ResultHelper.WrongParameterError();
            }
            if (string.IsNullOrWhiteSpace(endDate))
            {
                log.Error("Argument endDate is white space, null or empty");
                return ResultHelper.WrongParameterError();
            }
            try
            {
                startDateStr = Convert.ToDateTime(startDate);
            }
            catch
            {
                log.ErrorFormat("Wrong argument startDate {0}", startDate);
                return ResultHelper.WrongParameterError();
            }
            try
            {
                endDateStr = Convert.ToDateTime(endDate);
            }
            catch
            {
                log.ErrorFormat("Wrong argument endDate {0}", endDate);
                return ResultHelper.WrongParameterError();
            }
            return Download(startDateStr, endDateStr);
        }

        private HttpResponseMessage Download(DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                var logHelper = new LogHelper(result, startDate, endDate);
                logHelper.DownloadArchive();
                log.DebugFormat("Logs was downloaded. startDate={0} endDate={1}", startDate, endDate);
                return result;
            }
            catch(Exception ex)
            {
                log.ErrorFormat("Unexpected error on Download logs: {0} {1}",
    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.DownloadLogError);
            }
        }
    }
}