<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginHistory.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.LoginHistory" %>
<%@ Import Namespace="Resources" %>

<script id="loginEventTmpl" type="text/x-jquery-tmpl">
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

<p id="generate-text"><%= AuditResource.GenerateText %></p>
<p id="last-events-text"><%= AuditResource.LastLoginEventsText %></p>

<table id="events-table" class="table-list height32 display-none">
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
<div id="events-table-dscr" class="gray-text display-none"><%= AuditResource.TotalAuditItems %>: <span></span></div>

<a id="download-report-btn" class="button blue middle" href="#generate" target="_blank"><%= AuditResource.DownloadReportBtn %></a>

<div id="empty-screen">
    <asp:PlaceHolder ID="emptyScreenHolder" runat="server"></asp:PlaceHolder>
</div>