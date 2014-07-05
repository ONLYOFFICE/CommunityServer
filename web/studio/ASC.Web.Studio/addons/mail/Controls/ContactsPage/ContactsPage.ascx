<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactsPage.ascx.cs" Inherits="ASC.Web.Mail.Controls.ContactsPage" %>

<div id="id_contacts_page" class="hidden page_content">
    <div class="itemContainer">
        <div class="filterPanel hidden">
            <div id="crmFilter"></div>
        </div>
        <div class="filterPanel hidden">
            <div ID="tlFilter"></div>
        </div>
        <div class="containerBodyBlock hidden"></div>
    </div>
</div>

<div id="contactActionMenu" class="studio-action-panel">
    <div class="corner-top right"></div>
    <ul class="dropdown-content">
        <li><a class="viewContact dropdown-item"><%=ASC.Web.Mail.Resources.MailResource.ViewContact%></a></li>
        <li><a class="writeLetter dropdown-item"><%=ASC.Web.Mail.Resources.MailResource.WriteLetter%></a></li>
    </ul>
</div>




