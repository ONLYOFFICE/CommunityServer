<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TransferPortal.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TransferPortal" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Management" %>

<% if (IsVisibleMigration)
   { %>
<div id="migrationPortal" class="clearFix <%= PaidMigration && OwnerMigration ? "" : "disable" %>">
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
                <option <%= item.IsCurrentRegion ? "selected=\"selected\"" : "" %> value="<%= item.Name %>" data-url=".<%= item.BaseDomain %>">
                    <%= item.GetFullName() %>
                </option>            
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
                <label for="migrationMail"><%: Resource.IsMailMigration %></label>
            </div>
            <div>
                <input id="notifyAboutMigration" type="checkbox" checked="checked" />
                <label for="notifyAboutMigration">
                    <%= CustomNamingPeople.Substitute<Resource>("NotifyPortalMigration").HtmlEncode() %></label>
            </div>
        </div>
        <div class="header-base red-text"><%= Resource.Warning %></div>
        <div><%: Resource.TransferPortalWarning %></div>
        <div class="middle-button-container">
            <a id="transfer_button" class="button blue disable" href="javascript:void(0);">
                <%= Resource.TransferPortalButton %></a>
        </div>
        <div class="edition-block">
            <div id="transfer_error" class="errorText display-none"></div>
            <div id="transfer_ready" class="display-none"></div>
        </div>
    </div>
    <div class="settings-help-block">
        <% if (PaidMigration && OwnerMigration)
           { %>
        <p><%= String.Format(Resource.HelpAnswerTransferPortal.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
         <% if (!string.IsNullOrEmpty(HelpLink))
             { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#ChangingPortalRegion_block" %>" target="_blank"><%= Resource.LearnMore %></a>
         <% } %>
        <% }
           else
           { %>
            <p><%= String.Format(PaidMigration ? Resource.MigrationNotAvailableOwner : Resource.MigrationNotAvailable , "<b>", "</b>") %></p>
        <% } %>
    </div>
</div>
<div id="popupTransferStart" class="display-none">
    <sc:Container runat="server" id="popupTransferStart">
        <Header>
            <div><%: Resource.TransferPortalTitlePopup%></div>
        </Header>
        <Body>
            <%= String.Format(Resource.TransferPortalContentPopup.HtmlEncode(), "<p>","</p>")%>
            <div class="big-button-container">
                <a class="button blue middle"><% = Resource.ContinueButton %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>
<% } %>