/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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