<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DnsSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.DnsSettings.DnsSettings" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div class="clearFix <%= AvailableDnsSettings ? "" : "disable" %>">
    <div id="studio_enterDns" class="settings-block">
        <div class="header-base clearFix">
            <%= Resource.DnsSettings %>
        </div>

        <% if (CoreContext.Configuration.Standalone) { %>

            <div class="clearFix">
                <div class="clearFix dns-settings-title">
                    <input type="checkbox" id="studio_enableDnsName"
                        onclick="DnsSettings.CheckEnableDns(this.checked);"
                        <%= HasMappedDomain ? "checked='checked'" : "" %>
                        <%= AvailableDnsSettings ? "" : "disabled='disabled'" %> />
                    <label for="studio_enableDnsName" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                        <%= Resource.CustomDomainName %>
                    </label>
                </div>
                <div class="clearFix">
                    <input type="text" id="studio_dnsName" class="textEdit" maxlength="150"
                        value="<%= MappedDomain ?? string.Empty %>"
                        <%= (HasMappedDomain && AvailableDnsSettings) ? "" : "disabled='disabled'" %> />
                </div>
            </div>
            <div class="middle-button-container">
                <a id="studio_dnsNameBtn" class="button blue <%= AvailableDnsSettings ? "" : "disable" %>" onclick="<%= AvailableDnsSettings ? "DnsSettings.SaveDnsSettings(this);" : "" %>">
                    <%= Resource.SaveButton %></a>
            </div>

        <% } else { %>

            <div class="clearFix">
                <div class="header-base-small"><%= Resource.DNSSettingsCurrentDomain %>:</div>
                <div><%= CurrentDomain %></div>
                <% if (HasMappedDomain) { %>
                <br />
                <div class="header-base-small"><%= Resource.DNSSettingsCustomDomain %>:</div>
                <div><%= MappedDomain %></div>
                <% } %>
            </div>
            <div class="middle-button-container">
                <% if (AvailableDnsSettings) { %>
                <a class="button blue" href="<%= SupportLink %>" target="_blank"><%= Resource.DNSSettingsSendRequest %></a>
                <% } else { %>
                <a class="button blue disable"><%= Resource.DNSSettingsSendRequest %></a>
                <% } %>
            </div>

        <% } %>
    </div>
    <div class="settings-help-block">
        <% if (!AvailableDnsSettings)
           { %>
        <p>
            <%= Resource.ErrorNotAllowedOption %>
        </p>
        <% if (TenantExtra.EnableTariffSettings)
           { %>
        <a href="<%= TenantExtra.GetTariffPageLink() %>" target="_blank">
            <%= Resource.ViewTariffPlans %></a>
        <% } %>
        <% }
           else
           { %>
        <p><%= CoreContext.Configuration.Standalone ? String.Format(Resource.HelpAnswerDNSSettings.HtmlEncode(), "<br />", "<b>", "</b>") : Resource.HelpAnswerDNSSettingsSaas.HtmlEncode() %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#CustomizingPortal_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
        <% } %>
    </div>
</div>
