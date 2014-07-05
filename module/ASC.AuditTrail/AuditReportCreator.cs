/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using log4net;

namespace ASC.AuditTrail
{
    public static class AuditReportCreator
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");

        public static string CreateCsvReport(IEnumerable<LoginEvent> events, string reportName)
        {
            var reportPath = Path.GetTempPath() + reportName;

            using (var writer = File.CreateText(reportPath))
            {
                var csv = new CsvWriter(writer);
                csv.Configuration.RegisterClassMap<LoginEventMap>();

                csv.WriteHeader<LoginEvent>();

                foreach (var evt in events)
                {
                    csv.WriteRecord(evt);
                }
            }

            return reportPath;
        }

        internal class LoginEventMap : CsvClassMap<LoginEvent>
        {
            public override void CreateMap()
            {
                Map(m => m.IP).Name(AuditReportResource.IpCol);
                Map(m => m.Browser).Name(AuditReportResource.BrowserCol);
                Map(m => m.Platform).Name(AuditReportResource.PlatformCol);
                Map(m => m.Date).Name(AuditReportResource.DateCol);
                Map(m => m.UserName).Name(AuditReportResource.UserCol);
                Map(m => m.Page).Name(AuditReportResource.PageCol);
                Map(m => m.ActionText).Name(AuditReportResource.ActionCol);
            }
        }

        public static string CreateXlsxReport(IEnumerable<LoginEvent> events)
        {
            var reportPath = Path.GetTempPath() + "LH" + Guid.NewGuid() + ".xlsx";
            var reportFile = new FileInfo(reportPath);

            using (var package = new ExcelPackage(reportFile))
            {
                try
                {
                    var ws = package.Workbook.Worksheets.Add("Report");

                    ws.Column(1).Width = 20;
                    ws.Cells[1, 1].Value = AuditReportResource.IpCol;

                    ws.Column(2).Width = 20;
                    ws.Cells[1, 2].Value = AuditReportResource.BrowserCol;

                    ws.Column(3).Width = 20;
                    ws.Cells[1, 3].Value = AuditReportResource.PlatformCol;

                    ws.Column(4).Width = 20;
                    ws.Cells[1, 4].Value = AuditReportResource.DateCol;

                    ws.Column(5).Width = 20;
                    ws.Cells[1, 5].Value = AuditReportResource.UserCol;

                    ws.Column(6).Width = 40;
                    ws.Cells[1, 6].Value = AuditReportResource.PageCol;

                    ws.Column(7).Width = 40;
                    ws.Cells[1, 7].Value = AuditReportResource.ActionCol;

                    ws.Cells[1, 1, 1, 7].Style.Font.Bold = true;

                    var eventsList = events.ToList();
                    for (var i = 0; i < eventsList.Count; i++)
                    {
                        var row = i + 2;
                        var evt = eventsList[i];

                        ws.Cells[row, 1].Value = evt.IP;
                        ws.Cells[row, 2].Value = evt.Browser;
                        ws.Cells[row, 3].Value = evt.Platform;
                        ws.Cells[row, 4].Value = evt.Date.ToString("MM.dd.yyyy HH:mm");
                        ws.Cells[row, 5].Value = evt.UserName;
                        ws.Cells[row, 6].Value = evt.Page;
                        ws.Cells[row, 7].Value = evt.ActionText;
                    }

                    package.Save();
                }
                catch(Exception ex)
                {
                    log.Error("Error while generating report: " + ex);
                    File.Delete(reportPath);
                    return null;
                }
            }

            return reportPath;
        }

        public static string CreateCsvReport(IEnumerable<AuditEvent> events, string reportName)
        {
            var reportPath = Path.GetTempPath() + reportName;

            using (var writer = File.CreateText(reportPath))
            {
                var csv = new CsvWriter(writer);
                csv.Configuration.RegisterClassMap<AuditEventMap>();

                csv.WriteHeader<AuditEvent>();

                foreach (var evt in events)
                {
                    csv.WriteRecord(evt);
                }
            }

            return reportPath;
        }

        public static void DeleteReport(string reportPath)
        {
            if (reportPath != null)
            {
                File.Delete(reportPath);
            }
        }

        internal class AuditEventMap : CsvClassMap<AuditEvent>
        {
            public override void CreateMap()
            {
                Map(m => m.IP).Name(AuditReportResource.IpCol);
                Map(m => m.Browser).Name(AuditReportResource.BrowserCol);
                Map(m => m.Platform).Name(AuditReportResource.PlatformCol);
                Map(m => m.Date).Name(AuditReportResource.DateCol);
                Map(m => m.UserName).Name(AuditReportResource.UserCol);
                Map(m => m.Page).Name(AuditReportResource.PageCol);
                Map(m => m.ActionTypeText).Name(AuditReportResource.ActionTypeCol);
                Map(m => m.ActionText).Name(AuditReportResource.ActionCol);
                Map(m => m.Product).Name(AuditReportResource.ProductCol);
                Map(m => m.Module).Name(AuditReportResource.ModuleCol);
            }
        }

        public static string CreateXlsxReport(IEnumerable<AuditEvent> events)
        {
            var reportPath = Path.GetTempPath() + "AT" + Guid.NewGuid() + ".xlsx";
            var reportFile = new FileInfo(reportPath);

            using (var package = new ExcelPackage(reportFile))
            {
                try
                {
                    var ws = package.Workbook.Worksheets.Add("Report");

                    ws.Column(1).Width = 20;
                    ws.Cells[1, 1].Value = AuditReportResource.IpCol;

                    ws.Column(2).Width = 20;
                    ws.Cells[1, 2].Value = AuditReportResource.BrowserCol;

                    ws.Column(3).Width = 20;
                    ws.Cells[1, 3].Value = AuditReportResource.PlatformCol;

                    ws.Column(4).Width = 20;
                    ws.Cells[1, 4].Value = AuditReportResource.DateCol;

                    ws.Column(5).Width = 20;
                    ws.Cells[1, 5].Value = AuditReportResource.UserCol;

                    ws.Column(6).Width = 40;
                    ws.Cells[1, 6].Value = AuditReportResource.PageCol;

                    ws.Column(7).Width = 20;
                    ws.Cells[1, 7].Value = AuditReportResource.ActionTypeCol;

                    ws.Column(8).Width = 40;
                    ws.Cells[1, 8].Value = AuditReportResource.ActionCol;

                    ws.Column(9).Width = 20;
                    ws.Cells[1, 9].Value = AuditReportResource.ProductCol;

                    ws.Column(10).Width = 20;
                    ws.Cells[1, 10].Value = AuditReportResource.ModuleCol;

                    ws.Cells[1, 1, 1, 10].Style.Font.Bold = true;

                    var eventsList = events.ToList();
                    for (var i = 0; i < eventsList.Count; i++)
                    {
                        var row = i + 2;
                        var evt = eventsList[i];


                        ws.Cells[row, 1].Value = evt.IP;
                        ws.Cells[row, 2].Value = evt.Browser;
                        ws.Cells[row, 3].Value = evt.Platform;
                        ws.Cells[row, 4].Value = evt.Date.ToString("MM.dd.yyyy HH:mm");
                        ws.Cells[row, 5].Value = evt.UserName;
                        ws.Cells[row, 6].Value = evt.Page;
                        ws.Cells[row, 7].Value = evt.ActionTypeText;
                        ws.Cells[row, 8].Value = evt.ActionText;
                        ws.Cells[row, 9].Value = evt.Product;
                        ws.Cells[row, 10].Value = evt.Module;
                    }
                    package.Save();
                }
                catch(Exception ex)
                {
                    log.Error("Error while generating report: " + ex);
                    File.Delete(reportPath);
                    return null;
                }
            }

            return reportPath;
        }
    }
}