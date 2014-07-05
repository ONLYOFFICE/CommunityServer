<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TransferPortal.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TransferPortal" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% if (IsVisibleMigration)
   { %>
<div id="migrationPortal" class="clearFix <%= EnableMigration ? "" : "disable" %>">
    <div class="settings-block transfer-portal">
        <div class="header-base">
            <%= Resource.TransferPortalTitle %>
        </div>        
        <div class="header-base-small">
            <%= Resource.ServerRegion %>:
        </div>        
        <select id="transfer_region" data-value="<%= CurrentRegion %>" class="comboBox">
            <% foreach (var item in TransferRegions)
               {%>
                <option <%= item.IsCurrentRegion ? "selected=\"selected\"" : "" %> value="<%= item.Name %>" data-url=".<%= item.BaseDomain %>"><%= item.FullName %></option>            
            <% } %>
        </select>

        <div class="header-base-small">
            <%= Resource.PortalName %>:
        </div>
        <div>
            <span id="regionDomain">
                <%= CoreContext.TenantManager.GetCurrentTenant().TenantAlias %></span><span id="regionUrl">.<%= BaseDomain %></span>
        </div>
        <div class="clearFix notify-migration">
            <div>
                <input id="migrationMail" type="checkbox" />
                <label for="migrationMail">
                    <%= Resource.IsMailMigration %></label>
            </div>
            <div>
                <input id="notifyAboutMigration" type="checkbox" checked="checked" />
                <label for="notifyAboutMigration">
                    <%= Resource.NotifyPortalMigration %></label>
            </div>
        </div>
        <div class="header-base red-text"><%= Resource.Warning %></div>
        <div><%= Resource.TransferPortalWarning %></div>
        <div class="middle-button-container">
            <a id="transfer_button" class="button blue disable" href="javascript:void(0);">
                <%= Resource.TransferPortalButton %></a>
        </div>
        <div class="edition-block">
            <div id="transfer_progress" class="display-none progress">
                <div class="asc-progress-wrapper">
                    <div class="asc-progress-value"></div>
                </div>
                <div class="text-medium-describe migrating">
                    <%= Resource.MigratingPortal %>
                    <span id="transfer_percent"></span>
                </div>
            </div>            
            <div id="transfer_error" class="errorText display-none"></div>
            <div id="transfer_ready" class="display-none"></div>
        </div>
    </div>
    <div class="settings-help-block">
        <% if (EnableMigration)
           { %>
        <p><%= String.Format(Resource.HelpAnswerTransferPortal, "<br />", "<b>", "</b>") %></p>
        <a href="<%= CommonLinkUtility.GetHelpLink(true) + "gettingstarted/configuration.aspx#ChangingGeneralSettings_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% }
           else
           { %>
            <p><%= String.Format(Resource.MigrationNotAvailable, "<b>", "</b>") %></p>
        <% } %>
    </div>
</div>
<div id="popupTransferStart" class="display-none">
    <sc:Container runat="server" id="popupTransferStart">
        <Header>
        <div><% = Resource.TransferPortalTitlePopup%></div>
        </Header>
        <Body>
          <% = String.Format(Resource.TransferPortalContentPopup, "<p>","</p>")%>
    <div class="big-button-container">
        <a class="button blue middle"><% = Resource.ContinueButton %></a>
        <span class="splitter-buttons"></span>
         <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%= Resource.CancelButton %></a>
    </div>
        </Body>
    </sc:Container>
</div>
<% } %>