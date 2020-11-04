<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PortsChecker.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PortsChecker" %>
<%@ Import Namespace="Resources" %>

<script id="portStatusTmpl" type="text/x-jquery-tmpl">
    {{if status == 0 }}
    <tr class="__open">
    {{else}}
    <tr class="__close">
    {{/if}}
        <td class="monitoring-table_title">${name} ${number}</td>
        <td class="monitoring-table_status"><span>${statusDescription}</span></td>
    </tr>
</script>

<div class="clearFix">
    <div class="settings-block">
        <div class="header-base"><%= MonitoringResource.PortsTitle %></div>
        <p class="monitoring-describe"><%: MonitoringResource.PortsText %></p>
        <table id="portStatusTable" class="table-list height32 monitoring-table">
            <% foreach (var port in Ports) %>
            <% { %>
                <tr class="__pending">
                    <td class="monitoring-table_title"><%= port.Name %> <%= port.Number %></td>
                    <td class="monitoring-table_status"><span><%= GetStatusDescription(PortStatus.Open) %></span></td>
                </tr>
            <% } %>
        </table>
    </div>
    <div class="settings-help-block">
        <p><%: MonitoringResource.PortsHelpText %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "gettingstarted/configuration.aspx" %>" target="_blank"> <%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>