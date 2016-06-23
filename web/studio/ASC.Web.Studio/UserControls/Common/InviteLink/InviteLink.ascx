<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InviteLink.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.InviteLink.InviteLink" %>
<%@ Import Namespace="Resources" %>


<li id="menuInviteUsers" class="menu-item none-sub-list add-block-always" style="display:none;">
    <div class="category-wrapper">
        <a id="menuInviteUsersBtn" class="menu-item-label outer-text text-overflow">
            <span class="menu-item-icon inviteusers"></span>
            <span class="menu-item-label inner-text">
                <%= Resource.InviteUsersToPortalLink %>
            </span>
        </a>
    </div>
</li>