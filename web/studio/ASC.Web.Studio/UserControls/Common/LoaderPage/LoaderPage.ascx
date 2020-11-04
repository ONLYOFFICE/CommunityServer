<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoaderPage.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.LoaderPage.LoaderPage" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="loader-page <%= Static ? "static" : String.Empty  %>">
    <% if (DefaultSettings) { %>
    <div class="romb blue"></div>
    <div class="romb green"></div>
    <div class="romb red"></div>
    <% } else { %>
    <img src="<%= CommonLinkUtility.GetFullAbsolutePath(WebImageSupplier.GetAbsoluteWebPath("loader.svg").ToLower()) %>" alt=""/>
    <% } %>
    <div class="text"><%= Resource.LoadingProcessing %></div>
</div>
