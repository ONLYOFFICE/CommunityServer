<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InviteLink.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.InviteLink" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="inviteLinkContainer" class="display-none">
    <sc:Container ID="_inviteLinkContainer" runat="server">
        <Header><%= Resource.InviteLinkTitle %></Header>
        <Body>
            <div>
                <% if (EnableInviteLink)
                   { %>

                <p>
                    <%= String.Format(Resource.HelpAnswerLinkInviteSettings, "<br />", "<b>", "</b>", string.Format(Resource.NoteForInviteCollaborator+"<br/>", "<b>", "</b>"))%>
                </p>

                <% }
                   else
                   { %>

                <p>
                    <%= UserControlsCommonResource.TariffUserLimitReason%>
                    <%= Resource.KeepTariffInviteGuests%>
                </p>
                <% if (!ASC.Core.CoreContext.Configuration.Standalone && TenantExtra.EnableTarrifSettings)
                   { %>
                <a href="<%= TenantExtra.GetTariffPageLink() %>">
                    <%= Resource.UpgradePlan %>
                </a>
                <% } %>

                <% } %>
            </div>

            <div>
                <div id="linkInviteSettings">
                    <div class="share-link-row">
                        <div id="shareLinkPanel">
                            <a id="shareLinkCopy" class="link dotline small gray"><span><%= Resource.CopyToClipboard %></span></a>
                            <textarea id="shareLink" class="textEdit" cols="10" rows="2" <% if (!ASC.Web.Core.Mobile.MobileDetector.IsMobile)
                                                                                            { %> readonly="readonly" <%} %>><%= EnableInviteLink ? GeneratedUserLink : GeneratedVisitorLink%></textarea>
                        </div>
                    </div>
                </div>
                <div class="clearFix">
                    <input type="checkbox" id="chkVisitor" <%= EnableInviteLink ? "" : "disabled=\"disabled\" checked=\"checked\"" %> />
                    <label for="chkVisitor"><%= Resource.InviteUsersAsCollaborators%></label>
                    <% if (EnableInviteLink)
                       { %>
                    <input id="hiddenVisitorLink" type="hidden" value="<%= GeneratedVisitorLink%>" />
                    <input id="hiddenUserLink" type="hidden" value="<%= GeneratedUserLink%>" />
                    <% } %>
                </div>

            </div>
            <div class="middle-button-container">
                <a class="button blue middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.OKButton %>
                </a>
            </div>


        </Body>
    </sc:Container>
</div>
