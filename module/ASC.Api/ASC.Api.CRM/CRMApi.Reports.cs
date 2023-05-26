/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Security;

using ASC.Api.Attributes;
using ASC.Api.Documents;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.Files.Services.DocumentService;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>Returns a list of all the user report files.</summary>
        /// <short>Get report files</short>
        /// <category>Reports</category>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">Report files</returns>
        /// <exception cref="SecurityException">If the user can't create reports</exception>
        /// <path>api/2.0/crm/report/files</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"report/files")]
        public IEnumerable<FileWrapper> GetFiles()
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            var reportDao = DaoFactory.ReportDao;

            var files = reportDao.GetFiles();

            if (!files.Any())
            {
                var sampleSettings = CRMReportSampleSettings.LoadForCurrentUser();

                if (sampleSettings.NeedToGenerate)
                {
                    files = reportDao.SaveSampleReportFiles();
                    sampleSettings.NeedToGenerate = false;
                    sampleSettings.SaveForCurrentUser();
                }
            }

            return files.ConvertAll(file => new FileWrapper(file)).OrderByDescending(file => file.Id);
        }

        /// <summary>Deletes a report file with the ID specified in the request.</summary>
        /// <param type="System.Int32, System" method="url" name="fileid">File ID</param>
        /// <short>Delete a report file</short>
        /// <category>Reports</category>
        /// <returns></returns>
        /// <exception cref="SecurityException">If the user can't create reports</exception>
        /// <exception cref="ArgumentException">If file ID is less than 0</exception>
        /// <exception cref="ItemNotFoundException">If file is not found</exception>
        /// <path>api/2.0/crm/report/file/{fileid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"report/file/{fileid:[0-9]+}")]
        public void DeleteFile(int fileid)
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            if (fileid < 0) throw new ArgumentException();

            var file = DaoFactory.ReportDao.GetFile(fileid);

            if (file == null) throw new ItemNotFoundException("File not found");

            DaoFactory.ReportDao.DeleteFile(fileid);
        }

        /// <summary>Returns a status of the report generation task.</summary>
        /// <short>Get the report generation status</short>
        /// <category>Reports</category>
        /// <returns type="ASC.Web.Files.Services.DocumentService.ReportState, ASC.Web.Files">Report status</returns>
        /// <exception cref="SecurityException">If the user can't create reports</exception>
        /// <path>api/2.0/crm/report/status</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"report/status")]
        public ReportState GetStatus()
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            return DocbuilderReportsUtility.Status(ReportOrigin.CRM);
        }

        /// <summary>Terminates the report generation task.</summary>
        /// <short>Terminate the report generation</short>
        /// <category>Reports</category>
        /// <returns></returns>
        /// <exception cref="SecurityException">If the user can't create reports</exception>
        /// <path>api/2.0/crm/report/terminate</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"report/terminate")]
        public void Terminate()
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            DocbuilderReportsUtility.Terminate(ReportOrigin.CRM);
        }

        /// <summary>Checks the report data for the parameters specified in the request.</summary>
        /// <param type="ASC.CRM.Core.ReportType, ASC.CRM.Core" name="type">Report type</param>
        /// <param type="ASC.CRM.Core.ReportTimePeriod, ASC.CRM.Core" name="timePeriod">Report time period</param>
        /// <param type="System.Guid[], System" name="managers">Managers</param>
        /// <short>Check report data</short>
        /// <category>Reports</category>
        /// <returns>Report information</returns>
        /// <path>api/2.0/crm/report/check</path>
        /// <httpMethod>POST</httpMethod>
        /// <exception cref="SecurityException">If the user can't create reports</exception>
        [Create(@"report/check")]
        public object CheckReportData(ReportType type, ReportTimePeriod timePeriod, Guid[] managers)
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            return new
            {
                hasData = ReportHelper.CheckReportData(type, timePeriod, managers),
                missingRates = ReportHelper.GetMissingRates(type)
            };
        }

        /// <summary>Runs the report generation task with the parameters specified in the request.</summary>
        /// <param type="ASC.CRM.Core.ReportType, ASC.CRM.Core" name="type">Report type</param>
        /// <param type="ASC.CRM.Core.ReportTimePeriod, ASC.CRM.Core" name="timePeriod">Report time period</param>
        /// <param type="System.Guid[], System" name="managers">Managers</param>
        /// <short>Generate a report</short>
        /// <category>Reports</category>
        /// <returns type="ASC.Web.Files.Services.DocumentService.ReportState, ASC.Web.Files">Report status</returns>
        /// <exception cref="SecurityException">If the user can't create reports</exception>
        /// <path>api/2.0/crm/report/generate</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"report/generate")]
        public ReportState GenerateReport(ReportType type, ReportTimePeriod timePeriod, Guid[] managers)
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            return ReportHelper.RunGenareteReport(type, timePeriod, managers);
        }
    }
}