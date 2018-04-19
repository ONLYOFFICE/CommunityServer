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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Documents;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
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

        [Delete(@"report/file/{fileid:[0-9]+}")]
        public void DeleteFile(int fileid)
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            if (fileid < 0) throw new ArgumentException();

            var file = DaoFactory.ReportDao.GetFile(fileid);

            if (file == null) throw new Exception("File not found");

            DaoFactory.ReportDao.DeleteFile(fileid);
        }

        [Read(@"report/status")]
        public ReportTaskState GetStatus()
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            return ReportHelper.GetCurrentState();
        }

        [Read(@"report/terminate")]
        public void Terminate()
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            ReportHelper.Terminate();
        }

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

        [Create(@"report/generate")]
        public ReportTaskState GenerateReport(ReportType type, ReportTimePeriod timePeriod, Guid[] managers)
        {
            if (!Global.CanCreateReports)
                throw CRMSecurity.CreateSecurityException();

            var state = ReportHelper.GetCurrentState();

            return state ?? ReportHelper.RunGenareteReport(type, timePeriod, managers);
        }
    }
}