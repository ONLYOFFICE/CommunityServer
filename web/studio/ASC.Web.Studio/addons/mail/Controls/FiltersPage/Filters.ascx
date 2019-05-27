<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Filters.ascx.cs" Inherits="ASC.Web.Mail.Controls.FiltersPage" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<div id="filters_page" class="hidden page_content">
    <div class="containerBodyBlock">
        <div class="content-header">
            <a title="<%=MailResource.CreateFilterBtn%>" class="button gray" id="createFilter" style="margin-right: 4px;">
                <div class="plus" style="background-position: -2px 1px;"><%=MailResource.CreateFilterBtn%></div>
            </a>
            <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'FiltersHelperBlock'});"></span>
        </div>
    </div>
    
    <div id="filtersContainer">
    </div>
    
    <div id="filterActionMenu" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a class="enableFilter dropdown-item"><%= MailResource.EnableBtnLabel %></a></li>
            <li><a class="applyFilter dropdown-item"><%= MailResource.ApplyFilterBtnLabel %></a></li>
            <li><a class="editFilter dropdown-item"><%= MailResource.EditFilterBtn %></a></li>
            <li><a class="deleteFilter dropdown-item"><%= MailResource.DeleteFilterBtn %></a></li>
        </ul>
    </div>
</div>

<div class="popup_helper" id="FiltersHelperBlock">
    <p><%=MailResource.FiltersCommonInformationText%></p>
    <p><%=MailResource.FiltersCommonNotificationText%></p>
    <div class="cornerHelpBlock pos_top"></div>
</div>

<div id="filterWnd" style="display: none" apply_header="<%=MailResource.ApplyBtnLabel%>" delete_header="<%=MailResource.DeleteAccountLabel%>" confirm_header="<%=MailResource.ConfirmFilterActionDeleteForeverLabel%>">
   <sc:Container ID="filterPopup" runat="server">
        <header>
        </header>
        <body>
            <div class="mail-confirmationAction save">
                <p class="attentionText save"><%=MailResource.ApplyFilterAttention%></p>
                <p class="attentionText actions"></p>
            </div>
            <div class="mail-confirmationAction del">
                <p class="questionText"><%=MailResource.DeleteFilterItemShure%></p>
            </div>
            <div class="mail-confirmationAction confirm">
                <p class="questionText" style="word-break: initial;"><%=MailResource.DeleteFilterShure%></p>
            </div>
            <div class="buttons">
                <button class="button middle blue save" type="button"><%=MailResource.ApplyBtnLabel%></button>
                <button class="button middle blue del" type="button"><%=MailResource.DeleteBtnLabel%></button>
                <button class="button middle blue confirm" type="button"><%=MailScriptResource.FilterActionDeleteLabel%></button>
                <button class="button middle gray change" type="button"><%=MailScriptResource.FilterActionMoveToTarshLabel%></button>
                <button class="button middle gray cancel" type="button"><%=MailScriptResource.CancelBtnLabel%></button>
            </div>
        </body>
    </sc:Container>
</div>