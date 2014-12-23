<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DeactivatePortal.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.DeactivatePortal" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div id="accountDeactivationBlock" class="settings-block">
        <div class="header-base"><%=Resource.AccountDeactivation%></div>
        <div><%=Resource.DeactivationDesc%></div>
        <div class="middle-button-container">
                <a id="sendDeactivateInstructionsBtn" class="button blue middle"><%=Resource.DeactivateButton%></a>
        </div>
        <p id="deativate_sent" class="display-none"></p>
    </div>
    <div class="settings-help-block">
        <p><%=String.Format(Resource.HelpAnswerAccountDeactivation, "<br />", "<b>", "</b>")%></p>
    </div>
</div>

<div class="clearFix">
    <div id="accountDeletionBlock" class="settings-block">
        <div class="header-base"><%=Resource.AccountDeletion%></div>
        <div><%=Resource.DeletionDesc%></div>
        <div class="middle-button-container">
            <a id="sendDeleteInstructionsBtn" class="button blue middle"><%=Resource.DeleteButton%></a>
        </div>
        <p id="delete_sent" class="display-none"></p>
    </div>
</div>