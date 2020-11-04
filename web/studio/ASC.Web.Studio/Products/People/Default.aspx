<%@ Assembly Name="ASC.Web.People" %>

<%@ Page Language="C#" MasterPageFile="~/Products/People/Masters/PeopleBaseTemplate.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.People.Default" EnableViewState="false" %>

<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<asp:Content runat="server" ContentPlaceHolderID="TitleContent">
    <div class="clearFix default-title profile-title header-with-menu display-none">
        <span class="header text-overflow"><%= Resource.People %></span>
        <% if (IsAdmin) { %>
        <span class="menu-small" style="display:none;"></span>
        <% } %>
    </div>

    <% if (IsAdmin) { %>
    <div id="actionGroupMenu" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a class="dropdown-item update-group"><%= CustomNamingPeople.Substitute<Resource>("DepEditHeader").HtmlEncode()%></a></li>
            <li><a class="dropdown-item delete-group"><%= PeopleResource.DeleteButton %></a></li>
        </ul>
    </div>
    <% } %>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="FilterContent">
    <div id="filterContainer" class="display-none">
        <div id="peopleFilter"></div>
    </div>

    <% if (IsAdmin) { %>
    <div class="people-content">
        <ul id="peopleHeaderMenu" class="clearFix contentMenu contentMenuDisplayAll">
            <li class="menuAction menuActionSelectAll menuActionSelectLonely">
                <div class="menuActionSelect">
                    <input id="mainSelectAll" type="checkbox" title="<%= PeopleResource.SelectAll %>" onclick="ASC.People.PeopleController.selectAll(this);"/>
                </div>
            </li>
            <li class="menuAction menuChangeType" title="<%= PeopleResource.ChangeType %>">
                <span><%= PeopleResource.ChangeType %></span>
                <div class="down_arrow"></div>
            </li>
            <li class="menuAction menuChangeStatus" title="<%= PeopleResource.ChangeStatus %>">
                <span><%= PeopleResource.ChangeStatus %></span>
                <div class="down_arrow"></div>
            </li>
            <li class="menuAction menuSendInvite" title="<%= PeopleResource.LblSendActivation%>">
                <span><%= PeopleResource.LblSendActivation%></span>
            </li>
            <li class="menuAction menuWriteLetter" title="<%= PeopleResource.WriteButton%>">
                <span><%= PeopleResource.WriteButton%></span>
            </li>
            <li class="menuAction menuRemoveUsers" title="<%= PeopleResource.DeleteBtnHint%>">
                <span><%= PeopleResource.DeleteButton%></span>
            </li>
            <li class="menuAction otherFunctions unlockAction">
                <span>...</span>
                <div id="otherFunctionCnt" class="studio-action-panel other-actions">
                    <ul class="dropdown-content">
                         <li class="menuSendInvite unlockAction" title="<%= PeopleResource.LblSendActivation%>">
                            <a class="dropdown-item"><%= PeopleResource.LblSendActivation%></a>
                        </li>
                        <li class="menuWriteLetter unlockAction" title="<%= PeopleResource.WriteButton%>">
                           <a class="dropdown-item"><%= PeopleResource.WriteButton%></a>
                        </li>
                        <li class="menuRemoveUsers unlockAction" title="<%= PeopleResource.DeleteBtnHint%>">
                            <a class="dropdown-item"><%= PeopleResource.DeleteButton%></a>
                        </li>
                    </ul>
                </div>
            </li>
            
            <li class="menu-action-simple-pagenav">
            </li>
            <li class="menu-action-checked-count">
                <span></span>
                <a id="mainDeselectAll" class="link dotline small">
                    <%= PeopleResource.DeselectAll %>
                </a>
            </li>
            <li class="menu-action-on-top">
                <a class="on-top-link">
                    <%= PeopleResource.OnTop%>
                </a>
            </li>
        </ul>
        <div class="header-menu-spacer" style="display: none;"></div>
    </div>
    <% } %>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="PeoplePageContent">

    <asp:PlaceHolder ID="loaderHolder" runat="server"/>

    <div id="peopleContent" class="people-content">
        <div class="content-list_scrollable webkit-scrollbar">
            <table id="peopleData" class="table-list height48" cellpadding="7" cellspacing="0">
                <tbody>
                </tbody>
            </table>
        </div>
    </div>

    <div id="peopleActionMenu" class="studio-action-panel" data-canedit="<%= Actions.AllowEdit %>" data-candel="<%= Actions.AllowAddOrDelete %>"></div>

    <asp:PlaceHolder ID="userEmailChange" runat="server"/>
    <asp:PlaceHolder ID="userPwdChange" runat="server"/>
    <asp:PlaceHolder ID="userConfirmationDelete" runat="server"/>

    <% if (IsAdmin) { %>

    <div id="changeTypePanel" class="studio-action-panel group-actions">
        <ul class="dropdown-content">
            <li>
                <a class="dropdown-item" data-type="<%= (int)EmployeeType.User %>">
                    <%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %>
                </a>
            </li>
            <li>
                <a class="dropdown-item" data-type="<%= (int)EmployeeType.Visitor %>">
                    <%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %>
                </a>
            </li>
        </ul>
    </div>

    <div id="changeTypeDialogBody" class="display-none">
        <div id="userTypeInfo">
            <% if (DisplayPayments)
               { %>
            <div class="block-cnt-splitter">
                <span class="tariff-limit"></span>
                <%= PeopleResource.ChangeTypeDialogConstraint %>&nbsp;
                <% if (IsFreeTariff && !string.IsNullOrEmpty(HelpLink)) { %>
                <%= String.Format(PeopleResource.ReadAboutNonProfit, "<a class='link underline' href='" + HelpLink + "/gettingstarted/configuration.aspx#PublicPortals' target='_blank'>", "</a>") %>
                <% } %>
            </div>
            <% } %>
            <div class="block-cnt-splitter action-info">
                <%= CustomNamingPeople.Substitute<PeopleResource>("ChangeTypeDialogToUser").HtmlEncode() %>
                <br/>
                <%= PeopleResource.ChangeTypeDialogRestriction %>
            </div>
        </div>
        <div id="visitorTypeInfo">
            <div class="block-cnt-splitter">
                <%= CustomNamingPeople.Substitute<PeopleResource>("ChangeTypeDialogToGuest").HtmlEncode() %>
                <br/>
                <%= PeopleResource.ChangeTypeDialogRestriction %>
            </div>
        </div>
        <div class="selected-users-info">
            <a class="link dotline showBtn">
                <%= PeopleResource.ShowSelectedUserList %>
            </a>
            <a class="link dotline hideBtn display-none">
                <%= PeopleResource.HideSelectedUserList %>
            </a>
            <div class="user-list-for-group-operation display-none">
            </div>
        </div>
        <div class="error-popup display-none"></div>
        <div class="middle-button-container">
            <% if (!DisplayPaymentsFirst)
               { %>
            <a id="changeTypeDialogOk" class="button blue middle"><%= PeopleResource.ChangeType %></a>
            <span class="splitter-buttons"></span>
            <% } %>

            <% if (DisplayPayments)
               { %>
            <a id="changeTypeDialogTariff" class="button <%= DisplayPaymentsFirst ? "blue" : "gray" %> middle" href="<%= TenantExtra.GetTariffPageLink() %>">
                <%= UserControlsCommonResource.UpgradeButton %></a>
            <span class="splitter-buttons"></span>

            <% if (DisplayPaymentsFirst)
               { %>
            <a id="changeTypeDialogOk" class="button gray middle"><%= PeopleResource.ChangeType %></a>
            <span class="splitter-buttons"></span>
            <% } %>

            <% } %>
            <a id="changeTypeDialogCancel" class="button gray middle"><%= PeopleResource.LblCancelButton%></a>
        </div>
    </div>

    <div id="changeStatusPanel" class="studio-action-panel group-actions">
        <ul class="dropdown-content">
             <li>
                <a class="dropdown-item" data-status="<%= (int)EmployeeStatus.Active %>">
                    <%= PeopleResource.LblActive %>
                </a>
            </li>
            <li>
                <a class="dropdown-item" data-status="<%= (int)EmployeeStatus.Terminated %>">
                    <%= PeopleResource.LblTerminated %>
                </a>
            </li>
        </ul>
    </div>
    
    <div id="changeStatusDialogBody" class="display-none">
        <div id="activeStatusInfo">
            <% if (DisplayPayments)
               { %>
            <div class="block-cnt-splitter">
                <span class="tariff-limit"></span>
                <%= PeopleResource.ChangeStatusDialogConstraint %>&nbsp;
                <% if (IsFreeTariff && !string.IsNullOrEmpty(HelpLink)) { %>
                <%= String.Format(PeopleResource.ReadAboutNonProfit, "<a class='link underline' href='" + HelpLink + "/gettingstarted/configuration.aspx#PublicPortals' target='_blank'>", "</a>") %>
                <% } %>
            </div>
            <% } %>
            <div class="block-cnt-splitter action-info">
                <%= PeopleResource.ChangeStatusDialogToActive %>
                <br/>
                <%= PeopleResource.ChangeStatusDialogRestriction %>
            </div>
        </div>
        <div id="terminateStatusInfo" class="display-none">
            <div class="block-cnt-splitter">
                <%= PeopleResource.ChangeStatusDialogToTerminate %>
                <br/>
                <%= PeopleResource.ChangeStatusDialogRestriction %>
            </div>
        </div>
        <div class="selected-users-info">
            <a class="link dotline showBtn">
                <%= PeopleResource.ShowSelectedUserList %>
            </a>
            <a class="link dotline hideBtn display-none">
                <%= PeopleResource.HideSelectedUserList %>
            </a>
            <div class="user-list-for-group-operation display-none">
            </div>
        </div>
        <div class="error-popup display-none"></div>
        <div class="middle-button-container">
            <% if (!DisplayPaymentsFirst)
               { %>
            <a id="changeStatusOkBtn" class="button blue middle"><%= PeopleResource.ChangeStatusButton %></a>
            <span class="splitter-buttons"></span>
            <% } %>

            <% if (DisplayPayments)
               { %>
            <a id="changeStatusTariff" class="button <%= DisplayPaymentsFirst ? "blue" : "gray" %> middle" href="<%= TenantExtra.GetTariffPageLink() %>">
                <%= UserControlsCommonResource.UpgradeButton %></a>
            <span class="splitter-buttons"></span>

            <% if (DisplayPaymentsFirst)
               { %>
            <a id="changeStatusOkBtn" class="button gray middle"><%= PeopleResource.ChangeStatusButton %></a>
            <span class="splitter-buttons"></span>
            <% } %>

            <% } %>
            <a id="changeStatusCancelBtn" class="button gray middle"><%= PeopleResource.LblCancelButton%></a>
        </div>
    </div>

    <% } %>

    <div id="studio_deleteProfileDialogBody" class="display-none" data-header="<%= Resource.DeleteProfileTitle%>">
        <div id="remove_content">
            <div><%= Resource.DeleteProfileInfo%></div>
            <a target="_blank" href="" class="link blue underline email"></a>
        </div>
        <div class="clearFix middle-button-container">
            <a href="javascript:void(0);" class="button blue middle" onclick="SendInstrunctionsToRemoveProfile();"><%= Resource.SendButton %></a>
            <span class="splitter-buttons"></span>
            <a href="javascript:void(0);" class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%= Resource.CloseButton %></a>
        </div>
    </div>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="PagingContent">
    <table id="tableForPeopleNavigation" cellpadding="0" cellspacing="0" border="0" class="people-content display-none">
        <tbody>
            <tr>
                <td>
                    <div id="peoplePageNavigator"></div>
                </td>
                <td style="text-align:right;">
                    <span class="gray-text"><%= PeopleResource.TotalCount %>:&nbsp;</span>
                    <span class="gray-text" id="totalUsers"></span>
                    <span class="page-nav-info">
                        <span class="gray-text"><%= PeopleResource.ShowOnPage %>:&nbsp;</span>
                        <select id="countOfRows" class="top-align">
                            <option value="25">25</option>
                            <option value="50">50</option>
                            <option value="75">75</option>
                            <option value="100">100</option>
                        </select>
                    </span>
                </td>
            </tr>
        </tbody>
    </table>
</asp:Content>
