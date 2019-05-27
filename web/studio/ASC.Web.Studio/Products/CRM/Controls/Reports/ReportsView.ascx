<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportsView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Reports.ReportsView" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<div class="reports-content-container display-none">
    <% if (ViewFiles)
       { %>

    <div class="header-base"><%= CRMReportResource.GeneratedReports %></div><br />
    <p><%= CRMReportResource.GeneratedReportsDescription %></p>
    <br />
    <asp:PlaceHolder id="_phFilesView" runat="server"></asp:PlaceHolder>
    
    <% } else { %>
    
    <div class="header-base"><%= ReportHeader %></div><br />
    <p><%= ReportDescription %></p>
    <br />

    <% if (CurrentReportType == ReportType.WorkloadByVoip && !VoipNumberData.Allowed) %>
    <% { %>
    <div>
        <%= string.Format(CRMReportResource.WorkloadByVoipNotAllowed,
        "<a class=\"link underline\" href=\"" + CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) + "#crm\">",
        "</a>") %>
    </div>
    <% } else { %>
    <div class="headerPanelSmall header-base-small"><%= CRMReportResource.TimePeriod %>:</div>
    <select id="timePeriod_Reports">
    <% foreach (var reportTimePeriod in ReportTimePeriodArray)
       { %>
        <option value="<%= (int) reportTimePeriod.Key %>"><%= reportTimePeriod.Value %></option>
    <% } %>
    </select>
    <br />
    <br />
    <div class="headerPanelSmall header-base-small"><%= CRMReportResource.Managers %>:</div>
    <div id="userSelectorContainer_Reports"></div>
    <div class="big-button-container">
        <a id="generateBtn_Reports" class="button blue middle disable"><%= CRMReportResource.GenerateReport %></a>
    </div>
    <% } %>

    <% } %>

</div>

