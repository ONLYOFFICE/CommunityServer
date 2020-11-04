<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MailBox.ascx.cs" Inherits="ASC.Web.Mail.Controls.MailBox" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:PlaceHolder ID="OutContentPlaceHolder" runat="server"></asp:PlaceHolder>

<div id="mailBoxContainer" class="mailBoxContainer">

    <asp:PlaceHolder runat="server" ID="ControlPlaceHolder"></asp:PlaceHolder>

    <asp:PlaceHolder ID="TagsPageHolder" runat="server"></asp:PlaceHolder>
    <div id="popupDocumentUploader">
        <asp:PlaceHolder ID="_phDocUploader" runat="server"></asp:PlaceHolder>
    </div>

    <asp:PlaceHolder runat="server" ID="fileholder"></asp:PlaceHolder>

    <div id="itemContainer"></div>

    <div id="helpPanel" class="display-none"></div>

    <div id="messagesActionMenu" class="studio-action-panel">
        <ul class="dropdown-content">
            <li class="openMail">
                <a class="dropdown-item with-icon open-mail"><%= MailResource.OpenBtnLabel %></a>
            </li>
            <li class="openNewTabMail">
                <a class="dropdown-item with-icon new-tab"><%= MailResource.OpenInNewTabBtnLabel %></a>
            </li>
            <li class="openSeparator">
                <div class="dropdown-item-seporator"></div>
            </li>
            <li class="replyMail">
                <a class="dropdown-item with-icon reply"><%= MailResource.ReplyBtnLabel %></a>
            </li>
            <li class="replyAllMail">
                <a class="dropdown-item with-icon reply-all"><%= MailResource.ReplyAllBtnLabel %></a>
            </li>
            <li class="forwardMail">
                <a class="dropdown-item with-icon forward"><%= MailResource.ForwardLabel %></a>
            </li>
            <li class="createEmail">
                <a class="dropdown-item with-icon edit"><%= MailResource.CreateEmailToSenderLabel %></a>
            </li>
            <li class="composeSeparator">
                <div class="dropdown-item-seporator"></div>
            </li>
            <li class="setReadMail">
                <a class="dropdown-item with-icon mark-as-read"><%= MailResource.MarkAsRead %></a>
            </li>
            <li class="markImportant">
                <a class="dropdown-item with-icon mark-as-important"><%= MailResource.MarkAsImportant %></a>
            </li>
            <li class="markSeparator">
                <div class="dropdown-item-seporator"></div>
            </li>
            <% if (IsMailPrintAvailable())
               { %>

            <li class="printMail">
                <a class="dropdown-item with-icon print"><%= MailScriptResource.PrintBtnLabel %></a>
            </li>
             <li class="printSeparator">
                <div class="dropdown-item-seporator"></div>
            </li>
            <% } %>
            <li class="moveToFolder">
                <a class="dropdown-item with-icon spam"><%= MailScriptResource.SpamLabel %></a>
            </li>
            <li class="deleteMail">
                <a class="dropdown-item with-icon delete"><%= MailResource.DeleteBtnLabel %></a>
            </li>
        </ul>
    </div>
</div>


<div id="removeQuestionWnd" style="display: none">
    <sc:Container ID="QuestionPopup" runat="server">
        <Header>
        </Header>
        <Body>
            <div class="mail-confirmationAction">
                <p class="questionText"></p>
            </div>
            <div class="buttons">
                <button class="button middle blue remove" type="button"><%= MailResource.DeleteBtnLabel %></button>
                <button class="button middle gray cancel" type="button"><%= MailScriptResource.CancelBtnLabel %></button>
            </div>
        </Body>
    </sc:Container>
</div>


<div id="attachmentActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="downloadAttachment dropdown-item with-icon download"><%= MailResource.DownloadAttachment %></a></li>
        <li><a class="viewAttachment dropdown-item with-icon preview"><%= MailResource.ViewAttachment %></a></li>
        <li><a class="editAttachment dropdown-item with-icon edit"><%= MailResource.EditAttachment %></a></li>
        <li><a class="saveAttachmentToDocs dropdown-item with-icon document"><%= MailResource.SaveAttachToDocs %></a></li>
        <li><a class="saveAttachmentToCalendar dropdown-item with-icon calendar"><%= MailResource.SaveAttachToCalendar %></a></li>
    </ul>
</div>

<div id="attachmentEditActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="downloadAttachment dropdown-item with-icon download"><%= MailResource.DownloadAttachment %></a></li>
        <li><a class="viewAttachment dropdown-item with-icon preview"><%= MailResource.ViewAttachment %></a></li>
        <li><a class="deleteAttachment dropdown-item with-icon delete"><%= MailResource.DeleteAttachment %></a></li>
    </ul>
</div>

<div id="messageActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="singleViewMail dropdown-item with-icon new-tab"><%= MailResource.SingleViewLabel %></a></li>
        <li class="singleViewMailSeporator dropdown-item-seporator"></li>
        <li><a class="replyMail dropdown-item with-icon reply"><%= MailResource.ReplyBtnLabel %></a></li>
        <li><a class="replyAllMail dropdown-item with-icon reply-all"><%= MailResource.ReplyAllBtnLabel %></a></li>
        <li><a class="forwardMail dropdown-item with-icon forward"><%= MailResource.ForwardLabel %></a></li>
        <li class="dropdown-item-seporator"></li>
        <li><a class="alwaysHideImages dropdown-item with-icon image"><%= MailScriptResource.HideImagesLabel %></a></li>
        <li><a class="exportMessageToCrm dropdown-item with-icon export"><%= MailResource.ExportMessageToCRM %></a></li>
        <li><a class="createPersonalContact dropdown-item with-icon user-new"><%=MailScriptResource.CreatePersonalContact %></a></li>
        <li><a class="createCrmPerson dropdown-item with-icon create"><%= MailScriptResource.CreateNewCRMPerson %></a></li>
        <li><a class="createCrmCompany dropdown-item with-icon create"><%= MailScriptResource.CreateNewCRMCompany %></a></li>
        <% if (IsMailPrintAvailable())
           { %>
        <li class="dropdown-item-seporator"></li>
        <li><a class="printMail dropdown-item with-icon print"><%= MailScriptResource.PrintBtnLabel %></a></li>
        <% } %>
        <li class="dropdown-item-seporator"></li>
        <li><a class="deleteMail dropdown-item with-icon delete"><%= MailResource.DeleteBtnLabel %></a></li>
    </ul>
</div>
