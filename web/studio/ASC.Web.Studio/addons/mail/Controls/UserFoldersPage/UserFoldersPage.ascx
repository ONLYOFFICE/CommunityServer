<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserFoldersPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.UserFoldersPage" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div id="user_folders_page" class="hidden page_content">
    <div class="containerBodyBlock">
        <div class="content-header">
            <a title="<%=MailResource.CreateUserFolderBtn%>" class="button gray" id="createUserFolder" style="margin-right: 4px;">
                <div class="plus" style="background-position: -2px 1px;"><%=MailResource.CreateUserFolderBtn%></div>
            </a>
            <a title="<%=MailResource.EditUserFolderBtn%>" class="button gray" id="editUserFolder" style="margin-right: 4px;">
                <div style="background-position: -2px 1px;"><%=MailResource.EditUserFolderBtn%></div>
            </a>
            <a title="<%=MailResource.DeleteUserFolderBtn%>" class="button gray" id="deleteUserFolder">
                <div style="background-position: -2px 1px;"><%=MailResource.DeleteUserFolderBtn%></div>
            </a>
            <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'UserFoldersHelperBlock'});"></span>
        </div>
    </div>
    
    <div id="userFolderTree">
        <div class="userFolders"></div>
    </div>
</div>


<div class="popup_helper" id="UserFoldersHelperBlock">
    <p><%=MailResource.UserFolderCommonInformationText%></p>
    <p><%=MailResource.UserFolderCommonNotificationText%></p>
    <div class="cornerHelpBlock pos_top"></div>
</div>

<div id="selectUserFolderDropdown" class="actionPanel stick-over">
    <div class="existsUserFolders webkit-scrollbar"></div>
    <div class="h_line"></div>
    <div class="moveToMenu">
        <a title="<%: MailResource.MoveHere %>" class="button blue moveto_button"><%: MailResource.MoveHere %></a>
    </div>
</div>

<div id="userFolderWnd" style="display: none" create_header="<%=MailResource.CreateUserFolderLabel%>"
                                              edit_header="<%=MailResource.EditUserFolderLabel%>"
                                              delete_header="<%=MailResource.DeleteAccountLabel%>">
   <sc:Container ID="userFolderPopup" runat="server">
        <header>
        </header>
        <body>
            <div class="save requiredField">
                <span class="requiredErrorText required-hint"><%= MailScriptResource.ErrorEmptyField %></span>
                <table>
                    <tr>
                        <td>
                            <input id="mail_userFolderName" type="text" class="textEdit" maxlength="255" placeholder="<%=MailResource.UserFolderPlaceholder%>">
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label><%=MailScriptResource.UserFolderSelectorLabel%>
                                <a class="mail-foldersLink link dotline arrow-down" parent="0" title="<%=MailScriptResource.UserFolderSelectorLink%>"><%=MailScriptResource.UserFolderNoSelected%>></a>
                            </label>
                        </td>
                    </tr>
                </table>
            </div>
            <div class="mail-confirmationAction del">
                <p class="attentionText remove"><%=MailResource.DeleteUserFolderAttention%></p>
                <p class="questionText"></p>
            </div>
            <div class="buttons">
                <button class="button middle blue save" type="button"><%=MailResource.SaveBtnLabel%></button>
                <button class="button middle blue del" type="button"><%=MailResource.DeleteBtnLabel%></button>
                <button class="button middle gray cancel" type="button"><%=MailScriptResource.CancelBtnLabel%></button>
            </div>
        </body>
    </sc:Container>
</div>