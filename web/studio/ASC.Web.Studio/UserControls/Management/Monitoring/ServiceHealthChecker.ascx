<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ServiceHealthChecker.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ServiceHealthChecker" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div id="serviceStatusContainer" class="settings-block">
        <div class="header-base"><%= MonitoringResource.ServicesTitle %></div>
        <p class="monitoring-describe"><%: MonitoringResource.ServicesText %></p>
        <table id="serviceStatusTable" class="table-list height32 monitoring-table">
            <% foreach (string serviceName in GetServiceNames()) %>
            <% { %>
                   <tr id="tr_<%= GetServiceId(serviceName) %>" class="__open">
                       <td class="monitoring-table_title"><%= serviceName %></td>
                       <td class="monitoring-table_status"><%= GetServiceStatusDescription(ServiceStatus.Running) %></td>
                       <td class="monitoring-table_btn"><button type="button" class="button gray btn-icon __update disable"></button></td>
                   </tr>
            <% } %>
        </table>
    </div>
    <div class="settings-help-block">
        <p><%: MonitoringResource.ServicesHelpText %></p>
         <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx" %>" target="_blank"> <%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>

<div class="clearFix">
    <div class="settings-block">
        <div class="header-base"><%= MonitoringResource.CacheTitle %></div>
        <p class="monitoring-describe"><%= MonitoringResource.CacheText %></p>
        <div class="middle-button-container">
            <a id="clearCacheBtn" class="button blue middle"><%= MonitoringResource.CacheClearBtnText %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%: MonitoringResource.CacheHelpText %></p>
    </div>
</div>