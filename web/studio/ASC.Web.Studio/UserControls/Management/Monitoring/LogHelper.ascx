<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LogHelper.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.LogHelper" %>
<%@ Import Namespace="Resources" %>

<div class="monitoring-container clearFix">
    <div class="settings-block">
        <div class="header-base"><%= MonitoringResource.LogsTitle %></div>
        <p class="monitoring-describe"><%= MonitoringResource.LogsText %></p>
        <div class=""><%= MonitoringResource.LogsDateInterval %></div>
        <div class="interval-date">
            <input type="text" class="textEditCalendar start-date" autocomplete="off"/>
            <span class="separate-date"></span>
            <input type="text" class="textEditCalendar end-date" autocomplete="off"/>
        </div>
        <div class="middle-button-container">
            <a id="downloadLogsBtn" class="button blue middle"><%= Resource.DownloadButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= MonitoringResource.LogsHelpText.HtmlEncode() %></p>
    </div>
</div>

<div class="clearFix">
    <div class="settings-block">
        <div class="header-base"><%= MonitoringResource.ContactUsTitle %></div>
        <p class="monitoring-describe">
             <% if (!string.IsNullOrEmpty(HelpLink))
                { %>
            <%= string.Format(MonitoringResource.ContactUsText.HtmlEncode(), "<a href=\"" + HelpLink + "/faq/faq.aspx\" class=\"gray link underline\">", "</a>", "<a href=\"mailto:support@onlyoffice.com\")  class=\"gray link underline\">", "</a>") %>
            <% }
                else
                { %>
            <%= string.Format(MonitoringResource.ContactUsText2.HtmlEncode(), "<a href=\"mailto:support@onlyoffice.com\") class=\"gray link underline\">", "</a>") %>
            <% } %>
        </p>

    </div>
</div>
