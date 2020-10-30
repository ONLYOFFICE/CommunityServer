<%@ Page MasterPageFile="~/Masters/BaseTemplate.master" Language="C#" AutoEventWireup="true" CodeBehind="PaymentRequired.aspx.cs" Inherits="ASC.Web.Studio.PaymentRequired" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="Resources" %>

<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div class="tariff-page">
        <div class="current-tariff-desc">
            <b><%= UserControlsCommonResource.TariffOverdueHosted %></b>
        </div>
        <div class="license-section">
            <span class="header-base"><%= UserControlsCommonResource.HostedPayAndGet.HtmlEncode() %></span>
        </div>
        <table class="license-list" cellpadding="0" cellspacing="0">
            <tbody>
                <tr>
                    <td rowspan="5">
                        <div class="license-item">
                            <div class="license-item-modules"><%= UserControlsCommonResource.LicenseModules.HtmlEncode() %></div>
                            <%= string.Format(CoreContext.Configuration.CustomMode ? CustomModeResource.LicenseModulesListCustomMode : UserControlsCommonResource.LicenseModulesList, "- ", "<br />") %>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>

        <% if (!string.IsNullOrEmpty(Settings.BuyUrl))
           { %>
        <div class="license-section">
            <span class="header-base"><%= UserControlsCommonResource.LicenseKeyBuyLabelV11 %></span>
        </div>
        <div><%= UserControlsCommonResource.HostedBuyDescr.HtmlEncode() %></div>

        <div class="button-margin">
            <a href="<%= Settings.BuyUrl %>" class="button blue big" target="_blank"><%= Resource.TariffButtonBuy %></a>
        </div>
        <% } %>

        <% if (!string.IsNullOrEmpty(Settings.SalesEmail))
           { %>
        <div class="license-questions">
            <%= string.Format(UserControlsCommonResource.SalesQuestionsV11,
                              string.Format("<a href=\"mailto:{0}\" class=\"link-black-12\">{0}</a>", Settings.SalesEmail)) %>
        </div>
        <% } %>
    </div>
</asp:Content>
