<%@ Import Namespace="ASC.Web.Sample.Resources" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavigationSidePanel.ascx.cs" Inherits="ASC.Web.Sample.Controls.NavigationSidePanel" %>

<div class="page-menu">
    <ul class="menu-list">
        <li class="menu-item  none-sub-list<% if (CurrentPage == "default") { %> active<% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Default.aspx" title="<%= SampleResource.CreateModule %>">
                <span class="menu-item-icon sample"></span>
                <span class="menu-item-label inner-text"><%= SampleResource.CreateModule %></span>
            </a>
            <span id="feed-new-cases-count" class="feed-new-count"></span>
        </li>
        <li class="menu-item  none-sub-list<% if (CurrentPage == "styles") { %> active<% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Styles.aspx" title="<%= SampleResource.Styles %>">
                <span class="menu-item-icon sample"></span>
                <span class="menu-item-label inner-text"><%= SampleResource.Styles %></span>
            </a>
            <span id="Span1" class="feed-new-count"></span>
        </li>
        <li class="menu-item  none-sub-list <% if (CurrentPage == "elements") { %> active <% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Elements.aspx" title="<%= SampleResource.Elements %>">
                <span class="menu-item-icon sample"></span>
                <span class="menu-item-label inner-text"><%= SampleResource.Elements %></span>
            </a>
        </li>
        <li class="menu-item  none-sub-list <% if (CurrentPage == "usercontrols") { %> active <% } %>">
            <a class="menu-item-label outer-text text-overflow" href="UserControls.aspx" title="<%= SampleResource.Controls %>">
                <span class="menu-item-icon sample"></span>
                <span class="menu-item-label inner-text"><%= SampleResource.Controls %></span>
            </a>
        </li>
        <li class="menu-item  none-sub-list <% if (CurrentPage == "database") { %> active <% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Database.aspx" title="<%= SampleResource.Database %>">
                <span class="menu-item-icon sample"></span>
                <span class="menu-item-label inner-text"><%= SampleResource.Database %></span>
            </a>
        </li>
        <li class="menu-item  none-sub-list <% if (CurrentPage == "api") { %> active <% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Api.aspx" title="<%= SampleResource.Api %>">
                <span class="menu-item-icon sample"></span>
                <span class="menu-item-label inner-text"><%= SampleResource.Api %></span>
            </a>
        </li>
        <li class="menu-item  none-sub-list <% if (CurrentPage == "help") { %> active <% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Help.aspx" title="<%= SampleResource.Help %>">
                <span class="menu-item-icon sample"></span>
                <span class="menu-item-label inner-text"><%= SampleResource.Help %></span>
            </a>
        </li>
    </ul>

    <a href="https://github.com/ONLYOFFICE/CommunityServer-CustomModules" target="_blank" class="banner-link gray-text">
        <svg class="octicon octicon-mark-github v-align-middle" height="32" viewBox="0 0 16 16" width="32">
            <path fill-rule="evenodd" d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0 0 16 8c0-4.42-3.58-8-8-8z"></path>
        </svg>
        <span><%= SampleResource.ForkGitHub %></span>
    </a>
    
    <% if(!string.IsNullOrEmpty(ExceptionMessage)) { %>
    <div class="red-text"><%= ExceptionMessage %></div>
    <% } %>
</div>