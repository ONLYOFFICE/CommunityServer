<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccountsPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.AccountsPage" %>

<%@ Import Namespace="ASC.Web.Mail.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div id="id_accounts_page" class="hidden page_content">
    <div class="containerBodyBlock">
        <div class="content-header">
            <a title="<%=MailResource.CreateNewAccountBtn%>" href="#" class="button gray" id="createNewAccount">
                <div class="plus" style="background-position: -2px 1px;"><%=ASC.Web.Mail.Resources.MailResource.CreateNewAccountBtn%></div>
            </a>
            <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AccountsHelperBlock'});"></span>
        </div>
    </div>
</div>


<div class="popup_helper" id="AccountsHelperBlock">
    <p><%=MailResource.AccountsCommonInformationText%></p>
    <p><%=MailResource.AccountsCommonNotificationText%></p>
    <div class="cornerHelpBlock pos_top"></div>
</div>


<div id="accountActionMenu" class="studio-action-panel">
    <div class="corner-top right"></div>
    <ul class="dropdown-content">
        <li><a class="activateAccount dropdown-item"><%=MailResource.ActivateAccountLabel%></a></li>
        <li><a class="deactivateAccount dropdown-item"><%=MailResource.DeactivateAccountLabel%></a></li>
        <li><a class="selectAttachmentsFolder dropdown-item"><%=MailResource.SelectAttachmentsFolderLabel%></a></li>
        <li><a class="editAccount dropdown-item"><%=MailResource.EditAccountLabel%></a></li>
        <li><a class="deleteAccount dropdown-item"><%=MailResource.DeleteAccountLabel%></a></li>
    </ul>
</div>


<div id="questionWnd" style="display: none" delete_header="<%=MailResource.DeleteAccountLabel%>"
                                            activate_header="<%=MailResource.ActivateAccountLabel%>"
                                            deactivate_header="<%=MailResource.DeactivateAccountLabel%>">
   <sc:Container ID="accountQuestionPopup" runat="server">
        <header>
        </header>
        <body>
            <div class="mail-confirmationAction">
                <p class="attentionText remove"><%=MailResource.DeleteAccountAttention%></p>
                <p class="attentionText activate"><%=MailResource.ActivateAccountAttention%></p>
                <p class="attentionText deactivate"><%=MailResource.DeactivateAccountAttention%></p>
                <p class="questionText"></p>
            </div>
            <div class="buttons">
                <button class="button middle blue remove" type="button"><%=MailResource.DeleteBtnLabel%></button>
                <button class="button middle blue activate" type="button"><%=MailResource.ActivateBtnLabel%></button>
                <button class="button middle blue deactivate" type="button"><%=MailResource.DeactovateBtnLabel%></button>
                <button class="button middle gray cancel" type="button"><%=MailScriptResource.CancelBtnLabel%></button>
            </div>
        </body>
    </sc:Container>
</div>
