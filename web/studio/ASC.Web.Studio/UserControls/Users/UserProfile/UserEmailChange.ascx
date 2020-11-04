<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserEmailChange.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.UserEmailChange" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_emailChangeDialog" style="display: none;">
    <sc:Container runat="server" ID="_emailChangerContainer">
        <Header>
            <span id="emailActivationDialogPopupHeader" class="display-none"> <%=Resource.EmailActivationTitle %></span> 
            <span id="emailChangeDialogPopupHeader" class="display-none"> <%=Resource.EmailChangeTitle %></span>
            <span id="resendInviteDialogPopupHeader" class="display-none"> <%=Resource.ResendInviteTitle%></span>
        </Header>
        <Body>
            <div id="studio_emailOperationContent" class="display-none">
                <div class="clearFix">
                    <div id="emailInputContainer" class="display-none">
                        <div id="divEmailOperationError" class="errorBox display-none">
                        </div>
                        <div id="emailInput">
                            <div>
                                <%=Resource.EnterEmail %>
                            </div>
                            <div class="userEmailEdit">
                                <input type="email" id="emailOperation_email" class="textEdit" />
                            </div>
                        </div>
                        <div id="emailChangeDialogText" class="display-none">
                            <%= String.Format(Resource.EmailChangeDescription,HttpUtility.HtmlEncode(UserInfo.FirstName + " " + UserInfo.LastName))%>
                        </div>
                        <div id="emailActivationDialogText" class="display-none">
                            <%= String.Format(Resource.EmailActivationDescription,HttpUtility.HtmlEncode(UserInfo.FirstName + " " + UserInfo.LastName))%>
                        </div>
                         <div id="resendInviteDialogText" class="display-none">
                            <%= String.Format(Resource.ResendInviteDescription,HttpUtility.HtmlEncode(UserInfo.FirstName + " " + UserInfo.LastName))%>
                        </div>
                    </div>
                    <div id="emailMessageContainer" class="display-none">
                        <div id="emailActivationText" class="display-none">
                            <%= String.Format(Resource.MessageSendEmailActivationInstructionsOnEmail, "<a target='_blank' name='userEmail'></a>")%>
                        </div>
                        <div id="emailChangeText" class="display-none">
                            <%= String.Format(Resource.MessageSendEmailChangeInstructionsOnEmail, "<a target='_blank' name='userEmail'></a>")%>
                        </div>
                        <div id="resendInviteText" class="display-none">
                            <%= String.Format(Resource.MessageReSendInviteInstructionsOnEmail, "<a target='_blank' name='userEmail'></a>")%>
                        </div>
                    </div>
                </div>
                <div class="clearFix middle-button-container">
                    <a class="button blue middle" href="javascript:void(0);" id="btEmailOperationSend">
                        <%=Resource.SendButton%>
                    </a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle" href="javascript:void(0);"
                            onclick="PopupKeyUpActionProvider.CloseDialog();">
                        <%=Resource.CancelButton%>
                    </a>
                </div>
            </div>
            <div id="studio_emailOperationResult" class="clearFix">
                <div id="studio_emailOperationResultText" class="userResultText"></div>
                <div class="middle-button-container">
                    <a class="button gray middle" href="javascript:ASC.EmailOperationManager.closeEmailOperationWindow(); return false;"
                        onclick="ASC.EmailOperationManager.closeEmailOperationWindow(); return false;">
                        <%=Resource.CloseButton%>
                    </a>
                </div>
            </div>
        </Body>
    </sc:Container>
</div>
