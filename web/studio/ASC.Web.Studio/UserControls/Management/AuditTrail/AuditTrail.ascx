<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuditTrail.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.AuditTrail" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<script id="auditEventTmpl" type="text/x-jquery-tmpl">
    <tr>
        <td class="user">
            <span>${user}</span></td>
        <td class="date">
            <span>${displayDate}</span>
        </td>
        <td class="action">
            <span>${action}</span>
        </td>
    </tr>
</script>

<div id="events-box" class="display-none <%= EnableAuditTrail ? "" : "disable" %>">
    <div class="header-base"><%= AuditResource.AuditTrailNav %></div>
     <% if (!EnableAuditTrail)
         { %>
        <br/>
        <span class="settings-help-block">
            <span><%= String.Format(Resource.ErrorNotAllowedOption, "<b>", "</b>") %></span>
            <br/>
            <a href="<%= TariffPageLink %>" target="_blank"> <%= Resource.ViewTariffPlans %></a>
        </span>
    <%} %>
    <p><%= AuditResource.AuditLatestText %></p>
    
    <div class="header-base-small"><%= AuditResource.StoragePeriod %>:</div>
    <div class="audit-settings-block">
        <input id="lifetime-input" class="textEdit" <%= EnableAuditTrail ? "" : "disabled='disabled'" %> type="text" maxlength="3">
        <a id="save-settings-btn" class="button blue small <%= EnableAuditTrail ? "" : "disable" %>"><%= Resource.SaveButton %></a>
    </div>

    <p><%= AuditResource.AuditDownloadText %></p> 

    <table id="events-table" class="table-list height32">
        <thead>
            <tr>
                <th class="user">
                    <span><%= AuditResource.UserCol %></span>
                </th>
                <th class="date">
                    <span><%= AuditResource.DateCol %></span>
                </th>
                <th class="action">
                    <span><%= AuditResource.ActionCol %></span>
                </th>
            </tr>
        </thead>
        <tbody>
        </tbody>
    </table>

    <div id="events-table-dscr" class="gray-text"><%= AuditResource.TotalAuditItems %>: <span></span></div>

    <a id="download-report-btn" class="button blue middle <%= EnableAuditTrail ? "" : "disable" %>" href="#generate"><%= AuditResource.DownloadReportBtn %></a>
    <span id="generate-text" class="display-none"><%= AuditResource.GenerateText %></span>
</div>

<div id="empty-screen" class="display-none">
    <asp:PlaceHolder ID="emptyScreenHolder" runat="server"></asp:PlaceHolder>
</div>