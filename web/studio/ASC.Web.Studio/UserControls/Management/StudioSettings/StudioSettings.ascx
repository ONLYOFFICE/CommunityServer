<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StudioSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.StudioSettings" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<%--timezone & language--%>
<div class="clearFix">
    <div id="studio_lngTimeSettings" class="settings-block">
        <div class="header-base clearFix">
            <%= Resource.StudioTimeLanguageSettings %>
        </div>
           <asp:PlaceHolder ID="_timelngHolder" runat="server"></asp:PlaceHolder>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerLngTimeSettings, "<br />", "<b>", "</b>") %></p>
        <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
           { %>
        <a href="<%= CommonLinkUtility.GetHelpLink(true) + "gettingstarted/configuration.aspx#ChangingGeneralSettings_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>

<%--greeting settings--%>
<asp:PlaceHolder ID="_greetingSettings" runat="server"></asp:PlaceHolder>

<%--DNS settings--%>
<div class="clearFix <%= EnableDomain ? "" : "disable" %>">
    <div id="studio_enterDns" class="settings-block">
        <asp:PlaceHolder ID="_dnsSettingsHolder" runat="server">
            <div class="header-base clearFix">
                <%= Resource.DnsSettings %>
            </div>

                <div class="clearFix">
                    <div class="clearFix dns-settings-title">
                        <input type="checkbox" id="studio_enableDnsName"
                            onclick="jq('#studio_dnsName').attr('disabled', !this.checked);"
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
                    <a id="studio_dnsNameBtn" class="button blue" onclick="<%= EnableDomain ? "StudioSettings.SaveDnsSettings(this);" : "" %>">
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
        <a href="<%= TenantExtra.GetTariffPageLink() %>" target="_blank">
            <%= Resource.ViewTariffPlans %></a>
        <% }
           else
           { %>
        <p><%= String.Format(Resource.HelpAnswerDNSSettings, "<br />", "<b>", "</b>") %></p>
        <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
           { %>
        <a href="<%= CommonLinkUtility.GetHelpLink(true) + "gettingstarted/configuration.aspx#CustomizingPortal_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
        <% } %>
    </div>
</div>

<%--version settings--%>
<asp:PlaceHolder ID="_portalVersionSettings" runat="server"></asp:PlaceHolder>

<%-- Promo code --%>
<asp:PlaceHolder ID="promoCodeSettings" runat="server" />