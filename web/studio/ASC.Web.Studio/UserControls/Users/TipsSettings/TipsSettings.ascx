<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TipsSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.TipsSettings.TipsSettings" %>
<%@ Import Namespace="ASC.Web.People.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div id="tips-settings-box" class="user-block">
    <div class="tabs-section">
        <span class="header-base"><%= PeopleResource.LblTips %></span>
        <span class="toggle-button" data-switcher="1" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Show %>
        </span>
    </div>
    <div class="tabs-content">
        <span class="on_off_button <%= ShowTips ? "on" : "off" %>"></span>
        <%= Resource.ShowingTipsSettingMsg %>
    </div>
</div>