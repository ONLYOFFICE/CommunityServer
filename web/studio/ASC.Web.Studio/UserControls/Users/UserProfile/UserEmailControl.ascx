﻿<%@ Import Namespace="ASC.Core.Users" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserEmailControl.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.UserEmailControl" %>

<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<% if (!String.IsNullOrEmpty(User.Email)) { %>
<div class="field clearFix">
    <div class="field-title mail describe-text">
        <%= Resource.Email%>:
    </div>
    <div id="emailUserProfile" class="field-value">
    <% if (IsAdmin || Viewer.ID == User.ID) {
        if (User.ActivationStatus == EmployeeActivationStatus.Activated) { %>
        <div>
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%= User.Email.HtmlEncode() %>">
                <%= User.Email.HtmlEncode() %>
            </a>
           <% if (Viewer.ID != User.ID)
               { %>
            <a class="copyEmailAction" onclick="ASC.EmailOperationManager.copyEmailToClipboard('<%= User.Email.Replace("'", "\\'").HtmlEncode() %>', 'emailUserProfile');">&nbsp;</a>
            <% } %>
             <% if (User.Status != EmployeeStatus.Terminated && (!User.IsOwner() || Viewer.IsOwner()) && !User.IsLDAP() && !User.IsSSO())
               { %>
            <a class="linkAction baseLinkAction" onclick="ASC.EmailOperationManager.showEmailChangeWindow('<%= User.Email.Replace("'", "\\'").HtmlEncode() %>', '<%=User.ID%>');return false;">&nbsp;</a>
            <% } %>
        </div>
      <% } else if (User.ActivationStatus == EmployeeActivationStatus.NotActivated) {%>
        <div class="tintMedium">
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%= User.Email.HtmlEncode() %>">
                <%= User.Email.HtmlEncode() %>
            </a>
            <% if (User.Status != EmployeeStatus.Terminated && (!User.IsOwner() || Viewer.IsOwner()) && !User.IsLDAP() && !User.IsSSO())
               { %>
            <a class="linkAction baseLinkAction" onclick="ASC.EmailOperationManager.showEmailChangeWindow('<%= User.Email.Replace("'", "\\'").HtmlEncode() %>', '<%=User.ID%>');return false;">&nbsp;</a>
            <% } %>
            <div class="caption emailWarning"><%=Resource.EmailIsNotConfirmed%>
                <% if (User.Status != EmployeeStatus.Terminated) { %>
                <a id="linkNotActivatedActivation" href="javascript:void(0);" class="activate">
                    <%=Resource.ActivateEmailAgain%>
                </a>
                <% } %>
            </div>
        </div>
      <% } else if (User.ActivationStatus == EmployeeActivationStatus.Pending) { %>
        <div class="tintMedium">
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%= User.Email.HtmlEncode() %>">
                <%= User.Email.HtmlEncode() %>
            </a>
            <% if (User.Status != EmployeeStatus.Terminated && (!User.IsOwner() || Viewer.IsOwner()) && !User.IsLDAP() && !User.IsSSO())
               { %>
                <% if (Viewer.ID == User.ID) %>
                <% { %>
                <a class="linkAction baseLinkAction" onclick="ASC.EmailOperationManager.showEmailChangeWindow('<%= User.Email.Replace("'", "\\'").HtmlEncode() %>', '<%=User.ID%>');return false;">&nbsp;</a>
                <% } else { %>
                <a class="linkAction baseLinkAction" id="linkPendingEmailChange">&nbsp;</a>
                <% } %>
            <% } %>
            <div class="caption emailWarning"><%=Resource.PendingTitle%> 
                <% if (User.Status != EmployeeStatus.Terminated) { %>
                <a id="linkPendingActivation" href="javascript:void(0);" class="activate">
                    <%=Resource.SendInviteAgain%>
                </a>
                <% } %>
            </div>
        </div>
      <% } else if (User.ActivationStatus.HasFlag(EmployeeActivationStatus.AutoGenerated)) { %>
        <div class="tintMedium">
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%= User.Email.HtmlEncode() %>">
                <%= User.Email.HtmlEncode() %>
            </a>
            <% if (User.Status != EmployeeStatus.Terminated && (!User.IsOwner() || Viewer.IsOwner()) && !User.IsSSO())
               { %>
            <a class="linkAction baseLinkAction" onclick="ASC.EmailOperationManager.showEmailChangeWindow('<%= User.Email.Replace("'", "\\'").HtmlEncode() %>', '<%=User.ID%>');return false;">&nbsp;</a>
            <% } %>
            <% if (!User.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated)) { %>
                <div class="caption emailWarning"><%=Resource.EmailIsNotConfirmed%>
                    <a id="linkNotActivatedActivation" href="javascript:void(0);" class="activate">
                        <%=Resource.ActivateEmailAgain%>
                    </a>
                </div>
            <% } %>
        </div>
      <% } %>
    <% } else {%>
        <div>
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%= User.Email.HtmlEncode() %>">
                <%= User.Email.HtmlEncode() %>
            </a>
            <% if (User.ActivationStatus == EmployeeActivationStatus.Activated) { %>
            <a class="copyEmailAction" onclick="ASC.EmailOperationManager.copyEmailToClipboard('<%= User.Email.Replace("'", "\\'").HtmlEncode() %>', 'emailUserProfile');">&nbsp;</a>
            <% } %>
        </div>
    <% } %>
    </div>
</div>
<% } %>