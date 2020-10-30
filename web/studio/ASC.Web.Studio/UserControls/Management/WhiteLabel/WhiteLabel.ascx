<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WhiteLabel.ascx.cs" Inherits="ASC.Web.UserControls.WhiteLabel.WhiteLabel" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Core.WhiteLabel" %>

<div class="clearFix <%= WhiteLabelEnabledForPaid ? "" : "disable" %>">
    <div id="studio_whiteLabelSettings" class="settings-block">
        <div class="header-base greetingTitle clearFix">
            <%=  WhiteLabelResource.LogoSettings %>
        </div>
        <div class="clearFix ">
            <p><%= String.Format(WhiteLabelResource.LogoUploadRecommendation, "<b>","</b>")%></p>

            <div class="clearFix logo-setting-block">
                <div class="clearFix">
                    <div class="header-base-small whiteLabelLogoText">
                        <%=WhiteLabelResource.CompanyNameForCanvasLogo%>:
                    </div>
                    <div>
                        <input type="text" class="textEdit" maxlength="30" id="studio_whiteLabelLogoText" data-value="<%= LogoText.HtmlEncode() %>" value="<%= LogoText.HtmlEncode() %>"  <%= WhiteLabelEnabledForPaid ? "" : "disabled=\"disabled\"" %>/>
                    </div>
                </div>
                <% if (WhiteLabelEnabledForPaid) { %>
                <div class="clearFix">
                    <a id="useAsLogoBtn" class="button gray" href="javascript:void(0);"><%= WhiteLabelResource.UseAsLogoButton %></a>
                </div>
                <% } %>
            </div>

            <div class="clearFix logo-setting-block">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", WhiteLabelResource.LogoLightSmall, TenantWhiteLabelSettings.logoLightSmallSize.Width, TenantWhiteLabelSettings.logoLightSmallSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase theme-background logo_<%= (int)WhiteLabelLogoTypeEnum.LightSmall %>"
                            src="<%= String.Format("/TenantLogo.ashx?logotype={0}&general={1}", (int)WhiteLabelLogoTypeEnum.LightSmall, (!IsRetina).ToString().ToLower())%>" />
                        <canvas id="canvas_logo_<%= (int)WhiteLabelLogoTypeEnum.LightSmall %>" class="theme-background borderBase" width="284" height="46"
                            data-fontsize="36"
                            data-fontcolor="#fff">
                            <%= WhiteLabelResource.BrowserNoCanvasSupport %>
                        </canvas>
                    </div>
                    <% if (WhiteLabelEnabledForPaid) { %>
                        <input type="hidden" id="logoPath_<%= (int)WhiteLabelLogoTypeEnum.LightSmall %>" value="" />
                        <div class="logo-change-container">
                            <a id="logoUploader_<%= (int)WhiteLabelLogoTypeEnum.LightSmall %>" class="link dotline small">
                                <%= Resource.ChangeLogoButton %>
                            </a>
                        </div>
                     <% } %>
                </div>
            </div>

            <div class="clearFix logo-setting-block ">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", WhiteLabelResource.LogoDark, TenantWhiteLabelSettings.logoDarkSize.Width, TenantWhiteLabelSettings.logoDarkSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase logo_<%= (int)WhiteLabelLogoTypeEnum.Dark %>"
                            src="<%= String.Format("/TenantLogo.ashx?logotype={0}&general={1}", (int)WhiteLabelLogoTypeEnum.Dark, (!IsRetina).ToString().ToLower())%>" />
                        <canvas id="canvas_logo_<%= (int)WhiteLabelLogoTypeEnum.Dark %>" class="borderBase" width="432" height="70"
                            data-fontsize="54"
                            data-fontcolor="#333">
                            <%= WhiteLabelResource.BrowserNoCanvasSupport %>
                        </canvas>
                    </div>
                    <% if (WhiteLabelEnabledForPaid) { %>
                        <input type="hidden" id="logoPath_<%= (int)WhiteLabelLogoTypeEnum.Dark %>" value="" />
                        <div class="logo-change-container">
                            <a id="logoUploader_<%= (int)WhiteLabelLogoTypeEnum.Dark %>" class="link dotline small">
                                <%= Resource.ChangeLogoButton %>
                            </a>
                        </div>
                     <% } %>
                </div>
            </div>

            <div class="clearFix logo-setting-block">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", WhiteLabelResource.LogoFavicon, TenantWhiteLabelSettings.logoFaviconSize.Width, TenantWhiteLabelSettings.logoFaviconSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase logo_<%= (int)WhiteLabelLogoTypeEnum.Favicon %>"
                            src="<%= String.Format("/TenantLogo.ashx?logotype={0}&general={1}", (int)WhiteLabelLogoTypeEnum.Favicon, (!IsRetina).ToString().ToLower())%>" />
                        <canvas id="canvas_logo_<%= (int)WhiteLabelLogoTypeEnum.Favicon %>" class="borderBase" width="32" height="32"
                            data-fontsize="28"
                            data-fontcolor="#333">
                            <%= WhiteLabelResource.BrowserNoCanvasSupport %>
                        </canvas>
                    </div>
                    <% if (WhiteLabelEnabledForPaid) { %>
                        <input type="hidden" id="logoPath_<%= (int)WhiteLabelLogoTypeEnum.Favicon %>" value="" />
                        <div class="logo-change-container">
                            <a id="logoUploader_<%= (int)WhiteLabelLogoTypeEnum.Favicon %>" class="link dotline small">
                                <%= Resource.ChangeLogoButton %>
                            </a>
                        </div>
                     <% } %>
                </div>
            </div>

            <div class="clearFix logo-setting-block">
                <div class="header-base-small logo-header">
                    <%= String.Format("{0} ({1}x{2}):", WhiteLabelResource.LogoDocsEditor, TenantWhiteLabelSettings.logoDocsEditorSize.Width, TenantWhiteLabelSettings.logoDocsEditorSize.Height) %>
                </div>
                <div>
                    <div class="logo-img-container">
                        <img class="borderBase docs-style1-background logo_<%= (int)WhiteLabelLogoTypeEnum.DocsEditor %>"
                            src="<%= String.Format("/TenantLogo.ashx?logotype={0}&general={1}", (int)WhiteLabelLogoTypeEnum.DocsEditor, (!IsRetina).ToString().ToLower())%>" />
                        <img class="borderBase docs-style2-background logo_<%= (int)WhiteLabelLogoTypeEnum.DocsEditor %>"
                            src="<%= String.Format("/TenantLogo.ashx?logotype={0}&general={1}", (int)WhiteLabelLogoTypeEnum.DocsEditor, (!IsRetina).ToString().ToLower())%>" />
                        <img class="borderBase docs-style3-background logo_<%= (int)WhiteLabelLogoTypeEnum.DocsEditor %>"
                            src="<%= String.Format("/TenantLogo.ashx?logotype={0}&general={1}", (int)WhiteLabelLogoTypeEnum.DocsEditor, (!IsRetina).ToString().ToLower())%>" />
                    
                    
                        <canvas id="canvas_logo_<%= (int)WhiteLabelLogoTypeEnum.DocsEditor %>_1" class="borderBase" width="172" height="40"
                            data-fontsize="22"
                            data-fontcolor="#fff">
                            <%= WhiteLabelResource.BrowserNoCanvasSupport %>
                        </canvas>
                        <canvas id="canvas_logo_<%= (int)WhiteLabelLogoTypeEnum.DocsEditor %>_2" class="borderBase" width="172" height="40"
                            data-fontsize="22"
                            data-fontcolor="#fff">
                            <%= WhiteLabelResource.BrowserNoCanvasSupport %>
                        </canvas>
                        <canvas id="canvas_logo_<%= (int)WhiteLabelLogoTypeEnum.DocsEditor %>_3" class="borderBase" width="172" height="40"
                            data-fontsize="22"
                            data-fontcolor="#fff">
                            <%= WhiteLabelResource.BrowserNoCanvasSupport %>
                        </canvas>
                    </div>
                    <% if (WhiteLabelEnabledForPaid) { %>
                        <input type="hidden" id="logoPath_<%= (int)WhiteLabelLogoTypeEnum.DocsEditor %>" value="" />
                        <div class="logo-change-container">
                            <a id="logoUploader_<%= (int)WhiteLabelLogoTypeEnum.DocsEditor %>" class="link dotline small">
                                <%= Resource.ChangeLogoButton %>
                            </a>
                        </div>
                     <% } %>
                </div>
            </div>

            <div class="middle-button-container">
                <a id="saveWhiteLabelSettingsBtn" class="button blue <%= WhiteLabelEnabledForPaid ? "" : "disable" %>"><%= Resource.SaveButton %></a>
                <span class="splitter-buttons"></span>
                <a id="restoreWhiteLabelSettingsBtn" class="button gray <%= WhiteLabelEnabledForPaid ? "" : "disable" %>"><%= Resource.RestoreDefaultButton %></a>
            </div>
        </div>
    </div>
    <div class="settings-help-block">
        <% if (WhiteLabelEnabledForPaid) { %>
        <p><%= WhiteLabelResource.HelpAnswerWhiteLabelSettings %></p>
        <% } else { %>
            <p><%= String.Format(WhiteLabelResource.HelpAnswerWhiteLabelSettingsForPaid.HtmlEncode(), "<b>", "</b>") %></p>
        <% } %>
    </div>
</div>