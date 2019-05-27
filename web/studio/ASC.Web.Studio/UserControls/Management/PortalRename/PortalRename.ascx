<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PortalRename.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PortalRename" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<% if (Enabled)
   { %>
<div id="studio_portalRename" class="clearFix">
    <div class="settings-block">
        <div class="header-base clearFix">
            <div class="title">
                <%= Resource.PortalRenameTitle %>
            </div>
        </div>
        <div class="clearFix">
            <div class="header-base-small">
                <%= Resource.NewPortalName %>
            </div>
            <div>
                <input type="text" class="textEdit" maxlength="50" id="studio_tenantAlias" value="<%: CurrentTenantAlias %>" data-actualvalue="<%: CurrentTenantAlias %>"/>
            </div>
        </div>
        <div class="middle-button-container">
            <a class="button blue"><%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= string.Format(CoreContext.Configuration.CustomMode ? CustomModeResource.HelpAnswerPortalRenameCustomMode.HtmlEncode() : Resource.HelpAnswerPortalRename.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
    </div>
</div>
<% } %>