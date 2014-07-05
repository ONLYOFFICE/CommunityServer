<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InviteLink.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.InviteLink" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div class="settings-block">
        <div id="linkInviteSettingsTitle" class="header-base clearFix">
            <%= Resource.InviteLinkTitle %>
        </div>
        <div id="linkInviteSettings">
            <div class="share-link-row">
                <div id="shareLinkPanel">
                    <a id="shareLinkCopy" class="link dotline small gray"><span><%= Resource.CopyToClipboard %></span></a>
                    <textarea id="shareLink" class="textEdit" cols="10" rows="2"  <% if (!ASC.Web.Core.Mobile.MobileDetector.IsMobile){ %> readonly="readonly" <%} %>><%= EnableInviteLink ? GeneratedUserLink : GeneratedVisitorLink%></textarea>
                </div>
            </div>
        </div>
        <div class="clearFix">
            <input type="checkbox" id="chkVisitor" <%= EnableInviteLink ? "" : "disabled=\"disabled\" checked=\"checked\"" %> />
            <label for="chkVisitor"><%= Resource.InviteUsersAsCollaborators%></label>
            <% if (EnableInviteLink) { %>
            <input id="hiddenVisitorLink" type="hidden" value="<%= GeneratedVisitorLink%>"/>
            <input id="hiddenUserLink" type="hidden" value="<%= GeneratedUserLink%>"/>
            <% } %>
        </div>
        
    </div>
    <div class="settings-help-block">
        <% if (EnableInviteLink) { %>

        <p>
            <%= String.Format(Resource.HelpAnswerLinkInviteSettings, "<br />", "<b>", "</b>", string.Format(Resource.NoteForInviteCollaborator+"<br/>", "<b>", "</b>"))%>
        </p>

        <% } else { %>

        <p>
            <%= UserControlsCommonResource.TariffUserLimitReason%>
            <%= Resource.KeepTariffInviteGuests%>
        </p>
        <% if (!ASC.Core.CoreContext.Configuration.Standalone)
           { %>
        <a href="<%= TenantExtra.GetTariffPageLink() %>">
            <%= Resource.UpgradePlan %>
        </a>
        <% } %>

        <% } %>
    </div>  
</div>    
