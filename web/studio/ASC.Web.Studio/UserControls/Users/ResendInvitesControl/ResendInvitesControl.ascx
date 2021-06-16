<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ResendInvitesControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.ResendInvitesControl" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div id="inviteResender" style="display: none;">
    <sc:Container ID="_invitesResenderContainer" runat="server">
        <Header><%= Resource.ResendInviteTitle %></Header>
        <Body>
        <div id="resendInvitesContent">
            <div>
                <%=Resource.ResendInvitesText%>
            </div>
            <div class="clearFix middle-button-container">
                <a id="resendBtn" class="button blue middle" href="javascript:void(0);"><%=Resource.ResendInvitesButton%></a>
                <span class="splitter-buttons"></span>
                <a id="resendCancelBtn" class="button gray middle" href="javascript:void(0);"><%=Resource.CancelButton%></a>
            </div>
        </div>
        </Body>
    </sc:Container>
</div>
