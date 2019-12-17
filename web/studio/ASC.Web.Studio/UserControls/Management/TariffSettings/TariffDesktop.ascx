<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffDesktop.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffDesktop" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="auth-form-page">
    <div id="GreetingBlock" class="authForm help-block-none">
        <%--header and logo--%>
        <div class="header">
            <img class="logo" src="<%= LogoPath %>" alt="<%= TenantName.HtmlEncode() %>" />
            <h1 class="header-base big blue-text"><%= TenantName.HtmlEncode() %></h1>
        </div>

        <div class="current-tariff-desc">
            <div>
                <%= String.Format(UserControlsCommonResource.TariffOverdueDesktop.HtmlEncode(),
                                     "<span class='tariff-marked'>",
                                     "</span>",
                                     "<br />") %>
            </div>

            <div class="middle-button-container">
                <a class="button huge red"
                    href="<%= TenantExtra.GetTariffPageLink() %>"
                    title="<%= Resource.TariffSettings %>"
                    target="_blank"><%= Resource.TariffSettings %></a>
            </div>
        </div>
    </div>
</div>
