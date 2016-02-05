<%@ Import Namespace="ASC.Core.Users" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserEmailControl.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.UserEmailControl" %>

<% if (!String.IsNullOrEmpty(User.Email)) { %>
<div class="field clearFix">
    <span class="field-title mail describe-text"><%=Resources.Resource.Email%>:</span>
    <div id="emailUserProfile" class="field-value">
    <% if (Viewer.IsAdmin() || Viewer.ID == User.ID) {
        if (User.ActivationStatus == EmployeeActivationStatus.Activated) { %>
        <div>
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%=HttpUtility.HtmlEncode(User.Email.ToLower())%>">
                <%=HttpUtility.HtmlEncode(User.Email.ToLower())%>
            </a>
            <% if (User.Status != EmployeeStatus.Terminated && (!User.IsOwner() || Viewer.IsOwner()))
               { %>
            <a class="linkAction baseLinkAction" onclick="EmailOperationManager.ShowEmailChangeWindow('<%=User.Email%>','<%=User.ID%>');return false;">&nbsp;</a>
            <% } %>
        </div>
      <% } else if (User.ActivationStatus == EmployeeActivationStatus.NotActivated) {%>
        <div class="tintMedium">
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%=HttpUtility.HtmlEncode(User.Email.ToLower())%>">
                <%=HttpUtility.HtmlEncode(User.Email.ToLower())%>
            </a>
            <% if (User.Status != EmployeeStatus.Terminated && (!User.IsOwner() || Viewer.IsOwner()))
               { %>
            <a class="linkAction baseLinkAction" onclick="EmailOperationManager.ShowEmailChangeWindow('<%=User.Email%>','<%=User.ID%>');return false;">&nbsp;</a>
            <% } %>
            <div class="caption emailWarning"><%=Resources.Resource.EmailIsNotConfirmed%>
                <a id="linkNotActivatedActivation" href="javascript:void(0);" class="activate">
                    <%=Resources.Resource.ActivateEmailAgain%>
                </a>
            </div>
        </div>
      <% } else if (User.ActivationStatus == EmployeeActivationStatus.Pending) { %>
        <div class="tintMedium">
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%=HttpUtility.HtmlEncode(User.Email.ToLower())%>">
                <%=HttpUtility.HtmlEncode(User.Email.ToLower())%>
            </a>
            <% if (User.Status != EmployeeStatus.Terminated && (!User.IsOwner() || Viewer.IsOwner()))
               { %>
            <a class="linkAction baseLinkAction" onclick="EmailOperationManager.ShowEmailChangeWindow('<%=User.Email%>','<%=User.ID%>');return false;">&nbsp;</a>
            <% } %>
            <div class="caption emailWarning"><%=Resources.Resource.PendingTitle%> 
            <% if (User.Status != EmployeeStatus.Terminated) { %>
                <a id="linkPendingActivation" href="javascript:void(0);" class="activate">
                    <%=Resources.Resource.SendInviteAgain%>
                </a>
            <% } %>
            </div>
        </div>
      <% } %>
    <% } else {%>
        <div>
            <a class="mail" <%= RenderMailLinkAttribute() %> title="<%=HttpUtility.HtmlEncode(User.Email.ToLower())%>">
                <%=HttpUtility.HtmlEncode(User.Email.ToLower())%>
            </a>
        </div>
    <% } %>
    </div>
</div>
<% } %>