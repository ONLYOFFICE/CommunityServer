<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DealFullCardView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Deals.DealFullCardView" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.CRM.Core.Entities" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>

<div id="dealProfile" class="clearFix">
    <table border="0" cellpadding="0" cellspacing="0" class="crm-detailsTable">
        <colgroup>
            <col style="width: 50px;"/>
            <col style="width: 22px;"/>
            <col/>
        </colgroup>
        <tbody>
            <% if (TargetDealContact != null) %>
            <% { %>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMDealResource.ClientDeal%>:</td>
                <td></td>
                <td><%= TargetDealContact.RenderLinkForCard() %></td>
            </tr>
            <% } %>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMDealResource.CurrentDealMilestone%>:</td>
                <td></td>
                <td>
                    <span id="dealMilestoneSwitcher" class="baseLinkAction linkMedium crm-withArrowDown"><%= DaoFactory.DealMilestoneDao.GetByID(TargetDeal.DealMilestoneID).Title.HtmlEncode() %></span>
                </td>
            </tr>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMDealResource.ProbabilityOfWinning %>:</td>
                <td></td>
                <td>
                    <div id="dealMilestoneProbability"><%= TargetDeal.DealMilestoneProbability%>%</div>
                </td>
            </tr>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMDealResource.ExpectedValue %>:</td>
                <td></td>
                <td><%=GetExpectedValueStr()%></td>
            </tr>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMJSResource.ExpectedCloseDate %>:</td>
                <td></td>
                <td><%= GetExpectedCloseDateStr()%></td>
            </tr>
            <tr <%= TargetDeal.ActualCloseDate == DateTime.MinValue ? "class=\"display-none\"" : "" %>>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMJSResource.ActualCloseDate%>:</td>
                <td></td>
                <td id="closeDealDate"><%= GetActualCloseDateStr()%></td>
            </tr>

            <% if (TargetDeal.ResponsibleID != Guid.Empty) %>
            <% { %>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%=CRMDealResource.ResponsibleDeal%>:</td>
                <td></td>
                <td><%= ASC.Core.CoreContext.UserManager.GetUsers(TargetDeal.ResponsibleID).DisplayUserName()%></td>
            </tr>
            <% } %>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CRMCommonResource.Tags %>:</td>
                <td></td>
                <td id="dealTagsTD"></td>
            </tr>
            <% if (CRMSecurity.IsPrivate(TargetDeal)) %>
            <% { %>
            <tr>
                <td class="describe-text" style="white-space:nowrap;"><%= CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelAccessListLable").HtmlEncode() %>:</td>
                <td></td>
                <td class="dealAccessList"></td>
            </tr>
            <% } %>
        </tbody>
    </table>

    <% if (!String.IsNullOrEmpty(TargetDeal.Description)) %>
    <% { %>
    <table border="0" cellpadding="0" cellspacing="0" class="crm-detailsTable">
        <colgroup>
            <col style="width: 50px;"/>
            <col style="width: 22px;"/>
            <col/>
        </colgroup>
        <tbody>
            <tr class="headerToggleBlock" data-toggleId="-1">
                <td colspan="3" style="white-space:nowrap;">
                    <span class="headerToggle header-base"><%= CRMDealResource.DescriptionDeal %></span>
                    <span class="openBlockLink"><%= CRMCommonResource.Show %></span>
                    <span class="closeBlockLink"><%= CRMCommonResource.Hide %></span>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <%= TargetDeal.Description.HtmlEncode().Replace("\n", "<br/>").Replace("  ","&nbsp;") %>
                </td>
            </tr>
        </tbody>
    </table>
    <% } %>

    <table border="0" cellpadding="0" cellspacing="0" class="crm-detailsTable" id="dealHistoryTable">
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

<div id="dealMilestoneDropDown" class="studio-action-panel">
    <ul class="dropdown-content">
        <% foreach (var dm in AllDealMilestones) { %>
            <li>
                <a class="dropdown-item" onclick="ASC.CRM.DealFullCardView.changeDealMilestone(<%= TargetDeal.ID %>, <%= dm.ID %>);">
                    <%= dm.Title.HtmlEncode() %>
                </a>
            </li>
        <% } %>
    </ul>
</div>