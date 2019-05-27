<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ResetAppDialog.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ResetAppDialog" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Core.TFA" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_resetAppDialog" style="display: none;">
    <sc:Container runat="server" ID="_showBackupCodesContainer">
        <Header>
            <%= Resource.TfaAppResetAppHeader %>
        </Header>
        <Body>
            <input type="hidden" id="tfaHiddenUserInfoId" value="<%= User.ID.ToString() %>" />

            <div id="resetAppContent">
                <%= Resource.TfaAppResetAppDescription %>
            </div>
            <div id="errorResetApp" class="errorBox" style="display: none;"></div>
            <div class="middle-button-container">
                <a id="resetAppButton" class="button middle blue"><%= Resource.TfaAppResetApp %></a>
                <span class="splitter-buttons"></span>
                <a id="resetAppClose" class="button middle gray"><%= Resource.CloseButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>
