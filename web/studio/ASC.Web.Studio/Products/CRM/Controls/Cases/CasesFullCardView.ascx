<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CasesFullCardView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Cases.CasesFullCardView" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>

<div id="caseProfile" class="clearFix">
    <table border="0" cellpadding="0" cellspacing="0" class="crm-detailsTable">
        <colgroup>
            <col style="width: 50px;"/>
            <col style="width: 22px;"/>
            <col/>
        </colgroup>
        <tbody>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMCasesResource.CaseStatus%>:</td>
                <td></td>
                <td>
                    <span id="caseStatusSwitcher">
                        <%= TargetCase.IsClosed ? CRMJSResource.CaseStatusClosed : CRMJSResource.CaseStatusOpened%>
                    </span>
                </td>
            </tr>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMCommonResource.Tags %>:</td>
                <td></td>
                <td id="caseTagsTD"></td>
            </tr>
            <% if (CRMSecurity.IsPrivate(TargetCase)) %>
            <% { %>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelAccessListLable").HtmlEncode() %>:</td>
                <td></td>
                <td class="caseAccessList"></td>
            </tr>
            <% } %>
        </tbody>
    </table>

    <table border="0" cellpadding="0" cellspacing="0" class="crm-detailsTable" id="caseHistoryTable">
        <colgroup>
            <col style="width: 50px;"/>
            <col style="width: 22px;"/>
            <col/>
        </colgroup>
        <tbody>
            <tr class="headerToggleBlock" data-toggleId="-2">
                <td colspan="3" style="white-space:nowrap;">
                    <span class="headerToggle header-base"><%= CRMCommonResource.History %></span>
                    <span class="openBlockLink"><%= CRMCommonResource.Show %></span>
                    <span class="closeBlockLink"><%= CRMCommonResource.Hide %></span>
                </td>
            </tr>
            <tr>
                <td colspan="3" style="padding-top:10px;">
                    <asp:PlaceHolder runat="server" ID="_phHistoryView"></asp:PlaceHolder>
                </td>
            </tr>
        </tbody>
    </table>
</div>