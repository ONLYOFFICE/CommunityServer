<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PwdTool.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.PwdTool" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_pwdReminderDialog" class="display-none">
    <sc:Container runat="server" ID="_pwdRemainderContainer">
        <Header>
            <span id="pswdRecoveryDialogPopupHeader" class="display-none">
                <%= Resource.PasswordRecoveryTitle %>
            </span><span id="pswdChangeDialogPopupHeader" class="display-none">
                <%= Resource.PasswordChangeTitle %>
            </span>
        </Header>
        <Body>
            <div id="studio_pwdReminderContent">
                <input type="hidden" id="studio_pwdReminderInfoID" value="<%=_pwdRemainderContainer.ClientID%>_InfoPanel" />
                <div id="pswdRecoveryDialogText" class="display-none">
                    <%= Resource.MessageSendPasswordRecoveryInstructionsOnEmail.HtmlEncode() %>
                    <input type="email" id="studio_emailPwdReminder" class="textEdit" />
                </div>
                <div id="pswdChangeDialogText" class="display-none">
                    <%= String.Format(Resource.MessageSendPasswordChangeInstructionsOnEmail.HtmlEncode(), "<a target=\"_blank\" name=\"userEmail\"></a>")%>
                </div>
                <div id="pwd_rem_panel_buttons" class="middle-button-container">
                    <a class="button middle blue" onclick="PasswordTool.RemindPwd();">
                        <%= Resource.SendButton %>
                    </a>
                    <span class="splitter-buttons"></span>
                    <a class="button middle gray" onclick="PopupKeyUpActionProvider.CloseDialog();">
                        <%= Resource.CancelButton %>
                    </a>
                </div>
            </div>
        </Body>
    </sc:Container>
</div>
