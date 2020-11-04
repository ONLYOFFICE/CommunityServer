<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProfileActions.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfileActions" %>
<%@ Import Namespace="ASC.ActiveDirectory.Base.Settings" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="Resources" %>

<% if (HasActions)
   { %>
<div id="userMenu" class="menu-small"></div>
        
<div id="actionMenu" class="studio-action-panel"
    data-id="<%= ProfileHelper.UserInfo.ID %>"
    data-email="<%= ProfileHelper.UserInfo.Email.HtmlEncode() %>" 
    data-admin="<%= IsAdmin.ToString().ToLower()%>"
    data-displayname="<%= ProfileHelper.UserInfo.DisplayUserName() %>"
    data-username="<%= ProfileHelper.UserInfo.UserName %>"
    data-visitor="<%= ProfileHelper.UserInfo.IsVisitor() %>">
    <ul class="dropdown-content">
        <% if (Actions.AllowEdit)
           { %>
        <li class="edit-user <%= (ProfileHelper.UserInfo.Status != EmployeeStatus.Terminated) ? "" :  "display-none"%>">
            <a title="<%= Resource.EditButton %>" href="<%= ProfileEditLink %>"
                class="dropdown-item">
                <%= Resource.EditButton %>
            </a>
        </li>
        <% }
           if (Actions.AllowAddOrDelete)
           { %>
        <li class="enable-user <%= (ProfileHelper.UserInfo.Status == EmployeeStatus.Terminated) ? "" :  "display-none"%>">
            <a class="dropdown-item"
                title="<%= CustomNamingPeople.Substitute<Resource>("EnableUserHelp").HtmlEncode() %>">
                <%= Resource.EnableUserButton %>
            </a>
        </li>
        <% }
       if (Actions.AllowEdit && ProfileHelper.UserInfo.ActivationStatus == EmployeeActivationStatus.Activated && !ProfileHelper.UserInfo.IsLDAP() && !ProfileHelper.UserInfo.IsSSO())
           { %>
        <li class="psw-change <%= (ProfileHelper.UserInfo.Status != EmployeeStatus.Terminated) ? "" :  "display-none"%>">
            <a title="<%= Resource.PasswordChangeButton %>"
                class="dropdown-item">
                <%= Resource.PasswordChangeButton %>
            </a>
        </li>
        <% }
       if (Actions.AllowEdit && ProfileHelper.UserInfo.ActivationStatus == EmployeeActivationStatus.Activated && !ProfileHelper.UserInfo.IsLDAP() && !ProfileHelper.UserInfo.IsSSO())
           { %>
        <li class="email-change <%= (ProfileHelper.UserInfo.Status != EmployeeStatus.Terminated) ? "" :  "display-none"%>">
            <a title="<%= Resource.EmailChangeButton %>"
                class="dropdown-item">
                <%= Resource.EmailChangeButton %>
            </a>
        </li>
        <% }
       if (Actions.AllowEdit && ProfileHelper.UserInfo.ActivationStatus != EmployeeActivationStatus.Activated && !ProfileHelper.UserInfo.IsSSO())
           { %>
        <li class="email-activate <%= (ProfileHelper.UserInfo.Status != EmployeeStatus.Terminated) ? "" :  "display-none"%>">
            <a title="<%= Resource.ActivateEmailAgain %>"
                class="dropdown-item">
                <%= Resource.ActivateEmailAgain %>
            </a>
        </li>
        <% }
       if (Actions.AllowEdit && !MobileDetector.IsMobile && (!ProfileHelper.UserInfo.IsLDAP() || (ProfileHelper.UserInfo.IsLDAP() && ProfileHelper.UserInfo.HasAvatar())))
           { %>
        <li class="edit-photo">
            <a class="dropdown-item"
                title="<%= Resource.EditPhoto %>">
                <%= Resource.EditPhoto %>
            </a> 
        </li>
        <% }
           if (!MyStaff && Actions.AllowAddOrDelete && !ProfileHelper.UserInfo.IsLDAP())
           { %>
        <li class="disable-user <%= (ProfileHelper.UserInfo.Status != EmployeeStatus.Terminated) ? "" :  "display-none"%>">
            <a class="dropdown-item"
                title="<%= CustomNamingPeople.Substitute<Resource>("DisableUserHelp").HtmlEncode() %>">
                <%= Resource.DisableUserButton %>
            </a>
        </li>
        <% }
           if (MyStaff && !ProfileHelper.UserInfo.IsOwner() && !ProfileHelper.UserInfo.IsLDAP())
           { %>
        <li class="delete-user">
            <a class="dropdown-item" title="<%= Resource.DeleteProfileButton %>">
                <%= Resource.DeleteProfileButton %>
            </a>
        </li>
        <% }
           if (Actions.AllowAddOrDelete && ProfileHelper.UserInfo.Status == EmployeeStatus.Terminated)
           { %>
        <li class="reassign-data">
            <a class="dropdown-item" title="<%= Resource.ReassignData %>" href="<%= ReassignDataLink %>">
                <%= Resource.ReassignData %>
            </a>
        </li>
        <li class="remove-data">
            <a class="dropdown-item" title="<%= Resource.RemoveData %>" href="<%= ReassignDataLink + "&remove=true" %>">
                <%= Resource.RemoveData %>
            </a>
        </li>
        <li class="delete-self">
            <a class="dropdown-item" title="<%= Resource.DeleteSelfProfile %>">
                <%= Resource.DeleteSelfProfile %>
            </a>
        </li>
        <% } 
           if (MyStaff && CoreContext.Configuration.Personal)
           { %>
        <li class="subscribe-tips">
            <a class="dropdown-item" title="<%= SubscribeBtnText %>"><%= SubscribeBtnText %></a>
        </li>
        <% } %>
    </ul>
</div>

<% if (Actions.AllowAddOrDelete)
   { %>
<asp:PlaceHolder ID="_phConfirmationDeleteUser" runat="server"></asp:PlaceHolder>
    <% } %>
<% } %>