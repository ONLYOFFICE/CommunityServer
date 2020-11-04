<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActivateEmailPanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.ActivateEmailPanel" %>

<div class="info-box excl display-none">
    <div class="btn-close" onclick="ASC.EmailOperationManager.closeActivateEmailPanel(this);"></div>
    <div>
        <div class="first-step">
            <div class="header-base medium bold">            
                <%= Resources.Resource.EmailActivationPanelHeaderText %>    
            </div>
            <%= Resources.Resource.EmailActivationPanelBodyText%>
            <a class="link underline blue" onclick="ASC.EmailOperationManager.sendInstructions('<%=CurrentUser.ID%>', '<%= CurrentUser.Email.Replace("'", "\\'").HtmlEncode() %>');">
                <%= Resources.Resource.EmailActivationPanelLinkText%>
            </a>
        </div>
        <div class="second-step display-none">
            <%= Resources.Resource.EmailActivationPanelSendText%>
        </div>
    </div>
</div>