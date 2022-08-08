<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvitePanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.InvitePanel" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Core.WhiteLabel" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div id="invitePanelContainer" class="display-none"
    data-header="<%= Resource.InviteLinkTitle %>"
    data-ok="<%= Resource.CloseButton %>">
     <div>
        <% if (EnableInviteLink) { %>
        <p><%= Resource.HelpAnswerLinkInviteSettings %></p>
        <p><%= String.Format(Resource.InviteLinkValidInterval, SetupInfo.ValidEmailKeyInterval.Days) %></p>
        <% } else { %>
        <p>
            <%= UserControlsCommonResource.TariffUserLimitReason%>
            <%= CustomNamingPeople.Substitute<Resource>("KeepTariffInviteGuests").HtmlEncode() %>
        </p>
        <% if (TenantExtra.EnableTariffSettings) { %>
        <a href="<%= TenantExtra.GetTariffPageLink() %>">
            <%= Resource.UpgradePlan %>
        </a>
        <% } %>
        <% } %>
    </div>

    <div>
        <div id="linkInviteSettings">
            <div class="share-link-row">
                <div id="shareInviteUserLinkPanel" class="clearFix">

                    <span id="shareInviteUserLinkCopy" class="baseLinkAction text-medium-describe"><%= Resource.CopyToClipboard %></span>

                    <% if (UrlShortener.Enabled)
                       { %>
                    <span id="getShortenInviteLink" class="baseLinkAction text-medium-describe"><%= Resource.GetShortenLink %></span>
                    <% } %>

                    <div id="chkVisitorContainer" class="clearFix">
                        <input type="checkbox" id="chkVisitor" <%= EnableInviteLink && EnableInviteLinkVisitor ? "" : "disabled=\"disabled\"" %>  <%= EnableInviteLink ? "" : "checked=\"checked\"" %>/>
                        <label for="chkVisitor" <%= EnableInviteLinkVisitor ? "" : IsFreeTariff ? "title=\""+ Resource.DisableAddGuest +"\" class=\"disable-help-block-right\"" : "title=\""+ Resource.MaxGuestExceeded +"\" class=\"disable-help-block-right\""%>><%= CustomNamingPeople.Substitute<Resource>("InviteUsersAsCollaborators").HtmlEncode() %></label>
                         <% if (EnableInviteLinkVisitor) { %>
                        <input id="hiddenVisitorLink" type="hidden" value="<%= HttpUtility.HtmlEncode(GeneratedVisitorLink) %>" />
                         <% } %>
                        <% if (EnableInviteLink) { %>
                        <input id="hiddenUserLink" type="hidden" value="<%= HttpUtility.HtmlEncode(GeneratedUserLink) %>" />
                        <% } %>
                    </div>  
                    
                    <textarea id="shareInviteUserLink" class="textEdit" cols="10" rows="2" <% if (!ASC.Web.Core.Mobile.MobileDetector.IsMobile)
                                                                                    { %> readonly="readonly" <%} %>><%= HttpUtility.HtmlEncode(EnableInviteLink ? GeneratedUserLink : GeneratedVisitorLink) %></textarea>
                </div>
            </div>
        </div>
        <% if (!CoreContext.Configuration.CustomMode && CompanyWhiteLabelSettings.Instance.IsDefault) { %>
        <ul id="shareInviteLinkViaSocPanel" class="clearFix">
            <li><a class="facebook" target="_blank" title="<%= Resource.TitleFacebook %>"></a></li>
            <li><a class="twitter" target="_blank" title="<%= Resource.TitleTwitter %>"></a></li>
        </ul>
        <% } %>
    </div>
</div>