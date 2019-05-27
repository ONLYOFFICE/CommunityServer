<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PricingPageSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PricingPageSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<% if (TenantExtra.EnableTarrifSettings)
   { %>
<div class="clearFix">
    <div id="studio_pricingPageSettings" class="settings-block">
    <div class="header-base clearFix">
        <div class="title"><%= Resource.PricingPageSettingsTitle %></div>
    </div>
        <div class="clearFix">
            <input id="pricingPageSettingsCbx" type="checkbox" <%= Checked ? "checked=\"checked\"" : "" %>/>
            <label for="pricingPageSettingsCbx"><%= Resource.PricingPageSettingsCbxText %></label>
        </div>
        <div class="middle-button-container">
            <a id="pricingPageSettingsBtn" class="button blue"><%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.PricingPageSettingsHelp.HtmlEncode(), "<b>", "</b>") %></p>
    </div>  
</div>
<% } %>