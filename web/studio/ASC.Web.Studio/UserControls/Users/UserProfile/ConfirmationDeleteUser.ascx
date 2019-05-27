<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmationDeleteUser.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.ConfirmationDeleteUser" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="confirmationDeleteUserPanel" style="display: none;">
    <sc:Container id="_confirmationDeleteUserPanel" runat="server">
        <header>
            <%= Resource.Confirmation%>
        </header>
        <body>
            <div class="confirmationAction"></div>
            <div><%= UserControlsCommonResource.NotBeUndone %></div>
            <div class="warning-header red-text"><%= Resource.Warning %></div>
            <div><%= Resource.DeleteUserDataConfirmation %></div>
            <div class="middle-button-container">
                <a class="button blue middle remove-btn"><%= Resource.OKButton %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle reassign-btn"><%= Resource.ReassignData %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="jq.unblockUI();"><%= Resource.CancelButton %></a>
            </div>
        </body>
    </sc:Container>
</div>