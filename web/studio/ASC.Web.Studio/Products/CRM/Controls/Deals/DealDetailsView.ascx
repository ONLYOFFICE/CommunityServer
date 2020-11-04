<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DealDetailsView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Deals.DealDetailsView" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

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
        <table id="contactTable" class="table-list padding4" cellpadding="0" cellspacing="0">
            <tbody>
            </tbody>
        </table>
    </div>
</div>
<div id="invoicesTab" class="display-none">
    <table id="invoiceTable" class="table-list padding4" cellpadding="0" cellspacing="0">
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
        <ul class="dropdown-content">
            <li><a class="showProfileLink dropdown-item with-icon user"><%= CRMInvoiceResource.ShowInvoiceProfile %></a></li>
            <li><a class="showProfileLinkNewTab dropdown-item with-icon new-tab"><%= CRMInvoiceResource.ShowInvoiceProfileNewTab %></a></li>
            <li class="dropdown-item-seporator"></li>
            <% if (Global.CanDownloadInvoices) { %>
            <li><a class="downloadLink dropdown-item with-icon download"><%= CRMInvoiceResource.Download %></a></li>
            <% } %>
            <li><a class="printLink dropdown-item with-icon print"><%= CRMInvoiceResource.Print %></a></li>
            <% if (Global.CanDownloadInvoices) { %>
            <li><a class="sendLink dropdown-item with-icon email"><%= CRMInvoiceResource.SendByEmail %></a></li>
            <% } %>
            <li><a class="duplicateInvoiceLink dropdown-item with-icon move-or-copy"><%= CRMInvoiceResource.DuplicateInvoice %></a></li>
            <li class="dropdown-item-seporator"></li>
            <li><a class="editInvoiceLink dropdown-item with-icon edit"><%= CRMInvoiceResource.EditInvoice %></a></li>
            <li><a class="deleteInvoiceLink dropdown-item with-icon delete"><%= CRMInvoiceResource.DeleteThisInvoice %></a></li>
        </ul>
    </div>
</div>
<div id="filesTab" class="display-none">
    <asp:PlaceHolder id="_phFilesView" runat="server"></asp:PlaceHolder>
</div>

<div id="dealDetailsMenuPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <li>
            <a class="dropdown-item" href="<%= String.Format("Deals.aspx?action=manage&id={0}", TargetDeal.ID) %>"
                    title="<%= CRMDealResource.EditThisDealButton%>">
                <%= CRMDealResource.EditThisDealButton%>
            </a>
        </li>
        <% if (Global.CanCreateProjects()) %>
        <% { %>
        <li>
            <a class="createProject dropdown-item" target="_blank"
                href="<%= String.Format("{0}Projects.aspx?action=add&opportunityID={1}", CommonLinkUtility.ToAbsolute("~/Products/Projects/"), TargetDeal.ID) %>">
                    <%= CRMCommonResource.CreateNewProject %>
            </a>
        </li>
        <% } %>
    </ul>
</div>