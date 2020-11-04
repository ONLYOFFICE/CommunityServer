<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CasesDetailsView.ascx.cs"
        Inherits="ASC.Web.CRM.Controls.Cases.CasesDetailsView" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>


<div id="profileTab" class="display-none">
    <asp:PlaceHolder runat="server" ID="_phProfileView"></asp:PlaceHolder>
</div>
<div id="tasksTab" class="display-none">
    <div id="taskListTab">
    </div>
</div>
<div id="contactsTab" class="display-none">
    <div id="caseParticipantPanel">
        <div class="bold" style="margin-bottom:5px;"><%= CRMContactResource.AssignContactFromExisting%>:</div>
    </div>
    <div id="contactListBox">
        <table id="contactTable" class="table-list padding4" cellpadding="0" cellspacing="0">
            <tbody>
            </tbody>
        </table>
    </div>
</div>
<div id="filesTab" class="display-none">
    <asp:PlaceHolder id="_phFilesView" runat="server"></asp:PlaceHolder>
</div>


<div id="caseDetailsMenuPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <li>
            <a class="dropdown-item" href="<%= String.Format("Cases.aspx?id={0}&action=manage", TargetCase.ID) %>"
                    title="<%= CRMCasesResource.EditCase %>">
                <%= CRMCasesResource.EditCase %>
            </a>
        </li>
        <li id="openCase" <%= TargetCase.IsClosed ? "" : "style='display:none'"%>>
            <a  class="dropdown-item" onclick="ASC.CRM.CasesFullCardView.changeCaseStatus(0)">
                <%= CRMCasesResource.OpenCase %>
            </a>
        </li>
        <li id="closeCase" <%= TargetCase.IsClosed ? "style='display:none'" : "" %>>
            <a class="dropdown-item" onclick="ASC.CRM.CasesFullCardView.changeCaseStatus(1)">
                <%= CRMCasesResource.CloseCase %>
            </a>
        </li>
    </ul>
</div>