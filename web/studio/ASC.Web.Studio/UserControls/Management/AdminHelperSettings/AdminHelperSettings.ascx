<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdminHelperSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.AdminHelperSettings.AdminHelperSettings" %>

<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div class="admin-helper-settings" id="adminHelperSettings">
    <div class="admin-helper-image"></div>
    <div class="admin-helper-cancel" id="adminHelperCancel"></div>
    <div class="admin-helper-form">
        <div><%= Resource.AdminSettingsHelper.HtmlEncode() %>&nbsp;<a href="https://www.onlyoffice.com/blog/2020/03/use-this-checklist-to-ensure-maximum-security-of-your-enterprise-edition/" target="blank" class="link underline"><%= Resource.UseThisChecklist %></a></div>
        <a class="link underline" id="doNotShowItAgain"><%= Resource.DoNotShowItAgain%></a>
    </div>
</div>