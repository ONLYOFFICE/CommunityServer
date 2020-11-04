<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewNavigationPanel.ascx.cs"
            Inherits="ASC.Web.Studio.UserControls.Feed.NewNavigationPanel" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<style type="text/css">
    .page-menu .menu-list #feed-all-products-nav {
        margin-left: 10px;
    }

    .page-menu .menu-list #feed-menu-item .menu-sub-list .menu-sub-item {
        padding-left: 16px;
    }
</style>

<div id="feed-page-menu" class="page-menu">
    <ul class="menu-list">
        <li id="feed-menu-item" class="menu-item sub-list active open currentCategory">
            <div class="category-wrapper">
                <a id="feed-all-products-nav" class="menu-item-label outer-text text-overflow" href="Feed.aspx" title="<%= UserControlsCommonResource.WhatsNew %>">
                    <%= UserControlsCommonResource.WhatsNew %>
                </a>
            </div>
            <ul class="menu-sub-list">
                <% if (IsProductAvailable("community"))
                   { %>
                    <li class="menu-sub-item filter">
                        <span class="menu-item-icon community"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/feed-icons.svg#feedIconscommunity"></use></svg></span>
                        <a id="feed-community-product-nav" class="menu-item-label outer-text text-overflow" href="#" title="<%= FeedResource.CommunityProduct %>">
                            <%= FeedResource.CommunityProduct %>
                        </a>
                    </li>
                <% } %>
                <% if (IsProductAvailable("crm"))
                   { %>
                    <li class="menu-sub-item filter">
                        <span class="menu-item-icon company"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/feed-icons.svg#feedIconscrm"></use></svg></span>
                        <a id="feed-crm-product-nav" class="menu-item-label outer-text text-overflow" href="#" title="<%= FeedResource.CrmProduct %>">
                            <%= FeedResource.CrmProduct %>
                        </a>
                    </li>
                <% } %>
                <% if (IsProductAvailable("projects"))
                   { %>
                    <li class="menu-sub-item filter">
                        <span class="menu-item-icon projects"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/feed-icons.svg#feedIconsprojects"></use></svg></span>
                        <a id="feed-projects-product-nav" class="menu-item-label outer-text text-overflow" href="#" title="<%= FeedResource.ProjectsProduct %>">
                            <%= FeedResource.ProjectsProduct %>
                        </a>
                    </li>
                <% } %>

                <% if (IsProductAvailable("documents"))
                   { %>
                    <li class="menu-sub-item filter">
                        <span class="menu-item-icon documents"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/feed-icons.svg#feedIconsdocuments"></use></svg></span>
                        <a id="feed-documents-product-nav" class="menu-item-label outer-text text-overflow" href="#" title="<%= FeedResource.DocumentsProduct %>">
                            <%= FeedResource.DocumentsProduct %>
                        </a>
                    </li>
                <% } %>
            </ul>
        </li>
        <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="UserForumHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
    </ul>
</div>