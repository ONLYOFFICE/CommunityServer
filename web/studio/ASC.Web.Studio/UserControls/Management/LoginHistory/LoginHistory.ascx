<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginHistory.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.LoginHistory" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="Resources" %>

<script id="login-event-tmpl" type="text/x-jquery-tmpl">
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

<script id="online-user-tmpl" type="text/x-jquery-tmpl">
    <div class="online-user" data-userid="${id}">
        <div class="online-user-name">${displayName}</div>
        <div class="online-user-duration">${presenceDuration}</div>
    </div>
</script>

<div id="events-box" class="display-none">
    <div class="header-base"><%= AuditResource.LoginHistoryTitle%></div>

    <p><%= AuditResource.LoginLatestText %></p>
    
    <div class="header-base-small"><%= AuditResource.StoragePeriod %>:</div>
    <div class="audit-settings-block">
        <input id="lifetime-input" class="textEdit" type="text" pattern="[0-9.]+" maxlength="3">
        <a id="save-settings-btn" class="button blue small"><%= Resource.SaveButton %></a>
    </div>

    <p><%= AuditResource.LoginDownloadText %></p>

    <table id="events-list" class="table-list height32">
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

    <div id="events-list-dscr" class="gray-text"><%= AuditResource.TotalAuditItems %>: <span></span></div>

    <a id="download-report-btn" class="button blue middle" href="#generate"><%= AuditResource.DownloadReportBtn %></a>
    <span id="generate-text" class="display-none"><%= AuditResource.GenerateText %></span>
</div>


<div id="online-users-box" class="display-none">
    <div class="header-base"><%= AuditResource.AuditTrailOnlineUsers %></div>

    <div id="online-users-list"></div>
</div>

<div id="empty-screen" class="display-none">
    <asp:PlaceHolder ID="emptyScreenHolder" runat="server"></asp:PlaceHolder>
</div>