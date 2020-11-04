<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportFilters.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Reports.ReportFilters" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Projects.Core.Domain.Reports" %>

<div id="reportFilters" class="report-filter-container clearFix">

<% if (Report.ReportType == ReportType.UsersActivity || Report.ReportType == ReportType.TimeSpend) %>
<% { %>
<div class="filter-item-container">
    <span id="otherInterval" <%if(Report.Filter.TimeInterval != ReportTimeInterval.Absolute || Report.Filter.FromDate.Equals(DateTime.MinValue)){%> style="display:none;" <%} %> >
            <asp:TextBox runat="server" ID="fromDate" CssClass="textEditCalendar" Width="75px" />
            <div class="splitter-input"></div>
            <asp:TextBox runat="server" ID="toDate" CssClass="textEditCalendar" Width="75px" />
        <div class="errorText display-none float-left" style="margin:3px 0 0 10px;"><%=ReportResource.IncorrectDateRage%></div>
    </span>
</div>
<% } %>
</div>