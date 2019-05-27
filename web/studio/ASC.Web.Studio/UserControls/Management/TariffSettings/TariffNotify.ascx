<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffNotify.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffNotify" %>
<%@ Import Namespace="Resources" %>

<% if (Notify != null)
   { %>
<div id="tariffNotifyPanel" class="info-box excl">
    <% if (CanClose)
       { %>
    <div id="tariffNotifyClose" title="<%= Resource.CloseButton %>"></div>
    <% } %>
    <div class="header-base medium bold"><%= Notify.Item1 %></div>
    <div><%= Notify.Item2 %></div>
</div>
<% } %>