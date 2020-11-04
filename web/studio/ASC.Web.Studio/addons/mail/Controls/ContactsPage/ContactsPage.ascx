<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactsPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.ContactsPage" %>

<div id="id_contacts_page" class="hidden page_content">
    <div class="itemContainer">
        <div class="containerBodyBlock hidden"></div>
    </div>
</div>

<div id="contactActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="writeLetter dropdown-item with-icon email"><%=ASC.Web.Mail.Resources.MailResource.WriteLetter%></a></li>
        <li><a class="viewContact dropdown-item with-icon user"><%=ASC.Web.Mail.Resources.MailResource.ViewContact%></a></li>
        <li><a class="editContact dropdown-item with-icon edit"><%=ASC.Web.Mail.Resources.MailResource.EditContact%></a></li>
        <li><a class="deleteContact dropdown-item with-icon delete"><%=ASC.Web.Mail.Resources.MailScriptResource.Delete%></a></li>
    </ul>
</div>




