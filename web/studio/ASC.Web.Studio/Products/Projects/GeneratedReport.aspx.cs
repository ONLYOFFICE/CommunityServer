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
            Page.RegisterBodyScripts(ResolveUrl("~/js/third-party/sorttable.js"));
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("reports.js"));

            exportReportPopup.Options.IsPopup = true;
            HasData = true;
            TemplateNotFound = false;
            Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;

            int.TryParse(UrlParameters.ReportType, out GenerateReportType);
            var repType = (ReportType) GenerateReportType;

            var filter = TaskFilter.FromUri(HttpContext.Current.Request.GetUrlRewriter());
            int templateID;
            var reportName = "";

            if (int.TryParse(UrlParameters.EntityID, out templateID))
            {
                var template = Global.EngineFactory.GetReportEngine().GetTemplate(templateID);
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

            if (templateID != 0)
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