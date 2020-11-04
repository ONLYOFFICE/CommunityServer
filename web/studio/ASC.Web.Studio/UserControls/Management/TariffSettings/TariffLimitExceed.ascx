<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffLimitExceed.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffLimitExceed" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% if (TenantExtra.EnableTarrifSettings)
   { %>

<div id="tariffLimitExceedUsersPanel" style="display: none">
    <sc:Container runat="server" ID="tariffLimitExceedUsersDialog">
        <Header><%= UserControlsCommonResource.TariffUserLimitTitle %></Header>
        <Body>
            <div class="tariff-limitexceed-users">
                <span class="header-base-medium"><%= UserControlsCommonResource.TariffUserLimitHeader%></span>
                <br />
                <br />
                <%= UserControlsCommonResource.TariffUserLimitReason%>
                <br />
                <br />
                <%= UserControlsCommonResource.TariffLimitDecision%>
            </div>

            <div class="middle-button-container">
                <a class="blue button medium" href="<%= TenantExtra.GetTariffPageLink() %>">
                    <%= UserControlsCommonResource.TariffLimitOkButton%></a>
                <span class="splitter-buttons"></span>
                <a class="gray button medium" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.CancelButton %></a>
                <% if (IsFreeTariff && !string.IsNullOrEmpty(HelpLink)) { %>
                <span class="splitter-buttons"></span>
                <a class="link underline" href="<%= HelpLink + "/gettingstarted/configuration.aspx#PublicPortals" %>" target="_blank">
                    <%= UserControlsCommonResource.ReadAboutNonProfitUsage %>
                </a>
                <% } %>
            </div>
        </Body>
    </sc:Container>
</div>

<div id="tariffLimitExceedStoragePanel" style="display: none">
    <sc:Container runat="server" ID="tariffLimitExceedStorageDialog">
        <Header><%= UserControlsCommonResource.TariffStorageLimitTitle%></Header>
        <Body>
            <div class="tariff-limitexceed-storage">
                <span class="header-base-medium"><%= UserControlsCommonResource.TariffStorageLimitHeader%></span>
                <br />
                <br />
                <%= UserControlsCommonResource.TariffStorageLimitReason%>
                <br />
                <br />
                <%= UserControlsCommonResource.TariffLimitDecision%>
            </div>

            <div class="middle-button-container">
                <a class="blue button medium" href="<%= TenantExtra.GetTariffPageLink() %>">
                    <%= UserControlsCommonResource.TariffLimitOkButton%></a>
                <span class="splitter-buttons"></span>
                <a class="gray button medium" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<% if (IsFreeTariff)
   { %>
<div id="tariffLimitExceedFileSizePanel" style="display: none">
    <sc:Container runat="server" ID="tariffLimitExceedFileSizeDialog">
        <Header><%= UserControlsCommonResource.TariffFileSizeLimitTitle %></Header>
        <Body>
            <div class="tariff-limitexceed-storage">
                <span class="header-base-medium"><%= FileSizeComment.FileSizeExceptionString %></span>
                <br />
                <br />
                <%= UserControlsCommonResource.TariffFileSizeLimitReason %>
                <br />
                <br />
                <%= UserControlsCommonResource.TariffLimitDecision%>
            </div>

            <div class="middle-button-container">
                <a class="blue button medium" href="<%= TenantExtra.GetTariffPageLink() %>">
                    <%= UserControlsCommonResource.TariffLimitOkButton%></a>
                <span class="splitter-buttons"></span>
                <a class="gray button medium" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>
<% } %>

<% } %>

<% if (ASC.Core.CoreContext.Configuration.Personal)
   { %>
<div id="personalLimitExceedStoragePanel" style="display: none">
    <sc:Container runat="server" ID="personalLimitExceedStorageDialog">
        <Header><%= UserControlsCommonResource.PersonalStorageLimitExceededHeader%></Header>
        <Body>
            <div class="tariff-limitexceed-storage">
                <span class="header-base-medium"><%= UserControlsCommonResource.PersonalStorageLimitExceededSubHeader%></span>
                <br />
                <br />
                <%= UserControlsCommonResource.PersonalStorageLimitExceededBody %>
            </div>
            <div class="middle-button-container">
                <% if (!CoreContext.Configuration.CustomMode) { %>
                <a class="blue button medium" target="_blank" href="<%= MailWhiteLabelSettings.SupportUrl %>"><%= UserControlsCommonResource.ContactSupportBtn%></a>
                <span class="splitter-buttons"></span>
                <% } %>
                <a class="gray button medium" onclick="PopupKeyUpActionProvider.CloseDialog();"><%= CoreContext.Configuration.CustomMode ? Resource.OKButton : Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>
<% } %>