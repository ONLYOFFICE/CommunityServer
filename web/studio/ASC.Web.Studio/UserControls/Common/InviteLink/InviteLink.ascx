<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InviteLink.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.InviteLink.InviteLink" %>

<li id="menuInviteUsers" class="menu-item none-sub-list add-block-always" style="display:none;">
    <div class="category-wrapper">
        <a id="menuInviteUsersBtn" class="menu-item-label outer-text text-overflow" title="<%= LinkText %>">
            <span class="menu-item-icon inviteusers"></span>
            <span class="menu-item-label inner-text"><%= LinkText %></span>
        </a>
    </div>
</li>