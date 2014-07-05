<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccessRightsProductItem.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.AccessRightsProductItem" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>

<% if (!ProductItem.CanNotBeDisabled) %>
<% { %>
<div class="tabs-section <%= ProductItem.Disabled ? "accessRights-disabledText" : "" %>">
    <span class="header-base">
        <img src="<%= ProductItem.Disabled ? ProductItem.DisabledIconUrl : ProductItem.IconUrl %>" align="absmiddle" />
        <span><%= ProductItem.Name %></span>
    </span> 
    <span id="switcherAccessRights_<%=ProductItem.ItemName %>" data-id="<%=ProductItem.ItemName%>" class="toggle-button"
            data-switcher="0" data-showtext="<%=Resources.Resource.Show%>" data-hidetext="<%=Resources.Resource.Hide%>">
            <%=Resources.Resource.Hide%>
    </span>
</div>

<div id="accessRightsContainer_<%=ProductItem.ItemName%>" data-id="<%=ProductItem.ID%>" class="accessRights-content">
    <div <%= ProductItem.Disabled ? "style='display:none'" : "" %>>
        <table width="100%" cellspacing="0" cellpadding="0">
            <tbody>
                <tr>
                    <td width="390px" valign="top">
                        <div><%= String.Format(Resources.Resource.AccessRightsAccessToProduct, ProductItem.Name) %>:</div>
                        <div class="accessRightsItems">
                            <input type="radio" id="all_<%= ProductItem.ID %>" name="radioList_<%= ProductItem.ID %>"
                             onclick="ASC.Settings.AccessRights.changeAccessType(this,'<%= ProductItem.ItemName %>')">
                            <label for="all_<%= ProductItem.ID %>"><%= Resources.Resource.AccessRightsAllUsers %></label>
                        </div>
                        <div class="accessRightsItems">
                            <input type="radio" id="fromList_<%= ProductItem.ID %>" name="radioList_<%= ProductItem.ID %>"
                             onclick="ASC.Settings.AccessRights.changeAccessType(this,'<%= ProductItem.ItemName %>')">
                            <label for="fromList_<%= ProductItem.ID %>"><%= Resources.Resource.AccessRightsUsersFromList %></label>
                        </div>
                    </td>
                    <td valign="top">
                        <% if (ProductItem.UserOpportunities != null && ProductItem.UserOpportunities.Count > 0)%>
                        <% { %>
                        <div><%= ProductItem.UserOpportunitiesLabel %>:</div>
                        <% foreach (var item in ProductItem.UserOpportunities)%>
                        <% { %>
                        <div class="simple-marker-list"><%= item %>;</div>
                        <% } %>
                        <% } %>
                    </td>
                </tr>
            </tbody>
        </table>
        <div id="selectorContent_<%= ProductItem.ItemName %>" class="accessRightProductBlock">
            <div id="emptyUserListLabel_<%= ProductItem.ItemName %>" class="describe-text accessRightsEmptyUserList">
                <%= Resources.Resource.AccessRightsEmptyUserList %>
            </div>
            <div id="selectedUsers_<%= ProductItem.ItemName %>" class="clearFix">
                <% if (ProductItem.SelectedUsers.Count > 0)%>
                <% { %>
                    <% foreach (var user in ProductItem.SelectedUsers)%>
                    <% { %>
                        <div class="accessRights-selectedItem" id="selectedUser_<%= ProductItem.ItemName %>_<%= user.ID %>">
                            <img src="<%= PeopleImgSrc %>">
                            <img src="<%= TrashImgSrc %>" id="deleteSelectedUserImg_<%= ProductItem.ItemName %>_<%= user.ID %>"
                                title="<%= Resources.Resource.DeleteButton %>" class="display-none">
                            <%= user.DisplayUserName() %>
                        </div>
                    <% } %>
                <% } %>
            </div>
            <div id="selectedGroups_<%= ProductItem.ItemName %>" class="clearFix">
                <% if (ProductItem.SelectedGroups.Count > 0)%>
                <% { %>
                    <% foreach (var group in ProductItem.SelectedGroups)%>
                    <% { %>
                        <div class="accessRights-selectedItem" id="selectedGroup_<%= ProductItem.ItemName %>_<%= group.ID %>">
                            <img src="<%= GroupImgSrc %>">
                            <img src="<%= TrashImgSrc %>" id="deleteSelectedGroupImg_<%= ProductItem.ItemName %>_<%= group.ID %>"
                                title="<%= Resources.Resource.DeleteButton %>" class="display-none">
                            <%= group.Name.HtmlEncode() %>
                        </div>
                    <% } %>
                <% } %>
            </div>
            <div class="accessRights-selectorContent">
                <div id="userSelector_<%= ProductItem.ItemName %>" class="link dotline plus"><%= CustomNamingPeople.Substitute<Resources.Resource>("AccessRightsAddUser") %></div>
                <div id="groupSelector_<%= ProductItem.ItemName %>" class="link dotline plus"><%= CustomNamingPeople.Substitute<Resources.Resource>("AccessRightsAddGroup") %></div>
            </div>
        </div>
    </div>
    <div class="accessRights-disabledText" <%= ProductItem.Disabled ? "" : "style='display:none'" %>>
        <%= String.Format(Resources.Resource.AccessRightsDisabledProduct, ProductItem.Name) %>
    </div>
</div>
<% } %>