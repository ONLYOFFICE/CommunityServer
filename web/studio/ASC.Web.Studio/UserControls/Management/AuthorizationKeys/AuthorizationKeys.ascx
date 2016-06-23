<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthorizationKeys.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.AuthorizationKeys" %>
<%@ Import Namespace="Resources" %>

<div id="authKeysContainer">
    <div class="header-base"><%= Resource.AuthorizationKeys %></div>

    <p class="auth-service-text"><%: Resource.AuthorizationKeysText %> <br />
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/tipstricks/authorization-keys.aspx" %>" target="_blank"><% = Resource.LearnMore %></a>
        <% } %>
    </p>

    <div class="auth-service-block clearFix">
        <% foreach (var service in AuthServiceList)
           { %>
            <div class="auth-service-item">
                <span class="auth-service-name"><%= service.Title %></span>
                <% if (service.Id != null)
                   { %>
                    <input id="<%= service.Id.Name %>" type="text" class="auth-service-id textEdit" placeholder="<%= service.Id.Title %>" value="<%= service.Id.Value %>"/>
                <% } %>
                <% if (service.Key != null)
                   { %>
                    <input id="<%= service.Key.Name %>" type="text" class="auth-service-key textEdit" placeholder="<%= service.Key.Title %>" value="<%= service.Key.Value %>"/>
                <% } %>
            </div>
        <% } %>
    </div>
    
    <div class="middle-button-container">
        <a id="authKeysButtonSave" class="button blue" href="javascript:void(0);"><%= Resource.SaveButton %></a>
    </div>
</div>