<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Desktop.ascx.cs" Inherits="ASC.Web.Files.Controls.Desktop" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Web.Files.Core.Entries" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% if (!string.IsNullOrEmpty(Request["first"]))
   { %>
<div id="desktopWelcome" class="popup-modal">
    <sc:Container ID="desktopWelcomeDialog" runat="server">
        <Header><%= FilesUCResource.DesktopWelcomeHeader %></Header>
        <Body>
            <div class="desktop-welcome-head"><%= FilesUCResource.DesktopWelcome %></div>
            <div class="desktop-welcome-descr gray-text"><%= string.Format(FilesUCResource.DesktopDescr.HtmlEncode(), "<br />", "<b>", "</b>") %></div>
            <div class="middle-button-container">
                <a href="<%= CommonLinkUtility.GetFullAbsolutePath(AuthLink) %>" class="button blue big" target="_blank"
                    onclick="PopupKeyUpActionProvider.CloseDialog(); return true;"><%= FilesUCResource.DesktopOpenBrowser %></a>
            </div>
            
            <br/>
            <span class="text-medium-describe"><%= string.Format(FilesUCResource.DesktopSupport,
                                               string.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>",
                                               CommonLinkUtility.GetRegionalUrl(Setting.FeedbackAndSupportUrl, CultureInfo.CurrentCulture.TwoLetterISOLanguageName))) %></span>
            <br />
            <span class="text-medium-describe"><%= string.Format(FilesUCResource.DesktopSales,
                                        string.Format("<a href=\"mailto:{0}\" >{0}</a>", Setting.SalesEmail)) %></span>
        </Body>
    </sc:Container>
</div>
<% } %>

<% if (PrivacyRoomSettings.Available
        && PrivacyRoomSettings.Enabled)
   { 
    var keyPair = EncryptionKeyPair.GetKeyPair(); %>
<input type="hidden" id="encryptionKeyPair" value="<%: keyPair != null ? JsonConvert.SerializeObject(keyPair) : "" %>" />
<% } %>
