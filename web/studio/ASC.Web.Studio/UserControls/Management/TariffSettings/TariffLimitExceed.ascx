<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffLimitExceed.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffLimitExceed" %>
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
                <% if (IsFreeTariff) { %>
                <span class="splitter-buttons"></span>
                <a class="link underline" href="http://helpcenter.onlyoffice.com/gettingstarted/configuration.aspx#PublicPortals" target="_blank">
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

<div id="tariffLimitDocsEditionPanel" style="display: none">
    <sc:Container runat="server" ID="tariffLimitDocsEditionDialog">
        <Header><%: UserControlsCommonResource.TariffDocsEditionLimitTitle %></Header>
        <Body>
            <% if (IsDefaultTariff)
               { %>
            <div class="tariff-limit-docsedition">
                <span class="header-base-medium"><%: UserControlsCommonResource.TariffLicenseOver %></span>
                <br />
                <br />
                <%: UserControlsCommonResource.TariffDocsEditionLimitReasonLicense %>
            </div>
            <% }
               else
               { %>
            <div class="tariff-limit-docsedition">
                <span class="header-base-medium"><%: UserControlsCommonResource.TariffDocsEditionLimitHeader %></span>
                <br />
                <br />
                <%: UserControlsCommonResource.TariffDocsEditionLimitReason %>
                <br />
                <br />
                <%: UserControlsCommonResource.TariffLimitDecision %>
            </div>
            <% } %>

            <div class="middle-button-container">
                <a class="blue button medium" href="<%= TenantExtra.GetTariffPageLink() %>">
                    <%= IsDefaultTariff ? UserControlsCommonResource.TariffLimitOkButtonLicense : UserControlsCommonResource.TariffLimitOkButton %></a>
                <span class="splitter-buttons"></span>
                <a class="gray button medium" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<% if (HostedPartner != null && string.IsNullOrEmpty(HostedPartner.AuthorizedKey))
   { %>
<div id="UnauthorizedPartnerPanel" style="display: none">
    <sc:Container runat="server" ID="UnauthorizedPartnerDialog">
        <Header><%: UserControlsCommonResource.HostedUnauthorizedTitle %></Header>
        <Body>
            <div class="hosted-unauthorized">
                <span class="header-base-medium"><%: UserControlsCommonResource.HostedUnauthorizedEditing %></span>
                <br />
                <br />
                <%= string.Format(UserControlsCommonResource.HostedUnauthorizedEditingReason.HtmlEncode(),
                (HostedPartner == null || string.IsNullOrEmpty(HostedPartner.SupportEmail) ? "" : "<a href=\"mailto:" + HostedPartner.SupportEmail + "\">"),
                (HostedPartner == null || string.IsNullOrEmpty(HostedPartner.SupportEmail) ? "" : "</a>")) %>
                <br />
                <br />
            </div>

            <div class="middle-button-container">
                <a class="blue button medium" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.OKButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>
<% } %>

<% } %>