<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProfileActions.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfileActions" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="Resources" %>

<% if (HasActions)
   { %>
<div id="userMenu" class="menu-small"></div>
        
<div id="actionMenu" class="studio-action-panel" data-id="<%= ProfileHelper.UserInfo.ID %>" data-email="<%= ProfileHelper.UserInfo.Email %>" 
    data-admin="<%= IsAdmin.ToString().ToLower()%>" data-name="<%= ProfileHelper.UserInfo.DisplayUserName() %>" data-visitor="<%= ProfileHelper.UserInfo.IsVisitor() %>">
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
       if (Actions.AllowEdit && ProfileHelper.UserInfo.ActivationStatus != EmployeeActivationStatus.Activated && !ProfileHelper.UserInfo.IsLDAP() && !ProfileHelper.UserInfo.IsSSO())
           { %>
        <li class="email-activate <%= (ProfileHelper.UserInfo.Status != EmployeeStatus.Terminated) ? "" :  "display-none"%>">
            <a title="<%= Resource.ActivateEmailAgain %>"
                class="dropdown-item">
                <%= Resource.ActivateEmailAgain %>
            </a>
        </li>
        <% }
           if (Actions.AllowEdit && !MobileDetector.IsMobile)
           { %>
        <li class="edit-photo <%= UserHasAvatar ? "" : "display-none" %>">
            <a class="dropdown-item"
                title="<%= Resource.EditThumbnailPhoto %>">
                <%= Resource.EditThumbnailPhoto %>
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
           if (MyStaff && !ProfileHelper.UserInfo.IsOwner() && !CoreContext.Configuration.Personal && !ProfileHelper.UserInfo.IsLDAP())
           { %>
        <li class="delete-user">
            <a class="dropdown-item" title="<%= Resource.DeleteProfileButton %>">
                <%= Resource.DeleteProfileButton %>
            </a>
        </li>
        <% }
           if (Actions.AllowAddOrDelete)
           { %>
        <li class="delete-self <%= (ProfileHelper.UserInfo.Status == EmployeeStatus.Terminated) ? "" :  "display-none"%>">
            <a class="dropdown-item" title="<%= Resource.DeleteSelfProfile %>">
                <%= Resource.DeleteSelfProfile %>
            </a>
        </li>
        <% } %>
    </ul>
</div>

<% if (Actions.AllowAddOrDelete)
   { %>
<asp:PlaceHolder ID="_phConfirmationDeleteUser" runat="server"></asp:PlaceHolder>
    <% } %>
<% } %>