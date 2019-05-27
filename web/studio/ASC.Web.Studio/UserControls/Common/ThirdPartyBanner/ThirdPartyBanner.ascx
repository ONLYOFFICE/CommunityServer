<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ThirdPartyBanner.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.ThirdPartyBanner.ThirdPartyBanner" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<a class="banner-<%= CurrentBanner.Item1 %> banner-large banner-link gray-text"
    onclick="ThirdPartyBanner.CloseBanner('<%= CurrentBanner.Item1 %>', function(){});return true;"
    href="<%= CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) %>">
    <div class="cancelButton" onclick="ThirdPartyBanner.CloseBanner('<%= CurrentBanner.Item1 %>', function(){});jq('.banner-<%= CurrentBanner.Item1 %>').remove();return false;">
        &times;
    </div>
    <%= string.Format(CurrentBanner.Item2.HtmlEncode(), "<b>","</b>") %>
</a>
