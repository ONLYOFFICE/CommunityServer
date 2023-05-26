<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImpersonateUserConfirmationPanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ImpersonateUser.ImpersonateUserConfirmationPanel" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% if (ImpersonationSettings.Available)
    { %>
<div id="confirmationImpersonateUserPanel" style="display: none;">
    <sc:Container id="_confirmationImpersonateUserPanel" runat="server">
        <header>
            <%= Resource.ConfirmImpersonateUserTitle%>
        </header>
        <body>
            <div class="confirmationAction"></div>
            <div class="settings-help-block"><%= Resource.ConfirmImpersonateHelper %></div>
            <div class="middle-button-container">
                <a class="button blue middle impersonate-btn"><%= Resource.ImpersonateYesButton %></a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="jq.unblockUI();"><%= Resource.ImpersonateCancelButton %></a>
            </div>
        </body>
    </sc:Container>
</div>
<% } %>
