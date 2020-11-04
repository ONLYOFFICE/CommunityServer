<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Data.Storage" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactDetailsView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Contacts.ContactDetailsView" %>

<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.CRM.Core.Entities" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>


<div id="profileTab" class="display-none">
    <asp:PlaceHolder runat="server" ID="_phProfileView"></asp:PlaceHolder>
</div>
<div id="tasksTab" class="display-none">
    <div id="taskListTab">
    </div>
</div>
<div id="contactsTab" class="display-none">
    <div id="peopleInCompanyPanel">
        <div class="bold" style="margin-bottom:5px;"><%= CRMContactResource.AssignPersonFromExisting%>:</div>
    </div>
    <div id="contactListBox">
        <table id="contactTable" class="table-list padding4" cellpadding="0" cellspacing="0">
            <tbody>
            </tbody>
        </table>
    </div>
</div>
<div id="dealsTab" class="display-none">
    <div id="dealsInContactPanel">
        <div class="bold" style="margin-bottom:5px;"><%= CRMDealResource.LinkDealsFromExisting %>:</div>
    </div>
    <div id="dealList" class="clearFix" style="min-height: 200px;">
    </div>
    <div id="files_hintStagesPanel" class="hintDescriptionPanel">
        <%=CRMDealResource.TooltipStages%>
        <% if (!string.IsNullOrEmpty(HelpLink)) { %>
        <a href="<%= HelpLink + "/tipstricks/opportunity-stages.aspx" %>" target="_blank">
            <%=CRMCommonResource.ButtonLearnMore%>
        </a>
        <% } %>
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
<div id="projectsTab" class="display-none">
    <table id="projectsInContactPanel">
        <tr>
            <td class="selectProject">
                <% if (!MobileVer) %>
                <% { %>
                <div class="menuAction">
                    <span><%= CRMJSResource.LinkWithProject%></span>
                    <div class="down_arrow"></div>
                </div>
                <% } else { %>
                <select></select>
                <% } %>
            </td>

            <% if (Global.CanCreateProjects()) %>
            <% { %>
            <td class="createNewProject">
                <div class="menuAction unlockAction">
                    <%= CRMContactResource.CreateProject %>
                </div>
            </td>
            <% } %>
        </tr>
    </table>
    <table id="tableListProjects" class="simple-view-table table-list height40">
        <tbody>
        </tbody>
    </table>
</div>

<div id="socialMediaTab" class="display-none">

    <div id="divSocialMediaContent">

    </div>

    <div id="divSMProfilesWindow" class="borderBase">
        <div class="header-base-medium divHeader">
            <span></span>
            <label class="cancel_cross" title="<%= CRMCommonResource.CloseWindow%>" onclick="jq('#divSMProfilesWindow').hide();"></label>
        </div>
        <div class="divSMProfilesWindowBody mobile-overflow">
            <table id="sm_tbl_UserList">
            </table>
            <div class="divWait">
                <span class="loader-text-block">
                    <%= CRMSocialMediaResource.PleaseWait%></span>                
            </div>
            <div class="divNoProfiles">
                <%= TargetContact is Person ? CRMSocialMediaResource.NoAccountsHasBeenFound
                                            : CRMSocialMediaResource.NoCompaniesHasBeenFound%>
            </div>
        </div>
    </div>
</div>

<% if (CRMSecurity.CanEdit(TargetContact)) %>
<% { %>
<div id="contactDetailsMenuPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <li>
            <a class="dropdown-item" href="<%= String.Format("Default.aspx?id={0}&action=manage{1}",
                TargetContact.ID, (TargetContact is Company) ? string.Empty : "&type=people") %>">
                <%= (TargetContact is Company) ? CRMContactResource.EditProfileCompany : CRMContactResource.EditProfilePerson %>
            </a>
        </li>
        <li>
            <a class="dropdown-item" onclick="ASC.CRM.ContactFullCardView.showMergePanel();">
                <%= CRMContactResource.MergeButtonText %>
            </a>
        </li>
    </ul>
</div>
<% } %>

<% if (!MobileVer) %>
<% { %>
<div id="projectSelectorContainer" class="studio-action-panel display-none">
    <ul class="dropdown-content"></ul>
</div>
<% } %>