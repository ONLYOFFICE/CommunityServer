<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CoBranding.ascx.cs" Inherits="ASC.Web.UserControls.CoBranding.CoBranding" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Management.CoBranding.Resources" %>

<%@ Import Namespace="ASC.Web.Core.CoBranding" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="clearFix">
    <div id="studio_coBrandingSettings" class="settings-block">
        <div class="header-base greetingTitle clearFix">
            <%=  CoBrandingResource.LogoSettings %>
        </div>
        <div class="clearFix ">
            <p><%= String.Format(ASC.Web.Studio.UserControls.Management.CoBranding.Resources.CoBrandingResource.LogoUploadRecommendation, "<b>","</b>")%></p>

            <div class="clearFix logo-setting-block">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", CoBrandingResource.LogoLight, TenantCoBrandingSettings.logoLightSize.Width, TenantCoBrandingSettings.logoLightSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase theme-background logo_<%= (int)CoBrandingLogoTypeEnum.Light %>"
                            src="<%= String.Format("~/TenantLogo.ashx?logotype={0}&general={1}", (int)CoBrandingLogoTypeEnum.Light, (!IsRetina).ToString().ToLower())%>" />
                    </div>
                    <% if (!MobileDetector.IsMobile) { %>
                    <div class="logo-change-container">
                        <input type="hidden" id="logoPath_<%= (int)CoBrandingLogoTypeEnum.Light %>" value="" />
                        <a id="logoUploader_<%= (int)CoBrandingLogoTypeEnum.Light %>" class="link dotline small">
                            <%= Resources.Resource.ChangeLogoButton %>
                        </a>
                    </div>
                    <% } %>
                </div>
            </div>

            <div class="clearFix logo-setting-block">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", CoBrandingResource.LogoLightSmall, TenantCoBrandingSettings.logoLightSmallSize.Width, TenantCoBrandingSettings.logoLightSmallSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase theme-background logo_<%= (int)CoBrandingLogoTypeEnum.LightSmall %>"
                            src="<%= String.Format("~/TenantLogo.ashx?logotype={0}&general={1}", (int)CoBrandingLogoTypeEnum.LightSmall, (!IsRetina).ToString().ToLower())%>" />
                    </div>
                    <% if (!MobileDetector.IsMobile) { %>
                    <div class="logo-change-container">
                        <input type="hidden" id="logoPath_<%= (int)CoBrandingLogoTypeEnum.LightSmall %>" value="" />
                        <a id="logoUploader_<%= (int)CoBrandingLogoTypeEnum.LightSmall %>" class="link dotline small">
                            <%= Resources.Resource.ChangeLogoButton %>
                        </a>
                    </div>
                    <% } %>
                </div>
            </div>

            <div class="clearFix logo-setting-block ">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", CoBrandingResource.LogoDark, TenantCoBrandingSettings.logoDarkSize.Width, TenantCoBrandingSettings.logoDarkSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase logo_<%= (int)CoBrandingLogoTypeEnum.Dark %>"
                            src="<%= String.Format("~/TenantLogo.ashx?logotype={0}&general={1}", (int)CoBrandingLogoTypeEnum.Dark, (!IsRetina).ToString().ToLower())%>" />
                    </div>
                    <% if (!MobileDetector.IsMobile) { %>
                    <div class="logo-change-container">
                        <input type="hidden" id="logoPath_<%= (int)CoBrandingLogoTypeEnum.Dark %>" value="" />
                        <a id="logoUploader_<%= (int)CoBrandingLogoTypeEnum.Dark %>" class="link dotline small">
                            <%= Resources.Resource.ChangeLogoButton %>
                        </a>
                    </div>
                    <% } %>
                </div>
            </div>

            <div class="clearFix logo-setting-block">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", CoBrandingResource.LogoFavicon, TenantCoBrandingSettings.logoFaviconSize.Width, TenantCoBrandingSettings.logoFaviconSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase logo_<%= (int)CoBrandingLogoTypeEnum.Favicon %>"
                            src="<%= String.Format("~/TenantLogo.ashx?logotype={0}&general={1}", (int)CoBrandingLogoTypeEnum.Favicon, (!IsRetina).ToString().ToLower())%>" />
                    </div>
                    <% if (!MobileDetector.IsMobile) { %>
                    <div class="logo-change-container">
                        <input type="hidden" id="logoPath_<%= (int)CoBrandingLogoTypeEnum.Favicon %>" value="" />
                        <a id="logoUploader_<%= (int)CoBrandingLogoTypeEnum.Favicon %>" class="link dotline small">
                            <%= Resources.Resource.ChangeLogoButton %>
                        </a>
                    </div>
                    <% } %>
                </div>
            </div>

<%--            <div class="clearFix logo-setting-block">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", CoBrandingResource.LogoDocsEditor, TenantCoBrandingSettings.logoDocsEditorSize.Width, TenantCoBrandingSettings.logoDocsEditorSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase docs-style1-background logo_<%= (int)CoBrandingLogoTypeEnum.DocsEditor %>"
                            src="<%= String.Format("~/TenantLogo.ashx?logotype={0}&general={1}", (int)CoBrandingLogoTypeEnum.DocsEditor, (!IsRetina).ToString().ToLower())%>" />
                        <img class="borderBase docs-style2-background logo_<%= (int)CoBrandingLogoTypeEnum.DocsEditor %>"
                            src="<%= String.Format("~/TenantLogo.ashx?logotype={0}&general={1}", (int)CoBrandingLogoTypeEnum.DocsEditor, (!IsRetina).ToString().ToLower())%>" />
                        <img class="borderBase docs-style3-background logo_<%= (int)CoBrandingLogoTypeEnum.DocsEditor %>"
                            src="<%= String.Format("~/TenantLogo.ashx?logotype={0}&general={1}", (int)CoBrandingLogoTypeEnum.DocsEditor, (!IsRetina).ToString().ToLower())%>" />
                    </div>
                    <% if (!MobileDetector.IsMobile) { %>
                    <div class="logo-change-container">
                        <input type="hidden" id="logoPath_<%= (int)CoBrandingLogoTypeEnum.DocsEditor %>" value="" />
                        <a id="logoUploader_<%= (int)CoBrandingLogoTypeEnum.DocsEditor %>" class="link dotline small">
                            <%= Resources.Resource.ChangeLogoButton %>
                        </a>
                    </div>
                    <% } %>
                </div>
            </div>--%>

            <div class="middle-button-container">
                <a id="saveCoBrandingSettingsBtn" class="button blue"><%= Resource.SaveButton %></a>
                <span class="splitter-buttons"></span>
                <a id="restoreCoBrandingSettingsBtn" class="button gray"><%= Resource.RestoreDefaultButton %></a>
            </div>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= ASC.Web.Studio.UserControls.Management.CoBranding.Resources.CoBrandingResource.HelpAnswerCoBrandingSettings %></p>
    </div>
</div>