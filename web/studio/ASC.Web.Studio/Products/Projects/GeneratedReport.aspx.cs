/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.IO;
using System.Text;
using System.Web;
using ASC.Core.Tenants;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Core.Files;
using ASC.Web.Files.Utils;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls;
using ASC.Web.Projects.Controls.Reports;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using Global = ASC.Web.Projects.Classes.Global;
using PathProvider = ASC.Web.Projects.Classes.PathProvider;

namespace ASC.Web.Projects
{
    public partial class GeneratedReport : BasePage
    {
        protected int GenerateReportType;

        protected bool HasData { get; set; }

        protected bool TemplateNotFound { get; set; }

        protected override bool CheckSecurity { get { return !Participant.IsVisitor; } }

        protected override void PageLoad()
        {
            Page.RegisterBodyScripts("~/js/third-party/sorttable.js");
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("reports.js"));

            exportReportPopup.Options.IsPopup = true;
            HasData = true;
            TemplateNotFound = false;
            Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;

            var repType = UrlParameters.ReportType;

            var filter = TaskFilter.FromUri(HttpContext.Current.Request.GetUrlRewriter());
            var templateID = UrlParameters.EntityID;
            var reportName = "";

            if (templateID >= 0)
            {
                var template = EngineFactory.ReportEngine.GetTemplate(templateID);
                if (template != null)
                {
                    filter = template.Filter;
                    repType = template.ReportType;
                    GenerateReportType = (int) template.ReportType;

                    Title = HeaderStringHelper.GetPageTitle(string.Format(ReportResource.ReportPageTitle, HttpUtility.HtmlDecode(template.Name)));
                    reportName = string.Format(ReportResource.ReportPageTitle, template.Name);
                }
                else
                {
                    RedirectNotFound("reports.aspx");
                }
            }

            var report = Report.CreateNewReport(repType, filter);

            if (templateID == 0)
            {
                Title = HeaderStringHelper.GetPageTitle(string.Format(ReportResource.ReportPageTitle, report.ReportInfo.Title));
                reportName = string.Format(ReportResource.ReportPageTitle, report.ReportInfo.Title);
            }

            var filters = (ReportFilters)LoadControl(PathProvider.GetFileStaticRelativePath("Reports/ReportFilters.ascx"));
            filters.Report = report;
            _filter.Controls.Add(filters);

            var outputFormat = GetOutputFormat();
            var result = report.BuildReport(outputFormat, templateID);

            OutputData(report, result, reportName, outputFormat);
        }

        private static ReportViewType GetOutputFormat()
        {
            var outputFormat = ReportViewType.Html;
            switch (HttpContext.Current.Request["format"])
            {
                case "csv":
                    outputFormat = ReportViewType.Csv;
                    break;
                case "xml":
                    outputFormat = ReportViewType.Xml;
                    break;
                case "email":
                    outputFormat = ReportViewType.EMail;
                    break;
                case "html":
                    outputFormat = ReportViewType.Html;
                    break;
            }

            return outputFormat;
        }

        private void OutputData(Report report, string result, string reportName, ReportViewType outputFormat)
        {
            switch (outputFormat)
            {
                case ReportViewType.Html:
                    reportResult.Text = result;

                    var sb = new StringBuilder();
                    sb.Append("<div class='report-name'>");
                    sb.Append(reportName);
                    sb.Append("<span class='generation-date'> (");
                    sb.Append(TenantUtil.DateTimeNow().ToString(DateTimeExtension.ShortDatePattern));
                    sb.Append(")</span>");
                    sb.Append("</div>");
                    reportFilter.Text = sb.ToString();
                    break;

                case ReportViewType.Xml:
                case ReportViewType.EMail:
                    if (result != null)
                    {
                        var ext = outputFormat.ToString().ToLower();
                        Response.Clear();
                        Response.ContentType = "text/" + ext + "; charset=utf-8";
                        Response.ContentEncoding = Encoding.UTF8;
                        Response.Charset = Encoding.UTF8.WebName;
                        Response.AppendHeader("Content-Disposition", string.Format("attachment; filename={0}.{1}", report.FileName, ext));
                        Response.Write(result);
                        Response.End();
                    }
                    break;

                case ReportViewType.Csv:
                    string fileURL;

                    using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                    {
                        var file = FileUploader.Exec(Files.Classes.Global.FolderMy.ToString(), report.FileName + ".csv", result.Length, memStream, true);

                        fileURL = FilesLinkUtility.GetFileWebEditorUrl((int)file.ID);
                        fileURL += string.Format("&options={{\"delimiter\":{0},\"codePage\":{1}}}",
                                                 (int)Global.ReportCsvDelimiter.Key,
                                                 Encoding.UTF8.CodePage);
                    }

                    Response.Redirect(fileURL);

                    break;
            }
        }
    }
}