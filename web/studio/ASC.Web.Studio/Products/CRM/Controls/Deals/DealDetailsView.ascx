<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DealDetailsView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Deals.DealDetailsView" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<div id="DealTabs"></div>

<div id="profileTab" class="display-none">
    <asp:PlaceHolder runat="server" ID="_phProfileView"></asp:PlaceHolder>
</div>
<div id="tasksTab" class="display-none">
    <div id="taskListTab">
    </div>
</div>
<div id="contactsTab" class="display-none">
    <div id="dealParticipantPanel">
        <div class="bold" style="margin-bottom:5px;"><%= CRMContactResource.AssignContactFromExisting%>:</div>
    </div>
    <div id="contactListBox">
        <table id="contactTable" class="tableBase" cellpadding="4" cellspacing="0">
            <tbody>
            </tbody>
        </table>
    </div>
</div>
<div id="invoicesTab" class="display-none">
    <table id="invoiceTable" class="tableBase" cellpadding="4" cellspacing="0">
        <colgroup>
            <col style="width: 1%;"/>
            <col/>
            <col style="width: 1%;"/>
            <col style="width: 1%;"/>
            <col style="width: 40px;"/>
        </colgroup>
        <tbody>
        </tbody>
    </table>
    <div id="invoiceActionMenu" class="studio-action-panel">
        <div class="corner-top right"></div>
        <ul class="dropdown-content">
            <li><a class="showProfileLink dropdown-item"><%= CRMInvoiceResource.ShowInvoiceProfile %></a></li>
            <% if (Global.CanDownloadInvoices) { %>
            <li><a class="downloadLink dropdown-item"><%= CRMInvoiceResource.Download %></a></li>
            <% } %>
            <% if (false) { %>
            <li><a class="printLink dropdown-item"><%= CRMInvoiceResource.Print %></a></li>
            <% } %>
            <% if (Global.CanDownloadInvoices) { %>
            <li><a class="sendLink dropdown-item"><%= CRMInvoiceResource.SendByEmail %></a></li>
            <% } %>
            <li><a class="editInvoiceLink dropdown-item"><%= CRMInvoiceResource.EditInvoice %></a></li>
            <li><a class="duplicateInvoiceLink dropdown-item"><%= CRMInvoiceResource.DuplicateInvoice %></a></li>
            <li><a class="deleteInvoiceLink dropdown-item"><%= CRMInvoiceResource.DeleteThisInvoice %></a></li>
        </ul>
    </div>
</div>
<div id="filesTab" class="display-none">
    <asp:PlaceHolder id="_phFilesView" runat="server"></asp:PlaceHolder>
</div>

<div id="dealDetailsMenuPanel" class="studio-action-panel">
    <div class="corner-top left"></div>
    <ul class="dropdown-content">
        <li>
            <a class="dropdown-item" href="<%= String.Format("deals.aspx?action=manage&id={0}", TargetDeal.ID) %>"
                    title="<%= CRMDealResource.EditThisDealButton%>">
                <%= CRMDealResource.EditThisDealButton%>
            </a>
        </li>
        <% if (Global.CanCreateProjects()) %>
        <% { %>
        <li>
            <a class="createProject dropdown-item" target="_blank"
                href="<%= String.Format("{0}projects.aspx?action=add&opportunityID={1}", CommonLinkUtility.ToAbsolute("~/products/projects/"), TargetDeal.ID) %>">
                    <%= CRMCommonResource.CreateNewProject %>
            </a>
        </li>
        <% } %>
    </ul>
</div>