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
        /// <summary>Returns a list of all user report files</summary>
        /// <short>Get report files</short>
        /// <category>Reports</category>
        /// <returns>Report files</returns>
        /// <exception cref="SecurityException">if user can't create reports</exception>
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

        /// <summary>Delete the report file with the ID specified in the request</summary>
        /// <param name="fileid">File ID</param>
        /// <short>Delete report file</short>
        /// <category>Reports</category>
        /// <exception cref="SecurityException">if user can't create reports</exception>
        /// <exception cref="ArgumentException">if fileid les than 0</exception>
        /// <exception cref="ItemNotFoundException">if file not found</exception>
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

        /// <summary>Get the state of the report generation task</summary>
        /// <short>Get report generation state</short>
        /// <category>Reports</category>
        /// <returns>Report state</returns>
        /// <exception cref="SecurityException">if user can't create reports</exception>
        [Read(@"report/status")]
        public ReportState GetStatus()
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            return DocbuilderReportsUtility.Status(ReportOrigin.CRM);
        }

        /// <summary>Terminate the report generation task</summary>
        /// <short>Terminate report generation</short>
        /// <category>Reports</category>
        /// <exception cref="SecurityException">if user can't create reports</exception>
        [Read(@"report/terminate")]
        public void Terminate()
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            DocbuilderReportsUtility.Terminate(ReportOrigin.CRM);
        }

        /// <summary>Check data availability for a report</summary>
        /// <param name="type">Report type</param>
        /// <param name="timePeriod">Time period</param>
        /// <param name="managers">Managers</param>
        /// <short>Check report data</short>
        /// <category>Reports</category>
        /// <returns>Object</returns>
        /// <exception cref="SecurityException">if user can't create reports</exception>
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

        /// <summary>Run the report generation task</summary>
        /// <param name="type">Report type</param>
        /// <param name="timePeriod">Time period</param>
        /// <param name="managers">Managers</param>
        /// <short>Generate report</short>
        /// <category>Reports</category>
        /// <returns>Report state</returns>
        /// <exception cref="SecurityException">if user can't create reports</exception>
        [Create(@"report/generate")]
        public ReportState GenerateReport(ReportType type, ReportTimePeriod timePeriod, Guid[] managers)
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            return ReportHelper.RunGenareteReport(type, timePeriod, managers);
        }
    }
}