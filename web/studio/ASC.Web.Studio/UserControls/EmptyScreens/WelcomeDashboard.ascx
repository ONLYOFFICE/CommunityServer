<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WelcomeDashboard.ascx.cs" Inherits="ASC.Web.Studio.UserControls.EmptyScreens.WelcomeDashboard" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="System.Globalization" %>

<div id="dashboardBackdrop" class="backdrop display-none" blank-page=""></div>

<div id="dashboardContent" blank-page="" class="dashboard-center-box welcome display-none">
    <a class="close">&times;</a>
    <div class="content">
        <div class="slick-carousel">
            <% if (ProductDemo) { %>
                <div class="module-block slick-carousel-item clearFix">
                    <h1><%= UserControlsCommonResource.WelcomeDashboardHeader %></h1>
                    <div class="img creating-business-cloud"></div>
                    <div class="text">
                        <div class="title"><%= UserControlsCommonResource.WelcomeDashboardСreatingBusinessСloud %></div>
                        <p><%= UserControlsCommonResource.WelcomeDashboardСreatingBusinessСloudFirstLine %></p>
                        <p><a target="_blank" href="<%= CommonLinkUtility.GetRegionalUrl(SetupInfo.DemoOrder, CultureInfo.CurrentCulture.TwoLetterISOLanguageName) %>"><%= UserControlsCommonResource.WelcomeDashboardСreatingBusinessСloudSecondLine %></a></p>
                    </div>
                </div>
            <% } %>
            <div class="module-block slick-carousel-item clearFix">
                <h1><%= UserControlsCommonResource.WelcomeDashboardHeader %></h1>
                <div class="img tailored-cloud"></div>
                <div class="text">
                    <div class="title"><%= UserControlsCommonResource.WelcomeDashboardTailoredCloud %></div>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardTailoredCloudFirstLine, "<b>", "</b>") %> <a target="_blank" href="<%= VirtualPathUtility.ToAbsolute("~/Products/People/") %>"><%= UserControlsCommonResource.WelcomeDashboardInviteUsers %></a></p>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardTailoredCloudSecondLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.WhiteLabel) %>"><%= UserControlsCommonResource.WelcomeDashboardCustomizeNow %></a></p>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardTailoredCloudThirdLine, "<b>", "</b>") %> <a target="_blank" href="<%= VirtualPathUtility.ToAbsolute("~/Products/Files/") %>"><%= UserControlsCommonResource.WelcomeDashboardStartEditing %></a></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <h1><%= UserControlsCommonResource.WelcomeDashboardHeader %></h1>
                <div class="img ultimate-security"></div>
                <div class="text">
                    <div class="title"><%= UserControlsCommonResource.WelcomeDashboardUltimateSecurity %></div>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardUltimateSecurityFirstLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.PortalSecurity) %>"><%= UserControlsCommonResource.WelcomeDashboardСonfigureNow %></a></p>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardUltimateSecuritySecondLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.Backup) %>"><%= UserControlsCommonResource.WelcomeDashboardBackUp %></a></p>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardUltimateSecurityThirdLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.LoginHistory) %>"><%= UserControlsCommonResource.WelcomeDashboardTrackNow %></a></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <h1><%= UserControlsCommonResource.WelcomeDashboardHeader %></h1>
                <div class="img integration-with-infrastructure"></div>
                <div class="text">
                    <div class="title"><%= UserControlsCommonResource.WelcomeDashboardIntegration %></div>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardIntegrationFirstLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.LdapSettings) %>"><%= UserControlsCommonResource.WelcomeDashboardImportNow %></a></p>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardIntegrationSecondLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.SingleSignOnSettings) %>"><%= UserControlsCommonResource.WelcomeDashboardEnableNow %></a></p>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardIntegrationThirdLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.Customization) %>"><%= UserControlsCommonResource.WelcomeDashboardGoToDns %></a></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <h1><%= UserControlsCommonResource.WelcomeDashboardHeader %></h1>
                <div class="img apps-you-need"></div>
                <div class="text">
                    <div class="title"><%= UserControlsCommonResource.WelcomeDashboardApps %></div>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardAppsFirstLine, "<b>", "</b>") %> <a target="_blank" href="<%= VirtualPathUtility.ToAbsolute("~/addons/mail/") %>"><%= UserControlsCommonResource.WelcomeDashboardSetUpNow %></a></p>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardAppsSecondLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) %>"><%= UserControlsCommonResource.WelcomeDashboardConnectNow %></a></p>
                    <p><%= string.Format(UserControlsCommonResource.WelcomeDashboardAppsThirdLine, "<b>", "</b>") %> <a target="_blank" href="<%= CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) %>"><%= UserControlsCommonResource.WelcomeDashboardConnectNow %></a></p>
                </div>
            </div>
        </div>
    </div>
</div>
