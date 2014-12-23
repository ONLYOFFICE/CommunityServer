<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdministrationPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.AdministrationPage" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<div id="id_administration_page" class="hidden page_content">
    <div class="containerBodyBlock">
        <button id="mail-server-add-domain" class="button gray plus addDomain" type="button"><%= MailAdministrationResource.AddNewDomainButton %></button>
        <div class="controls-container">
            <div class="domains_list_position view_control"></div>
        </div>
    </div>
</div>


<div id="mailboxActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="editMailbox dropdown-item"><%= MailAdministrationResource.EditMailboxAliasesLabel %></a></li>
        <li><a class="deleteMailbox dropdown-item"><%= MailAdministrationResource.DeleteMailboxLabel %></a></li>
    </ul>
</div>

<div id="groupActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="editGroup dropdown-item"><%= MailAdministrationResource.EditGroupAddressesLabel %></a></li>
        <li><a class="deleteGroup dropdown-item"><%= MailAdministrationResource.DeleteGroupLabel %></a></li>
    </ul>
</div>

<div id="domainActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="showDnsSettingsDomain dropdown-item"><%= MailAdministrationResource.DNSSettingsLabel %></a></li>
        <li><a class="deleteDomain dropdown-item"><%= MailAdministrationResource.DeleteDomain %></a></li>
    </ul>
</div>