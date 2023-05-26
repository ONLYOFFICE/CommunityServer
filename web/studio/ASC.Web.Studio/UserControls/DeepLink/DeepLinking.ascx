<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DeepLinking.ascx.cs" Inherits="ASC.Web.Studio.UserControls.DeepLinking" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div id="deepLinkForm" class="deep-link-form">
    <% if (string.IsNullOrEmpty(ErrorMsg)) %>
    <% { %>
    <div class="file-description">
        <div class="file">
            <div class="thumb-file" title="<%= FileTitle %>"></div>
            <span class="name"><%= FileName %></span><span class="file-extension"><%=FileExtension%></span>
        </div>
        <span class="description"><%= Resource.DeepLinkDescription %></span>
    </div>
    <label id="remember" for="rememberSelector" class="checkbox">
        <input id="rememberSelector" type="checkbox" />
        <span><%= Resource.DeepLinkRemember %></span>
    </label>
    <div class="buttonContainer">
        <a id="browserButton" class="button white big signIn"><%= Resource.DeepLinkBrowserBtn %></a>
        <a id="appButton" class="button blue big signIn" onclick="" <%if (!String.IsNullOrEmpty(DeepLinkUrl))
            { %>href="<%=DeepLinkUrl%>"
            <%} %>><%= Resource.DeepLinkAppBtn %></a>
    </div>
    <% } else { %>
    <div class="exclamation"><%: ErrorMsg %></div>
    <div class="buttonContainer">
        <a class="button white big" href="/"><%= Resource.DeepLinkGoToPortalBtn %></a>
    </div>
    <% } %>
</div>
