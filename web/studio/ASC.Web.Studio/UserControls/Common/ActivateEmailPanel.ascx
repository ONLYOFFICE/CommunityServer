<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActivateEmailPanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.ActivateEmailPanel" %>

<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div class="info-box excl">
    <div class="btn-close" onclick="ASC.EmailOperationManager.closeActivateEmailPanel(this);"></div>
    <div>
        <div class="first-step">
            <div class="header-base medium bold">            
                <%= Resource.EmailActivationPanelHeaderText %>    
            </div>
            <%= Resource.EmailActivationPanelBodyText%>
            <a class="link underline blue" onclick="ASC.EmailOperationManager.sendInstructions('<%=CurrentUser.ID%>', '<%= (CurrentUser.Email ?? "").Replace("'", "\\'").HtmlEncode() %>');">
                <%= Resource.EmailActivationPanelLinkText%>
            </a>
        </div>
        <div class="second-step display-none">
            <%= Resource.EmailActivationPanelSendText%>
        </div>
    </div>
</div>