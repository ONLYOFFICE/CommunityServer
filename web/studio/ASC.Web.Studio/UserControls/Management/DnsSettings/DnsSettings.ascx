<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DnsSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.DnsSettings.DnsSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix <%= EnableDomain ? "" : "disable" %>">
    <div id="studio_enterDns" class="settings-block">
        <asp:PlaceHolder ID="_dnsSettingsHolder" runat="server">
            <div class="header-base clearFix">
                <%= Resource.DnsSettings %>
            </div>

            <div class="clearFix">
                <div class="clearFix dns-settings-title">
                    <input type="checkbox" id="studio_enableDnsName"
                        onclick="DnsSettings.CheckEnableDns(this.checked);"
                        <%= EnableDnsChange ? "checked='checked'" : "" %>
                        <%= EnableDomain ? "" : "disabled='disabled'" %> />
                    <label for="studio_enableDnsName" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                        <%= Resource.CustomDomainName %>
                    </label>
                </div>
                <div class="clearFix">
                    <input type="text" id="studio_dnsName" class="textEdit" maxlength="150"
                        value="<%= ASC.Core.CoreContext.TenantManager.GetCurrentTenant().MappedDomain ?? string.Empty %>"
                        <%= (EnableDnsChange && EnableDomain) ? "" : "disabled='disabled'" %> />
                </div>
            </div>
            <div class="middle-button-container">
                <a id="studio_dnsNameBtn" class="button blue" onclick="<%= EnableDomain ? "DnsSettings.SaveDnsSettings(this);" : "" %>">
                    <%= Resource.SaveButton %></a>
            </div>

        </asp:PlaceHolder>
    </div>
    <div class="settings-help-block">
        <% if (!EnableDomain)
           { %>
        <p>
            <%= Resource.ErrorNotAllowedOption %>
        </p>
        <% if (TenantExtra.EnableTarrifSettings)
           { %>
        <a href="<%= TenantExtra.GetTariffPageLink() %>" target="_blank">
            <%= Resource.ViewTariffPlans %></a>
        <% } %>
        <% }
           else
           { %>
        <p><%= String.Format(Resource.HelpAnswerDNSSettings.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#CustomizingPortal_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
        <% } %>
    </div>
</div>
