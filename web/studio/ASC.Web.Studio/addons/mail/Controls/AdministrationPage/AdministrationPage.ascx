<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdministrationPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.AdministrationPage" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<div id="id_administration_page" class="hidden page_content">
    <div class="containerBodyBlock">
        <div class="controls-container">
            <div class="domains_list_position view_control"></div>
        </div>
    </div>
</div>


<div id="mailboxActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="editMailbox dropdown-item with-icon edit"><%= MailAdministrationResource.EditMailboxLabel %></a></li>
        <li><a class="changeMailboxPassword dropdown-item with-icon block"><%= MailResource.ChangeAccountPasswordLabel %></a></li>
        <li><a class="deleteMailbox dropdown-item with-icon delete"><%= MailAdministrationResource.DeleteMailboxLabel %></a></li>
    </ul>
</div>

<div id="groupActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="editGroup dropdown-item with-icon edit"><%= MailAdministrationResource.EditGroupAddressesLabel %></a></li>
        <li><a class="deleteGroup dropdown-item with-icon delete"><%= MailAdministrationResource.DeleteGroupLabel %></a></li>
    </ul>
</div>

<div id="domainActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="showDnsSettingsDomain dropdown-item with-icon settings"><%= MailAdministrationResource.DNSSettingsLabel %></a></li>
        <li><a class="showConnectionSettingsDomain dropdown-item with-icon settings"><%= MailResource.ViewAccountConnectionSettingsLabel %></a></li>
        <li><a class="deleteDomain dropdown-item with-icon delete"><%= MailAdministrationResource.DeleteDomain %></a></li>
    </ul>
</div>