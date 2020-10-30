<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AppBanner.ascx.cs" Inherits="ASC.Web.Files.Controls.AppBanner" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<div class="mobile-app-tree clearFix">
    <div class="header-base medium gray-text"><%= FilesUCResource.MobileApp %></div>
    <a href="https://www.onlyoffice.com/download-desktop.aspx#windows"
        target="_blank" class="mobile-app-banner mobile-app-win" title="<%: FilesUCResource.MobileWin %>"></a>
    <a href="https://www.onlyoffice.com/download-desktop.aspx#mac"
        target="_blank" class="mobile-app-banner mobile-app-mac" title="<%: FilesUCResource.MobileMac %>"></a>
    <a href="https://www.onlyoffice.com/download-desktop.aspx#linux"
        target="_blank" class="mobile-app-banner mobile-app-linux" title="<%: FilesUCResource.MobileLinux %>"></a>
    <a href="<%= !MobileDetector.IsMobile ? "https://www.onlyoffice.com/office-for-android.aspx" : "https://play.google.com/store/apps/details?id=com.onlyoffice.documents" %>"
        target="_blank" class="mobile-app-banner mobile-app-android" title="<%: FilesUCResource.MobileAndroid %>"></a>
    <a href="<%= !MobileDetector.IsMobile ? "https://www.onlyoffice.com/office-for-ios.aspx" : "https://itunes.apple.com/app/onlyoffice-documents/id944896972" %>"
        target="_blank" class="mobile-app-banner mobile-app-ios" title="<%: FilesUCResource.MobileIos %>"></a>
</div>
