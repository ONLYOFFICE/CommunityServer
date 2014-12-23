<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TipsSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.TipsSettings.TipsSettings" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>

<div id="tips-settings-box" class="user-block">
    <div class="tabs-section">
        <span class="header-base"><%= PeopleResource.LblTips %></span>
        <span class="toggle-button" data-switcher="1" data-showtext="<%= Resources.Resource.Show %>" data-hidetext="<%= Resources.Resource.Hide %>">
            <%= Resources.Resource.Show %>
        </span>
    </div>
    <div class="tabs-content">
        <span class="on_off_button <%= ShowTips ? "on" : "off" %>"></span>
        <%= Resources.Resource.ShowingTipsSettingMsg %>
    </div>
</div>