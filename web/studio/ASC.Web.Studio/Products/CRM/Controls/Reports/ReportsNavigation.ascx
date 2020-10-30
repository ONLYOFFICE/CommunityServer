<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportsNavigation.ascx.cs" Inherits="ASC.Web.CRM.Controls.Reports.ReportsNavigation" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div class="reports-menu-container display-none">
    <span class="reports-category"><%= CRMReportResource.Reports %></span>
    <ul>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.SalesByManagers %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.SalesByManagers ? "active" : string.Empty %>">
                <%= CRMReportResource.SalesByManagersReport %>
            </a>
        </li>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.SalesForecast %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.SalesForecast ? "active" : string.Empty %>">
                <%= CRMReportResource.SalesForecastReport %>
            </a>
        </li>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.SalesFunnel %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.SalesFunnel ? "active" : string.Empty %>">
                <%= CRMReportResource.SalesFunnelReport %>
            </a>
        </li>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.WorkloadByContacts %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.WorkloadByContacts ? "active" : string.Empty %>">
                <%= CRMReportResource.WorkloadByContactsReport %>
            </a>
        </li>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.WorkloadByTasks %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.WorkloadByTasks ? "active" : string.Empty %>">
                <%= CRMReportResource.WorkloadByTasksReport %>
            </a>
        </li>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.WorkloadByDeals %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.WorkloadByDeals ? "active" : string.Empty %>">
                <%= CRMReportResource.WorkloadByDealsReport %>
            </a>
        </li>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.WorkloadByInvoices %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.WorkloadByInvoices ? "active" : string.Empty %>">
                <%= CRMReportResource.WorkloadByInvoicesReport %>
            </a>
        </li>
        <% if (!CoreContext.Configuration.CustomMode) { %>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.WorkloadByVoip %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.WorkloadByVoip ? "active" : string.Empty %>">
                <%= CRMReportResource.WorkloadByVoipReport %>
            </a>
        </li>
        <% } %>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.SummaryForThePeriod %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.SummaryForThePeriod ? "active" : string.Empty %>">
                <%= CRMReportResource.SummaryForThePeriodReport %>
            </a>
        </li>
        <li>
            <a href="Reports.aspx?reportType=<%= (int)ReportType.SummaryAtThisMoment %>" class="menu-report-name <%= !ViewFiles && CurrentReportType == ReportType.SummaryAtThisMoment ? "active" : string.Empty %>">
                <%= CRMReportResource.SummaryAtThisMomentReport %>
            </a>
        </li>
    </ul>
    <br/>
    <a href="Reports.aspx" class="reports-category menu-report-name  <%= ViewFiles ? "active" : string.Empty %>">
        <%= CRMReportResource.GeneratedReports %>
    </a>
</div>